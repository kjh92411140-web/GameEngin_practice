using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MapLayoutData
{
    [TextArea(3, 5)]
    public string[] layout;
}

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 3;
    public int gridHeight = 3;

    [Header("Core Tile Prefab")]
    [Tooltip("구조물을 담을 수 있는 '빈 타일 프레임' 프리팹을 연결하세요.")]
    public GameObject tileFramePrefab;

    [Header("Tile Blueprints")]
    [Tooltip("생성될 타일들에게 순서대로 적용될 맵 설계도 목록입니다.")]
    public List<MapLayoutData> tileLayouts;

    [Header("Game Mechanics")]
    public float swapAnimationSpeed = 8f;

    private GameObject[,] grid;
    private Tile selectedTile1 = null;
    private Tile selectedTile2 = null;
    private bool isSwapping = false;

    void Start()
    {
        // ================== [핵심 수정] ==================
        // 게임이 시작되면, 혹시 남아있을지 모를 모든
        // '미리보기'용 타일들을 완전히 파괴하고 시작합니다.
        ClearAllTiles();
        // ===============================================

        CreateGrid();
    }

    // ================== [코드 추가 및 수정] ==================
    // GridManager의 자식으로 있는 모든 타일 오브젝트를 파괴하는 함수
    private void ClearAllTiles()
    {
        // GetComponentsInChildren는 자기 자신도 포함하므로, 
        // Tile 컴포넌트만 골라서 모두 파괴합니다.
        Tile[] existingTiles = GetComponentsInChildren<Tile>();

        // 게임 실행 중에는 Destroy, 에디터에서는 DestroyImmediate를 사용해야 합니다.
        if (Application.isPlaying)
        {
            foreach (var tile in existingTiles)
            {
                Destroy(tile.gameObject);
            }
        }
        else
        {
            foreach (var tile in existingTiles)
            {
                DestroyImmediate(tile.gameObject);
            }
        }
    }
    // =======================================================

    void CreateGrid()
    {
        if (tileFramePrefab == null)
        {
            Debug.LogError("Tile Frame Prefab이 할당되지 않았습니다!");
            return;
        }

        BoxCollider tileCollider = tileFramePrefab.GetComponent<BoxCollider>();
        if (tileCollider == null)
        {
            Debug.LogError("Tile Frame Prefab에 BoxCollider가 없습니다!");
            return;
        }

        Vector3 tileSize = tileCollider.size;
        Vector3 prefabScale = tileFramePrefab.transform.localScale;
        Vector2 tileSpacing = new Vector2(tileSize.x * prefabScale.x, tileSize.y * prefabScale.y);

        grid = new GameObject[gridWidth, gridHeight];
        Vector3 gridOffset = new Vector3(-(gridWidth - 1) * tileSpacing.x / 2f, -(gridHeight - 1) * tileSpacing.y / 2f, 0);

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector3 position = new Vector3(x * tileSpacing.x, y * tileSpacing.y, 0) + gridOffset;
                GameObject newTileFrame = Instantiate(tileFramePrefab, position, Quaternion.identity, transform);
                newTileFrame.name = $"Tile ({x}, {y})";

                Tile tileComponent = newTileFrame.GetComponent<Tile>();
                tileComponent.SetPosition(x, y);

                int tileIndex = y * gridWidth + x;
                if (tileLayouts != null && tileIndex < tileLayouts.Count)
                {
                    tileComponent.mapLayout = tileLayouts[tileIndex].layout;
                    tileComponent.GenerateStructures();
                }

                grid[x, y] = newTileFrame;
            }
        }
    }

    void Update() { if (Input.GetMouseButtonDown(0) && !isSwapping) HandleSelection(); }

    void HandleSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Tile clickedTile = hit.collider.GetComponentInParent<Tile>();
            if (clickedTile != null) SelectTile(clickedTile);
        }
    }

    void SelectTile(Tile tile)
    {
        if (!tile.isSwappable) return;
        if (selectedTile1 == null) { selectedTile1 = tile; selectedTile1.Highlight(true); }
        else
        {
            if (selectedTile1 == tile) { selectedTile1.Highlight(false); selectedTile1 = null; return; }
            selectedTile2 = tile;
            if (selectedTile2.isSwappable) StartCoroutine(SwapTilesAnimation());
        }
    }

    IEnumerator SwapTilesAnimation()
    {
        isSwapping = true;
        selectedTile1.Highlight(false); selectedTile2.Highlight(false);
        Vector3 pos1 = selectedTile1.transform.position; Vector3 pos2 = selectedTile2.transform.position;
        float t = 0;
        while (t < 1.0f)
        {
            t += Time.deltaTime * swapAnimationSpeed;
            selectedTile1.transform.position = Vector3.Lerp(pos1, pos2, t);
            selectedTile2.transform.position = Vector3.Lerp(pos2, pos1, t);
            yield return null;
        }
        selectedTile1.transform.position = pos2; selectedTile2.transform.position = pos1;
        int x1 = selectedTile1.gridX, y1 = selectedTile1.gridY; int x2 = selectedTile2.gridX, y2 = selectedTile2.gridY;
        grid[x1, y1] = selectedTile2.gameObject; grid[x2, y2] = selectedTile1.gameObject;
        selectedTile1.SetPosition(x2, y2); selectedTile2.SetPosition(x1, y1);
        selectedTile1 = null; selectedTile2 = null;
        isSwapping = false;
    }

    // ================== [에디터용 편의 기능] ==================
    // 에디터에서 그리드를 미리 생성해볼 때 사용하는 버튼입니다.
    [ContextMenu("Generate Grid In Editor")]
    public void GenerateGridInEditor()
    {
        // 먼저 싹 지우고, 그 다음에 새로 만듭니다.
        ClearAllTiles();
        CreateGrid();
    }

    // 에디터에서 생성했던 모든 타일을 지울 때 사용합니다.
    [ContextMenu("Clear Grid In Editor")]
    public void ClearGridInEditor()
    {
        ClearAllTiles();
    }
    // =======================================================
}