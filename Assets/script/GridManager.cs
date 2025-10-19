using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
    =================================================================
    GridManager.cs (Final Corrected Logic)
    - 이제 이 스크립트는 오직 '빈 타일 프레임'을 생성하고 교체하는 역할만 합니다.
    - 각 타일의 내부는 해당 Tile 스크립트가 독립적으로 관리합니다.
    =================================================================
*/
public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 3;
    public int gridHeight = 3;

    [Header("Core Tile Prefab")]
    [Tooltip("구조물을 담을 수 있는 '빈 타일 프레임' 프리팹을 연결하세요.")]
    public GameObject tileFramePrefab;

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

        Vector2 tileSpacing = new Vector2(tileCollider.size.x, tileCollider.size.y);

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