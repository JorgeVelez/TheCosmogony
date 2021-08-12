using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;

public class CheckEther : MonoBehaviour
{

	void Start()
    {
        // A correct website page.
        StartCoroutine(GetRequest("https://min-api.cryptocompare.com/data/price?fsym=ETH&tsyms=ETH,USD"));

        // A non-existing page.
        //StartCoroutine(GetRequest("https://error.html"));
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
					string result =webRequest.downloadHandler.text;
		result=result.Split(new string[] { "USD\":" }, StringSplitOptions.None)[1];
		//Debug.Log(result);
		Debug.Log("Ethereum price right now is $"+result.Split('}')[0]+" ");
                    break;
            }
        }
    }

	
	
	
}
