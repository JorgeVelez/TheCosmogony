using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate =-1;
        QualitySettings.vSyncCount = 0;
         Screen.SetResolution(1440  , 3440, true);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
