using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using System.Collections;
using System;

namespace VirtualHome
{
    [System.Serializable]
    public class Product
    {
        public int productID;

        [JsonProperty("name")]
        public string productName;

        [JsonProperty("price")]
        public string productPrice;

        [JsonProperty("short")]
        public string productShortDescription;

        [JsonProperty("description")]
        public string productDescription;

        [JsonProperty("prefab")]
        public string prefabAddress;

        [JsonIgnore]
        public GameObject prefab;

        public Vector3 productDimensions;

        public bool isSale;

        public string productSale;

        public string selectedColor;

        // Colors list from materials
        public List<string> colors = new List<string>();

        // List to store loaded materials
        public List<Material> productMaterials = new List<Material>();

        // List to store loaded sprites
        public List<Sprite> productSprites = new List<Sprite>();

        // List to store tags
        public List<String> tags = new List<String>();

        private AsyncOperationHandle<GameObject>? loadedHandle;

        // JSON parsing for materials (color and material path)
        public class MaterialData
        {
            public string color;
            public string material;
        }

        [JsonProperty("materials")]
        private string materialsJson;

        [JsonProperty("sprites")]
        private string spritesJson;

        // Deserialize JSON array into List<Product>
        public static List<Product> FromJsonArray(string jsonString)
        {
            return JsonConvert.DeserializeObject<List<Product>>(jsonString);
        }

        // Load Prefab asynchronously using Addressables
        public void LoadPrefab()
        {
            if (!string.IsNullOrEmpty(prefabAddress))
            {
                loadedHandle = Addressables.LoadAssetAsync<GameObject>(prefabAddress);
                loadedHandle.Value.Completed += OnPrefabLoaded;

                void OnPrefabLoaded(AsyncOperationHandle<GameObject> handle)
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        prefab = handle.Result;
                        Debug.Log("Prefab loaded successfully for " + productName);
                    }
                    else
                    {
                        Debug.LogError("Failed to load prefab for " + productName);
                    }
                }
            }
        }
        public void ReleasePrefab()
        {
            if (loadedHandle.HasValue)
            {
                Addressables.Release(loadedHandle.Value);
                loadedHandle = null;
                prefab = null;
                Debug.Log("Prefab released for " + productName);
            }
        }

        public IEnumerator LoadMaterials()
        {
            if (!string.IsNullOrEmpty(materialsJson))
            {
                List<MaterialData> materialDataList = JsonConvert.DeserializeObject<List<MaterialData>>(materialsJson);

                foreach (var matData in materialDataList)
                {
                    colors.Add(matData.color);

                    AsyncOperationHandle<Material> matHandle = Addressables.LoadAssetAsync<Material>(matData.material);
                    yield return matHandle;

                    if (matHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        productMaterials.Add(matHandle.Result);
                    }
                    else
                    {
                        Debug.LogError($"Failed to load material {matData.material} for {productName}");
                    }
                }
                selectedColor = colors[0];
            }
        }

        // Load Sprites based on JSON field
        public IEnumerator LoadSprites()
        {
            if (!string.IsNullOrEmpty(spritesJson))
            {
                List<string> spritePaths = JsonConvert.DeserializeObject<List<string>>(spritesJson);

                foreach (string spritePath in spritePaths)
                {
                    AsyncOperationHandle<Sprite> spriteHandle = Addressables.LoadAssetAsync<Sprite>(spritePath);
                    yield return spriteHandle;

                    if (spriteHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        productSprites.Add(spriteHandle.Result);
                    }
                    else
                    {
                        Debug.LogError($"Failed to load sprite {spritePath} for {productName}");
                    }
                }
            }
        }

        public IEnumerator FetchTags()
        {
            string url = $"https://virtualhome.hopto.org/gettags/{productID}"; // Construct the URL

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Error fetching tags for product {productID}: {webRequest.error}");
                }
                else
                {
                    string jsonResponse = webRequest.downloadHandler.text;

                    // Deserialize the JSON response into a List<string>
                    try
                    {
                        tags = JsonConvert.DeserializeObject<List<string>>(jsonResponse);
                        Debug.Log($"Tags for product {productID}: {string.Join(", ", tags)}");
                    }
                    catch (JsonReaderException ex)
                    {
                        Debug.LogError($"JSON Parsing Error: {ex.Message}");
                    }
                }
            }
        }

        public IEnumerator FetchDimensions()
        {
            string url = $"https://virtualhome.hopto.org/getdimensions/{productID}"; // Construct the URL

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest(); // Wait for the response

                // Check for errors
                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Error fetching dimensions for product {productID}: {webRequest.error}");
                }
                else
                {
                    string jsonResponse = webRequest.downloadHandler.text;

                    try
                    {
                        var dimensionList = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(jsonResponse);
                        if (dimensionList != null && dimensionList.Count > 0)
                        {
                            productDimensions = new Vector3(
                                float.Parse(dimensionList[0]["x"]),
                                float.Parse(dimensionList[0]["y"]),
                                float.Parse(dimensionList[0]["z"])
                            );
                            Debug.Log($"Dimensions for product {productID}: {productDimensions}");
                        }
                        else
                        {
                            Debug.LogWarning($"No dimensions found for product {productID}.");
                        }
                    }
                    catch (JsonReaderException ex)
                    {
                        Debug.LogError($"JSON Parsing Error: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"An unexpected error occurred: {ex.Message}");
                    }
                }
            }
        }
        public IEnumerator FetchSales()
        {
            string url = $"https://virtualhome.hopto.org/getsales/{productID}"; // Construct the URL

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest(); // Wait for the response

                // Check for errors
                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Error fetching sales for product {productID}: {webRequest.error}");
                }
                else
                {
                    // Get the response as a string
                    string jsonResponse = webRequest.downloadHandler.text;

                    // Deserialize the JSON response directly
                    try
                    {
                        // Using inline JSON deserialization
                        var saleList = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(jsonResponse);
                        if (saleList != null && saleList.Count > 0)
                        {
                            productSale = saleList[0]["salePrice"];
                            isSale = saleList[0]["isSale"] == "1" ? true : false;

                        }
                        else
                        {
                            Debug.LogWarning($"No sales found for product {productID}.");
                        }
                    }
                    catch (JsonReaderException ex)
                    {
                        Debug.LogError($"JSON Parsing Error: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"An unexpected error occurred: {ex.Message}");
                    }
                }
            }
        }
    }
}