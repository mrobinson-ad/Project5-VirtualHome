using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;

public class ProductCanvasHandler : MonoBehaviour
{
    [SerializeField] private Product_SO product;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private GameObject buttonColor;
    [SerializeField] private Transform colorPanel;
    [SerializeField] private MeshRenderer meshRenderer;

    private void Start()
    {
        Debug.Log("ProductCanvasHandler Start method called.");
        Debug.Log(product.productName);
        TextMeshProUGUI[] Labels = infoPanel.GetComponentsInChildren<TextMeshProUGUI>();

        if (Labels.Length >= 2)
        {
            Labels[0].text = product.productName;
            Labels[1].text = $"Size: {product.productDimension.x} x {product.productDimension.y} x {product.productDimension.z}";
        }
        if (product.colors.Count > 1)
            for (int i = 0; i < product.colors.Count; i++)
            {
                GameObject newButton = Instantiate(buttonColor, new Vector2(colorPanel.position.x, colorPanel.position.y + i * 20), Quaternion.identity);
                newButton.transform.SetParent(colorPanel, false);
                newButton.name ="colorButton_"+ product.colors[i];
                newButton.GetComponentInChildren<TextMeshProUGUI>().text = product.colors[i];
                newButton.GetComponent<Button>().onClick.AddListener(() => ChangeMaterial(newButton.transform.GetSiblingIndex()));
            }
    }

    public void ChangeMaterial(int color)
    {
        meshRenderer.material = product.materials[color];
    }
    public void TogglePanel()
    {
        infoPanel.SetActive(!infoPanel.activeSelf);
    }

    public void DeleteObject()
    {
        Destroy(transform.parent.gameObject);
    }


}
