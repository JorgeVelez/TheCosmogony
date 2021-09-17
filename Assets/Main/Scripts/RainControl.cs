using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using RaindropFX;

public class RainControl : MonoBehaviour
{
    Volume volume;
    RaindropFX_GPU rain;
    public bool isShowring=false;
    void Awake()
    {
        volume = gameObject.GetComponent<Volume>();

        volume.profile.TryGet(out rain);
    }

    public void Show()
    {
        rain.fadeout_fadein_switch.value=false;
        isShowring=true;
    }

    public void Hide()
    {
        rain.fadeout_fadein_switch.value=true;
        isShowring=false;
    }
}




