using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class SliderTest : MonoBehaviour
{
    [SerializeField] Slider _slider;
    [SerializeField] Text _text;

    private void Start()
    {
        _slider.onValueChanged.AddListener(Change);
        _slider.value = 35f;
    }

    private void OnDestroy()
    {
        _slider.onValueChanged.RemoveListener(Change);
    }

    private void Change(float arg0)
    {
        Target.KOEF_MOTION = arg0;
        _text.text = arg0.ToString("F3"); //CultureInfo.InvariantCulture);
    }
}
