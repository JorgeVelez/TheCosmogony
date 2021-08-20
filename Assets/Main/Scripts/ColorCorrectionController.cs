using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class ColorCorrectionController : Singleton<ColorCorrectionController>
{
    public GameObject sliderPrefab;

    public List<Volume> volumes;


        public GameObject UI;

    void Start()
    {
        foreach (Volume item in volumes)
        {
            Slider sli=Instantiate(sliderPrefab, sliderPrefab.transform.parent).GetComponent<Slider>();
            sli.gameObject.SetActive(true);
            Volume tempVol=item;
            sli.onValueChanged.AddListener((val)=>{
                tempVol.weight=val;
            });
        }
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
