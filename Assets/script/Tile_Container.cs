// /Assets/script/GridManager.cs

using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("그리드 설정")]
    public int width = 10;
    public int height = 10;
    public float spacing = 1.1f;

    [Header("프리팹 설정")]
    // 이제 GridManager는 MovableTile이 아닌 Tile_Container 프리팹을 사용합니다.
    public Tile_Container tileContainerPrefab;

    void Start()
    {
        GenerateGrid();
    }

    /// <summary>
    /// 그리드를 생성하고 각 위치에 Tile_Container를 배치합니다.
    /// </summary>
    void GenerateGrid()
    {
        if (tileContainerPrefab == null)
        {
            Debug.LogError("tileContainerPrefab이 할당되지 않았습니다! 인스펙터에서 프리팹을 연결해주세요.");
            return;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 생성 위치 계산
                Vector3 spawnPosition = new Vector3(x * spacing, 0, y * spacing);

                // Tile_Container 프리팹을 인스턴스화합니다.
                Tile_Container newContainer = Instantiate(tileContainerPrefab, spawnPosition, Quaternion.identity);

                // 생성된 컨테이너를 GridManager의 자식으로 설정하여 계층 구조를 정리합니다.
                newContainer.transform.SetParent(transform);
                newContainer.name = $"TileContainer_{x}_{y}";

                // 컨테이너 초기화 함수를 호출하여 자식 오브젝트(타일, 구조물)를 스스로 찾게 합니다.
                newContainer.Initialize();
            }
        }
    }
}