using UnityEngine;
using System.Collections;

/*
    =================================================================
    GridManager.cs (Back to Basics Version)
    - 오직 타일 생성과 교체 기능에만 집중한 가장 기본적인 버전입니다.
    - Inspector 창의 텍스트로 맵 레이아웃을 디자인할 수 있습니다.
    - 이 스크립트를 비어 있는 게임 오브젝트에 추가하세요.
    =================================================================
*/
public class GridManager : MonoBehaviour
{
    [Header("Tile Prefabs")]
    public GameObject normalTilePrefab; // 교체 가능한 일반 타일
    public GameObject wallTilePrefab;   // 교체 불가능한 벽 타일
    public GameObject emptyTilePrefab;  // 교체 가능한 빈 공간

    [Header("Map Layout (W: Wall, N: Normal, E: Empty)")]
    [TextArea(5, 10)]
    // 기본값을 삭제하여 Inspector에서 설정하도록 변경
    public string[] mapLayout;

    [Header("Game Mechanics")]
    public float swapAnimationSpeed = 8f; // 타일이 움직이는 속도

    // --- 내부 변수들 ---
    private GameObject[,] grid;
    private Tile selectedTile1 = null;
    private Tile selectedTile2 = null;
    private bool isSwapping = false; // 애니메이션 중복 실행 방지용

    void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        if (normalTilePrefab == null)
        {
            Debug.LogError("Normal Tile Prefab이 할당되지 않았습니다!");
            return;
        }

        // 프리팹의 X와 Y 스케일 값을 각각 읽어옵니다.
        Vector2 tileSize = new Vector2(
            normalTilePrefab.transform.localScale.x,
            normalTilePrefab.transform.localScale.y
        );

        int gridHeight = mapLayout.Length;
        if (gridHeight == 0) return;
        int gridWidth = mapLayout[0].Length;
        grid = new GameObject[gridWidth, gridHeight];

        // 실제 타일 크기를 바탕으로 그리드를 화면 정중앙에 배치하기 위한 계산
        Vector3 gridOffset = new Vector3(-(gridWidth - 1) * tileSize.x / 2f, -(gridHeight - 1) * tileSize.y / 2f, 0);

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (x >= mapLayout[y].Length) continue;

                char tileChar = mapLayout[y][x];
                GameObject prefabToUse = null;
                bool isSwappable = false;

                switch (tileChar)
                {
                    case 'W': prefabToUse = wallTilePrefab; isSwappable = false; break;
                    case 'N': prefabToUse = normalTilePrefab; isSwappable = true; break;
                    case 'E': prefabToUse = emptyTilePrefab; isSwappable = true; break;
                }

                if (prefabToUse != null)
                {
                    int spawnY = gridHeight - 1 - y;
                    // X위치는 tileSize.x로, Y위치는 tileSize.y로 계산합니다.
                    Vector3 position = new Vector3(x * tileSize.x, spawnY * tileSize.y, 0) + gridOffset;
                    GameObject newTile = Instantiate(prefabToUse, position, Quaternion.identity, transform);

                    Tile tileComponent = newTile.GetComponent<Tile>();
                    if (tileComponent == null) tileComponent = newTile.AddComponent<Tile>(); // 스크립트가 없으면 추가

                    tileComponent.SetPosition(x, spawnY);
                    tileComponent.isSwappable = isSwappable;
                    grid[x, spawnY] = newTile;
                }
            }
        }
    }

    void Update()
    {
        // 애니메이션이 실행 중일 때는 입력을 받지 않음
        if (Input.GetMouseButtonDown(0) && !isSwapping)
        {
            HandleSelection();
        }
    }

    void HandleSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Tile clickedTile = hit.collider.GetComponent<Tile>();
            if (clickedTile != null)
            {
                SelectTile(clickedTile);
            }
        }
    }

    void SelectTile(Tile tile)
    {
        if (!tile.isSwappable) return; // 교체 불가능한 타일은 선택 안 됨

        if (selectedTile1 == null)
        {
            selectedTile1 = tile;
            selectedTile1.Highlight(true); // 첫 번째 타일 선택 (하이라이트)
        }
        else
        {
            if (selectedTile1 == tile) // 같은 타일 다시 클릭 시 선택 취소
            {
                selectedTile1.Highlight(false);
                selectedTile1 = null;
                return;
            }

            selectedTile2 = tile;
            if (selectedTile2.isSwappable) // 두 번째 타일도 교체 가능하면
            {
                StartCoroutine(SwapTilesAnimation()); // 교체 애니메이션 시작
            }
        }
    }

    IEnumerator SwapTilesAnimation()
    {
        isSwapping = true;
        selectedTile1.Highlight(false);

        Vector3 pos1 = selectedTile1.transform.position;
        Vector3 pos2 = selectedTile2.transform.position;

        // 애니메이션 실행
        float t = 0;
        while (t < 1.0f)
        {
            t += Time.deltaTime * swapAnimationSpeed;
            selectedTile1.transform.position = Vector3.Lerp(pos1, pos2, t);
            selectedTile2.transform.position = Vector3.Lerp(pos2, pos1, t);
            yield return null; // 다음 프레임까지 대기
        }

        // 최종 위치 고정
        selectedTile1.transform.position = pos2;
        selectedTile2.transform.position = pos1;

        // GridManager가 관리하는 데이터 업데이트 (매우 중요)
        int x1 = selectedTile1.gridX, y1 = selectedTile1.gridY;
        int x2 = selectedTile2.gridX, y2 = selectedTile2.gridY;
        grid[x1, y1] = selectedTile2.gameObject;
        grid[x2, y2] = selectedTile1.gameObject;

        // 각 타일이 스스로 기억하는 위치 정보 업데이트
        selectedTile1.SetPosition(x2, y2);
        selectedTile2.SetPosition(x1, y1);

        // 선택 상태 초기화
        selectedTile1 = null;
        selectedTile2 = null;
        isSwapping = false;
    }
}