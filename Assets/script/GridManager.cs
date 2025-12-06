using UnityEngine;
using System.Collections;

public class GridManager : MonoBehaviour
{
    [Header("Tile Prefabs")]
    public GameObject normalTilePrefab; // 교체 가능한 일반 타일
    public GameObject wallTilePrefab;   // 교체 불가능한 벽 타일
    public GameObject emptyTilePrefab;  // 교체 가능한 빈 공간

    [Header("Object Prefabs")] 
    public GameObject myCustomObjectPrefab;

    [Header("Map Layout (W: Wall, N: Normal, E: Empty)")]
    [TextArea(5, 10)]
    
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
        Tile[] allTiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);

        if (allTiles.Length == 0)
        {
            Debug.LogError("씬에서 'Tile' 컴포넌트를 가진 오브젝트를 찾을 수 없습니다! 수동으로 배치하고 Grid X, Grid Y를 설정했는지 확인하세요.");
            return;
        }

        int maxGridX = 0;
        int maxGridY = 0;
        foreach (Tile tile in allTiles)
        {
            if (tile.gridX > maxGridX) maxGridX = tile.gridX;
            if (tile.gridY > maxGridY) maxGridY = tile.gridY;
        }

        grid = new GameObject[maxGridX + 1, maxGridY + 1];

        foreach (Tile tile in allTiles)
        {
            grid[tile.gridX, tile.gridY] = tile.gameObject;

            if (tile.transform.parent != this.transform)
            {
                tile.transform.SetParent(this.transform);
            }
        }

        Debug.Log($"씬에서 {grid.GetLength(0)}x{grid.GetLength(1)} 크기의 그리드를 초기화했습니다. 총 타일 수: {allTiles.Length}");
    }

    void Update()
    {
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

        
        int x1 = selectedTile1.gridX, y1 = selectedTile1.gridY;
        int x2 = selectedTile2.gridX, y2 = selectedTile2.gridY;
        grid[x1, y1] = selectedTile2.gameObject;
        grid[x2, y2] = selectedTile1.gameObject;

        selectedTile1.SetPosition(x2, y2);
        selectedTile2.SetPosition(x1, y1);

        // 선택 상태 초기화
        selectedTile1 = null;
        selectedTile2 = null;
        isSwapping = false;
    }
    public GameObject GetTileObject(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < grid.GetLength(0) && y < grid.GetLength(1))
        {
            return grid[x, y];
        }
        return null;
    }

    public bool IsValidMove(int x, int y)
    {
        GameObject tile = GetTileObject(x, y);

        if (tile == null)
        {
            return false;
        }

        Tile tileComponent = tile.GetComponent<Tile>();

        if (tileComponent == null || !tileComponent.isSwappable)
        {
            return false;
        }

        return true;
    }
}