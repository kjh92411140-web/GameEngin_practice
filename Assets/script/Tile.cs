
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

    public void Highlight(bool isHighlighted)
    {
        if (tileRenderer != null)
        {
            tileRenderer.material.color = isHighlighted ? Color.cyan : originalColor;
        }
    }
}