/*
    =================================================================
    Tile.cs (Back to Basics Version)
    - Ÿ���� ��ǥ�� ��ü ���� ���θ� �����ϴ� ���� �ܼ��� �����Դϴ�.
    - �� ��ũ��Ʈ�� ��� Ÿ�� �����տ� �ڵ����� �߰��˴ϴ�.
    =================================================================
*/
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int gridX;
    public int gridY;
    public bool isSwappable;

    private Color originalColor;
    private Renderer tileRenderer;

    void Start()
    {
        tileRenderer = GetComponent<Renderer>();
        if (tileRenderer != null)
        {
            originalColor = tileRenderer.material.color;
        }
    }

    public void SetPosition(int x, int y)
    {
        gridX = x;
        gridY = y;
    }

    // ���õǾ��� �� �ð��� �ǵ���� �ִ� �Լ�
    public void Highlight(bool isHighlighted)
    {
        if (tileRenderer != null)
        {
            tileRenderer.material.color = isHighlighted ? Color.cyan : originalColor;
        }
    }
}