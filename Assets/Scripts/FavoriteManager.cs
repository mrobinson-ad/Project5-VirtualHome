using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using UnityEngine.ResourceManagement.AsyncOperations;
using VirtualHome;
using System.Linq;



public class FavoriteManager : MonoBehaviour
{
    public static FavoriteManager Instance { get; private set; } // Singleton

    public List<Product> favoriteList;

    //public List<Bundle_SO> favoriteBundleList;

    public Dictionary<CartItem, int> cartDict = new Dictionary<CartItem, int>();

    //public List<Bundle_SO> cartBundleList;

    [SerializeField] public List<Promo_SO> promos;

    private AsyncOperationHandle<PromoList_SO>? loadedHandle;
    [SerializeField] private string promoAddress;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        loadedHandle = Addressables.LoadAssetAsync<PromoList_SO>(promoAddress);
        loadedHandle.Value.Completed += OnPrefabLoaded;

        void OnPrefabLoaded(AsyncOperationHandle<PromoList_SO> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                promos = handle.Result.content.list;
                Debug.Log("Promo list loaded successfuly");
            }
            else
            {
                Debug.LogError("Failed to load promo list");
            }
        }

    }
    public void StartUserList(string id)
    {
        StartCoroutine(GetUser(id));
    }

    private IEnumerator GetUser(string userID)
    {
        string url = $"http://localhost/MYG/API/getuser/{userID}"; // Construct the URL

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error fetching user data: " + webRequest.error);
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Response: " + jsonResponse);

                JObject parsedJson = JObject.Parse(jsonResponse);

                JArray favoriteArray = parsedJson["favoriteList"] as JArray;
                if (favoriteArray != null)
                {
                    List<int> favoriteIDs = favoriteArray.ToObject<List<int>>();
                    favoriteList.Clear();

                    foreach (int id in favoriteIDs)
                    {
                        Product product = UIManager.Instance.productList.Find(p => p.productID == id);
                        if (product != null)
                        {
                            favoriteList.Add(product);
                        }
                        else
                        {
                            Debug.LogWarning("Product with ID " + id + " not found in productList.");
                        }
                    }
                    Debug.Log("Favorite products added to the list.");
                }
                else
                {
                    Debug.Log("No favoriteList found or favoriteList is null.");
                }

                JArray cartArray = parsedJson["cartList"] as JArray;
                if (cartArray != null)
                {
                    cartDict.Clear();

                    foreach (JObject cartItemObj in cartArray)
                    {
                        int productId = (int)cartItemObj["id"];
                        int amt = (int)cartItemObj["amt"];
                        int matIndex = (int)cartItemObj["mat"] - 1; // Adjusting for zero-based index

                        Product product = UIManager.Instance.productList.Find(p => p.productID == productId);

                        if (product != null)
                        {
                            if (matIndex >= 0 && matIndex < product.colors.Count)
                            {
                                string color = product.colors[matIndex];
                                CartItem cartItem = new CartItem(product, color);
                                cartDict[cartItem] = amt;

                            }
                            else
                            {
                                Debug.LogWarning($"Invalid material index {matIndex + 1} for product {product.productName}.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Product with ID " + productId + " not found in productList.");
                        }
                    }
                    UIManager.Instance.cartAmount = cartDict.Values.Sum();
                    Debug.Log("Cart items added to the dictionary.");
                }
                else
                {
                    Debug.Log("No cartList found or cartList is null.");
                }
            }
        }
    }

    public void StartAddFavorite(string userID,string productID)
    {
        StartCoroutine(AddFavorite(userID, productID));
    }

    private IEnumerator AddFavorite(string userID,string productID)
    {
        string url = $"http://localhost/MYG/API/addfavorite/{userID}/{productID}"; // Construct the URL

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest(); // Wait for the response

            // Check for errors
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error adding favorite {productID}: {webRequest.error}");
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;

                if (jsonResponse.Contains("\"success\""))
                {
                    Debug.Log("Favorite added successfully");
                } else 
                {
                    Debug.Log("Failed to add favorite");
                }
            }
        }
    }

    public void StartDelFavorite(string userID,string productID)
    {
        StartCoroutine(DelFavorite(userID, productID));
    }

    private IEnumerator DelFavorite(string userID,string productID)
    {
        string url = $"http://localhost/MYG/API/removefavorite/{userID}/{productID}"; // Construct the URL

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest(); // Wait for the response

            // Check for errors
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error removing favorite {productID}: {webRequest.error}");
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;

                if (jsonResponse.Contains("\"success\""))
                {
                    Debug.Log("Favorite removed successfully");
                } else 
                {
                    Debug.Log("Failed to add favorite");
                }
            }
        }
    }

    public void StartAddCart(string userID, string productID, string matID)
    {
        StartCoroutine(AddCart(userID, productID, matID));
    }

    private IEnumerator AddCart(string userID, string productID, string matID)
    {
        string url = $"http://localhost/MYG/API/addcart/{userID}/{productID}/{matID}"; // Construct the URL

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest(); // Wait for the response

            // Check for errors
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error adding favorite {productID}: {webRequest.error}");
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;

                if (jsonResponse.Contains("\"success\""))
                {
                    Debug.Log("Favorite added successfully");
                } else 
                {
                    Debug.Log("Failed to add favorite");
                }
            }
        }
    }

    public void StartDelCart(string userID, string productID, string matID)
    {
        StartCoroutine(DelCart(userID, productID, matID));
    }

    private IEnumerator DelCart(string userID, string productID, string matID)
    {
        string url = $"http://localhost/MYG/API/removecart/{userID}/{productID}/{matID}"; // Construct the URL

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest(); // Wait for the response

            // Check for errors
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error adding favorite {productID}: {webRequest.error}");
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;

                if (jsonResponse.Contains("\"success\""))
                {
                    Debug.Log("Favorite added successfully");
                } else 
                {
                    Debug.Log("Failed to add favorite");
                }
            }
        }
    }
}

public class CartItem
{
    public Product product;
    public string color;

    public CartItem(Product product, string color)
    {
        this.product = product;
        this.color = color;
    }

    // Override Equals to compare CartItems based on product and color
    public override bool Equals(object obj)
    {
        if (obj is CartItem other)
        {
            return product == other.product && color == other.color;
        }
        return false;
    }

    // Override GetHashCode to match the criteria of Equals
    public override int GetHashCode()
    {
        return (product, color).GetHashCode();
    }
}
