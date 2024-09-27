using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class PromoCheck : MonoBehaviour
{

    public void StartPromoCheck(string code, Action<string> callback)
    {
        StartCoroutine(CheckPromo(code, callback));
    }
    private IEnumerator CheckPromo(string code, Action<string> callback)
    {
        string url = $"http://localhost/MYG/API/checkpromo/{code}"; // Construct the URL

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest(); 

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error fetching promo for code {code}: {webRequest.error}");
            }
            else
            {
                
                string jsonResponse = webRequest.downloadHandler.text;
                
                try
                {
                    if(jsonResponse.Contains("error"))
                    {
                        callback("failed");
                        Debug.LogWarning(jsonResponse);
                    }
                    else
                    {
                        callback(jsonResponse);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"An unexpected error occurred: {ex.Message}");
                }
            }
        }
    }
}
