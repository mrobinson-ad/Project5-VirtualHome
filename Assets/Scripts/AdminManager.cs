using System.Collections;
using UnityEngine;
using UnityEngine.Networking;


public class AdminManager : MonoBehaviour
{
    public void StartUpdatePrice(string userID, string productID, string price, string isSale, string salePrice)
    {
        StartCoroutine(UpdatePrice(userID, productID, price, isSale, salePrice));
    }
    private IEnumerator UpdatePrice(string userID, string productID, string price, string isSale, string salePrice)
    {
        WWWForm form = new WWWForm();
        form.AddField("action", "updateprice");
        form.AddField("userID", userID);
        form.AddField("productID", productID);
        form.AddField("price", price);
        form.AddField("isSale", isSale);
        form.AddField("salePrice", salePrice);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://localhost/MYG/api/index.php", form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error Updating price: " + webRequest.error);
                yield break;
            }
            string jsonResponse = webRequest.downloadHandler.text;
            Debug.Log("Response: " + jsonResponse);
        }
    }
}
