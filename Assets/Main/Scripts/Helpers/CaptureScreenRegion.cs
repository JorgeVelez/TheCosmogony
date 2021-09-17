using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class CaptureScreenRegion : MonoBehaviour
{
    int width = 1; // width of the object to capture
    int height = 10; // height of the object to capture

    int startX = 0;
    int startY = 0;

    public ColorCorrectionController colorCorrectionController;
    void Start()
    {
        startX = 0;
        startY = Screen.height / 2;

        InvokeRepeating("check", 3f, 1f);
    }

    void check()
    {
            StartCoroutine(takeScreenShot());
    }

    public IEnumerator takeScreenShot()
    {
        yield return new WaitForEndOfFrame(); // it must be a coroutine 

        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(startX, startY, width, height), 0, 0);
        tex.Apply();

        Color[] originalPixels = tex.GetPixels(0);
                Destroy(tex);

        bool noColor=true;
        for (int i = 0; i < originalPixels.Length; i++)
        {
            Color cc = originalPixels[i];
            if(cc.r+cc.g+cc.b>.5f)
                noColor=false;
        }

        if(noColor){
            Debug.Log("problem");
            CancelInvoke();
            SendEmail.instance.SendEMail("jorgeluisvelez@gmail.com", "blackout error");
            colorCorrectionController.restartScene();
    }}
}