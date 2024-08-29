using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FavoriteManager : MonoBehaviour
{
    public static FavoriteManager Instance { get; private set; } // Singleton

    public List<Product_SO> favoriteList;

    public List<Bundle_SO> favoriteBundleList;

    public Dictionary<CartItem, int> cartDict;

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