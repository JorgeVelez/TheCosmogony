
using System;
using System.IO;
using UnityEngine;
using SRDebugger;
using UnityEditor;

public class KeystrokeListener : MonoBehaviour
{


  private void Update()
  {
    if (Input.GetKeyUp(KeyCode.Space)){
      if(!SRDebug.Instance.IsDebugPanelVisible)
        SRDebug.Instance.ShowDebugPanel();
      else
        SRDebug.Instance.HideDebugPanel();

      Debug.Log("sadasdasd");
    }
      if (Input.GetKeyUp(KeyCode.Tab)){
        ConfigurationUIManager.Instance.toggleVisibility();
      }
       if (Input.GetKeyUp(KeyCode.K)){
      ColorCorrectionController.Instance.toggleVisibility();
       }
if (Input.GetKeyUp(KeyCode.Escape)){
Application.Quit();
       #if UNITY_EDITOR
 if(EditorApplication.isPlaying) 
 {
      UnityEditor.EditorApplication.isPlaying = false;
 }
 #endif
  }
}
}
