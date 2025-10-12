using UnityEngine;

/*
    =================================================================
    GridManager.cs
    - 이 스크립트는 게임의 전체 맵(그리드)을 관리합니다.
    - 맵 조각(타일)을 생성하고, 사용자의 입력을 받아 타일을 교체하는 로직을 처리합니다.
    - 빈 게임 오브젝트에 이 스크립트를 추가하여 사용하세요.
    =================================================================
*/
public class GridManager : MonoBehaviour
{
    // --- Unity Inspector에서 설정할 변수들 ---
    [Header("Grid Settings")]
    public int gridWidth = 8; // 그리드 가로 크기
    public int gridHeight = 5; // 그리드 세로 크기
    public float tileSpacing = 1.1f; // 타일 사이의 간격

    [Header("Tile Prefab")]
    public GameObject tilePrefab; // 맵 조각으로 사용할 프리팹

    // --- 내부 변수들 ---
    private GameObject[,] grid; // 그리드 상의 타일들을 저장할 2D 배열
    private Tile selectedTile1 = null; // 첫 번째로 선택된 타일
    private Tile selectedTile2 = null; // 두 번째로 선택된 타일

    void Start()
    {
        grid = new GameObject[gridWidth, gridHeight];
        CreateGrid();
    }

    // 그리드와 타일을 생성하는 함수
    void CreateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // 타일 생성 위치 계산
                Vector3 position = new Vector3(x * tileSpacing, y * tileSpacing, 0);

                // 프리팹으로부터 타일 인스턴스 생성
                GameObject newTile = Instantiate(tilePrefab, position, Quaternion.identity);
                newTile.transform.parent = this.transform; // GridManager 오브젝트의 자식으로 설정

                // 타일 스크립트에 그리드 좌표 정보 저장
                Tile tileComponent = newTile.GetComponent<Tile>();
                if (tileComponent != null)
                {
                    tileComponent.SetPosition(x, y);
                }

                // 그리드 배열에 생성된 타일 저장
                grid[x, y] = newTile;
            }
        }
    }

    void Update()
    {
        // 마우스 왼쪽 버튼 클릭 감지
        if (Input.GetMouseButtonDown(0))
        {
            HandleSelection();
        }
    }

    // 마우스 클릭으로 타일을 선택하는 로직 처리
    void HandleSelection()
    {
        // 화면상의 마우스 위치를 월드 좌표로 변환
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // 2D 환경이므로 z좌표는 0으로 고정
        mouseWorldPos.z = 0;

        // 가장 가까운 타일 찾기 (간단한 방식)
        // 실제 프로젝트에서는 Raycast 사용을 권장합니다.
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

    // 타일 선택 로직
    void SelectTile(Tile tile)
    {
        if (selectedTile1 == null)
        {
            // 첫 번째 타일 선택
            selectedTile1 = tile;
            selectedTile1.Highlight(true); // 선택된 타일 하이라이트
        }
        else
        {
            // 두 번째 타일 선택
            // 같은 타일을 다시 클릭한 경우 선택 취소
            if (selectedTile1 == tile)
            {
                selectedTile1.Highlight(false);
                selectedTile1 = null;
                return;
            }

            selectedTile2 = tile;
            selectedTile2.Highlight(true);

            // 두 타일이 모두 선택되었으므로 교체 실행
            SwapTiles();
        }
    }

    // 두 타일의 위치를 교체하는 함수
    void SwapTiles()
    {
        // 1. 월드 상의 위치(Position) 교체
        Vector3 tempPosition = selectedTile1.transform.position;
        selectedTile1.transform.position = selectedTile2.transform.position;
        selectedTile2.transform.position = tempPosition;

        // 2. 그리드 배열에서의 위치 교체
        int x1 = selectedTile1.gridX;
        int y1 = selectedTile1.gridY;
        int x2 = selectedTile2.gridX;
        int y2 = selectedTile2.gridY;

        grid[x1, y1] = selectedTile2.gameObject;
        grid[x2, y2] = selectedTile1.gameObject;

        // 3. 각 타일이 가진 그리드 좌표 정보 업데이트
        selectedTile1.SetPosition(x2, y2);
        selectedTile2.SetPosition(x1, y1);

        // 선택 상태 초기화
        selectedTile1.Highlight(false);
        selectedTile2.Highlight(false);
        selectedTile1 = null;
        selectedTile2 = null;
    }
}


/*
    =================================================================
    Tile.cs
    - 이 스크립트는 개별 맵 조각(타일)의 동작을 담당합니다.
    - 자신의 그리드 좌표를 저장하고, 선택되었을 때 시각적 피드백을 줍니다.
    - 맵 조각으로 사용할 프리팹에 이 스크립트를 추가하세요.
    =================================================================
*/
public class Tile : MonoBehaviour
{
    public int gridX; // 그리드 상의 x 좌표
    public int gridY; // 그리드 상의 y 좌표

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // 자신의 그리드 좌표를 설정하는 함수
    public void SetPosition(int x, int y)
    {
        gridX = x;
        gridY = y;
    }

    // 선택되었을 때 색상을 변경하여 하이라이트 효과를 주는 함수
    public void Highlight(bool isHighlighted)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isHighlighted ? Color.cyan : Color.white;
        }
    }
}
