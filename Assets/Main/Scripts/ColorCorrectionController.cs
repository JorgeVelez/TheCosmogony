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
    public Button PauseBt;
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
            volumes[i].weight=0;

            sli.onValueChanged.AddListener((val)=>{
            sli.transform.Find("Value").GetComponent<Text>().text=val.ToString("F2");
                tempVol.weight=val;
            });
        }

        totalTime=timeperForecast*volumes.Count;

        globalSlider.transform.Find("Label").GetComponent<Text>().text="Global";
        globalSlider.maxValue=totalTime;

        globalSlider.onValueChanged.AddListener((val)=>{
        globalSlider.transform.Find("Value").GetComponent<Text>().text=val.ToString("F2");

        int currentVol=Mathf.FloorToInt(val/timeperForecast);
        //volumes[currentVol].weight=val-currentVol;

        float currentValue=((val/timeperForecast)-currentVol);
        sliders[currentVol].value=currentValue;
        icons[currentVol].SetActive(true);

        if(currentVol>0){
        sliders[currentVol-1].value=1-currentValue;
        if(sliders[currentVol-1].value>sliders[currentVol].value){
            icons[currentVol].SetActive(false);
            icons[currentVol-1].SetActive(true);

        }

        }
        for (int j = 0; j < icons.Count; j++)
                    icons[j].SetActive(false);

        });

        PLayBt.onClick.AddListener(playAnimation);
        PauseBt.onClick.AddListener(pauseAnimation);

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
        PLayBt.gameObject.SetActive(false);
        PauseBt.gameObject.SetActive(true);

    }
     void pauseAnimation()    
    {
        MonitorTime=false;
        PauseBt.gameObject.SetActive(false);
        PLayBt.gameObject.SetActive(true);


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
