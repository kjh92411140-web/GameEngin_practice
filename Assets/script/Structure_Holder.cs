// /Assets/script/Tile_Container.cs

using UnityEngine;

/// <summary>
/// �׸����� �� ĭ(Cell)�� ��Ÿ����, Ÿ�ϰ� �� ���� �������� �����ϴ� �����̳� Ŭ�����Դϴ�.
/// </summary>
public class Tile_Container : MonoBehaviour
{
    // === ���� ���� (�ܺο��� ����) ===
    public MovableTile MyTile { get; private set; }
    public Structure_Holder MyStructure { get; private set; }

    // === �ʱ�ȭ ===
    /// <summary>
    /// �����̳ʸ� �ʱ�ȭ�ϰ�, �ڽ����� �ִ� Ÿ�ϰ� �������� ã�� �����մϴ�.
    /// </summary>
    public void Initialize()
    {
        MyTile = GetComponentInChildren<MovableTile>();
        MyStructure = GetComponentInChildren<Structure_Holder>();

        if (MyTile == null)
        {
            Debug.LogError($"{name}���� MovableTile�� ã�� �� �����ϴ�!");
        }
    }

    /// <summary>
    /// �� �����̳ʿ� ���ο� �������� ��ġ�մϴ�.
    /// </summary>
    /// <param name="structurePrefab">������ ������ ������</param>
    public void SetStructure(Structure_Holder structurePrefab)
    {
        if (MyStructure != null)
        {
            Debug.LogWarning($"{name}�� �̹� �������� �ֽ��ϴ�. ���� �������� �����ϰ� ���� ��ġ�մϴ�.");
            Destroy(MyStructure.gameObject);
        }

        Structure_Holder newStructure = Instantiate(structurePrefab, transform);
        MyStructure = newStructure;
        Debug.Log($"{name}�� {newStructure.name} ������ ��ġ �Ϸ�.");
    }
}