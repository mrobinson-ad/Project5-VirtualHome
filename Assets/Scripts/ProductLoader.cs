using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class ProductLoader : MonoBehaviour
{
    private string apiUrl = "http://localhost/MYG/API/getall";  // Example URL
    public List<Product> products;
    void Awake()
    {
        StartCoroutine(GetProductData());
    }

    // Coroutine to make a web request and get product JSON array
    IEnumerator GetProductData()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error fetching product data: " + webRequest.error);
            }
            else
            {
                // Get JSON response as string (this will be an array of products)
                string jsonResponse = webRequest.downloadHandler.text;

                // Parse JSON response into a list of Product objects
                products = Product.FromJsonArray(jsonResponse);

                // Process each product
                foreach (Product product in products)
                {
                    Debug.Log($"Product Name: {product.productName}, Price: {product.productPrice}");

                    // Load materials and sprites asynchronously
                    StartCoroutine(product.LoadMaterials());
                    StartCoroutine(product.LoadSprites());
                    StartCoroutine(product.FetchTags());
                    StartCoroutine(product.FetchDimensions());
                    StartCoroutine(product.FetchSales());
                }
            }
        }
    }
}