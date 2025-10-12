/*
    =================================================================
    Tile.cs (3D Version with Base - CORRECTED)
    - ���� 3D Ÿ���� ������ �����մϴ�.
    - ��ü��/�׸��� ȿ���� ���� �Ʒ��� '���̽�' ������Ʈ�� �����մϴ�.
    - �� ��ũ��Ʈ�� �� �������� ����� 3D �����տ� �߰��ؾ� �մϴ�.
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
    public Color baseColor = new Color(0.2f, 0.2f, 0.2f, 1f); // ���̽��� ��ο� ȸ��
    public float baseDepth = 0.1f; // ���̽��� �ڷ� �и��� ����

    private MeshRenderer meshRenderer;
    private Color originalColor;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }

        // 'Empty' Ÿ�� Ÿ�Ͽ��� ���̽��� �������� �ʽ��ϴ�.
        if (tileType != TileType.Empty)
        {
            CreateBase();
        }
    }

    // 3D ȿ���� ���� Ÿ�� �ڿ� ��ο� ���̽� ������Ʈ�� �����մϴ�. (������ ����)
    void CreateBase()
    {
        // ���̽��� ���� ���ο� ť�긦 �����մϴ�.
        GameObject baseObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseObject.name = "Base";

        // Ŭ�� ������ ���ذ� ���� �ʵ��� ���̽��� Collider�� �����մϴ�.
        Destroy(baseObject.GetComponent<BoxCollider>());

        // ������ ���̽��� ���� Ÿ���� �ڽ����� ����ϴ�.
        baseObject.transform.SetParent(this.transform);

        // ���̽��� ���� Ÿ�Ϻ��� �ణ �ڷ� ��ġ�ϰ� ũ�⸦ ����ϴ�.
        baseObject.transform.localPosition = new Vector3(0, 0, baseDepth);
        baseObject.transform.localRotation = Quaternion.identity;
        baseObject.transform.localScale = Vector3.one; // �θ�(Ÿ��)�� �������� ���󰩴ϴ�.

        // ���̽��� ������ �����մϴ�.
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