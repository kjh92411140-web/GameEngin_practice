/*
    =================================================================
    Tile.cs (Final, Independent Builder Version)
    - 이제 이 스크립트 자체가 독립적인 미니 맵 빌더 역할을 합니다.
    - 각 타일은 자신만의 Structure Library와 Map Layout을 가집니다.
    =================================================================
*/
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StructureMapping
{
    public char character;
    public GameObject structurePrefab;
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
    [Tooltip("이 타일 내부를 디자인할 설계도입니다.")]
    [TextArea(3, 5)]
    public string[] mapLayout;

    private List<Renderer> structureRenderers = new List<Renderer>();
    private List<Color> originalColors = new List<Color>();

    void Start()
    {
        GenerateStructures();
    }

    [ContextMenu("Generate Structures In Editor")]
    public void GenerateStructures()
    {
        for (int i = transform.childCount - 1; i >= 0; i--) { DestroyImmediate(transform.GetChild(i).gameObject); }
        structureRenderers.Clear();
        originalColors.Clear();

        if (mapLayout == null || mapLayout.Length == 0) return;

        var prefabDict = new Dictionary<char, GameObject>();
        foreach (var mapping in structureLibrary)
        {
            if (!prefabDict.ContainsKey(mapping.character)) { prefabDict.Add(mapping.character, mapping.structurePrefab); }
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
                if (prefabDict.TryGetValue(c, out GameObject prefab))
                {
                    int spawnY = internalHeight - 1 - y;
                    Vector3 position = new Vector3(x * internalBlockSize.x, spawnY * internalBlockSize.y, 0) + offset;
                    GameObject structure = Instantiate(prefab, transform);
                    structure.transform.localPosition = position;
                    structure.transform.localScale = new Vector3(internalBlockSize.x, internalBlockSize.y, internalBlockSize.y);

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

