using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TooltipContentProvider : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea]
    public string Text;
    public GridLayoutGroup.Corner Corner;

    private new RectTransform transform;
    private Vector2 pivot;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!string.IsNullOrEmpty(Text))
        {
            TooltipInfoController.Show(transform.position, pivot, Text);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipInfoController.Hide();
    }

    private void Awake()
    {
        transform = base.transform as RectTransform;
        switch (Corner)
        {
            case GridLayoutGroup.Corner.UpperLeft:
                pivot = new Vector2(1, 0);
                break;
            case GridLayoutGroup.Corner.UpperRight:
                pivot = new Vector2(0, 0);
                break;
            case GridLayoutGroup.Corner.LowerLeft:
                pivot = new Vector2(1, 1);
                break;
            case GridLayoutGroup.Corner.LowerRight:
                pivot = new Vector2(0, 1);
                break;
        }
    }
}
