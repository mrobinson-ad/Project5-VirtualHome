using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace VirtualHome
{
    public enum CurrentPage
    {
        Sales,
        Search,
        Favorites,
        AR,
        User,
        Cart
    }

    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; } // Singleton

        [SerializeField] private UIDocument currentUIDocument;

        [SerializeField] private List<VisualTreeAsset> uIDocuments;

        [SerializeField] private List<VisualTreeAsset> uITemplates;

        private VisualElement root;

        [SerializeField] private List<Product_SO> productList;

        private CurrentPage currentPage = CurrentPage.Sales;


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
            LoadSales();
        }

        private void LoadSales()
        {
            currentUIDocument.visualTreeAsset = uIDocuments.Find(doc => doc.name == "Sales Page");
            root = currentUIDocument.rootVisualElement;
            ScrollView scrollView = root.Q<ScrollView>("Flash-Scroll");

            foreach (var product in productList)
            {
                // Find and instantiate the template
                VisualTreeAsset template = uITemplates.Find(t => t.name == "Product-Box");

                if (template != null)
                {
                    // Clone the template into a new VisualElement
                    VisualElement newItem = template.CloneTree();

                    // Find and assign values to elements in the template
                    newItem.Q<Label>("Product-Name").text = product.productName;
                    newItem.Q<Label>("Product-Price").text = $"<s>${product.productPrice} </s>";
                    newItem.Q<Label>("Product-Price-Sale").text = $"${product.productSale}";
                    newItem.Q<VisualElement>("Product-Image-Box").style.backgroundImage = new StyleBackground(product.productSprite);
                    newItem.Q<Label>("Product-Short").text = product.productShortDescription;
                    var heart = newItem.Q<VisualElement>("Heart");
                    heart.RegisterCallback<ClickEvent>(evt =>
                    {
                        if (!heart.ClassListContains("heart-filled"))
                        {
                            heart.AddToClassList("heart-filled");
                            FavoriteManager.Instance.favoriteList.Add(product);
                        }
                        else
                        {
                            heart.RemoveFromClassList("heart-filled");
                            FavoriteManager.Instance.favoriteList.Remove(product);
                        }
                    });
                    scrollView.Add(newItem);
                }
            }

        }
    }
}