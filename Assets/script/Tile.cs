using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StructureMapping
{
    public char character;
    public GameObject structurePrefab;

    [Tooltip("구조물의 크기를 직접 지정합니다. (0, 0, 0)으로 두면 타일 내 그리드 크기에 자동으로 맞춰집니다.")]
    public Vector3 customScale = Vector3.zero;
}

public class Tile : MonoBehaviour
{
    [Header("Tile Position")]
    public int gridX;
    public int gridY;
    public bool isSwappable = true;

    [Header("Internal Map Builder")]
    [Tooltip("이 타일 안에서 사용할 구조물들을 등록하세요.")]
    public List<StructureMapping> structureLibrary;

    [Tooltip("생성된 구조물들이 자식으로 들어갈 부모 오브젝트입니다.")]
    public Transform structureHolder;

    [Tooltip("이 타일 내부를 디자인할 설계도입니다. (GridManager에 의해 채워집니다)")]
    [TextArea(3, 5)]
    public string[] mapLayout;

    private List<Renderer> structureRenderers = new List<Renderer>();
    private List<Color> originalColors = new List<Color>();

    // ================== [코드 추가] ==================
    // 구조물만 깨끗하게 청소하는 함수입니다. (Editor-only)
    [ContextMenu("Clear Structures In Editor")]
    public void ClearStructures()
    {
        if (structureHolder == null) return;

        // DestroyImmediate는 에디터에서 즉시 오브젝트를 파괴할 때 사용합니다.
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
        // ================== [코드 수정] ==================
        // 이제 청소 함수를 먼저 호출합니다.
        ClearStructures();
        // ===============================================

        if (structureHolder == null)
        {
            Debug.LogError($"[{gameObject.name}] ### 실패 ###: Structure Holder가 할당되지 않았습니다! 프리팹을 확인해주세요.", this.gameObject);
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