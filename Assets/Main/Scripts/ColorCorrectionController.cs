using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class ColorCorrectionController : Singleton<ColorCorrectionController>
{
    private float elapsedTime = 0;
    private float totalTime = 0;
    private bool MonitorTime = false;

    enum DayState { ComienzoDia, Dia, ComienzoNoche, Noche };

    private DayState estado;
    private int currentVol;
    private int previousVol;
    private int nextVol;

    private float DuracionDia;
    private float DuracionNoche;
    private float DuracionTransicion;

    public Text StateLabel;
    public Text WeatherLabel;
    public InputField iFieldDuracionDia;

    public InputField iFieldDuracionNoche;

    public InputField iFieldDuracionTransicion;

    public GameObject sliderPrefab;
    public Slider globalSlider;
    public GameObject UI;
    public Button RestartBt;
    public Button DeltetePrefsBt;

    public List<Volume> volumes;
    private List<Slider> sliders = new List<Slider>();
    public List<GameObject> icons;


    void Start()
    {

         RestartBt.onClick.AddListener(RestartAnimation);
        DeltetePrefsBt.onClick.AddListener(restorePlayerPrefDefaults);

        if (PlayerPrefs.HasKey("DuracionDia"))
        {
            iFieldDuracionDia.text = PlayerPrefs.GetString("DuracionDia");
        }
        if (PlayerPrefs.HasKey("DuracionNoche"))
        {
            iFieldDuracionNoche.text = PlayerPrefs.GetString("DuracionNoche");
        }
        if (PlayerPrefs.HasKey("DuracionTransicion"))
        {
            iFieldDuracionTransicion.text = PlayerPrefs.GetString("DuracionTransicion");
        }


        iFieldDuracionDia.onValueChanged.AddListener((val) =>
        {
            try
            {
                iFieldDuracionDia.transform.Find("Value").GetComponent<Text>().text = TimeSpan.FromSeconds(float.Parse(iFieldDuracionDia.text) * 60.0f * 60.0f).ToString(@"hh\:mm\:ss");
                PlayerPrefs.SetString("DuracionDia", iFieldDuracionDia.text);
                Debug.Log("done");
            }
            catch (Exception ex)
            {
                Debug.Log("illegit");

            }

        });

        iFieldDuracionNoche.onValueChanged.AddListener((val) =>
        {

            try
            {
                iFieldDuracionNoche.transform.Find("Value").GetComponent<Text>().text = TimeSpan.FromSeconds(float.Parse(iFieldDuracionNoche.text) * 60.0f * 60.0f).ToString(@"hh\:mm\:ss");
                PlayerPrefs.SetString("DuracionNoche", iFieldDuracionNoche.text);
                Debug.Log("done");
            }
            catch (Exception ex)
            {
                Debug.Log("illegit");

            }
        });

        iFieldDuracionTransicion.onValueChanged.AddListener((val) =>
        {
            try
            {
                iFieldDuracionTransicion.transform.Find("Value").GetComponent<Text>().text = TimeSpan.FromSeconds(float.Parse(iFieldDuracionTransicion.text) * 60.0f * 60.0f).ToString(@"hh\:mm\:ss");
                PlayerPrefs.SetString("DuracionTransicion", iFieldDuracionTransicion.text);
                Debug.Log("done");
            }
            catch (Exception ex)
            {
                Debug.Log("illegit");

            }


        });


        //////////////////////////////////////////////////////////////////////////////////////////////////
        ArrancaCalculos();

        iFieldDuracionDia.transform.Find("Value").GetComponent<Text>().text = TimeSpan.FromSeconds(float.Parse(iFieldDuracionDia.text) * 60.0f * 60.0f).ToString(@"hh\:mm\:ss");
        iFieldDuracionNoche.transform.Find("Value").GetComponent<Text>().text = TimeSpan.FromSeconds(float.Parse(iFieldDuracionNoche.text) * 60.0f * 60.0f).ToString(@"hh\:mm\:ss");
        iFieldDuracionTransicion.transform.Find("Value").GetComponent<Text>().text = TimeSpan.FromSeconds(float.Parse(iFieldDuracionTransicion.text) * 60.0f * 60.0f).ToString(@"hh\:mm\:ss");


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
                sli.transform.Find("Value").GetComponent<Text>().text = val.ToString("F4");
                //tempVol.weight=val;
            });
        }


        globalSlider.transform.Find("Label").GetComponent<Text>().text = "Global";


        //////////////////////////////////////////////////////////////////////
        globalSlider.onValueChanged.AddListener((hora) =>
        {
            globalSlider.transform.Find("Value").GetComponent<Text>().text = TimeSpan.FromSeconds(hora).ToString(@"hh\:mm\:ss") + "/" + TimeSpan.FromSeconds(totalTime).ToString(@"hh\:mm\:ss");

            if (estado == DayState.ComienzoDia && hora < DuracionTransicion)
            {
                float valVol = hora / DuracionTransicion;
                volumes[currentVol * 2].weight = valVol;
                sliders[currentVol * 2].value = valVol;



                if (previousVol >= 0)
                {
                    volumes[(previousVol * 2) + 1].weight = 1 - valVol;
                    sliders[(previousVol * 2) + 1].value = 1 - valVol;
                }

            }
            else if (estado == DayState.ComienzoDia && hora >= DuracionTransicion)
            {
                estado = DayState.Dia;
                StateLabel.text = estado.ToString();
            }
            else if (estado == DayState.Dia && hora < DuracionDia)
            {
                volumes[currentVol * 2].weight = 1;
                sliders[currentVol * 2].value = 1;

                if (previousVol >= 0)
                {
                    volumes[(previousVol * 2) + 1].weight = 0;
                    sliders[(previousVol * 2) + 1].value = 0;
                }
            }
            else if (estado == DayState.Dia && hora >= DuracionDia)
            {
                estado = DayState.ComienzoNoche;
                StateLabel.text = estado.ToString();
                WeatherLabel.text = volumes[(currentVol * 2) + 1].gameObject.name;

            }
            else if (estado == DayState.ComienzoNoche && hora < (DuracionDia + DuracionTransicion))
            {
                float valVol = (hora - DuracionDia) / DuracionTransicion;

                volumes[(currentVol * 2) + 1].weight = valVol;
                sliders[(currentVol * 2) + 1].value = valVol;

                volumes[currentVol * 2].weight = 1 - valVol;
                sliders[currentVol * 2].value = 1 - valVol;
            }
            else if (estado == DayState.ComienzoNoche && hora >= (DuracionDia + DuracionTransicion))
            {
                estado = DayState.Noche;
                StateLabel.text = estado.ToString();
            }
            else if (estado == DayState.Noche && hora < DuracionNoche + DuracionDia)
            {
                volumes[(currentVol * 2) + 1].weight = 1;
                sliders[(currentVol * 2) + 1].value = 1;

                volumes[currentVol * 2].weight = 0;
                sliders[currentVol * 2].value = 0;
            }
            else if (estado == DayState.Noche && hora >= DuracionNoche + DuracionDia)
            {
                estado = DayState.ComienzoDia;
                StateLabel.text = estado.ToString();

                previousVol = currentVol;
                while (currentVol == previousVol)
                {
                    currentVol = UnityEngine.Random.Range(0, volumes.Count / 2);
                }
                elapsedTime = 0;
                WeatherLabel.text = volumes[currentVol * 2].gameObject.name;

                StartCoroutine(ChangeIcon());
            }

        });
        ///////////////////////////////////////////////////////////



       

        Hide();

        MonitorTime = true;
    }

    IEnumerator ChangeIcon()
    {
        float alpha = 0f;
        float duration = 5;

        Color32 white = Color.white;
        white.a = 0;

        Renderer rend = icons[currentVol].GetComponent<Renderer>();
        rend.material.SetColor("_BaseColor", white);

        Renderer prevRend = null;


        if (previousVol >= 0)
            prevRend = icons[previousVol].GetComponent<Renderer>();

        while (alpha < 1f)
        {
            alpha += Time.deltaTime / duration;
            white.a = (byte)(alpha * 255f);
            rend.material.SetColor("_BaseColor", white);
            if (previousVol >= 0)
            {
                white.a = (byte)(255 - (alpha * 255f));
                prevRend.material.SetColor("_BaseColor", white);
            }

            yield return null;
        }
    }

    void ArrancaCalculos()
    {
        DuracionDia = float.Parse(iFieldDuracionDia.text) * 60.0f * 60.0f;

        DuracionNoche = float.Parse(iFieldDuracionNoche.text) * 60.0f * 60.0f;

        DuracionTransicion = float.Parse(iFieldDuracionTransicion.text) * 60.0f * 60.0f;

        totalTime = DuracionDia + DuracionNoche;

        globalSlider.maxValue = totalTime;


        currentVol = UnityEngine.Random.Range(0, volumes.Count / 2);

        previousVol = -1;
        elapsedTime = 0;
        estado = DayState.ComienzoDia;

        WeatherLabel.text = volumes[currentVol * 2].gameObject.name;
        StateLabel.text = estado.ToString();


        StartCoroutine(ChangeIcon());
    }

    void Update()
    {


        if (MonitorTime)
        {
            elapsedTime += Time.deltaTime;

            globalSlider.value = elapsedTime;


        }
    }

    void RestartAnimation()
    {
        ArrancaCalculos();

        for (int i = 0; i < volumes.Count; i++)
        {
            volumes[i].weight = 0;
            sliders[i].value = 0;
        }
    }
    void restorePlayerPrefDefaults()
    {
        PlayerPrefs.DeleteAll();
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
