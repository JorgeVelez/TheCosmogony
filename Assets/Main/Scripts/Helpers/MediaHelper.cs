﻿﻿/*MediaHelper
 
Helper for classifying media and loading images
 
 */
 
 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class MediaHelper : MonoBehaviour
{
 
    static string[] imageExtensions = {
        ".PNG", ".JPG", ".JPEG", ".BMP", ".GIF"
    };
 
    static string[] restrictedImageExtensions = {
        ".PNG", ".JPG", ".JPEG"
    };
 
    static string[] videoExtensions = {
        ".AVI", ".MP4", ".DIVX", ".WMV", ".MOV", ".WEBM"
    };
 
    static string[] restrictedVideoExtensions = {
        ".MP4", ".MOV", ".WEBM"
    };
 
    static string[] audioExtensions = {
        ".WAV", ".MID", ".MIDI", ".WMA", ".MP3", ".OGG", ".RMA"
    };
 
    static string[] restrictedAudioExtensions = {
        ".WAV", ".MP3"
    };
 
    static string[] restrictedSubtitleExtensions = {
        ".SRT"
    };
    static string[] subtitleExtensions = {
        ".SRT"
    };
 
    public static bool IsVideoFile(string path)
    {
        if (path == "" || path == null)
            return false;
 
        return -1 != Array.IndexOf(videoExtensions, Path.GetExtension(path).ToUpperInvariant());
    }
 
    public static bool IsImageFile(string path)
    {
        if (path == "" || path == null)
            return false;
 
        return -1 != Array.IndexOf(imageExtensions, Path.GetExtension(path).ToUpperInvariant());
    }
 
    public static bool IsAudioFile(string path)
    {
        if (path == "" || path == null)
            return false;
 
        return -1 != Array.IndexOf(audioExtensions, Path.GetExtension(path).ToUpperInvariant());
    }
    public static bool IsRestrictedVideoFile(string path)
    {
        if (path == "" || path == null)
            return false;
 
        return -1 != Array.IndexOf(videoExtensions, Path.GetExtension(path).ToUpperInvariant());
    }
 
    public static bool IsRestrictedSubtitleFile(string path)
    {
        if (path == "" || path == null)
            return false;
        return -1 != Array.IndexOf(restrictedSubtitleExtensions, Path.GetExtension(path).ToUpperInvariant());
    }
 
    public static bool IsRestrictedImageFile(string path)
    {
        if (path == "" || path == null)
            return false;
        return -1 != Array.IndexOf(imageExtensions, Path.GetExtension(path).ToUpperInvariant());
    }
 
    public static bool IsRestrictedAudioFile(string path)
    {
        if (path == "" || path == null)
            return false;
 
        return -1 != Array.IndexOf(audioExtensions, Path.GetExtension(path).ToUpperInvariant());
    }
 
    public static string getExtension(string path)
    {
        if (path == "" || path == null)
            return "";
        return Path.GetExtension(path);
    }
 
    public static Texture2D loadErrorImage()
    {
        Texture2D targetTexture = new Texture2D(16,16);
 
        for (int y = 0; y < targetTexture.height; y++)
        {
            for (int x = 0; x < targetTexture.width; x++)
            {
                targetTexture.SetPixel(x, y, Color.cyan);
            }
        }
        targetTexture.Apply();
 
        return targetTexture;
    }
 
    public static void checkDirectory(string path)
    {
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            UnityEngine.Debug.Log("created directory " + Path.GetDirectoryName(path));
        }
    }
 
    public static Texture2D loadImage(string filePath)
    {
        if(filePath == null || filePath == "")
        {
            Debug.Log("<color=red>Image does not exist "  + "</color>");
            return null;
        }
        filePath = Path.Combine(ConfigurationManager.Instance.globalConfig.uris.RESOURCES_PATH, filePath);
 
        Texture2D tex = null;
        byte[] fileData;
 
        if (File.Exists(filePath + ".png"))
        {
            fileData = File.ReadAllBytes(filePath + ".png");
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
        }
        else if (File.Exists(filePath + ".jpg"))
        {
            fileData = File.ReadAllBytes(filePath + ".jpg");
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
        }
        else
        {
            Debug.LogError("image does not exist " + filePath);
        }
        return tex;
    }  
}
