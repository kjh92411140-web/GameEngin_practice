// /Assets/script/Tile_Container.cs

using UnityEngine;

/// <summary>
/// 그리드의 한 칸(Cell)을 나타내며, 타일과 그 위의 구조물을 관리하는 컨테이너 클래스입니다.
/// </summary>
public class Tile_Container : MonoBehaviour
{
    // === 공개 변수 (외부에서 접근) ===
    public MovableTile MyTile { get; private set; }
    public Structure_Holder MyStructure { get; private set; }

    // === 초기화 ===
    /// <summary>
    /// 컨테이너를 초기화하고, 자식으로 있는 타일과 구조물을 찾아 연결합니다.
    /// </summary>
    public void Initialize()
    {
        MyTile = GetComponentInChildren<MovableTile>();
        MyStructure = GetComponentInChildren<Structure_Holder>();

        if (MyTile == null)
        {
            Debug.LogError($"{name}에서 MovableTile을 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 이 컨테이너에 새로운 구조물을 배치합니다.
    /// </summary>
    /// <param name="structurePrefab">생성할 구조물 프리팹</param>
    public void SetStructure(Structure_Holder structurePrefab)
    {
        if (MyStructure != null)
        {
            Debug.LogWarning($"{name}에 이미 구조물이 있습니다. 기존 구조물을 삭제하고 새로 배치합니다.");
            Destroy(MyStructure.gameObject);
        }

        Structure_Holder newStructure = Instantiate(structurePrefab, transform);
        MyStructure = newStructure;
        Debug.Log($"{name}에 {newStructure.name} 구조물 배치 완료.");
    }
}