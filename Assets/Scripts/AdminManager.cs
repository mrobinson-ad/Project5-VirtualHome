using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace VirtualHome
{
    public class AdminManager : MonoBehaviour
    {

        public List<Promo> promos = new List<Promo>();
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
                    Debug.Log("Error Updating price: " + webRequest.error);
                    yield break;
                }
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Response: " + jsonResponse);
            }
        }

        public void StartGetPromos(string userID)
        {
            StartCoroutine(GetPromos(userID));
        }
        private IEnumerator GetPromos(string userID)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get("http://localhost/MYG/API/getpromos/" + userID))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error fetching product data: " + webRequest.error);
                }
                else
                {
                    string jsonResponse = webRequest.downloadHandler.text;

                    if (jsonResponse.Contains("error"))
                    {
                        Debug.Log("Error getting promos");
                        yield break;
                    }
                    // Parse JSON response into a list of Promo objects
                    promos = Promo.FromJsonArray(jsonResponse);
                }
            }
        }

        public void StartUpdatePromo(string userID, string code, string value, string amount)
        {
            StartCoroutine(UpdatePromo(userID, code, value, amount));
        }
        private IEnumerator UpdatePromo(string userID, string code, string value, string amount)
        {
            WWWForm form = new WWWForm();
            form.AddField("action", "updatepromo");
            form.AddField("userID", userID);
            form.AddField("code", code);
            form.AddField("value", value);
            form.AddField("amount", amount);

            using (UnityWebRequest webRequest = UnityWebRequest.Post("http://localhost/MYG/api/index.php", form))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log("Error updating promo: " + webRequest.error);
                    yield break;
                }
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Response: " + jsonResponse);
            }
        }


    }

    [System.Serializable]
    public class Promo
    {
        [JsonProperty("promoID")]
        public string promoID;

        [JsonProperty("code")]
        public string promoCode;

        [JsonProperty("value")]
        public string promoValue;

        [JsonProperty("amount")]
        public string promoAmount;
        public static List<Promo> FromJsonArray(string jsonString)
        {
            return JsonConvert.DeserializeObject<List<Promo>>(jsonString);
        }
    }
}