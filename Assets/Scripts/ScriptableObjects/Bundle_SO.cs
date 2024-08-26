using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Bundle", menuName = "ScriptableObjects/Bundle")]
public class Bundle_SO : ScriptableObject
{
    public BundleContainer content;
    public string bundleName;
    public float bundlePrice;
    public string bundleSale;
    public string bundleDescription;
    public float bundlePercent;


    public void CalculatePrice()
    {
        bundlePrice = 0;
        foreach (Product_SO product in content.bundle)
        {
            bundlePrice += float.Parse(product.productPrice);
        }
        bundleSale = (bundlePrice - bundlePrice * bundlePercent).ToString();
    }
}

[Serializable]
public class BundleContainer
{
    public List<Product_SO> bundle;
}
