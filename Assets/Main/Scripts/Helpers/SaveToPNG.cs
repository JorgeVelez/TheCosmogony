using UnityEngine;
using System.Collections;
using System.IO;

public class SaveToPNG : MonoBehaviour
{
    Camera camera;
    CameraClearFlags prevClearFlags;
    void Start()
    {
        camera = Camera.main;
    }
    /// <summary>
    /// 
    /// </summary>
        public void GrabScreen()
        {
        prevClearFlags = camera.clearFlags;
        camera.clearFlags = CameraClearFlags.Nothing;
            Texture2D scrTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
            RenderTexture scrRenderTexture = new RenderTexture(scrTexture.width, scrTexture.height, 24);
            RenderTexture camRenderTexture = camera.targetTexture;

            camera.targetTexture = scrRenderTexture;
            camera.Render();
            camera.targetTexture = camRenderTexture;

            RenderTexture.active = scrRenderTexture;
            scrTexture.ReadPixels(new Rect(0, 0, scrTexture.width, scrTexture.height), 0, 0);
            scrTexture.Apply();

        camera.clearFlags = prevClearFlags;

        // Encode texture into PNG
        byte[] bytes = scrTexture.EncodeToPNG();
        string path =System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments;
        path=path+"ScreenShots/ScreenGrab_"+ Screen.width +"x"+ Screen.height+ "_"+System.DateTime.Now+".png";
         if(!Directory.Exists(path)){    
                Directory.CreateDirectory(path);
            }
        File.WriteAllBytes( path, bytes);

        Object.Destroy(scrTexture);
        Debug.Log("Screen captured");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            GrabScreen();
        }
    }
}