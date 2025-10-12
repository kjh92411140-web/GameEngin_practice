using UnityEngine;
using System.Collections;

/*
    =================================================================
    GridManager.cs (Side-View Version with Optimized Borders)
    - ���� �׸���, Ÿ�� ����, �׵θ� �׸��⸦ ��� �����մϴ�.
    - Inspector â���� �ؽ�Ʈ�� �� ���̾ƿ��� �������� �� �ֽ��ϴ�.
    - �� ��ũ��Ʈ�� ��� �ִ� ���� ������Ʈ�� �߰��ؾ� �մϴ�.
    =================================================================
*/
public class GridManager : MonoBehaviour
{
    // --- Unity Inspector���� ������ ������ ---
    [Header("Grid Visuals")]
    [Tooltip("The prefab to use for drawing borders (e.g., a thin cube).")]
    public GameObject borderPrefab; // �׵θ��� �׸��� �� ����� ������
    [Tooltip("The thickness of the border lines.")]
    public float borderThickness = 0.3f; // �׵θ� ���� �β�
    public float swapAnimationSpeed = 8f; // Ÿ�� ��ü �ִϸ��̼� �ӵ�

    [Header("Tile Prefabs")]
    public GameObject normalTilePrefab; // 'N' Ÿ�� ������
    public GameObject wallTilePrefab;   // 'W' Ÿ�� ������
    public GameObject emptyTilePrefab;  // 'E' Ÿ�� ������

    [Header("Map Layout (W: Wall, N: Normal, E: Empty)")]
    [Tooltip("W = Wall (not swappable)\nN = Normal Tile (swappable)\nE = Empty Space (swappable)")]
    [TextArea(5, 10)]
    public string[] mapLayout = new string[]
    {
        "WWWWWWWW",
        "WNNNNNNW",
        "WNNENNNW",
        "WNNNNNNW",
        "WWWWWWWW"
    };

    // --- ���� ������ ---
    private GameObject[,] grid;
    private Tile selectedTile1 = null;
    private Tile selectedTile2 = null;
    private bool isSwapping = false;
    private float tileSize;
    private float cellSpacing;

    void Start()
    {
        CalculateSpacing();
        CreateGrid();
    }

    void CalculateSpacing()
    {
        if (normalTilePrefab != null)
        {
            tileSize = normalTilePrefab.transform.localScale.x;
        }
        else
        {
            tileSize = 1.0f;
        }
        cellSpacing = tileSize + borderThickness;
    }

    void CreateGrid()
    {
        int gridHeight = mapLayout.Length;
        if (gridHeight == 0) return;
        int gridWidth = mapLayout[0].Length;

        grid = new GameObject[gridWidth, gridHeight];

        // Ÿ�� ����
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (x >= mapLayout[y].Length) continue;

                char tileChar = mapLayout[y][x];
                GameObject prefabToUse = null;
                Tile.TileType type = Tile.TileType.Normal;
                bool isSwappable = true;

                switch (tileChar)
                {
                    case 'W': prefabToUse = wallTilePrefab; type = Tile.TileType.Wall; isSwappable = false; break;
                    case 'N': prefabToUse = normalTilePrefab; type = Tile.TileType.Normal; break;
                    case 'E': prefabToUse = emptyTilePrefab; type = Tile.TileType.Empty; break;
                }

                if (prefabToUse != null)
                {
                    int spawnY = gridHeight - 1 - y;
                    Vector3 position = new Vector3(x * cellSpacing, spawnY * cellSpacing, 0);
                    GameObject newTile = Instantiate(prefabToUse, position, Quaternion.identity, this.transform);

                    Tile tileComponent = newTile.GetComponent<Tile>();
                    if (tileComponent != null)
                    {
                        tileComponent.SetPosition(x, spawnY);
                        tileComponent.tileType = type;
                        tileComponent.isSwappable = isSwappable;
                    }
                    grid[x, spawnY] = newTile;
                }
            }
        }

        // ����ȭ�� �׵θ� �׸���
        if (borderPrefab != null)
        {
            DrawGridBorders(gridWidth, gridHeight);
        }
    }

    // ����ȭ�� ��ü �׸��� �׵θ� �׸��� �Լ�
    void DrawGridBorders(int width, int height)
    {
        GameObject borderHolder = new GameObject("GridBorders");
        borderHolder.transform.parent = this.transform;

        float totalWidth = (width * tileSize) + (width * borderThickness);
        float totalHeight = (height * tileSize) + (height * borderThickness);

        float startX = -tileSize / 2f;
        float startY = -tileSize / 2f;

        // ���μ�
        for (int y = 0; y <= height; y++)
        {
            float yPos = startY + y * cellSpacing - (cellSpacing - tileSize) / 2;
            Vector3 position = new Vector3(startX + (totalWidth - tileSize) / 2f, yPos, 0);
            GameObject hBorder = Instantiate(borderPrefab, position, Quaternion.identity, borderHolder.transform);
            hBorder.transform.localScale = new Vector3(totalWidth, borderThickness, borderThickness);
        }

        // ���μ�
        for (int x = 0; x <= width; x++)
        {
            float xPos = startX + x * cellSpacing - (cellSpacing - tileSize) / 2;
            Vector3 position = new Vector3(xPos, startY + (totalHeight - tileSize) / 2f, 0);
            GameObject vBorder = Instantiate(borderPrefab, position, Quaternion.identity, borderHolder.transform);
            vBorder.transform.localScale = new Vector3(borderThickness, totalHeight, borderThickness);
        }
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
        if (!tile.isSwappable) return;

        if (selectedTile1 == null)
        {
            selectedTile1 = tile;
            selectedTile1.Highlight(true);
        }
        else
        {
            if (selectedTile1 == tile)
            {
                selectedTile1.Highlight(false);
                selectedTile1 = null;
                return;
            }
            selectedTile2 = tile;
            if (selectedTile2.isSwappable)
            {
                StartCoroutine(SwapTilesAnimation());
            }
        }
    }

    IEnumerator SwapTilesAnimation()
    {
        isSwapping = true;
        selectedTile1.Highlight(false);

        Vector3 pos1 = selectedTile1.transform.position;
        Vector3 pos2 = selectedTile2.transform.position;

        float t = 0;
        while (t < 1.0f)
        {
            t += Time.deltaTime * swapAnimationSpeed;
            selectedTile1.transform.position = Vector3.Lerp(pos1, pos2, t);
            selectedTile2.transform.position = Vector3.Lerp(pos2, pos1, t);
            yield return null;
        }

        selectedTile1.transform.position = pos2;
        selectedTile2.transform.position = pos1;

        int x1 = selectedTile1.gridX, y1 = selectedTile1.gridY;
        int x2 = selectedTile2.gridX, y2 = selectedTile2.gridY;

        grid[x1, y1] = selectedTile2.gameObject;
        grid[x2, y2] = selectedTile1.gameObject;

        selectedTile1.SetPosition(x2, y2);
        selectedTile2.SetPosition(x1, y1);

        selectedTile1 = null;
        selectedTile2 = null;
        isSwapping = false;
    }
}