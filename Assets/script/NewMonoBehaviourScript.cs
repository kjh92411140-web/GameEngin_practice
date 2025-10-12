using UnityEngine;

/*
    =================================================================
    GridManager.cs
    - �� ��ũ��Ʈ�� ������ ��ü ��(�׸���)�� �����մϴ�.
    - �� ����(Ÿ��)�� �����ϰ�, ������� �Է��� �޾� Ÿ���� ��ü�ϴ� ������ ó���մϴ�.
    - �� ���� ������Ʈ�� �� ��ũ��Ʈ�� �߰��Ͽ� ����ϼ���.
    =================================================================
*/
public class GridManager : MonoBehaviour
{
    // --- Unity Inspector���� ������ ������ ---
    [Header("Grid Settings")]
    public int gridWidth = 8; // �׸��� ���� ũ��
    public int gridHeight = 5; // �׸��� ���� ũ��
    public float tileSpacing = 1.1f; // Ÿ�� ������ ����

    [Header("Tile Prefab")]
    public GameObject tilePrefab; // �� �������� ����� ������

    // --- ���� ������ ---
    private GameObject[,] grid; // �׸��� ���� Ÿ�ϵ��� ������ 2D �迭
    private Tile selectedTile1 = null; // ù ��°�� ���õ� Ÿ��
    private Tile selectedTile2 = null; // �� ��°�� ���õ� Ÿ��

    void Start()
    {
        grid = new GameObject[gridWidth, gridHeight];
        CreateGrid();
    }

    // �׸���� Ÿ���� �����ϴ� �Լ�
    void CreateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Ÿ�� ���� ��ġ ���
                Vector3 position = new Vector3(x * tileSpacing, y * tileSpacing, 0);

                // ���������κ��� Ÿ�� �ν��Ͻ� ����
                GameObject newTile = Instantiate(tilePrefab, position, Quaternion.identity);
                newTile.transform.parent = this.transform; // GridManager ������Ʈ�� �ڽ����� ����

                // Ÿ�� ��ũ��Ʈ�� �׸��� ��ǥ ���� ����
                Tile tileComponent = newTile.GetComponent<Tile>();
                if (tileComponent != null)
                {
                    tileComponent.SetPosition(x, y);
                }

                // �׸��� �迭�� ������ Ÿ�� ����
                grid[x, y] = newTile;
            }
        }
    }

    void Update()
    {
        // ���콺 ���� ��ư Ŭ�� ����
        if (Input.GetMouseButtonDown(0))
        {
            HandleSelection();
        }
    }

    // ���콺 Ŭ������ Ÿ���� �����ϴ� ���� ó��
    void HandleSelection()
    {
        // ȭ����� ���콺 ��ġ�� ���� ��ǥ�� ��ȯ
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // 2D ȯ���̹Ƿ� z��ǥ�� 0���� ����
        mouseWorldPos.z = 0;

        // ���� ����� Ÿ�� ã�� (������ ���)
        // ���� ������Ʈ������ Raycast ����� �����մϴ�.
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
        {
            Tile clickedTile = hit.collider.GetComponent<Tile>();
            if (clickedTile != null)
            {
                SelectTile(clickedTile);
            }
        }
    }

    // Ÿ�� ���� ����
    void SelectTile(Tile tile)
    {
        if (selectedTile1 == null)
        {
            // ù ��° Ÿ�� ����
            selectedTile1 = tile;
            selectedTile1.Highlight(true); // ���õ� Ÿ�� ���̶���Ʈ
        }
        else
        {
            // �� ��° Ÿ�� ����
            // ���� Ÿ���� �ٽ� Ŭ���� ��� ���� ���
            if (selectedTile1 == tile)
            {
                selectedTile1.Highlight(false);
                selectedTile1 = null;
                return;
            }

            selectedTile2 = tile;
            selectedTile2.Highlight(true);

            // �� Ÿ���� ��� ���õǾ����Ƿ� ��ü ����
            SwapTiles();
        }
    }

    // �� Ÿ���� ��ġ�� ��ü�ϴ� �Լ�
    void SwapTiles()
    {
        // 1. ���� ���� ��ġ(Position) ��ü
        Vector3 tempPosition = selectedTile1.transform.position;
        selectedTile1.transform.position = selectedTile2.transform.position;
        selectedTile2.transform.position = tempPosition;

        // 2. �׸��� �迭������ ��ġ ��ü
        int x1 = selectedTile1.gridX;
        int y1 = selectedTile1.gridY;
        int x2 = selectedTile2.gridX;
        int y2 = selectedTile2.gridY;

        grid[x1, y1] = selectedTile2.gameObject;
        grid[x2, y2] = selectedTile1.gameObject;

        // 3. �� Ÿ���� ���� �׸��� ��ǥ ���� ������Ʈ
        selectedTile1.SetPosition(x2, y2);
        selectedTile2.SetPosition(x1, y1);

        // ���� ���� �ʱ�ȭ
        selectedTile1.Highlight(false);
        selectedTile2.Highlight(false);
        selectedTile1 = null;
        selectedTile2 = null;
    }
}


/*
    =================================================================
    Tile.cs
    - �� ��ũ��Ʈ�� ���� �� ����(Ÿ��)�� ������ ����մϴ�.
    - �ڽ��� �׸��� ��ǥ�� �����ϰ�, ���õǾ��� �� �ð��� �ǵ���� �ݴϴ�.
    - �� �������� ����� �����տ� �� ��ũ��Ʈ�� �߰��ϼ���.
    =================================================================
*/
public class Tile : MonoBehaviour
{
    public int gridX; // �׸��� ���� x ��ǥ
    public int gridY; // �׸��� ���� y ��ǥ

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // �ڽ��� �׸��� ��ǥ�� �����ϴ� �Լ�
    public void SetPosition(int x, int y)
    {
        gridX = x;
        gridY = y;
    }

    // ���õǾ��� �� ������ �����Ͽ� ���̶���Ʈ ȿ���� �ִ� �Լ�
    public void Highlight(bool isHighlighted)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isHighlighted ? Color.cyan : Color.white;
        }
    }
}
