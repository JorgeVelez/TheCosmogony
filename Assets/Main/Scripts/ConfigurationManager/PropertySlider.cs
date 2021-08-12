﻿using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
 
public class PropertySlider : MonoBehaviour
{
    Slider slider;
    Text valueText;
    public string label;
    public float value;
 
    Action<float> callback;
    internal int minValue;
    internal int maxValue;
 
    void Start()
    {
    }
    public void Init()
    {
        gameObject.SetActive(true);
        slider = gameObject.GetComponentInChildren<Slider>();
        valueText = transform.Find("Value").GetComponentInChildren<Text>();
 
        transform.Find("Label").GetComponentInChildren<Text>().text = label;
 
        slider.onValueChanged.AddListener(onSliderValueChangedHandler);
 
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.value = value;
        gameObject.name = label;
 
        valueText.text = slider.value.ToString("F2");
 
 
    }
 
    public void OnValueChange(Action<float> ActionToExecute)
    {
        callback = ActionToExecute;
    }
 
    private void onSliderValueChangedHandler(float val) 
    {
        valueText.text = val.ToString("F2");
        callback(val);
    }
}
