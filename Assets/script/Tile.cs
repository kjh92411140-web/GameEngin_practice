/*
    =================================================================
    Tile.cs (Back to Basics Version)
    - 타일의 좌표와 교체 가능 여부만 저장하는 가장 단순한 버전입니다.
    - 이 스크립트는 모든 타일 프리팹에 자동으로 추가됩니다.
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

    // 선택되었을 때 시각적 피드백을 주는 함수
    public void Highlight(bool isHighlighted)
    {
        if (tileRenderer != null)
        {
            tileRenderer.material.color = isHighlighted ? Color.cyan : originalColor;
        }
    }
}