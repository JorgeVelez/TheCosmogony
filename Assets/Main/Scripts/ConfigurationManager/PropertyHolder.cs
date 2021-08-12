﻿using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using SharpConfig;
 
 
public class PropertyHolder : MonoBehaviour
{
    private InputField value;
    private string storedValue;
 
    Action<string> callback;
 
    private Button btCancel;
    private Button btSave;
 
    Setting settingToAffec;
 
    public void Init(ref Setting _settingToAffec, Action<string> ActionToExecute)
    {
        gameObject.SetActive(true);
 
        value = gameObject.GetComponentInChildren<InputField>();
 
        btSave = transform.Find("Save").GetComponent<Button>();
        btCancel = transform.Find("Cancel").GetComponent<Button>();
 
        btCancel.onClick.AddListener(Cancel);
        btSave.onClick.AddListener(Save);
 
        
        value.text = _settingToAffec.StringValue;
        storedValue = _settingToAffec.StringValue;
 
        transform.Find("Label").GetComponentInChildren<Text>().text = _settingToAffec.Name;
 
        value.onValueChanged.AddListener(onValueChangedHandler);
 
        gameObject.name = _settingToAffec.Name;
 
        callback = ActionToExecute;
 
        settingToAffec = _settingToAffec;
    }
 
    private void onValueChangedHandler(string arg0)
    {
        btSave.gameObject.SetActive(true);
        btCancel.gameObject.SetActive(true);
       
    }
 
    public void Save()
    {
        settingToAffec.StringValue = value.text;
 
        string msg = ConfigurationManager.Instance.SaveToFile();
 
        if (msg == "ok")
        {
            btSave.gameObject.SetActive(false);
            btCancel.gameObject.SetActive(false);
 
 
            Debug.Log("<color=green>Saved Succesfullly " + "</color>");
 
            callback("Saved Succesfullly");
        }
        else
        {
            callback(msg);
        }
 
       
    }
 
    public void Cancel()
    {
        value.text = storedValue;
 
        btSave.gameObject.SetActive(false);
        btCancel.gameObject.SetActive(false);
    }
}
