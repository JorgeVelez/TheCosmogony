using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class ColorCorrectionController : Singleton<ColorCorrectionController>
{
    public GameObject sliderPrefab;

    public Slider globalSlider;

    public List<Volume> volumes;

    public List<GameObject> icons;
        public GameObject UI;

    void Start()
    {
        for (int i = 0; i < volumes.Count; i++)
        {
            Slider sli=Instantiate(sliderPrefab, sliderPrefab.transform.parent).GetComponent<Slider>();
            sli.gameObject.SetActive(true);
            sli.transform.Find("Label").GetComponent<Text>().text=volumes[i].gameObject.name;

            Volume tempVol=volumes[i];
            volumes[i].weight=0;

            sli.onValueChanged.AddListener((val)=>{
            sli.transform.Find("Value").GetComponent<Text>().text=val.ToString("F2");
                tempVol.weight=val;
                MeshRenderer renderer = icons[i].GetComponent(typeof(MeshRenderer)) as MeshRenderer;
                renderer.material.SetColor("_BaseColor", new Color(1.0f, 0.0f, 0.0f, val));
            });
        }

                globalSlider.onValueChanged.AddListener((val)=>{
                globalSlider.transform.Find("Label").GetComponent<Text>().text="Global";
                globalSlider.transform.Find("Value").GetComponent<Text>().text=val.ToString("F2");
            });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

      internal void toggleVisibility()
    { 
 
        Debug.Log("<color=green>toggleVisibility " + UI.gameObject.activeSelf + "</color>");
 
        if (!UI.gameObject.activeSelf)
            Show();
        else
            Hide();
    }
  public void Show()
    {
        UI.gameObject.SetActive(true);
 
     }
    public void Hide()
    {
        UI.gameObject.SetActive(false);
 
    }
}
