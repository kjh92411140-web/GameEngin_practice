// /Assets/script/GridManager.cs

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 3;
    public int gridHeight = 3;

    [Header("Core Prefab")]
    [Tooltip("MovableTile�� StructureHolder�� ���� 'Tile_Container' �������� �����ϼ���.")]
    public Tile_Container tileContainerPrefab; // TileFrame -> Tile_Container

    [Header("Tile Blueprints")]
    [Tooltip("������ Ÿ�ϵ鿡�� ������� ����� �� ���赵 ����Դϴ�.")]
    public List<MapLayoutData> tileLayouts;

    [Header("Game Mechanics")]
    public float swapAnimationSpeed = 8f;

    // --- ���� ���� ---
    private Tile_Container[,] grid; // GameObject -> Tile_Container
    private Tile_Container selectedTile1 = null; // Tile -> Tile_Container
    private Tile_Container selectedTile2 = null; // Tile -> Tile_Container
    private bool isSwapping = false;


    void Start()
    {
        ClearAllContainers();
        CreateGrid();
    }

    void CreateGrid()
    {
        if (tileContainerPrefab == null)
        {
            Debug.LogError("Tile Container Prefab�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        BoxCollider tileCollider = tileContainerPrefab.GetComponent<BoxCollider>();
        if (tileCollider == null)
        {
            Debug.LogError("Tile Container Prefab�� BoxCollider�� �����ϴ�!");
            return;
        }

        Vector3 tileSize = tileCollider.size;
        Vector3 prefabScale = tileContainerPrefab.transform.localScale;
        Vector2 tileSpacing = new Vector2(tileSize.x * prefabScale.x, tileSize.y * prefabScale.y);

        grid = new Tile_Container[gridWidth, gridHeight];
        Vector3 gridOffset = new Vector3(-(gridWidth - 1) * tileSpacing.x / 2f, -(gridHeight - 1) * tileSpacing.y / 2f, 0);

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector3 position = new Vector3(x * tileSpacing.x, y * tileSpacing.y, 0) + gridOffset;

                // �̸��� ������ Container �������� ����
                Tile_Container newContainer = Instantiate(tileContainerPrefab, position, Quaternion.identity, transform);
                newContainer.name = $"TileContainer ({x}, {y})";
                newContainer.SetPosition(x, y);

                int tileIndex = y * gridWidth + x;
                if (tileLayouts != null && tileIndex < tileLayouts.Count)
                {
                    newContainer.mapLayout = tileLayouts[tileIndex].layout;
                    newContainer.GenerateStructures();
                }

                grid[x, y] = newContainer;
            }
        }
    }

    // --- Ÿ�� ���� �� ��ü ���� (Container �������� ����) ---
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isSwapping) HandleSelection();
    }

    void HandleSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Tile_Container clickedContainer = hit.collider.GetComponentInParent<Tile_Container>();
            if (clickedContainer != null) SelectTile(clickedContainer);
        }
    }

    void SelectTile(Tile_Container container)
    {
        if (!container.isSwappable) return;
        if (selectedTile1 == null)
        {
            selectedTile1 = container;
            selectedTile1.Highlight(true);
        }
        else
        {
            if (selectedTile1 == container)
            {
                selectedTile1.Highlight(false);
                selectedTile1 = null;
                return;
            }
            selectedTile2 = container;
            if (selectedTile2.isSwappable) StartCoroutine(SwapTilesAnimation());
        }
    }

    IEnumerator SwapTilesAnimation()
    {
        isSwapping = true;
        selectedTile1.Highlight(false);
        selectedTile2.Highlight(false);

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

        grid[x1, y1] = selectedTile2;
        grid[x2, y2] = selectedTile1;

        selectedTile1.SetPosition(x2, y2);
        selectedTile2.SetPosition(x1, y1);

        selectedTile1 = null;
        selectedTile2 = null;
        isSwapping = false;
    }

    // --- �����Ϳ� ���� ��� (Container �������� �̸� ����) ---
    private void ClearAllContainers()
    {
        Tile_Container[] existingContainers = GetComponentsInChildren<Tile_Container>();
        foreach (var container in existingContainers)
        {
            if (Application.isPlaying) Destroy(container.gameObject);
            else DestroyImmediate(container.gameObject);
        }
    }

    [ContextMenu("Generate Grid In Editor")]
    public void GenerateGridInEditor()
    {
        ClearAllContainers();
        CreateGrid();
    }

    [ContextMenu("Clear Grid In Editor")]
    public void ClearGridInEditor()
    {
        ClearAllContainers();
    }
}

// GridManager�� MapLayoutData�� ����ϹǷ�, �� ���� �ȿ� ���� �Ӵϴ�.
[System.Serializable]
public class MapLayoutData
{
    [TextArea(3, 5)]
    public string[] layout;
}