using UnityEngine.UI;
using UnityEngine;
using SharpConfig;
 
public class ConfigurationUIManager : Singleton<ConfigurationUIManager>
{   
    public Text alert;
 
    public GameObject UI;
 
    public Transform SectionPrefab;
    public Transform TabPrefab;
    public Transform PropPrefab;
 
    void Start()
    {  
 
        UI.SetActive(false);
 
        bool isFirst = true;
         
        foreach (Section section in ConfigurationManager.Instance.cfg)
        {
            if (section.SettingCount == 0)
                continue;
 
            GameObject goTabPrefab = Instantiate(TabPrefab, TabPrefab.parent).gameObject;
            goTabPrefab.SetActive(true);
            goTabPrefab.GetComponentInChildren<Text>().text = section.Name;
            goTabPrefab.GetComponent<Image>().color = Color.gray;
 
            GameObject goSectionPrefab = Instantiate(SectionPrefab, SectionPrefab.parent).gameObject;
            goSectionPrefab.name = section.Name;
            if (isFirst)
            {
                goSectionPrefab.SetActive(true);
                goTabPrefab.GetComponent<Image>().color = Color.green;
                isFirst = false;
            }
 
            goTabPrefab.GetComponent<Button>().onClick.AddListener(() => openSection(goTabPrefab, goSectionPrefab));
 
            foreach (Setting setting in section)
            {
                GameObject goPropPrefab = Instantiate(PropPrefab, goSectionPrefab.transform).gameObject;
                goPropPrefab.SetActive(true);
                Setting settingToAffec = setting;
                goPropPrefab.GetComponentInChildren<PropertyHolder>().Init(ref settingToAffec, executeSave);
            }
        }
 
       // ConfigurationManager.Instance.PrintConfig();   
    }
 
    public void Show()
    {
        UI.gameObject.SetActive(true);
 
 
        alert.text = "";   
    }
 
    void executeSave(string msg)
    {
        alert.text = msg;
 
        CancelInvoke("hideAlert");
        Invoke("hideAlert", 3);
    }
 
    void hideAlert()
    {
        alert.text = "";
 
    }
 
    void openSection(GameObject tab, GameObject section)
    {
        foreach (Transform child in SectionPrefab.parent) 
        {
            child.gameObject.SetActive(false);  
        }  
 
        foreach (Transform child in TabPrefab.parent)
        {
            child.gameObject.GetComponent<Image>().color = Color.gray;
        }
        section.SetActive(true);
        tab.GetComponent<Image>().color = Color.green;
    }
 
    internal void toggleVisibility()
    { 
 
        Debug.Log("<color=green>toggleVisibility " + UI.gameObject.activeSelf + "</color>");
 
        if (!UI.gameObject.activeSelf)
            Show();
        else
            Hide();
    }
 
    public void Hide()
    {
        UI.gameObject.SetActive(false);
 
    }
  
    
}
