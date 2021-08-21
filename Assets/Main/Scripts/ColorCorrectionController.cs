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


        foreach (Volume item in volumes)
        {
            Slider sli=Instantiate(sliderPrefab, sliderPrefab.transform.parent).GetComponent<Slider>();
            sli.gameObject.SetActive(true);
            sli.transform.Find("Label").GetComponent<Text>().text=item.gameObject.name;

            Volume tempVol=item;
            item.weight=0;

            sli.onValueChanged.AddListener((val)=>{
            sli.transform.Find("Value").GetComponent<Text>().text=val.ToString("F2");
                tempVol.weight=val;
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
