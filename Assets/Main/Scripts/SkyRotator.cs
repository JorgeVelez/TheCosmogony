using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class SkyRotator : MonoBehaviour
{
    Volume volume;
    HDRISky sky;
    public float rotationSpeed = 0.01f;
    void Awake()
    {
        volume = gameObject.GetComponent<Volume>();

        volume.profile.TryGet(out sky);
    }

    void Update()
    {
        sky.rotation.value = Mathf.Lerp(sky.rotation.value, sky.rotation.value + 10, rotationSpeed * Time.deltaTime);
        if (sky.rotation.value == sky.rotation.max)
        {
            sky.rotation.value = 0;
        }
    }
}




