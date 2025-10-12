/*
    =================================================================
    Tile.cs (3D Version with Base - CORRECTED)
    - 개별 3D 타일의 동작을 제어합니다.
    - 입체감/그림자 효과를 위해 아래에 '베이스' 오브젝트를 생성합니다.
    - 이 스크립트는 맵 조각으로 사용할 3D 프리팹에 추가해야 합니다.
    =================================================================
*/
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileType { Normal, Wall, Empty }

    [Header("Tile Info")]
    public int gridX;
    public int gridY;
    public TileType tileType = TileType.Normal;
    public bool isSwappable = true;

    [Header("Visuals")]
    public Color baseColor = new Color(0.2f, 0.2f, 0.2f, 1f); // 베이스의 어두운 회색
    public float baseDepth = 0.1f; // 베이스가 뒤로 밀리는 깊이

    private MeshRenderer meshRenderer;
    private Color originalColor;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }

        // 'Empty' 타입 타일에는 베이스를 생성하지 않습니다.
        if (tileType != TileType.Empty)
        {
            CreateBase();
        }
    }

    // 3D 효과를 위해 타일 뒤에 어두운 베이스 오브젝트를 생성합니다. (수정된 버전)
    void CreateBase()
    {
        // 베이스를 위한 새로운 큐브를 생성합니다.
        GameObject baseObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseObject.name = "Base";

        // 클릭 감지에 방해가 되지 않도록 베이스의 Collider는 제거합니다.
        Destroy(baseObject.GetComponent<BoxCollider>());

        // 생성된 베이스를 현재 타일의 자식으로 만듭니다.
        baseObject.transform.SetParent(this.transform);

        // 베이스를 메인 타일보다 약간 뒤로 배치하고 크기를 맞춥니다.
        baseObject.transform.localPosition = new Vector3(0, 0, baseDepth);
        baseObject.transform.localRotation = Quaternion.identity;
        baseObject.transform.localScale = Vector3.one; // 부모(타일)의 스케일을 따라갑니다.

        // 베이스의 색상을 변경합니다.
        MeshRenderer baseRenderer = baseObject.GetComponent<MeshRenderer>();
        if (baseRenderer != null)
        {
            baseRenderer.material.color = baseColor;
        }
    }


    public void SetPosition(int x, int y)
    {
        gridX = x;
        gridY = y;
    }

    public void Highlight(bool isHighlighted)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = isHighlighted ? Color.cyan : originalColor;
        }
    }
}