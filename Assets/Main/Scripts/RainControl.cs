using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using RaindropFX;

public class RainControl : MonoBehaviour
{
    Volume volume;
    RaindropFX_GPU rain;
    public float currentRotation=0;
    void Awake()
    {
        volume = gameObject.GetComponent<Volume>();

        volume.profile.TryGet(out rain);
    }

    public void Show()
    {
        rain.fadeout_fadein_switch.value=false;
    }

    public void Hide()
    {
        rain.fadeout_fadein_switch.value=true;
    }
}




