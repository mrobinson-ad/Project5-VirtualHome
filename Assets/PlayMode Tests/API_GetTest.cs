using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Networking;

public class API_GetTest
{
    private const string baseUrl = "http://localhost/MYG/api/";

    // Helper function to send GET requests
    private UnityWebRequest CreateWebRequest(string action, string parameters = "")
    {
        string url = baseUrl + action;
        if (!string.IsNullOrEmpty(parameters))
        {
            url += "/" + parameters;
        }
        return UnityWebRequest.Get(url);
    }

    // Test for 'getall' action
    [UnityTest]
    public IEnumerator Test1_GetAll_Products()
    {
        UnityWebRequest request = CreateWebRequest("getall");

        yield return request.SendWebRequest();

        Assert.IsFalse(request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError, "API call failed");
        Assert.IsNotNull(request.downloadHandler.text, "Response is null");

        Debug.Log(request.downloadHandler.text);
        
        Assert.IsTrue(request.downloadHandler.text.Contains("productID"), "No products returned");
    }

    // Test for 'gettags' action
    [UnityTest]
    public IEnumerator Test2_GetTags_ByProductID()
    {
        string productID = "1";  // Lamp ID
        UnityWebRequest request = CreateWebRequest("gettags", productID);

        yield return request.SendWebRequest();

        Assert.IsFalse(request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError, "API call failed");
        Assert.IsNotNull(request.downloadHandler.text, "Response is null");

        Debug.Log(request.downloadHandler.text);
        Assert.IsTrue(request.downloadHandler.text.Contains("Baroque"), "No tags returned");
    }

    // Test for 'getdimensions' action
    [UnityTest]
    public IEnumerator Test3_GetDimensions_ByProductID()
    {
        string productID = "1";  // Lamp ID
        UnityWebRequest request = CreateWebRequest("getdimensions", productID);

        yield return request.SendWebRequest();

        Assert.IsFalse(request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError, "API call failed");
        Assert.IsNotNull(request.downloadHandler.text, "Response is null");

        Debug.Log(request.downloadHandler.text);
        Assert.IsTrue(request.downloadHandler.text.Contains("x"), "No x returned");
        Assert.IsTrue(request.downloadHandler.text.Contains("y"), "No y returned");
        Assert.IsTrue(request.downloadHandler.text.Contains("z"), "No z returned");
    }

    // Test for 'addfavorite' action
    [UnityTest]
    public IEnumerator Test4_AddFavorite()
    {
        string userID = "6";      // Test user ID
        string productID = "2";   // Couch ID
        UnityWebRequest request = CreateWebRequest("addfavorite", userID + "/" + productID);

        yield return request.SendWebRequest();

        Assert.IsFalse(request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError, "API call failed");
        Debug.Log(request.downloadHandler.text);
        Assert.IsTrue(request.downloadHandler.text.Contains("Favorite added"), "Failed to add favorite");
    }

    // Test for 'removefavorite' action
    [UnityTest]
    public IEnumerator Test5_RemoveFavorite()
    {
        string userID = "6";      // Example user ID
        string productID = "2";   // Example product ID
        UnityWebRequest request = CreateWebRequest("removefavorite", userID + "/" + productID);

        yield return request.SendWebRequest();

        Assert.IsFalse(request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError, "API call failed");
        Debug.Log(request.downloadHandler.text);
        Assert.IsTrue(request.downloadHandler.text.Contains("Favorite removed"), "Failed to remove favorite");
    }

    // Test for 'checkpromo' action
    [UnityTest]
    public IEnumerator Test6_CheckPromo_Code()
    {
        string promoCode = "OPENPROMO1";  
        UnityWebRequest request = CreateWebRequest("checkpromo", promoCode);

        yield return request.SendWebRequest();

        Assert.IsFalse(request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError, "API call failed");
        Debug.Log(request.downloadHandler.text);
        Assert.IsTrue(request.downloadHandler.text.Contains("40"), "Promo code failed");
    }
}