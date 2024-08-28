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
                if (product.isSale)
                {
                    VisualTreeAsset template = uITemplates.Find(t => t.name == "Product-Box");

                    if (template != null)
                    {

                        VisualElement newItem = template.CloneTree();

                        newItem.Q<Label>("Product-Name").text = product.productName;
                        newItem.Q<Label>("Product-Price").text = $"<s>${product.productPrice} </s>";
                        newItem.Q<Label>("Product-Price-Sale").text = $"${product.productSale}";
                        newItem.Q<VisualElement>("Product-Image-Box").style.backgroundImage = new StyleBackground(product.productSprites[0]);
                        newItem.Q<Label>("Product-Short").text = product.productShortDescription;
                        var heart = newItem.Q<VisualElement>("Heart");
                        newItem.Q<VisualElement>("Product-Image-Box").RegisterCallback<ClickEvent>(evt =>
                        {
                            SetProductPage(product, heart);
                        });

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
                        newItem.Q<VisualElement>("Product-Image-" + i).style.backgroundImage = new StyleBackground(bundle.content.bundle[i].productSprites[0]);
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

        #region LoadSearch
        private void LoadSearch()
        {
            currentPage = CurrentPage.Search;
            // Sets the current Page to Sales and the root reference
            currentUIDocument.visualTreeAsset = uIDocuments.Find(doc => doc.name == "Search Page");
            root = currentUIDocument.rootVisualElement;


            SetNavBar();
            root.Q<VisualElement>("Search").SetEnabled(false);
        }

        #endregion

        #region LoadUser

        private void LoadUser()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region LoadCart

        private void LoadCart()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region LoadFavorites
        private void LoadFavorites()
        {
            currentPage = CurrentPage.Favorites;
            // Sets the current Page to Favorites and the root reference
            currentUIDocument.visualTreeAsset = uIDocuments.Find(doc => doc.name == "Favorites Page");
            root = currentUIDocument.rootVisualElement;

            SetListView();

            SetNavBar();
            var star = root.Q<VisualElement>("Star");
            star.pickingMode = PickingMode.Ignore;
            star.AddToClassList("star-filled");

            var gridView = root.Q<VisualElement>("Grid-View");
            var listView = root.Q<VisualElement>("List-View");

            gridView.RegisterCallback<ClickEvent>(evt =>
            {
                SetGridView();
                listView.SetEnabled(true);
            });

            listView.RegisterCallback<ClickEvent>(evt =>
            {
                SetListView();
                gridView.SetEnabled(true);
            });

        }

        private void SetListView()
        {
            ScrollView listView = root.Q<ScrollView>("List-Scroll");
            listView.Clear();
            listView.style.display = DisplayStyle.Flex;
            root.Q<VisualElement>("Grid-Scroll").style.display = DisplayStyle.None;
            root.Q<VisualElement>("List-View").SetEnabled(false);

            foreach (var product in FavoriteManager.Instance.favoriteList)
            {
                VisualTreeAsset template = uITemplates.Find(t => t.name == "Product-Line");

                if (template != null)
                {
                    VisualElement newItem = template.CloneTree();

                    newItem.Q<Label>("Product-Name").text = product.productName;

                    if (product.isSale)
                        newItem.Q<Label>("Product-Price").text = $"${product.productSale}";
                    else
                        newItem.Q<Label>("Product-Price").text = $"${product.productPrice}";

                    newItem.Q<VisualElement>("Product-Image").style.backgroundImage = new StyleBackground(product.productSprites[0]);
                    newItem.Q<VisualElement>("Product-Image").RegisterCallback<ClickEvent>(evt =>
                    {
                        SetProductPage(product, newItem);
                    });

                    newItem.Q<Label>("Product-Short").text = product.productShortDescription;
                    newItem.Q<VisualElement>("Remove-Product").RegisterCallback<ClickEvent>(evt =>
                    {
                        FavoriteManager.Instance.favoriteList.Remove(product);
                        listView.Remove(newItem);
                    });
                    listView.Add(newItem);
                }
            }
        }

        private void SetGridView()
        {
            ScrollView gridView = root.Q<ScrollView>("Grid-Scroll");
            gridView.Clear();
            gridView.style.display = DisplayStyle.Flex;
            root.Q<VisualElement>("List-Scroll").style.display = DisplayStyle.None;
            root.Q<VisualElement>("Grid-View").SetEnabled(false);


            foreach (var product in FavoriteManager.Instance.favoriteList)
            {
                VisualTreeAsset template = uITemplates.Find(t => t.name == "Product-Grid");

                if (template != null)
                {
                    VisualElement newItem = template.CloneTree();

                    newItem.Q<Label>("Product-Name").text = product.productName;

                    if (product.isSale)
                        newItem.Q<Label>("Product-Price").text = $"${product.productSale}";
                    else
                        newItem.Q<Label>("Product-Price").text = $"${product.productPrice}";

                    newItem.Q<VisualElement>("Product-Image").style.backgroundImage = new StyleBackground(product.productSprites[0]);
                    newItem.Q<VisualElement>("Product-Image").RegisterCallback<ClickEvent>(evt =>
                    {
                        SetProductPage(product, newItem);
                    });

                    newItem.Q<Label>("Product-Short").text = product.productShortDescription;
                    newItem.Q<VisualElement>("Remove-Product").RegisterCallback<ClickEvent>(evt =>
                    {
                        FavoriteManager.Instance.favoriteList.Remove(product);
                        gridView.Remove(newItem);
                    });
                    gridView.Add(newItem);
                }
            }
        }

        #endregion

        #region LoadAR

        private void LoadAR()
        {
            throw new NotImplementedException();
        }

        #endregion

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

        private void SetProductPage(Product_SO product, VisualElement templateHeart)
        {
            Debug.Log(product.productName);
            GroupBox page = root.Q<GroupBox>("Product-Page");
            VisualTreeAsset template = uITemplates.Find(t => t.name == "Product-Tag");
            page.Q<VisualElement>("Tag-Box").Clear();
            if (template != null)
            {
                foreach (var tag in product.tags)
                {
                    VisualElement newItem = template.CloneTree();

                    newItem.Q<Label>("Product-Tag").text = tag;
                    page.Q<VisualElement>("Tag-Box").Add(newItem);
                }
            }
            page.Q<Label>("Product-Name").text = product.productName;
            page.Q<Label>("Product-Price").text = $"${product.productPrice}";
            if (product.isSale)
            {
                page.Q<Label>("Product-Price-Sale").text = $"<s>${product.productPrice} </s>";
                page.Q<Label>("Product-Price-Sale").style.visibility = Visibility.Visible;
                page.Q<Label>("Product-Price").text = $"${product.productSale}";
            }


            page.Q<Label>("Product-Description").text = product.productShortDescription;

            var colorDropdown = page.Q<DropdownField>("Color-Dropdown");
            colorDropdown.choices.Clear();
            colorDropdown.choices.AddRange(product.colors);
            if (product.selectedColor == "")
                product.selectedColor = product.colors[0];
            colorDropdown.value = product.selectedColor;
            page.Q<VisualElement>("Product-Image").style.backgroundImage = new StyleBackground(product.productSprites[product.colors.IndexOf(product.selectedColor)]);

            EventCallback<ChangeEvent<string>> colorChange = evt =>
            {
                product.selectedColor = colorDropdown.value;
                page.Q<VisualElement>("Product-Image").style.backgroundImage = new StyleBackground(product.productSprites[product.colors.IndexOf(product.selectedColor)]);
            };

            colorDropdown.RegisterCallback(colorChange);



            VisualElement heart = page.Q<VisualElement>("Heart");

            EventCallback<ClickEvent> heartClick = evt =>
                {
                    if (!heart.ClassListContains("heart-filled"))
                    {
                        heart.AddToClassList("heart-filled");
                        templateHeart.AddToClassList("heart-filled");
                        FavoriteManager.Instance.favoriteList.Add(product);
                    }
                    else
                    {
                        heart.RemoveFromClassList("heart-filled");
                        templateHeart.RemoveFromClassList("heart-filled");
                        FavoriteManager.Instance.favoriteList.Remove(product);
                    }
                };

            if (heart != null)
            {
                if (FavoriteManager.Instance.favoriteList.Contains(product))
                    heart.AddToClassList("heart-filled");
                heart.RegisterCallback(heartClick);
            }
            var returnArrow = page.Q<VisualElement>("Return-Arrow");

            EventCallback<ClickEvent> returnClick = evt =>
            {
                colorDropdown.UnregisterCallback(colorChange);
                if (heart != null)
                    heart.UnregisterCallback(heartClick);
                page.AddToClassList("product-page-off");
            };

            returnArrow.RegisterCallback(returnClick);

            page.RemoveFromClassList("product-page-off");
        }
    }
    #endregion

}
