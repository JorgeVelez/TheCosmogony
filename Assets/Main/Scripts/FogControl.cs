using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

//[ExecuteInEditMode]
public class FogControl : MonoBehaviour
{
    public Light luz;
    public DensityVolume dVolume;

    [Range(0, 1.0f)]
    public float intensity = 0;

    float lastIntensity = 0;

    float lightMaxIntensity;
    float fogMaxIntensity;
    public float duration = 55;

    public string fogstate="Clear";
    void Start()
    {
        lightMaxIntensity = luz.intensity;
        fogMaxIntensity = dVolume.parameters.distanceFadeEnd;
    }

    void Update()
    {
        // if (Intensity != lastIntensity)
        // {
        //     if (Intensity == 0)
        //     {
        //         luz.gameObject.SetActive(false);
        //         dVolume.gameObject.SetActive(false);
        //     }
        //     else
        //     {
        //         luz.gameObject.SetActive(true);
        //         dVolume.gameObject.SetActive(true);

        //         luz.intensity = lightMaxIntensity * Intensity;
        //         dVolume.parameters.distanceFadeEnd = fogMaxIntensity * Intensity;
        //     }

        //     lastIntensity = Intensity;
        // }

    }

    public void EnterFog(float _duration)
    {
        fogstate="Foggy";
        StopAllCoroutines();
        StartCoroutine(FadeIn(_duration));
        
    }

    public void ExitFog(float _duration)
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut(_duration));
        fogstate="Clear";
    }

    IEnumerator FadeIn(float _duration)
    {
        intensity = 0f;

        duration = _duration;

        luz.intensity = 0;
        dVolume.parameters.distanceFadeEnd = 0;

        luz.gameObject.SetActive(true);
        dVolume.gameObject.SetActive(true);

        while (intensity < 1f)
        {
            intensity += Time.deltaTime / duration;
            intensity = Mathf.Clamp01(intensity);

            luz.intensity = lightMaxIntensity * intensity;
            dVolume.parameters.distanceFadeEnd = fogMaxIntensity * intensity;

            yield return null;
        }

    }

    IEnumerator FadeOut(float _duration)
    {
        intensity = 1f;

        duration = _duration;

        luz.intensity = lightMaxIntensity;
        dVolume.parameters.distanceFadeEnd = fogMaxIntensity;

        while (intensity > 0)
        {
            intensity -= Time.deltaTime / duration;
            intensity = Mathf.Clamp01(intensity);

            luz.intensity = lightMaxIntensity * intensity;
            dVolume.parameters.distanceFadeEnd = fogMaxIntensity * intensity;

            yield return null;
        }

        luz.gameObject.SetActive(false);
        dVolume.gameObject.SetActive(false);

    }
}
