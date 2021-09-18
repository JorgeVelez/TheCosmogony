using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEngine.VFX;
using UnityEngine.SceneManagement;

public class ColorCorrectionController : MonoBehaviour
{
    public bool DoDebugWeatherOcurrences = false;
    private float elapsedTime = 0;
    private float totalTime = 0;
    private bool MonitorTime = false;

    enum DayState { ComienzoDia, Dia, ComienzoNoche, Noche };
    private List<string> seasons = new List<string>() { "Spring", "Summer", "Autum", "Winter" };
    private string currentSeason;
    private string previousSeason;
    private DayState estado;
    private int currentVol;
    private int alternateVol;
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
    public RainControl rainControl;

    private int daysCounter = 0;

    public List<VisualEffect> butterflies;

    private Dictionary<string, int> weathersOcurridos = new Dictionary<string, int>();

    public bool modifyRain = true;

    bool restartedDay = false;
    bool restartedNight = false;

    void Awake()
    {

        Debug.Log("color correction controller started");

        RestartBt.onClick.AddListener(RestartAnimation);
        DeltetePrefsBt.onClick.AddListener(restorePlayerPrefDefaults);

        if (!PlayerPrefs.HasKey("RestartFlag"))
            PlayerPrefs.SetInt("RestartFlag", 0);


        if (PlayerPrefs.HasKey("DuracionDia"))
        {
            Debug.Log("duracion dia si exte en playerprefs, asignando");
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
        globalSlider.onValueChanged.AddListener(ProcessFrameChange);

        Hide();

        MonitorTime = true;
    }

    public void StopProcessing()
    {
        MonitorTime = false;

    }

    public void resetAll()
    {
        resetVolumes();
        Stopbutterflies();
        fogControl.ExitFog(0);
        rainControl.Hide();
        resetIcons();
    }


    public void restartScene()
    {
        Debug.Log("restarted scene");
        PlayerPrefs.SetInt("RestartFlag", 1);

        resetAll();

        PlayerPrefs.SetFloat("currentRotation", skyRotator.elapsedTime);

        PlayerPrefs.SetString("currentSeason", currentSeason);
        PlayerPrefs.SetInt("currentVol", currentVol);
        PlayerPrefs.SetInt("alternateVol", alternateVol);
        PlayerPrefs.SetInt("previousVol", previousVol);
        PlayerPrefs.SetInt("estado", (int)estado);
        PlayerPrefs.SetString("weathersOcurridos", JsonUtility.ToJson(weathersOcurridos));
        PlayerPrefs.SetFloat("elapsedTime", elapsedTime);

        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public void resetVolumes()
    {

        for (int i = 0; i < volumes.Count; i++)
        {
            volumes[i].weight = 0;
        }
    }

    public void resetIcons()
    {
        StopAllCoroutines();
        for (int i = 0; i < icons.Count; i++)
        {
            Image rend = icons[i].GetComponent<Image>();
            Color32 color32 = rend.color;
            color32.a = 0;
            rend.color = color32;
        }
    }

    public string triggerVolume(int vol)
    {
        volumes[vol].weight = 1;

        string weather = volumes[vol].gameObject.name;

        Debug.Log("Recording " + weather);

        resetIcons();
        StartCoroutine(FadeInIcon(icons[vol / 2], .01f));

        if (weather.Contains("Night"))
        {
            if (weather.ToLower().Contains("sunny"))
            {
                resetIcons();
                StartCoroutine(FadeInIcon(icons[8], .01f));
            }
            else if (weather.ToLower().Contains("cloudy"))
            {
                resetIcons();
                StartCoroutine(FadeInIcon(icons[9], .01f));
            }
            else if (weather.ToLower().Contains("extreme"))
            {
                resetIcons();
                StartCoroutine(FadeInIcon(icons[10], .01f));
            }
        }



        if (volumes[vol].gameObject.name.ToLower().Contains("foggy"))
            fogControl.EnterFog(DuracionTransicion);
        else
        {
            if (fogControl.fogstate == "Foggy")
                fogControl.ExitFog(DuracionTransicion);
        }

        if (volumes[vol].gameObject.name.ToLower().Contains("rainy"))
        {
            if (modifyRain)
                rainControl.Show();
            Debug.Log("show rain");
        }
        else
        {
            if (modifyRain)
                rainControl.Hide();
            Debug.Log("hide rain");

        }

        return weather;
    }

    public void Stopbutterflies()
    {
        StartCoroutine(FadeOutMariposas());

    }

    void ProcessFrameChange(float hora)
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

            volumes[currentVol * 2].weight = 1;
            sliders[currentVol * 2].value = 1;

            resetIcons();
            StartCoroutine(FadeInIcon(icons[currentVol]));
            // if (alternateVol == -1)
            // StartCoroutine(FadeOutIcon(icons[previousVol]));
            // else
            //StartCoroutine(FadeOutIcon(icons[alternateVol]));
        }
        else if (estado == DayState.Dia && hora < DuracionDia)
        {
            volumes[currentVol * 2].weight = 1;
            sliders[currentVol * 2].value = 1;

            if (hora > ((DuracionDia / 2) + (DuracionTransicion / 2)) && !restartedDay)
            {
                restartedDay = true;
                restartedNight = false;
                // restartScene();
            }

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
            alternateVol = -1;
            //aqui cambiar icono si es con sol



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



            volumes[(currentVol * 2) + 1].weight = 1;
            sliders[(currentVol * 2) + 1].value = 1;

            if (WeatherLabel.text.ToLower().Contains("sunny"))
            {
                //StartCoroutine(FadeOutIcon(icons[currentVol]));
                alternateVol = 8;
                resetIcons();
                StartCoroutine(FadeInIcon(icons[8]));
            }
            else if (WeatherLabel.text.ToLower().Contains("cloudy"))
            {
                // StartCoroutine(FadeOutIcon(icons[currentVol]));
                alternateVol = 9;
                resetIcons();
                StartCoroutine(FadeInIcon(icons[9]));
            }
            else if (WeatherLabel.text.ToLower().Contains("extreme"))
            {
                // StartCoroutine(FadeOutIcon(icons[currentVol]));
                alternateVol = 10;
                resetIcons();
                StartCoroutine(FadeInIcon(icons[10]));
            }
        }
        else if (estado == DayState.Noche && hora < DuracionNoche + DuracionDia)
        {
            if (hora > ((DuracionDia) + (DuracionNoche / 2) + (DuracionTransicion)) && restartedDay && !restartedNight)
            {
                restartedDay = false;
                restartedNight = true;
                restartScene();
            }

            volumes[(currentVol * 2) + 1].weight = 1;
            sliders[(currentVol * 2) + 1].value = 1;

            volumes[currentVol * 2].weight = 0;
            sliders[currentVol * 2].value = 0;
        }
        else if (estado == DayState.Noche && hora >= DuracionNoche + DuracionDia)
        {
            estado = DayState.ComienzoDia;
            StateLabel.text = estado.ToString();

            alternateVol = -1;

            daysCounter++;

            if (daysCounter > DuracionSeason)
            {
                daysCounter = 0;
                previousSeason = currentSeason;

                string weatherLog = "Season: " + currentSeason + " time: " + DateTime.Now.ToString("MMddyyyyHHmmss") + "\n";
                foreach (var item in weathersOcurridos)
                {
                    weatherLog += item.Key + " had " + item.Value + " ocurrences \n";
                }
                weatherLog += "\n";

                if (DoDebugWeatherOcurrences)
                {
                    File.AppendAllText(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "weatherOcurrences.txt"), weatherLog);
                    weathersOcurridos.Clear();
                }
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

            if (!weathersOcurridos.ContainsKey(WeatherLabel.text))
                weathersOcurridos.Add(WeatherLabel.text, 1);
            else
                weathersOcurridos[WeatherLabel.text]++;


            if (volumes[currentVol * 2].gameObject.name.ToLower().Contains("foggy"))
                fogControl.EnterFog(DuracionTransicion);
            else
            {
                if (fogControl.fogstate == "Foggy")
                    fogControl.ExitFog(DuracionTransicion);
            }

            if (volumes[currentVol * 2].gameObject.name.ToLower().Contains("rainy"))
            {
                if (modifyRain)
                    rainControl.Show();
                Debug.Log("show rain");
            }
            else
            {
                if (modifyRain)
                    rainControl.Hide();
                Debug.Log("hide rain");

            }

        }

    }

    IEnumerator FadeInIcon(GameObject icon, float _duration = 1f)
    {
        //Debug.Log("appear icon " + icon.name);
        float alpha = 0f;
        float duration = _duration;

        Color32 white = Color.white;
        white.a = 0;

        Image rend = icon.GetComponent<Image>();
        rend.color = white;

        while (alpha < 1f)
        {
            alpha += Time.deltaTime / duration;
            alpha = Mathf.Clamp01(alpha);

            white.a = (byte)(alpha * 255f);
            rend.color = white;


            yield return null;
        }
        // Debug.Log("finished fading in icon " + icon.name);

    }

    // IEnumerator FadeOutIcon(GameObject icon, float _duration = 1f)
    // {
    //     //Debug.Log("dissappear icon " + icon.name);
    //     float alpha = 1f;
    //     float duration = _duration;

    //     Color32 white = Color.white;
    //     white.a = (byte)255;

    //     Image rend = icon.GetComponent<Image>();
    //     rend.color = white;

    //     while (alpha > 0)
    //     {
    //         alpha -= Time.deltaTime / duration;
    //         alpha = Mathf.Clamp01(alpha);

    //         white.a = (byte)(alpha * 255f);
    //         rend.color = white;

    //         yield return null;
    //     }
    //     // Debug.Log("finished fading out icon " + icon.name);

    // }


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

        if (DoDebugWeatherOcurrences)
        {

            DuracionDia = .0001f * 60.0f * 60.0f;

            DuracionNoche = .0001f * 60.0f * 60.0f;

            DuracionTransicion = .00005f * 60.0f * 60.0f;

            DuracionSeason = 88;
        }

        totalTime = DuracionDia + DuracionNoche;

        globalSlider.maxValue = totalTime;

        seasonsSlider.maxValue = DuracionSeason;
        seasonsSlider.transform.Find("Value").GetComponent<Text>().text = daysCounter + "/" + DuracionSeason;


        currentSeason = seasons[UnityEngine.Random.Range(0, seasons.Count)];
        if (PlayerPrefs.GetInt("RestartFlag") == 1)
        {
            currentSeason = PlayerPrefs.GetString("currentSeason");

        }

        


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
        estado = DayState.Dia;

        if (PlayerPrefs.GetInt("RestartFlag") == 1)
        {
            currentVol = PlayerPrefs.GetInt("currentVol");
            previousVol = PlayerPrefs.GetInt("previousVol");
            estado = (DayState)PlayerPrefs.GetInt("estado");
            elapsedTime = PlayerPrefs.GetFloat("elapsedTime");
            alternateVol = PlayerPrefs.GetInt("alternateVol");
            globalSlider.value = elapsedTime;

            skyRotator.elapsedTime=PlayerPrefs.GetFloat("currentRotation");
        }


        WeatherLabel.text = volumes[currentVol * 2].gameObject.name.Replace(" - Day", "").Replace(" - Night", "");
        StateLabel.text = estado.ToString();
        SeasonLabel.text = currentSeason.ToString();

        if (!weathersOcurridos.ContainsKey(WeatherLabel.text))
            weathersOcurridos.Add(WeatherLabel.text, 1);
        else
            weathersOcurridos[WeatherLabel.text]++;

        if (PlayerPrefs.GetInt("RestartFlag") == 1)
        {
            weathersOcurridos = JsonUtility.FromJson<Dictionary<string, int>>(PlayerPrefs.GetString("weathersOcurridos"));

        }

        

        if (volumes[currentVol * 2].gameObject.name.ToLower().Contains("rainy"))
        {
            if (modifyRain)
                rainControl.Show();
            Debug.Log("show rain");
        }
        else
        {
            if (modifyRain)
                rainControl.Hide();
            Debug.Log("hide rain");

        }
        resetIcons();
        StartCoroutine(FadeInIcon(icons[currentVol]));

       

        string weatherIc = volumes[currentVol].gameObject.name;


        if (alternateVol>0)
        {
                    resetIcons();

                StartCoroutine(FadeInIcon(icons[alternateVol], .01f));
        }else{

        }

         if (volumes[currentVol * 2].gameObject.name.ToLower().Contains("foggy"))
            fogControl.EnterFog(DuracionTransicion);

            Debug.Log("lleva mariposas: " + ((PlayerPrefs.GetInt(currentSeason + "_Mariposas") == 1) ? true : false));
        if ((PlayerPrefs.GetInt(currentSeason + "_Mariposas") == 1) ? true : false)
        {
            StartCoroutine(FadeInMariposas());
        }
        else
            StartCoroutine(FadeOutMariposas());

        PlayerPrefs.SetInt("RestartFlag", 0);

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

        // Debug.Log("<color=green>toggleVisibility " + UI.gameObject.activeSelf + "</color>");

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
