
using System;
using System.IO;
using UnityEngine;
using UnityEditor;

public class KeystrokeListener : MonoBehaviour
{

    public ColorCorrectionController colorCorrectionController;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (!SRDebug.Instance.IsDebugPanelVisible)
                SRDebug.Instance.ShowDebugPanel();
            else
                SRDebug.Instance.HideDebugPanel();

            Debug.Log("sadasdasd");
        }

        if(Input.GetKeyDown(KeyCode.R)){
             colorCorrectionController.restartScene();
         }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            ConfigurationUIManager.Instance.toggleVisibility();
        }
        if (Input.GetKeyUp(KeyCode.K))
        {
            colorCorrectionController.toggleVisibility();
        }
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
#endif
        }
    }
}
