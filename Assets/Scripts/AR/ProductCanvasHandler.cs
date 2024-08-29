using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class ProductCanvasHandler : MonoBehaviour
{
    [SerializeField] private Product_SO product;
    [SerializeField] private GameObject infoPanel;

    private void Start()
    {
         TextMeshProUGUI[] Labels = infoPanel.GetComponentsInChildren<TextMeshProUGUI>();

         if (Labels.Length >= 3)
         {
            Labels[0].text = product.productName;
            Labels[1].text = $"Size: {product.productDimension.x} x {product.productDimension.y} x {product.productDimension.z}";
            Labels[2].text = $"Price: ${product.productPrice}";
         }
    }

    public void TogglePanel()
    {
        infoPanel.SetActive(!infoPanel.activeSelf);
    }

    public void DeleteObject()
    {
        Destroy(this.gameObject);
    }
}
