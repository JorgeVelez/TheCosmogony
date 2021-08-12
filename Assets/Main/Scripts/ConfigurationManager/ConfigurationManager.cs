/*ConfigurationManager
 
Handles xml config
 
 */
 
using System.IO;
using UnityEngine;
using System;
using SharpConfig;
 
public class GlobalConfiguration
{
    public URIs uris;
    public Audio audio;
}
 
public class URIs
{
    public string WALL = "RIGHT";
    public string RESOURCES_PATH = @"C:\LP\configs\";
 
    public string SERVER_URI = "https://cloud.squidex.io/";
    public string SIGNALR_URI = "http://fldcapi.localprojects.com/clientHub/";

}
 
 
public class Audio
{
    /*
**************************
Audio.
**************************
*/
    public float minDistanceThreshold = 1f;
    public float maxDistanceThreshold = 3f;
}
 
 
 
public class ConfigurationManager : Singleton<ConfigurationManager>
{
    public delegate void ConfigurationManagerAction();
    public event ConfigurationManagerAction OnConfigurationLoaded;
 
    public GlobalConfiguration globalConfig;
    public Configuration cfg;
    public string configPath = @"C:\LP\configs\UnityGallery.config";
 
    public void PrintConfig()
    {
        foreach (Section section in cfg)
        {
            Debug.Log("<color=red>section.Name " + section.Name + "</color>");
 
            foreach (Setting setting in section)
            {
                Debug.Log("<color=red>section:" + section.Name + " setting: " + setting.Name + " value: " + setting.StringValue + " </color>");
            }
 
        }
    }
 
    void Awake()
    {
        Debug.Log("<color=green>ConfigurationManager>> started</color>");
 
        cfg = new Configuration();
        globalConfig = new GlobalConfiguration();
 
        MediaHelper.checkDirectory(configPath);
 
        if (!File.Exists(configPath))
        {
            Debug.Log("Created new config file");
            cfg.Add(Section.FromObject("uris", new URIs()));
            cfg.Add(Section.FromObject("audio", new Audio()));
            cfg.SaveToFile(configPath);
 
            Refresh();
        }
        else
        {
            Debug.Log("Loaded config file");
            cfg = Configuration.LoadFromFile(configPath);
      
            globalConfig.audio = cfg["audio"].ToObject<Audio>();
            cfg.Remove("audio");
            cfg.Add(Section.FromObject("audio", globalConfig.audio)); 
 
            globalConfig.uris = cfg["uris"].ToObject<URIs>();
            cfg.Remove("uris");
            cfg.Add(Section.FromObject("uris", globalConfig.uris));
 
        }
 
        //PrintConfig(cfg); 
    }
 
    public string SaveToFile()
    {
        try
        {
            Refresh();
            cfg.SaveToFile(configPath);
 
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
 
        return "ok";
    }
 
    public void Refresh()
    {
        Debug.Log("<color=green>Refreshing variables " + "</color>");
 
        globalConfig.uris = cfg["uris"].ToObject<URIs>();
        globalConfig.audio = cfg["audio"].ToObject<Audio>();
    }
 
}
 
 
 
 
