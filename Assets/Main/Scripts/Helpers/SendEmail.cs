using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;

public class SendEmail : Singleton<SendEmail>
{

    public void SendEMail(string email, string mesj)
    {
        // A correct website page.
        StartCoroutine(GetRequest("https://jorgevelez.net/sitio_jv/Tools/sendEmail.php?email=" + email + "&asunto=reporte%20de%20gastos&mensaje=" + mesj));

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
