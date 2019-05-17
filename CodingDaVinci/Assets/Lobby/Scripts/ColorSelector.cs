using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ColorSelector : MonoBehaviour
{
    public Slider sliderRed;
    public Slider sliderGreen;
    public Slider sliderBlue;
    public Image colorPreview;

    private Color _color;
    public Color color
    {
        get
        {
            return _color;
        }
        set
        {
            _color = value;
            _color.a = 1;
            sliderRed.value = _color.r;
            sliderGreen.value = _color.g;
            sliderBlue.value = _color.b;
            updatePreviewImage();
        }
    }

    public ColorEvent OnColorChanged;

    private void updatePreviewImage()
    {
        colorPreview.color = _color;
        OnColorChanged.Invoke(_color);
    }

    public void colorChangedRed()
    {
        _color.r = sliderRed.value;
        updatePreviewImage();
    }

    public void colorChangedGreen()
    {
        _color.g = sliderGreen.value;
        updatePreviewImage();
    }

    public void colorChangedBlue()
    {
        _color.b = sliderBlue.value;
        updatePreviewImage();
    }
}

[Serializable]
public class ColorEvent : UnityEvent<Color>
{

}
