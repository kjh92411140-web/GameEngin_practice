using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
    =================================================================
    GridManager.cs (수정된 버전)
    - 이제 여러 개의 '맵 설계도'를 리스트로 관리합니다.
    - 타일 생성 시, 순서에 맞는 설계도를 각 타일에 주입해줍니다.
    =================================================================
*/

// 인스펙터 창에서 맵 레이아웃을 편하게 관리하기 위한 보조 클래스
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

    // ================== [코드 추가] ==================
    [Header("Tile Blueprints")]
    [Tooltip("생성될 타일들에게 순서대로 적용될 맵 설계도 목록입니다.")]
    public List<MapLayoutData> tileLayouts;
    // ===============================================

    [Header("Game Mechanics")]
    public float swapAnimationSpeed = 8f;

    private GameObject[,] grid;
    private Tile selectedTile1 = null;
    private Tile selectedTile2 = null;
    private bool isSwapping = false;

    void Start()
    {
        CreateGrid();
    }

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

                // ================== [코드 수정] ==================
                // 현재 타일의 순번(인덱스)을 계산합니다.
                int tileIndex = y * gridWidth + x;

                // 설계도 목록에 해당 순번의 설계도가 있는지 확인하고 적용합니다.
                if (tileLayouts != null && tileIndex < tileLayouts.Count)
                {
                    // GridManager가 가지고 있는 설계도를 타일에 주입합니다.
                    tileComponent.mapLayout = tileLayouts[tileIndex].layout;

                    // 설계도가 적용되었으니, 구조물을 즉시 생성하도록 명령합니다.
                    tileComponent.GenerateStructures();
                }
                // ===============================================

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
}