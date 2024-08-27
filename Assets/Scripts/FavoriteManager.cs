using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FavoriteManager : MonoBehaviour
{
    public static FavoriteManager Instance { get; private set; } // Singleton

    public List<Product_SO> favoriteList;

    public List<Bundle_SO> favoriteBundleList;

    public List<Product_SO> cartList;

    public List<Bundle_SO> cartBundleList;
    
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
}