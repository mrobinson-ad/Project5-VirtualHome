using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


[CreateAssetMenu(fileName = "Product", menuName = "ScriptableObjects/Furniture")]
public class Product_SO : ScriptableObject
{
    public string productName;
    public List<Sprite> productSprites;
    public GameObject prefab;
    public string productPrice;
    public string productSale;
    public string productDescription;
    public string productShortDescription;
    public bool isSale;
    public List<string> tags;
    public List<string> colors;
    public string selectedColor;
    public List<Material> materials;
    public Vector3 productDimension;
    public string prefabAddress;

    private AsyncOperationHandle<GameObject>? loadedHandle;

    public void LoadPrefab()
    {
        loadedHandle = Addressables.LoadAssetAsync<GameObject>(prefabAddress);
        loadedHandle.Value.Completed += OnPrefabLoaded;

        void OnPrefabLoaded(AsyncOperationHandle<GameObject> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject loadedPrefab = handle.Result;
                prefab = loadedPrefab; // Store the loaded prefab reference
                Debug.Log("Prefab loaded successfully for " + productName);
            }
            else
            {
                Debug.LogError("Failed to load prefab for " + productName);
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
}
