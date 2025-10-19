// /Assets/script/GridManager.cs

using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("�׸��� ����")]
    public int width = 10;
    public int height = 10;
    public float spacing = 1.1f;

    [Header("������ ����")]
    // ���� GridManager�� MovableTile�� �ƴ� Tile_Container �������� ����մϴ�.
    public Tile_Container tileContainerPrefab;

    void Start()
    {
        GenerateGrid();
    }

    /// <summary>
    /// �׸��带 �����ϰ� �� ��ġ�� Tile_Container�� ��ġ�մϴ�.
    /// </summary>
    void GenerateGrid()
    {
        if (tileContainerPrefab == null)
        {
            Debug.LogError("tileContainerPrefab�� �Ҵ���� �ʾҽ��ϴ�! �ν����Ϳ��� �������� �������ּ���.");
            return;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // ���� ��ġ ���
                Vector3 spawnPosition = new Vector3(x * spacing, 0, y * spacing);

                // Tile_Container �������� �ν��Ͻ�ȭ�մϴ�.
                Tile_Container newContainer = Instantiate(tileContainerPrefab, spawnPosition, Quaternion.identity);

                // ������ �����̳ʸ� GridManager�� �ڽ����� �����Ͽ� ���� ������ �����մϴ�.
                newContainer.transform.SetParent(transform);
                newContainer.name = $"TileContainer_{x}_{y}";

                // �����̳� �ʱ�ȭ �Լ��� ȣ���Ͽ� �ڽ� ������Ʈ(Ÿ��, ������)�� ������ ã�� �մϴ�.
                newContainer.Initialize();
            }
        }
    }
}