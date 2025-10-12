using UnityEngine;
using System.Collections;

/*
    =================================================================
    GridManager.cs (Back to Basics Version)
    - ���� Ÿ�� ������ ��ü ��ɿ��� ������ ���� �⺻���� �����Դϴ�.
    - Inspector â�� �ؽ�Ʈ�� �� ���̾ƿ��� �������� �� �ֽ��ϴ�.
    - �� ��ũ��Ʈ�� ��� �ִ� ���� ������Ʈ�� �߰��ϼ���.
    =================================================================
*/
public class GridManager : MonoBehaviour
{
    [Header("Tile Prefabs")]
    public GameObject normalTilePrefab; // ��ü ������ �Ϲ� Ÿ��
    public GameObject wallTilePrefab;   // ��ü �Ұ����� �� Ÿ��
    public GameObject emptyTilePrefab;  // ��ü ������ �� ����

    [Header("Map Layout (W: Wall, N: Normal, E: Empty)")]
    [TextArea(5, 10)]
    // �⺻���� �����Ͽ� Inspector���� �����ϵ��� ����
    public string[] mapLayout;

    [Header("Game Mechanics")]
    public float swapAnimationSpeed = 8f; // Ÿ���� �����̴� �ӵ�

    // --- ���� ������ ---
    private GameObject[,] grid;
    private Tile selectedTile1 = null;
    private Tile selectedTile2 = null;
    private bool isSwapping = false; // �ִϸ��̼� �ߺ� ���� ������

    void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        if (normalTilePrefab == null)
        {
            Debug.LogError("Normal Tile Prefab�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        // �������� X�� Y ������ ���� ���� �о�ɴϴ�.
        Vector2 tileSize = new Vector2(
            normalTilePrefab.transform.localScale.x,
            normalTilePrefab.transform.localScale.y
        );

        int gridHeight = mapLayout.Length;
        if (gridHeight == 0) return;
        int gridWidth = mapLayout[0].Length;
        grid = new GameObject[gridWidth, gridHeight];

        // ���� Ÿ�� ũ�⸦ �������� �׸��带 ȭ�� ���߾ӿ� ��ġ�ϱ� ���� ���
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
                    // X��ġ�� tileSize.x��, Y��ġ�� tileSize.y�� ����մϴ�.
                    Vector3 position = new Vector3(x * tileSize.x, spawnY * tileSize.y, 0) + gridOffset;
                    GameObject newTile = Instantiate(prefabToUse, position, Quaternion.identity, transform);

                    Tile tileComponent = newTile.GetComponent<Tile>();
                    if (tileComponent == null) tileComponent = newTile.AddComponent<Tile>(); // ��ũ��Ʈ�� ������ �߰�

                    tileComponent.SetPosition(x, spawnY);
                    tileComponent.isSwappable = isSwappable;
                    grid[x, spawnY] = newTile;
                }
            }
        }
    }

    void Update()
    {
        // �ִϸ��̼��� ���� ���� ���� �Է��� ���� ����
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
        if (!tile.isSwappable) return; // ��ü �Ұ����� Ÿ���� ���� �� ��

        if (selectedTile1 == null)
        {
            selectedTile1 = tile;
            selectedTile1.Highlight(true); // ù ��° Ÿ�� ���� (���̶���Ʈ)
        }
        else
        {
            if (selectedTile1 == tile) // ���� Ÿ�� �ٽ� Ŭ�� �� ���� ���
            {
                selectedTile1.Highlight(false);
                selectedTile1 = null;
                return;
            }

            selectedTile2 = tile;
            if (selectedTile2.isSwappable) // �� ��° Ÿ�ϵ� ��ü �����ϸ�
            {
                StartCoroutine(SwapTilesAnimation()); // ��ü �ִϸ��̼� ����
            }
        }
    }

    IEnumerator SwapTilesAnimation()
    {
        isSwapping = true;
        selectedTile1.Highlight(false);

        Vector3 pos1 = selectedTile1.transform.position;
        Vector3 pos2 = selectedTile2.transform.position;

        // �ִϸ��̼� ����
        float t = 0;
        while (t < 1.0f)
        {
            t += Time.deltaTime * swapAnimationSpeed;
            selectedTile1.transform.position = Vector3.Lerp(pos1, pos2, t);
            selectedTile2.transform.position = Vector3.Lerp(pos2, pos1, t);
            yield return null; // ���� �����ӱ��� ���
        }

        // ���� ��ġ ����
        selectedTile1.transform.position = pos2;
        selectedTile2.transform.position = pos1;

        // GridManager�� �����ϴ� ������ ������Ʈ (�ſ� �߿�)
        int x1 = selectedTile1.gridX, y1 = selectedTile1.gridY;
        int x2 = selectedTile2.gridX, y2 = selectedTile2.gridY;
        grid[x1, y1] = selectedTile2.gameObject;
        grid[x2, y2] = selectedTile1.gameObject;

        // �� Ÿ���� ������ ����ϴ� ��ġ ���� ������Ʈ
        selectedTile1.SetPosition(x2, y2);
        selectedTile2.SetPosition(x1, y1);

        // ���� ���� �ʱ�ȭ
        selectedTile1 = null;
        selectedTile2 = null;
        isSwapping = false;
    }
}