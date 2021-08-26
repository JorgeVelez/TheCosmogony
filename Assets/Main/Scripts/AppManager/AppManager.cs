using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//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 
public class AppManager : MonoBehaviour
{
    public bool erasePlayerPrefs = true;
    void Awake()
    {
        if (PlayerPrefs.HasKey("version"))
        {
            if (Application.version != PlayerPrefs.GetString("version"))
            {
                PlayerPrefs.SetString("version", Application.version); 
                if(erasePlayerPrefs){
                    PlayerPrefs.DeleteAll();
                    Debug.Log("deleted all player prefs ");
                }
            }
        }else{
           PlayerPrefs.SetString("version", Application.version); 
        }

    }

    void Start()
    {
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 0;
        Screen.SetResolution(2160, 3840, true);

    }

    // Update is called once per frame
    void Update()
    {

    }
}
