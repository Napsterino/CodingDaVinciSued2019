using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipInfoController : MonoBehaviour
{
    public static TooltipInfoController Instance;

    public static void Show(Vector3 position, Vector2 pivot, string text)
    {
        if (Instance)
        {
            Instance.show(position, pivot, text);
        }
        else
        {
            throw new Exception("No instance!");
        }
    }

    internal static void Hide()
    {
        Instance.hide();
    }

    private void show(Vector3 position, Vector2 pivot, string text)
    {
        transform.position = position;
        transform.pivot = pivot;
        Tooltip_Text = text;
        gameObject.SetActive(true);
    }

    private void hide()
    {
        gameObject.SetActive(false);
    }

    private string tooltiptext;
    public string Tooltip_Text
    {
        get => tooltiptext;
        set
        {
            tooltiptext = value;
            Tooltip.text = value;
        }
    }

    public Text Tooltip;

    private new RectTransform transform;

    private void Awake()
    {
        Instance = this;
        transform = base.transform as RectTransform;
        if (!transform)
        {
            Debug.LogError("Tooltip Info Controller has to be attached to a GUI GameObject!");
        }
    }
}
