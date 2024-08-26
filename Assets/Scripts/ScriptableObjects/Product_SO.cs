using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Product", menuName = "ScriptableObjects/Furniture")]
public class Product_SO : ScriptableObject
{
    public string productName;
    public Sprite productSprite;
    public GameObject prefab;
    public string productPrice;
    public string productSale;
    public string productDescription;
    public string productShortDescription;
    public Vector3 productDimension;
}
