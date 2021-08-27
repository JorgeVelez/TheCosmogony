using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class FogControl : MonoBehaviour
{
    public Light luz;
    public DensityVolume dVolume;

    public float Intensity = 0;

    float lastIntensity = 0;

    float lightMaxIntensity;
    float fogMaxIntensity;
    void Start()
    {
        lightMaxIntensity=luz.intensity;
        fogMaxIntensity=dVolume.parameters.distanceFadeEnd;
    }

    void Update()
    {
        if (Intensity != lastIntensity)
        {
            if (Intensity==0){
                luz.gameObject.SetActive(false);
                dVolume.gameObject.SetActive(false);
            }

            lastIntensity = Intensity;
        }

    }

    IEnumerator FadeIn()
    {
        //Debug.Log("appear icon " + icon.name);
        float _intensity = 0f;
        float duration = 5;


        while (_intensity < 1f)
        {
            _intensity += Time.deltaTime / duration;
            _intensity = Mathf.Clamp01(_intensity);



            yield return null;
        }

    }
}
