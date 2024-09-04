using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class FavoriteManager : MonoBehaviour
{
    public static FavoriteManager Instance { get; private set; } // Singleton

    public List<Product_SO> favoriteList;

    public List<Bundle_SO> favoriteBundleList;

    public Dictionary<CartItem, int> cartDict;

    public List<Bundle_SO> cartBundleList;

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
}

public class CartItem
{
    public Product_SO product;
    public string color;

    public CartItem(Product_SO product, string color)
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