using System.Collections;
using UnityEngine;

public class UIGrid : UnityEngine.UI.GridLayoutGroup
{
    public int Columns = 2;

    protected override void Start()
    {
        base.Start();
        UpdateSize();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        UpdateSize();
    }

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        UpdateSize();
    }

    private void UpdateSize()
    {
        Vector2 cell = cellSize;
        cell.x = (Mathf.RoundToInt(rectTransform.rect.width / Columns) - padding.right / 2f - padding.left / 2f - spacing.x * (Columns - 1) / 2f) - 1;
        cellSize = cell;
    }
}