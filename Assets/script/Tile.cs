using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StructureMapping
{
    public char character;
    public GameObject structurePrefab;

    [Tooltip("�������� ũ�⸦ ���� �����մϴ�. (0, 0, 0)���� �θ� Ÿ�� �� �׸��� ũ�⿡ �ڵ����� �������ϴ�.")]
    public Vector3 customScale = Vector3.zero;
}

public class Tile : MonoBehaviour
{
    [Header("Tile Position")]
    public int gridX;
    public int gridY;
    public bool isSwappable = true;

    [Header("Internal Map Builder")]
    [Tooltip("�� Ÿ�� �ȿ��� ����� ���������� ����ϼ���.")]
    public List<StructureMapping> structureLibrary;

    [Tooltip("������ ���������� �ڽ����� �� �θ� ������Ʈ�Դϴ�.")]
    public Transform structureHolder;

    [Tooltip("�� Ÿ�� ���θ� �������� ���赵�Դϴ�. (GridManager�� ���� ä�����ϴ�)")]
    [TextArea(3, 5)]
    public string[] mapLayout;

    private List<Renderer> structureRenderers = new List<Renderer>();
    private List<Color> originalColors = new List<Color>();

    // ================== [�ڵ� �߰�] ==================
    // �������� �����ϰ� û���ϴ� �Լ��Դϴ�. (Editor-only)
    [ContextMenu("Clear Structures In Editor")]
    public void ClearStructures()
    {
        if (structureHolder == null) return;

        // DestroyImmediate�� �����Ϳ��� ��� ������Ʈ�� �ı��� �� ����մϴ�.
        for (int i = structureHolder.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(structureHolder.GetChild(i).gameObject);
        }
        structureRenderers.Clear();
        originalColors.Clear();
    }
    // ===============================================

    [ContextMenu("Generate Structures In Editor")]
    public void GenerateStructures()
    {
        // ================== [�ڵ� ����] ==================
        // ���� û�� �Լ��� ���� ȣ���մϴ�.
        ClearStructures();
        // ===============================================

        if (structureHolder == null)
        {
            Debug.LogError($"[{gameObject.name}] ### ���� ###: Structure Holder�� �Ҵ���� �ʾҽ��ϴ�! �������� Ȯ�����ּ���.", this.gameObject);
            return;
        }

        if (mapLayout == null || mapLayout.Length == 0) return;

        var mappingDict = new Dictionary<char, StructureMapping>();
        foreach (var mapping in structureLibrary)
        {
            if (!mappingDict.ContainsKey(mapping.character))
            {
                mappingDict.Add(mapping.character, mapping);
            }
        }

        int internalHeight = mapLayout.Length;
        if (internalHeight == 0) return;
        int internalWidth = mapLayout[0].Length;
        if (internalWidth == 0) return;

        Vector3 parentSize = GetComponent<BoxCollider>().size;
        Vector2 internalBlockSize = new Vector2(parentSize.x / internalWidth, parentSize.y / internalHeight);
        Vector3 offset = new Vector3(-parentSize.x / 2f + internalBlockSize.x / 2f, -parentSize.y / 2f + internalBlockSize.y / 2f, 0);

        for (int y = 0; y < internalHeight; y++)
        {
            for (int x = 0; x < internalWidth; x++)
            {
                if (x >= mapLayout[y].Length) continue;
                char c = mapLayout[y][x];

                if (char.IsWhiteSpace(c)) continue;

                if (mappingDict.TryGetValue(c, out StructureMapping currentMapping))
                {
                    if (currentMapping.structurePrefab == null) continue;

                    GameObject prefab = currentMapping.structurePrefab;
                    int spawnY = internalHeight - 1 - y;
                    Vector3 position = new Vector3(x * internalBlockSize.x, spawnY * internalBlockSize.y, 0) + offset;
                    GameObject structure = Instantiate(prefab, structureHolder);
                    structure.transform.localPosition = position;

                    if (currentMapping.customScale == Vector3.zero)
                    {
                        structure.transform.localScale = new Vector3(internalBlockSize.x, internalBlockSize.y, internalBlockSize.y);
                    }
                    else
                    {
                        structure.transform.localScale = currentMapping.customScale;
                    }

                    Renderer rend = structure.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        structureRenderers.Add(rend);
                        originalColors.Add(rend.material.color);
                    }
                }
            }
        }
    }

    public void SetPosition(int x, int y) { gridX = x; gridY = y; }
    public void Highlight(bool isHighlighted)
    {
        for (int i = 0; i < structureRenderers.Count; i++)
        {
            if (structureRenderers[i] != null)
                structureRenderers[i].material.color = isHighlighted ? Color.cyan : originalColors[i];
        }
    }
}