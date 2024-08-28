using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
}
