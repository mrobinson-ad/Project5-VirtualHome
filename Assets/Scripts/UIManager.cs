using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System;

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

        [SerializeField] private List<Bundle_SO> bundleList;

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
        }

        private void Start()
        {
            LoadSales();
        }

        #region PageLoading

        #region LoadSales
        private void LoadSales()
        {
            currentPage = CurrentPage.Sales;
            // Sets the current Page to Sales and the root reference
            currentUIDocument.visualTreeAsset = uIDocuments.Find(doc => doc.name == "Sales Page");
            root = currentUIDocument.rootVisualElement;

            //Instantiates the templates to populate the Flash sales scroll view
            ScrollView FlashView = root.Q<ScrollView>("Flash-Scroll");
            foreach (var product in productList)
            {
                VisualTreeAsset template = uITemplates.Find(t => t.name == "Product-Box");

                if (template != null)
                {

                    VisualElement newItem = template.CloneTree();

                    newItem.Q<Label>("Product-Name").text = product.productName;
                    newItem.Q<Label>("Product-Price").text = $"<s>${product.productPrice} </s>";
                    newItem.Q<Label>("Product-Price-Sale").text = $"${product.productSale}";
                    newItem.Q<VisualElement>("Product-Image-Box").style.backgroundImage = new StyleBackground(product.productSprite);
                    newItem.Q<Label>("Product-Short").text = product.productShortDescription;
                    var heart = newItem.Q<VisualElement>("Heart");

                    if (FavoriteManager.Instance.favoriteList.Contains(product))
                    heart.AddToClassList("heart-filled");

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
                    FlashView.Add(newItem);
                }
            }
            //Instantiates the templates to populate the Bundle sales scroll view
            ScrollView BundleView = root.Q<ScrollView>("Bundle-Scroll");
            foreach (var bundle in bundleList)
            {
                VisualTreeAsset template = uITemplates.Find(t => t.name == "Bundle-Box");
                bundle.CalculatePrice();
                if (template != null)
                {
                    VisualElement newItem = template.CloneTree();

                    newItem.Q<Label>("Product-Name").text = bundle.bundleName;
                    newItem.Q<Label>("Product-Price").text = $"<s>${bundle.bundlePrice} </s>";
                    newItem.Q<Label>("Product-Price-Sale").text = $"${bundle.bundleSale}";
                    newItem.Q<Label>("Product-Short").text = bundle.bundleDescription;

                    for (int i = 0; i < 3; i++)
                    {
                        newItem.Q<VisualElement>("Product-Image-" + i).style.backgroundImage = new StyleBackground(bundle.content.bundle[i].productSprite);
                    }
                    
                    var heart = newItem.Q<VisualElement>("Heart");

                    if (FavoriteManager.Instance.favoriteBundleList.Contains(bundle))
                    heart.AddToClassList("heart-filled");

                    heart.RegisterCallback<ClickEvent>(evt =>
                    {
                        if (!heart.ClassListContains("heart-filled"))
                        {
                            heart.AddToClassList("heart-filled");
                            FavoriteManager.Instance.favoriteBundleList.Add(bundle);
                        }
                        else
                        {
                            heart.RemoveFromClassList("heart-filled");
                            FavoriteManager.Instance.favoriteBundleList.Remove(bundle);
                        }
                    });
                    BundleView.Add(newItem);    
                }
            }
            SetNavBar();
            root.Q<VisualElement>("Percent").SetEnabled(false);
        }

        #endregion

        private void LoadSearch()
        {
            currentPage = CurrentPage.Search;
            // Sets the current Page to Sales and the root reference
            currentUIDocument.visualTreeAsset = uIDocuments.Find(doc => doc.name == "Search Page");
            root = currentUIDocument.rootVisualElement;

            SetNavBar();
            root.Q<VisualElement>("Search").SetEnabled(false);
        }

        private void LoadUser()
        {
            throw new NotImplementedException();
        }

        private void LoadCart()
        {
            throw new NotImplementedException();
        }

        private void LoadFavorites()
        {
            throw new NotImplementedException();
        }

        private void LoadAR()
        {
            throw new NotImplementedException();
        }

        private void SetNavBar()
        {
            root.Q<VisualElement>("Percent").RegisterCallback<ClickEvent>(evt =>
            {
                LoadSales();
            });

            root.Q<VisualElement>("Search").RegisterCallback<ClickEvent>(evt =>
            {
                LoadSearch();
            });

            root.Q<VisualElement>("User").RegisterCallback<ClickEvent>(evt =>
            {
                LoadUser();
            });

            root.Q<VisualElement>("Cart").RegisterCallback<ClickEvent>(evt =>
            {
                LoadCart();
            });

            root.Q<VisualElement>("Star").RegisterCallback<ClickEvent>(evt =>
            {
                LoadFavorites();
            });

            root.Q<VisualElement>("Camera").RegisterCallback<ClickEvent>(evt =>
            {
                LoadAR();
            });
        }
        #endregion

    }
}