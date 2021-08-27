using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEngine.VFX;

public class ColorCorrectionController : Singleton<ColorCorrectionController>
{
    private float elapsedTime = 0;
    private float totalTime = 0;
    private bool MonitorTime = false;

    enum DayState { ComienzoDia, Dia, ComienzoNoche, Noche };
    private List<string> seasons = new List<string>() { "Spring", "Summer", "Autum", "Winter" };
    private string currentSeason;
    private string previousSeason;
    private DayState estado;
    private int currentVol;
    private int previousVol;
    private int nextVol;

    private float DuracionDia;
    private float DuracionNoche;
    private float DuracionTransicion;

    private int DuracionSeason;


    public Text StateLabel;
    public Text WeatherLabel;
    public Text SeasonLabel;
    public InputField iFieldDuracionDia;

    public InputField iFieldDuracionNoche;

    public InputField iFieldDuracionTransicion;

    public InputField iFieldDuracionSeason;

    public GameObject sliderPrefab;
    public Slider globalSlider;
    public Slider seasonsSlider;
    public GameObject UI;
    public Button RestartBt;
    public Button DeltetePrefsBt;

    public List<Volume> volumes;
    private List<Slider> sliders = new List<Slider>();
    public List<GameObject> icons;

    public GameObject seasonPrefab;
    public SkyRotator skyRotator;

    public FogControl fogControl;

    private int daysCounter = 0;

    public List<VisualEffect> butterflies;


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

        if (PlayerPrefs.HasKey("DuracionSeason"))
        {
            iFieldDuracionSeason.text = PlayerPrefs.GetString("DuracionSeason");
        }


        iFieldDuracionSeason.onValueChanged.AddListener((val) =>
        {
            try
            {
                PlayerPrefs.SetString("DuracionSeason", iFieldDuracionSeason.text);
            }
            catch (Exception ex)
            {
                Debug.Log("illegit");

            }

        });
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


        globalSlider.transform.Find("Label").GetComponent<Text>().text = "Progreso dia";


        //////////////////////////////////////////////////////////////////////
        //speed sky slider
        skyRotator.completeRotationDurationHours = totalTime;

        //////////////////////////////////////////////////////////////////////
        //global slider
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
                WeatherLabel.text = volumes[(currentVol * 2) + 1].gameObject.name.Replace(" - Day", "").Replace(" - Night", "");

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

                daysCounter++;

                if (daysCounter > DuracionSeason)
                {
                    daysCounter = 0;
                    previousSeason = currentSeason;
                    while (previousSeason == currentSeason)
                    {
                        currentSeason = seasons[UnityEngine.Random.Range(0, seasons.Count)];
                    }

                    Debug.Log("lleva mariposas: " + ((PlayerPrefs.GetInt(currentSeason + "_Mariposas") == 1) ? true : false));
                    if ((PlayerPrefs.GetInt(currentSeason + "_Mariposas") == 1) ? true : false)
                    {
                        StartCoroutine(FadeInMariposas());
                    }
                    else
                        StartCoroutine(FadeOutMariposas());
                }

                SeasonLabel.text = currentSeason.ToString();

                seasonsSlider.value = daysCounter;
                seasonsSlider.transform.Find("Value").GetComponent<Text>().text = daysCounter + "/" + DuracionSeason;

                List<int> listaPosiblesWeathers = new List<int>();
                for (int ix = 0; ix < volumes.Count; ix = ix + 2)
                {
                    string weather = volumes[ix].gameObject.name.Replace(" - Day", "");
                    int cantidad = int.Parse(PlayerPrefs.GetString(currentSeason + "_" + weather));
                    // Debug.Log(weather + ":" + cantidad);

                    for (int i = 0; i < cantidad; i++)
                    {
                        listaPosiblesWeathers.Add(ix / 2);
                        //Debug.Log(weather + " added ");
                    }
                }

                previousVol = currentVol;
                while (currentVol == previousVol)
                {
                    currentVol = listaPosiblesWeathers[UnityEngine.Random.Range(0, listaPosiblesWeathers.Count)];

                }
                elapsedTime = 0;
                WeatherLabel.text = volumes[currentVol * 2].gameObject.name.Replace(" - Day", "").Replace(" - Night", "");

                StartCoroutine(FadeInIcon(icons[currentVol]));
                StartCoroutine(FadeOutIcon(icons[previousVol]));

                if (volumes[currentVol * 2].gameObject.name.ToLower().Contains("foggy"))
                    fogControl.EnterFog(DuracionTransicion);
                else
                {
                    if (fogControl.fogstate == "Foggy")
                        fogControl.ExitFog(DuracionTransicion);
                }




            }

        });
        ///////////////////////////////////////////////////////////  
        //GENERATE SEASON CONFIGS
        ///////////////////////////////////////////////////////////

        for (int i = 0; i < seasons.Count; i++)
        {
            RectTransform season = Instantiate(seasonPrefab, seasonPrefab.transform.parent).GetComponent<RectTransform>();
            season.gameObject.SetActive(true);
            string temporada = seasons[i];
            season.Find("Label").GetComponent<Text>().text = temporada;

            if (PlayerPrefs.HasKey(temporada + "_Mariposas"))
                season.Find("Mariposas").GetComponent<Toggle>().isOn = PlayerPrefs.GetInt(temporada + "_Mariposas") == 1 ? true : false;
            else
                PlayerPrefs.SetInt(temporada + "_Mariposas", season.Find("Mariposas").GetComponent<Toggle>().isOn ? 1 : 0);

            season.Find("Mariposas").GetComponent<Toggle>().onValueChanged.AddListener((value) =>
            {
                PlayerPrefs.SetInt(temporada + "_Mariposas", value ? 1 : 0);
            });

            for (int ix = 0; ix < volumes.Count; ix = ix + 2)
            {
                RectTransform seasonWeather = Instantiate(season.Find("Weathers/Weather").transform, season.Find("Weathers/Weather").transform.parent).GetComponent<RectTransform>();
                seasonWeather.gameObject.SetActive(true);

                string nombre = volumes[ix].gameObject.name.Replace(" - Day", "");


                seasonWeather.Find("Label").GetComponent<Text>().text = nombre;

                if (PlayerPrefs.HasKey(temporada + "_" + nombre))
                {
                    seasonWeather.GetComponent<InputField>().text = PlayerPrefs.GetString(temporada + "_" + nombre);
                    //Debug.Log("si exixtia prefs de este input field " + temporada + "_" + nombre + " y era " + seasonWeather.GetComponent<InputField>().text);
                }
                else
                {
                    PlayerPrefs.SetString(temporada + "_" + nombre, seasonWeather.GetComponent<InputField>().text);
                    //Debug.Log("no exixtia prefs de este input field " + temporada + "_" + nombre + " y se creo con el valor " + seasonWeather.GetComponent<InputField>().text);

                }
                seasonWeather.GetComponent<InputField>().onValueChanged.AddListener((value) =>
                {
                    // Debug.Log("se modifico este input field " + temporada + "_" + nombre + " y se guarda con el valor " + seasonWeather.GetComponent<InputField>().text);

                    PlayerPrefs.SetString(temporada + "_" + nombre, seasonWeather.GetComponent<InputField>().text);

                });

            }
        }



        Hide();

        MonitorTime = true;
    }

    IEnumerator FadeInIcon(GameObject icon)
    {
        //Debug.Log("appear icon " + icon.name);
        float alpha = 0f;
        float duration = 5;

        Color32 white = Color.white;
        white.a = 0;

        Renderer rend = icon.GetComponent<Renderer>();
        rend.material.SetColor("_BaseColor", white);

        while (alpha < 1f)
        {
            alpha += Time.deltaTime / duration;
            alpha = Mathf.Clamp01(alpha);

            white.a = (byte)(alpha * 255f);
            rend.material.SetColor("_BaseColor", white);

            yield return null;
        }
        // Debug.Log("finished fading in icon " + icon.name);

    }

    IEnumerator FadeOutIcon(GameObject icon)
    {
        //Debug.Log("dissappear icon " + icon.name);
        float alpha = 1f;
        float duration = 2;

        Color32 white = Color.white;
        white.a = (byte)255;

        Renderer rend = icon.GetComponent<Renderer>();
        rend.material.SetColor("_BaseColor", white);

        while (alpha > 0)
        {
            alpha -= Time.deltaTime / duration;
            alpha = Mathf.Clamp01(alpha);

            white.a = (byte)(alpha * 255f);
            rend.material.SetColor("_BaseColor", white);

            yield return null;
        }
        // Debug.Log("finished fading out icon " + icon.name);

    }


    IEnumerator FadeInMariposas()
    {
        float alpha = 0f;
        float duration = 5;


        while (alpha < 1f)
        {
            alpha += Time.deltaTime / duration;
            alpha = Mathf.Clamp01(alpha);

            foreach (VisualEffect vfx in butterflies)
            {
                vfx.SetFloat("Alpha", alpha);
            }

            yield return null;
        }

    }

    IEnumerator FadeOutMariposas()
    {
        float alpha = 1f;
        float duration = 5;



        while (alpha > 0)
        {
            alpha -= Time.deltaTime / duration;
            alpha = Mathf.Clamp01(alpha);
            foreach (VisualEffect vfx in butterflies)
            {
                vfx.SetFloat("Alpha", alpha);
            }

            yield return null;
        }

    }

    void ArrancaCalculos()
    {
        DuracionDia = float.Parse(iFieldDuracionDia.text) * 60.0f * 60.0f;

        DuracionNoche = float.Parse(iFieldDuracionNoche.text) * 60.0f * 60.0f;

        DuracionTransicion = float.Parse(iFieldDuracionTransicion.text) * 60.0f * 60.0f;

        DuracionSeason = int.Parse(iFieldDuracionSeason.text);

        totalTime = DuracionDia + DuracionNoche;

        globalSlider.maxValue = totalTime;

        seasonsSlider.maxValue = DuracionSeason;
        seasonsSlider.transform.Find("Value").GetComponent<Text>().text = daysCounter + "/" + DuracionSeason;


        currentSeason = seasons[UnityEngine.Random.Range(0, seasons.Count)];

        Debug.Log("lleva mariposas: " + ((PlayerPrefs.GetInt(currentSeason + "_Mariposas") == 1) ? true : false));
        if ((PlayerPrefs.GetInt(currentSeason + "_Mariposas") == 1) ? true : false)
        {
            StartCoroutine(FadeInMariposas());
        }
        else
            StartCoroutine(FadeOutMariposas());

        List<int> listaPosiblesWeathers = new List<int>();
        for (int ix = 0; ix < volumes.Count; ix = ix + 2)
        {
            string weather = volumes[ix].gameObject.name.Replace(" - Day", "");
            int cantidad = int.Parse(PlayerPrefs.GetString(currentSeason + "_" + weather));
            //Debug.Log(weather + ":" + cantidad);

            for (int i = 0; i < cantidad; i++)
            {
                listaPosiblesWeathers.Add(ix / 2);
                //Debug.Log(weather + " added ");
            }
        }

        currentVol = listaPosiblesWeathers[UnityEngine.Random.Range(0, listaPosiblesWeathers.Count)];

        previousVol = -1;
        elapsedTime = 0;
        estado = DayState.ComienzoDia;

        WeatherLabel.text = volumes[currentVol * 2].gameObject.name.Replace(" - Day", "").Replace(" - Night", "");
        StateLabel.text = estado.ToString();
        SeasonLabel.text = currentSeason.ToString();

        if (volumes[currentVol * 2].gameObject.name.ToLower().Contains("foggy"))
            fogControl.EnterFog(DuracionTransicion);

        StartCoroutine(FadeInIcon(icons[currentVol]));


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

#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
#elif UNITY_STANDALONE_WIN
         System.Diagnostics.Process.Start(Application.dataPath.Replace("_Data", ".exe")); //new program
        Application.Quit(); //kill current process
        Debug.Log("shut down");
#endif

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
