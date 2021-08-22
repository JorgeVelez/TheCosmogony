using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class ColorCorrectionController : Singleton<ColorCorrectionController>
{
    private float elapsedTime = 0;
    private float totalTime = 0;
    private bool MonitorTime = false;

    enum DayState {ComienzoDia, Dia, ComienzoNoche, Noche};

    private DayState estado;
    private int currentVol;
    private int previousVol;
    private int nextVol;

    private float DuracionDia;
    private float DuracionNoche;
    private float DuracionTransicion;

    public InputField iFieldDuracionDia;

    public InputField iFieldDuracionNoche;

    public InputField iFieldDuracionTransicion;

    public GameObject sliderPrefab;
    public Slider globalSlider;
    public GameObject UI;
    public Button PLayBt;
    public Button PauseBt;

    public List<Volume> volumes;
    private List<Slider> sliders = new List<Slider>();
    public List<GameObject> icons;


    void Start()
    {
        DuracionDia = float.Parse(iFieldDuracionDia.text);

        DuracionNoche = float.Parse(iFieldDuracionNoche.text);

        DuracionTransicion = float.Parse(iFieldDuracionTransicion.text);

        for (int i = 0; i < volumes.Count; i++)
        {
            Slider sli = Instantiate(sliderPrefab, sliderPrefab.transform.parent).GetComponent<Slider>();
            sli.gameObject.SetActive(true);
            sli.transform.Find("Label").GetComponent<Text>().text = volumes[i].gameObject.name;
            sliders.Add(sli);
            //Volume tempVol=volumes[i];
            volumes[i].weight = 0;

            sli.onValueChanged.AddListener((val) =>
            {
                sli.transform.Find("Value").GetComponent<Text>().text = val.ToString("F2");
                //tempVol.weight=val;
            });
        }

        totalTime = DuracionDia + DuracionNoche;

        globalSlider.transform.Find("Label").GetComponent<Text>().text = "Global";
        globalSlider.maxValue = totalTime;

        currentVol = UnityEngine.Random.Range(0, volumes.Count / 2);
        GenerateNextWeather();
        previousVol=-1;
        estado=DayState.ComienzoDia;

        //////////////////////////////////////////////////////////////////////
        globalSlider.onValueChanged.AddListener((hora) =>
        {
            TimeSpan time = TimeSpan.FromHours(hora);
            globalSlider.transform.Find("Value").GetComponent<Text>().text =time.ToString(@"hh\:mm");

            if(estado==DayState.ComienzoDia && hora<DuracionTransicion){
                //hora/DuracionTransicion;
            }

            /* float currentValue = ((val) - currentVol);
            sliders[currentVol].value = currentValue;

            for (int j = 0; j < icons.Count; j++)
                icons[j].SetActive(false);
            icons[currentVol].SetActive(true);

            if (currentVol > 0)
            {
                sliders[currentVol - 1].value = 1 - currentValue;
                if (sliders[currentVol - 1].value > sliders[currentVol].value)
                {
                    icons[currentVol].SetActive(false);
                    icons[currentVol - 1].SetActive(true);

                }

            } */


        });
        ///////////////////////////////////////////////////////////

        PLayBt.onClick.AddListener(playAnimation);
        PauseBt.onClick.AddListener(pauseAnimation);

        Hide();
    }

    void GenerateNextWeather()
    {
        nextVol = UnityEngine.Random.Range(0, volumes.Count / 2);
        while (nextVol == currentVol)
        {
            nextVol = UnityEngine.Random.Range(0, volumes.Count / 2);
        }


    }
    void Update()
    {
        if (MonitorTime)
        {
            elapsedTime += Time.deltaTime;

            globalSlider.value = elapsedTime;

            if (elapsedTime > totalTime){
                MonitorTime = false;
                estado=DayState.ComienzoDia;
                elapsedTime=0;
            }
        }
    }

    void playAnimation()
    {
        MonitorTime = true;
        PLayBt.gameObject.SetActive(false);
        PauseBt.gameObject.SetActive(true);

    }
    void pauseAnimation()
    {
        MonitorTime = false;
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
