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

    [Header("Object Prefabs")] // <--- 이 부분을 추가합니다.
    public GameObject myCustomObjectPrefab;

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
        InitializeGridFromScene();
    }

    void InitializeGridFromScene()
    {
        // 씬에 있는 모든 Tile 컴포넌트(스크립트)를 가진 오브젝트를 찾습니다.
        Tile[] allTiles = FindObjectsOfType<Tile>();

        if (allTiles.Length == 0)
        {
            Debug.LogError("씬에서 'Tile' 컴포넌트를 가진 오브젝트를 찾을 수 없습니다! 타일을 배치하고 Grid X, Grid Y를 설정했는지 확인하세요.");
            return;
        }

        // 씬에 배치된 타일 중 가장 큰 좌표(Max X, Max Y)를 찾아 그리드 크기를 결정합니다.
        int maxGridX = 0;
        int maxGridY = 0;
        foreach (Tile tile in allTiles)
        {
            if (tile.gridX > maxGridX) maxGridX = tile.gridX;
            if (tile.gridY > maxGridY) maxGridY = tile.gridY;
        }

        // 그리드 배열을 초기화합니다. (크기는 최대 좌표 + 1)
        grid = new GameObject[maxGridX + 1, maxGridY + 1];

        // 찾은 모든 타일을 GridManager의 2차원 배열에 저장합니다.
        foreach (Tile tile in allTiles)
        {
            // 타일 오브젝트를 GridManager 배열에 정확한 좌표에 저장합니다. (GridManager의 핵심 데이터)
            grid[tile.gridX, tile.gridY] = tile.gameObject;

            // Hierarchy를 깔끔하게 유지하기 위해, GridManager의 자식으로 설정할 수 있습니다.
            if (tile.transform.parent != this.transform)
            {
                tile.transform.SetParent(this.transform);
            }
        }

        Debug.Log($"씬에서 {grid.GetLength(0)}x{grid.GetLength(1)} 크기의 그리드를 초기화했습니다. 총 타일 수: {allTiles.Length}");
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