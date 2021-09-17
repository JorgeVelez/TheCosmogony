using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class SkyRotator : MonoBehaviour
{
    Volume volume;
    HDRISky sky;
    public float elapsedTime=0;

    public float completeRotationDurationHours=24f;

    public float currentRotation=0;
    void Awake()
    {
        volume = gameObject.GetComponent<Volume>();

        volume.profile.TryGet(out sky);
    }

    void Update()
    {
        elapsedTime+= (Time.deltaTime);
        currentRotation=elapsedTime/(completeRotationDurationHours*60f*60.0f/360.0f);
        sky.rotation.value =currentRotation;
        if (sky.rotation.value == sky.rotation.max)
        {
            sky.rotation.value = 0;
        }
    }
}




