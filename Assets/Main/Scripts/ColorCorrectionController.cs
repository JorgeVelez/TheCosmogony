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

    private List<Slider> sliders=new List<Slider>();
    
    public List<GameObject> icons;
    public GameObject UI;

    public Button PLayBt;
     public InputField iFieldTimePerForecast;

    private bool MonitorTime=false;

    private float elapsedTime=0;

    private float totalTime=0;
    private float timeperForecast=0;

    void Start()
    {
        timeperForecast=float.Parse(iFieldTimePerForecast.text);

        for (int i = 0; i < volumes.Count; i++)
        {
            Slider sli=Instantiate(sliderPrefab, sliderPrefab.transform.parent).GetComponent<Slider>();
            sli.gameObject.SetActive(true);
            sli.transform.Find("Label").GetComponent<Text>().text=volumes[i].gameObject.name;
sliders.Add(sli);
            Volume tempVol=volumes[i];
            GameObject tempIcon=icons[i];
            volumes[i].weight=0;

            sli.onValueChanged.AddListener((val)=>{
            sli.transform.Find("Value").GetComponent<Text>().text=val.ToString("F2");
                tempVol.weight=val;
                for (int j = 0; j < icons.Count; j++)
                    icons[j].SetActive(false);

                if(val>.5f)
                tempIcon.SetActive(true);

                //MeshRenderer renderer = icons[i].GetComponent(typeof(MeshRenderer)) as MeshRenderer;
                //renderer.material.SetColor("_BaseColor", new Color(1.0f, 0.0f, 0.0f, val));
            });
        }

        totalTime=timeperForecast*volumes.Count;

        globalSlider.transform.Find("Label").GetComponent<Text>().text="Global";
        globalSlider.maxValue=totalTime;

        globalSlider.onValueChanged.AddListener((val)=>{
        globalSlider.transform.Find("Value").GetComponent<Text>().text=val.ToString("F2");

        int currentVol=Mathf.FloorToInt(val/volumes.Count);
        //volumes[currentVol].weight=val-currentVol;
        sliders[currentVol].value=((val/volumes.Count)-currentVol);

        if(currentVol>0)
        sliders[currentVol-1].value=1-((val/volumes.Count)-currentVol);

        });

        PLayBt.onClick.AddListener(playAnimation);

        Hide();
    }

    // Update is called once per frame
    void Update()
    {
        if(MonitorTime)
        {

            elapsedTime += Time.deltaTime;

            globalSlider.value=elapsedTime;

            if(elapsedTime>totalTime)
            MonitorTime=false;
        }
    }

    void playAnimation()    
    {
        MonitorTime=true;
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
