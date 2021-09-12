
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SRDebugger;
using UnityEditor;
 using UnityEngine.SceneManagement;


public class ScreenCheck : MonoBehaviour
{
    public Image image;


 private void Update()
    {
        if (Input.GetKeyUp(KeyCode.C))
        {
            ApplyChanges();
        }
    }
 private void ApplyChanges()
     {
         Texture2D snapShot = new Texture2D((int)100, (int)100, TextureFormat.ARGB32, false);
         RenderTexture snapShotRT = new RenderTexture((int)100, (int)100, 24, RenderTextureFormat.ARGB32);

         RenderTexture.active = snapShotRT;

         snapShot.ReadPixels(new Rect(0, 0, snapShotRT.width, snapShotRT.height), 0, 0);
         snapShot.Apply();
         image.gameObject.SetActive(true);
         image.sprite = Sprite.Create(snapShot, new Rect(0, 0, snapShot.width, snapShot.height), new Vector2(0.5f, 0.5f));
         RenderTexture.active = null;
 }
}
