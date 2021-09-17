using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;

public class SendEmail : Singleton<SendEmail>
{

    public void SendEMail(string email, string mesj)
    {
        string uri = "https://maker.ifttt.com/trigger/requestReceived/with/key/dEj4z-y8Rx4fTQbHegTCO0?Value1=12345";
        Debug.Log("uri " + uri);

        StartCoroutine(GetRequest(uri));

    }

    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    //Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    string result = webRequest.downloadHandler.text;
                    Debug.Log("Response from mail server " + result);
                    break;
            }
        }
    }




}
