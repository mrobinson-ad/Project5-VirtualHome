using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Linq;


namespace VirtualHome
{
    public enum Category
    {
        Tables,
        Sofas,
        Lamps
    }

    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; } // Singleton

        [SerializeField] private UIDocument currentUIDocument;

        [SerializeField] private List<VisualTreeAsset> uIDocuments;

        [SerializeField] private List<VisualTreeAsset> uITemplates;

        private VisualElement root;

        [SerializeField] private List<Product> productList;

        //[SerializeField] private List<Bundle_SO> bundleList;

        [SerializeField] private ProductLoader productLoader;

        public ContentHandler contentHandler;

        public GameObject mainCamera;


        public int cartAmount = 0;

        private string currentUser;


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
            StartCoroutine(Init());

        }

        private IEnumerator Init()
        {
            productList = productLoader.products;
            yield return new WaitForSeconds(0.5f);
            LoadSales();
        }

        #region PageLoading

        #region LoadSales
        private void LoadSales()
        {
            // Sets the current Page to Sales and the root reference
            currentUIDocument.visualTreeAsset = uIDocuments.Find(doc => doc.name == "Sales Page");
            root = currentUIDocument.rootVisualElement;

            //Instantiates the templates to populate the Flash sales scroll view
            ScrollView flashView = root.Q<ScrollView>("Flash-Scroll");
            foreach (var product in productList)
            {
                if (product.isSale)
                {
                    VisualTreeAsset template = uITemplates.Find(t => t.name == "Product-Box");

                    if (template != null)
                    {
                        SetProductGrid(template, flashView, product);
                    }
                }
            }
            /*
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
            }*/
            SetNavBar();
            root.Q<VisualElement>("Percent").SetEnabled(false);
        }

        #endregion

        #region LoadSearch
        private void LoadSearch()
        {

            // Sets the current Page to Sales and the root reference
            currentUIDocument.visualTreeAsset = uIDocuments.Find(doc => doc.name == "Search Page");
            root = currentUIDocument.rootVisualElement;
            var searchField = root.Q<TextField>("Search-Field");
            var listScroll = root.Q<ScrollView>("List-Scroll");
            LoadCategory(root.Q<VisualElement>("Table-Sprite"), Category.Tables);
            LoadCategory(root.Q<VisualElement>("Sofa-Sprite"), Category.Sofas);
            LoadCategory(root.Q<VisualElement>("Lamp-Sprite"), Category.Lamps);
            searchField.RegisterValueChangedCallback(evt =>
            {
                listScroll.Clear();
                if (searchField.value != "Search ...")
                    LoadResults(searchField.value, listScroll);
            });
            root.Q<VisualElement>("Search-X").RegisterCallback<ClickEvent>(evt =>
            {
                searchField.value = "Search ...";
                root.Q<Label>("Category-Label").style.display = DisplayStyle.None;
                root.Q<ScrollView>("Grid-Scroll").style.display = DisplayStyle.None;
                root.Q<ScrollView>("Category-Scroll").style.display = DisplayStyle.Flex;
                listScroll.style.display = DisplayStyle.None;
            });
            SetNavBar();
            root.Q<VisualElement>("Search").SetEnabled(false);
        }

        private void LoadCategory(VisualElement ve, Category category)
        {
            var gridScroll = root.Q<ScrollView>("Grid-Scroll");
            var categoryLabel = root.Q<Label>("Category-Label");
            var categoryScroll = root.Q<ScrollView>("Category-Scroll");
            VisualTreeAsset template = uITemplates.Find(t => t.name == "Product-Box");
            ve.RegisterCallback<ClickEvent>(evt =>
            {
                gridScroll.Clear();
                categoryScroll.style.display = DisplayStyle.None;
                switch (category)
                {
                    case Category.Tables:
                        var tableList = TagProducts("table");
                        foreach (Product product in tableList)
                        {
                            SetProductGrid(template, gridScroll, product);
                        }
                        categoryLabel.text = "Tables";
                        break;
                    case Category.Sofas:
                        var sofaList = TagProducts("couch");
                        foreach (Product product in sofaList)
                        {
                            SetProductGrid(template, gridScroll, product);
                        }
                        categoryLabel.text = "Sofas";
                        break;
                    case Category.Lamps:
                        var lampList = TagProducts("lamp");
                        foreach (Product product in lampList)
                        {
                            SetProductGrid(template, gridScroll, product);
                        }
                        categoryLabel.text = "Lamps";
                        break;
                }
                gridScroll.style.display = DisplayStyle.Flex;
                categoryLabel.style.display = DisplayStyle.Flex;
            });
        }

        private void LoadResults(string input, ScrollView listView)
        {
            if (listView.style.display != DisplayStyle.Flex)
            {
                var categoryLabel = root.Q<Label>("Category-Label");
                categoryLabel.text = "Results";
                categoryLabel.style.display = DisplayStyle.Flex;
                root.Q<ScrollView>("Grid-Scroll").style.display = DisplayStyle.None;
                root.Q<ScrollView>("Category-Scroll").style.display = DisplayStyle.None;
                listView.style.display = DisplayStyle.Flex;
            }
            var productList = SearchProducts(input);
            VisualTreeAsset template = uITemplates.Find(t => t.name == "Product-Line");
            foreach (var product in productList)
            {

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
                    newItem.Q<VisualElement>("Remove-Product").style.display = DisplayStyle.None;
                    listView.Add(newItem);
                }
            }
        }

        #endregion

        #region LoadUser

        private void LoadUser()
        {

            // Sets the current Page to User and the root reference
            currentUIDocument.visualTreeAsset = uIDocuments.Find(doc => doc.name == "User Page");
            root = currentUIDocument.rootVisualElement;
            var loginBar = root.Q<VisualElement>("Login-Box");
            var userBar = root.Q<VisualElement>("User-Name-Box");
            var shortcuts = root.Q<ScrollView>("Shortcut-View");
            var signBox = root.Q<GroupBox>("Sign-Box");
            var registerBox = root.Q<GroupBox>("Register-Box");
            var error = signBox.Q<Label>("Sign-Error");
            var regError = registerBox.Q<Label>("Different-Label");
            var paymentScroll = root.Q<ScrollView>("Payment-Scroll");
            var paymentBox = root.Q<GroupBox>("Billing-Box");
            var paymentMessage = paymentBox.Q<Label>("Payment-Message");
            var addressBox = root.Q<GroupBox>("Address-Box");
            var addressMessage = addressBox.Q<Label>("Address-Message");
            loginBar.Q<Button>("Sign-In-Button").RegisterCallback<ClickEvent>(evt =>
            {
                signBox.Query<TextField>().ForEach(textField => textField.value = string.Empty);
                error.style.display = DisplayStyle.None;
                signBox.style.display = DisplayStyle.Flex;
                registerBox.style.display = DisplayStyle.None;
                shortcuts.style.display = DisplayStyle.None;
            });
            SetSignIn();
            loginBar.Q<Button>("Register-Button").RegisterCallback<ClickEvent>(evt =>
            {
                registerBox.Query<TextField>().ForEach(textField => textField.value = string.Empty);
                registerBox.style.display = DisplayStyle.Flex;
                signBox.style.display = DisplayStyle.None;
                shortcuts.style.display = DisplayStyle.None;
                regError.style.display = DisplayStyle.None;
            });
            SetRegister();

            userBar.Q<Button>("Logout-Button").RegisterCallback<ClickEvent>(evt =>
            {
                currentUser = null;
                userBar.style.display = DisplayStyle.None;
                loginBar.style.display = DisplayStyle.Flex;
            });

            root.Q<Button>("Billing-Button").RegisterCallback<ClickEvent>(evt =>
            {
                paymentScroll.RemoveFromClassList("shortcut-off");
                paymentBox.Query<TextField>().ForEach(textField => textField.value = string.Empty);
                paymentMessage.style.display = DisplayStyle.None;
                addressBox.Query<TextField>().ForEach(textField => textField.value = string.Empty);
                addressMessage.style.display = DisplayStyle.None;
            });
            paymentScroll.Q<VisualElement>("Return-Arrow").RegisterCallback<ClickEvent>(evt =>
            {
                paymentScroll.AddToClassList("shortcut-off");
            });
            //SetPayment();

            //SetAddress();


            SetNavBar();

            #region SetLoginFields

            void SetSignIn()
            {
                var emailField = signBox.Q<TextField>("Email-Field");
                var passwordField = signBox.Q<TextField>("Password-Field");
                signBox.Q<Button>("Sign-In").RegisterCallback<ClickEvent>(evt =>
                {
                    if (string.IsNullOrEmpty(emailField.value) || string.IsNullOrEmpty(passwordField.value))
                    {
                        error.text = "Make sure both fields are filled";
                        error.style.display = DisplayStyle.Flex;
                        return;
                    }
                    UserManager.Instance.StartLogin(emailField.value, passwordField.value, (result) =>
                            {
                                if (result == "success")
                                {
                                    loginBar.style.display = DisplayStyle.None;
                                    signBox.style.display = DisplayStyle.None;
                                    userBar.style.display = DisplayStyle.Flex;
                                    shortcuts.style.display = DisplayStyle.Flex;
                                    currentUser = UserManager.Instance.currentUser;
                                    userBar.Q<Label>("User-Name-Label").text = currentUser;
                                    return;
                                }
                                else
                                {
                                    error.text = "Email or password is incorrect";
                                    error.style.display = DisplayStyle.Flex;
                                    return;
                                }
                            });
                });
            }

            void SetRegister()
            {
                var firstField = registerBox.Q<TextField>("First-Name-Field");
                var lastField = registerBox.Q<TextField>("Last-Name-Field");
                var userField = registerBox.Q<TextField>("User-Field");
                var emailField = registerBox.Q<TextField>("Email-Field");
                var passwordField = registerBox.Q<TextField>("Password-Field");
                var confirmPasswordField = registerBox.Q<TextField>("Password-Confirm-Field");
                passwordField.RegisterValueChangedCallback(evt =>
                {
                    SetClassList(passwordField.Q<Label>("Characters-Label"), "right-pass", passwordField.value.Length >= 6);
                    SetClassList(passwordField.Q<Label>("Letter-Label"), "right-pass", passwordField.value.Any(char.IsUpper) && passwordField.value.Any(char.IsLower));
                    SetClassList(passwordField.Q<Label>("Number-Label"), "right-pass", passwordField.value.Any(char.IsDigit));
                    if (passwordField.value == confirmPasswordField.value)
                        regError.style.display = DisplayStyle.None;
                    else
                    {
                        regError.style.display = DisplayStyle.Flex;
                        regError.text = "Passwords must match";
                    }
                });

                confirmPasswordField.RegisterValueChangedCallback(evt =>
                {
                    if (passwordField.value == confirmPasswordField.value)
                        regError.style.display = DisplayStyle.None;
                    else
                    {
                        regError.style.display = DisplayStyle.Flex;
                        regError.text = "Passwords must match";
                    }
                });

                registerBox.Q<Button>("Register").RegisterCallback<ClickEvent>(evt =>
                {
                    if (string.IsNullOrEmpty(firstField.value) || string.IsNullOrEmpty(lastField.value) || string.IsNullOrEmpty(userField.value) || string.IsNullOrEmpty(passwordField.value) || string.IsNullOrEmpty(emailField.value) || string.IsNullOrEmpty(confirmPasswordField.value))
                    {
                        regError.style.display = DisplayStyle.Flex;
                        regError.text = "All fields must be filled";
                        return;
                    }
                    if (passwordField.value == confirmPasswordField.value && passwordField.value.Length >= 6 && passwordField.value.Any(char.IsUpper) && passwordField.value.Any(char.IsLower) && passwordField.value.Any(char.IsDigit))
                    {
                        UserManager.Instance.StartRegisterUser(firstField.value, lastField.value, userField.value, emailField.value, passwordField.value, (result) =>
                            {
                                if (result == "success")
                                {
                                    signBox.Query<TextField>().ForEach(textField => textField.value = string.Empty);
                                    error.style.display = DisplayStyle.None;
                                    signBox.style.display = DisplayStyle.Flex;
                                    registerBox.style.display = DisplayStyle.None;
                                }

                                else if (result == "email")
                                {
                                    regError.style.display = DisplayStyle.Flex;
                                    regError.text = "Email is already registered";
                                }
                                else if (result == "connection")
                                {
                                    regError.style.display = DisplayStyle.Flex;
                                    regError.text = "Connection error";
                                }
                                else
                                {
                                    regError.style.display = DisplayStyle.Flex;
                                    regError.text = "Incorrect information";
                                    return;
                                }
                            });
                    }
                });
            }
            /*
            void SetPayment()
            {
                root.Q<Button>("Save-Payment").RegisterCallback<ClickEvent>(evt =>
                {
                    string firstName = paymentBox.Q<TextField>("First-Name-Field").value;
                    string lastName = paymentBox.Q<TextField>("Last-Name-Field").value;
                    string card = paymentBox.Q<TextField>("Card-Field").value;
                    string cvv = paymentBox.Q<TextField>("CVV-Field").value;
                    string date = paymentBox.Q<DropdownField>("Expiry-Month").value + "/" + paymentBox.Q<DropdownField>("Expiry-Year").value;
                    string type = paymentBox.Q<DropdownField>("Card-Dropdown").value;
                    if (IsAnyNullOrEmpty(firstName, lastName, card, cvv, date, type))
                    {
                        SetClassList(paymentMessage, "right-pass", false);
                        paymentMessage.style.display = DisplayStyle.Flex;
                        paymentMessage.text = "Please fill all the fields";
                        return;
                    }
                    UserPayment userPayment = new UserPayment(firstName, lastName, card, cvv, date, type);
                    UserManager.Instance.AddPayment(currentUser, userPayment);
                    SetClassList(paymentMessage, "right-pass", true);
                    paymentMessage.style.display = DisplayStyle.Flex;
                    paymentMessage.text = "Payment method has been added";
                });
            }
            void SetAddress()
            {

                root.Q<Button>("Save-Address").RegisterCallback<ClickEvent>(evt =>
                {
                    string firstName = addressBox.Q<TextField>("First-Name-Field").value;
                    string lastName = addressBox.Q<TextField>("Last-Name-Field").value;
                    string address = addressBox.Q<TextField>("Address-Field").value;
                    string city = addressBox.Q<TextField>("City-Field").value;
                    string state = addressBox.Q<TextField>("State-Field").value;
                    string postal = addressBox.Q<TextField>("Postal-Field").value;
                    if (IsAnyNullOrEmpty(firstName, lastName, address, city, state, postal))
                    {
                        SetClassList(addressMessage, "right-pass", false);
                        addressMessage.style.display = DisplayStyle.Flex;
                        addressMessage.text = "Please fill all the fields";
                    }
                    UserAddress userAddress = new UserAddress(firstName, lastName, address, city, state, postal);
                    UserManager.Instance.AddAddress(currentUser, userAddress);
                    SetClassList(addressMessage, "right-pass", true);
                    addressMessage.style.display = DisplayStyle.Flex;
                    addressMessage.text = "Address has been added";
                });
            } */
            #endregion
        }

        #endregion

        #region LoadCart

        private void LoadCart()
        {
            // Sets the current Page to Favorites and the root reference
            currentUIDocument.visualTreeAsset = uIDocuments.Find(doc => doc.name == "Cart Page");
            root = currentUIDocument.rootVisualElement;

            SetCartList();

            SetNavBar();
            root.Q<VisualElement>("Cart").SetEnabled(false);
        }

        private void SetCartList()
        {
            ScrollView listView = root.Q<ScrollView>("List-Scroll");
            listView.Clear();
            float totalPrice = 0;
            if (FavoriteManager.Instance.cartDict != null)
                foreach (var item in FavoriteManager.Instance.cartDict)
                {
                    VisualTreeAsset template = uITemplates.Find(t => t.name == "Product-Line-Cart");
                    CartItem cartItem = item.Key;
                    Product product = cartItem.product;
                    if (template != null)
                    {
                        VisualElement newItem = template.CloneTree();

                        newItem.Q<Label>("Product-Name").text = product.productName;

                        if (product.isSale)
                        {
                            totalPrice += float.Parse(product.productSale) * item.Value;
                            newItem.Q<Label>("Product-Price").text = $"${product.productSale}";
                        }
                        else
                        {
                            totalPrice += float.Parse(product.productPrice) * item.Value;
                            newItem.Q<Label>("Product-Price").text = $"${product.productPrice}";
                        }

                        newItem.Q<VisualElement>("Product-Image").style.backgroundImage = new StyleBackground(product.productSprites[product.colors.IndexOf(product.selectedColor)]);
                        newItem.Q<Label>("Product-Amount").text = "x" + item.Value;
                        newItem.Q<Label>("Product-Color").text = item.Key.color;

                        newItem.Q<VisualElement>("Remove-Product").RegisterCallback<ClickEvent>(evt =>
                        {
                            FavoriteManager.Instance.cartDict[cartItem]--;
                            cartAmount--;
                            root.Q<Label>("Cart-Amount").text = cartAmount.ToString();
                            if (product.isSale)
                            {
                                totalPrice -= float.Parse(product.productSale);
                                root.Q<Label>("Total-Label").text = $"Total: ${totalPrice}";
                            }
                            else
                            {
                                totalPrice -= float.Parse(product.productPrice);
                                root.Q<Label>("Total-Label").text = $"Total: ${totalPrice}";
                            }
                            if (FavoriteManager.Instance.cartDict[cartItem] <= 0)
                            {
                                FavoriteManager.Instance.cartDict.Remove(cartItem);
                                listView.Remove(newItem);
                                return;
                            }

                            newItem.Q<Label>("Product-Amount").text = "x" + FavoriteManager.Instance.cartDict[cartItem];
                        });
                        listView.Add(newItem);
                    }
                }
            EventCallback<ClickEvent> checkoutClick = evt =>
            {
                if (FavoriteManager.Instance.cartDict == null || FavoriteManager.Instance.cartDict.Count == 0)
                    root.Q<Label>("Empty-Label").style.display = DisplayStyle.Flex;
                else
                    SetCheckoutList();
            };
            root.Q<Button>("Checkout-Button").RegisterCallback(checkoutClick);
            root.Q<Label>("Total-Label").text = $"Total: ${totalPrice}";
        }

        private void SetCheckoutList()
        {
            root.Q<VisualElement>("Cart-View").style.display = DisplayStyle.None;
            root.Q<ScrollView>("Checkout-View").style.display = DisplayStyle.Flex;
            /*var paymentDrop = root.Q<DropdownField>("Payment-Dropdown");
            if (currentUser != null)
            {
                var paymentList =;
                if (paymentList != null)
                {
                    paymentDrop.choices.Clear();
                    foreach (var payment in paymentList)
                    {
                        paymentDrop.choices.Add(payment);
                    }
                }
            }
            var addressDrop = root.Q<DropdownField>("Shipping-Dropdown");
            if (currentUser != null)
            {
                var addressList = UserManager.Instance.GetAddress(currentUser);
                if (addressList != null)
                {
                    addressDrop.choices.Clear();
                    foreach (var address in addressList)
                    {
                        addressDrop.choices.Add(address);
                    }
                }
            }*/
            GroupBox checkView = root.Q<GroupBox>("Price-List");
            checkView.Clear();
            float totalPrice = 0;
            VisualTreeAsset template = uITemplates.Find(t => t.name == "Priced-Line");
            foreach (var item in FavoriteManager.Instance.cartDict)
            {
                CartItem cartItem = item.Key;
                Product product = cartItem.product;
                if (template != null)
                {
                    VisualElement newItem = template.CloneTree();

                    newItem.Q<Label>("Product-Name").text = product.productName;

                    if (product.isSale)
                    {
                        totalPrice += float.Parse(product.productSale) * item.Value;
                        newItem.Q<Label>("Product-Price").text = $"x{item.Value} ${product.productSale}";
                        newItem.Q<Label>("Product-Total").text = $"${float.Parse(product.productSale) * item.Value}";
                    }
                    else
                    {
                        totalPrice += float.Parse(product.productPrice) * item.Value;
                        newItem.Q<Label>("Product-Price").text = $"x{item.Value} ${product.productPrice}";
                        newItem.Q<Label>("Product-Total").text = $"${float.Parse(product.productPrice) * item.Value}";
                    }

                    newItem.Q<Label>("Product-Color").text = item.Key.color;

                    checkView.Add(newItem);
                }
            }
            VisualElement taxItem = template.CloneTree();
            taxItem.Q<Label>("Product-Name").text = "TVA 10%";
            taxItem.Q<Label>("Product-Color").text = "";
            taxItem.Q<Label>("Product-Price").text = "";
            taxItem.Q<Label>("Product-Total").text = $"${(totalPrice * 0.1):0.00}";
            checkView.Add(taxItem);
            Toggle expressShipping = root.Q<Toggle>("Express-Toggle");
            VisualElement shippingItem = null;
            EventCallback<ChangeEvent<bool>> shippingChange = evt =>
            {

                if (expressShipping.value == true)
                {
                    if (shippingItem == null)
                    {
                        shippingItem = template.CloneTree();
                        shippingItem.Q<Label>("Product-Name").text = "Express shipping";
                        shippingItem.Q<Label>("Product-Color").text = "";
                        shippingItem.Q<Label>("Product-Price").text = "";
                        shippingItem.Q<Label>("Product-Total").text = "$14";
                    }
                    totalPrice += 14;
                    taxItem.Q<Label>("Product-Total").text = $"${(totalPrice * 0.1):0.00}";
                    root.Q<Label>("Billing-Label").text = $"Total: ${(totalPrice * 1.1):0.00}";
                    int insertIndex = Mathf.Max(0, checkView.childCount - 1);
                    checkView.Insert(insertIndex, shippingItem);
                }
                else
                {
                    totalPrice -= 14;
                    taxItem.Q<Label>("Product-Total").text = $"${(totalPrice * 0.1):0.00}";
                    root.Q<Label>("Billing-Label").text = $"Total: ${(totalPrice * 1.1):0.00}";
                    checkView.Remove(shippingItem);
                }
            };
            expressShipping.RegisterCallback(shippingChange);
            var promoLabel = root.Q<Label>("Promo-Label");
            var redeemButton = root.Q<Button>("Redeem-Button");
            EventCallback<ClickEvent> redeemClicked = evt =>
            {
                foreach (var promo in FavoriteManager.Instance.promos)
                {
                    if (promo.code == root.Q<TextField>("Promo-Field").value)
                    {
                        VisualElement promoItem = template.CloneTree();
                        if (promo.type == PromoType.Shipping)
                        {
                            promoItem.Q<Label>("Product-Name").text = "Shipping Promo";
                            promoItem.Q<Label>("Product-Color").text = "";
                            promoItem.Q<Label>("Product-Price").text = "";
                            promoItem.Q<Label>("Product-Total").text = "<color=green>$-14 </color>";
                            totalPrice -= 14;
                            taxItem.Q<Label>("Product-Total").text = $"${(totalPrice * 0.1):0.00}";
                            root.Q<Label>("Billing-Label").text = $"Total: ${(totalPrice * 1.1):0.00}";
                        }
                        if (promo.type == PromoType.Flat)
                        {
                            promoItem.Q<Label>("Product-Name").text = "Promo";
                            promoItem.Q<Label>("Product-Color").text = "";
                            promoItem.Q<Label>("Product-Price").text = "";
                            promoItem.Q<Label>("Product-Total").text = $"<color=green>$-{promo.amount} </color>";
                            totalPrice -= promo.amount;
                            taxItem.Q<Label>("Product-Total").text = $"${(totalPrice * 0.1):0.00}";
                            root.Q<Label>("Billing-Label").text = $"Total: ${(totalPrice * 1.1):0.00}";
                        }
                        if (promo.type == PromoType.Percent)
                        {
                            promoItem.Q<Label>("Product-Name").text = "Promo";
                            promoItem.Q<Label>("Product-Color").text = "";
                            promoItem.Q<Label>("Product-Price").text = $"-{promo.amount}%";
                            promoItem.Q<Label>("Product-Total").text = $"<color=green>$-{(promo.amount * totalPrice / 100):0.00} </color>";
                            totalPrice -= promo.amount * totalPrice / 100;
                            taxItem.Q<Label>("Product-Total").text = $"${(totalPrice * 0.1):0.00}";
                            root.Q<Label>("Billing-Label").text = $"Total: ${(totalPrice * 1.1):0.00}";
                        }
                        int insertIndex = Mathf.Max(0, checkView.childCount - 1);
                        checkView.Insert(insertIndex, promoItem);
                        promoLabel.text = promo.message;
                        promoLabel.style.display = DisplayStyle.Flex;
                        SetClassList(promoLabel, "right-pass", true);
                        redeemButton.SetEnabled(false);
                        return;
                    }
                    else
                    {
                        promoLabel.text = root.Q<TextField>("Promo-Field").value + " is not a valid code";
                        promoLabel.style.display = DisplayStyle.Flex;
                        SetClassList(promoLabel, "right-pass", false);
                    }
                }
            };
            redeemButton.RegisterCallback(redeemClicked);
            root.Q<Label>("Billing-Label").text = $"Total: ${(totalPrice * 1.1):0.00}";
        }

        #endregion

        #region LoadFavorites

        private void LoadFavorites()
        {
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
            VisualTreeAsset template = uITemplates.Find(t => t.name == "Product-Line");
            foreach (var product in FavoriteManager.Instance.favoriteList)
            {

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
                        product.ReleasePrefab();
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
                        product.ReleasePrefab();
                    });
                    gridView.Add(newItem);
                }
            }
        }

        #endregion

        #region LoadAR

        private void LoadAR()
        {
            // Sets the current Page to AR and the root reference
            currentUIDocument.visualTreeAsset = uIDocuments.Find(doc => doc.name == "AR Page");
            root = currentUIDocument.rootVisualElement;
            mainCamera.SetActive(false);
            SceneManager.LoadScene(1, LoadSceneMode.Additive);
            StartCoroutine(WaitLoad());

            ScrollView scrollView = root.Q<ScrollView>("Model-View");
            VisualTreeAsset template = uITemplates.Find(t => t.name == "Model-Button");
            foreach (var product in productList)
            {
                if (product.prefab != null)
                    if (template != null)
                    {
                        VisualElement newItem = template.CloneTree();

                        var model = newItem.Q<VisualElement>("Model-Button");

                        model.style.backgroundImage = new StyleBackground(product.productSprites[0]);
                        model.RegisterCallback<ClickEvent>(evt =>
                        {
                            contentHandler.ChangeObject(product);
                        });
                        scrollView.Add(newItem);
                    }
            }
            bool isDropped = false;

            root.Q<Button>("Dropdown-Button").RegisterCallback<ClickEvent>(evt =>
            {
                var chevron = root.Q<VisualElement>("Chevron-Arrow");
                if (!isDropped)
                {
                    chevron.style.rotate = new Rotate(-180);
                    scrollView.style.display = DisplayStyle.Flex;
                    isDropped = true;
                }
                else
                {
                    chevron.style.rotate = new Rotate(0);
                    scrollView.style.display = DisplayStyle.None;
                    isDropped = false;
                }
            });

            root.Q<VisualElement>("Return-Arrow").RegisterCallback<ClickEvent>(evt =>
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName("Main"));
                SceneManager.UnloadSceneAsync(1);
                LoadSales();
                mainCamera.SetActive(true);
            });
        }

        private IEnumerator WaitLoad()
        {
            yield return new WaitForSeconds(1f);
            contentHandler = GameObject.Find("Plane Finder").GetComponent<ContentHandler>();
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("AR"));
        }

        #endregion

        private void SetNavBar()
        {
            root.Q<Label>("Cart-Amount").text = cartAmount.ToString();
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

        #region SetProductPage
        private void SetProductPage(Product product, VisualElement templateHeart)
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

            VisualElement cartButton = page.Q<VisualElement>("Cart-Button");

            EventCallback<ClickEvent> cartClick = evt =>
            {
                if (product.prefab == null)
                {
                    product.LoadPrefab();
                }
                CartItem cartItem = new CartItem(product, product.selectedColor);
                var cartDict = FavoriteManager.Instance.cartDict;
                if (cartDict == null)
                {
                    FavoriteManager.Instance.cartDict = new Dictionary<CartItem, int>();
                    cartDict = FavoriteManager.Instance.cartDict;
                }
                if (!cartDict.TryAdd(cartItem, 1))
                {
                    // Increment the count if the item was already present
                    cartDict[cartItem]++;
                }
                cartAmount++;
                root.Q<Label>("Cart-Amount").text = cartAmount.ToString();
            };

            cartButton.RegisterCallback(cartClick);

            VisualElement heart = page.Q<VisualElement>("Heart");

            EventCallback<ClickEvent> heartClick = evt =>
                {
                    if (!heart.ClassListContains("heart-filled"))
                    {
                        heart.AddToClassList("heart-filled");
                        templateHeart.AddToClassList("heart-filled");
                        FavoriteManager.Instance.favoriteList.Add(product);
                        if (product.prefab == null)
                        {
                            product.LoadPrefab();
                        }
                    }
                    else
                    {
                        heart.RemoveFromClassList("heart-filled");
                        templateHeart.RemoveFromClassList("heart-filled");
                        FavoriteManager.Instance.favoriteList.Remove(product);
                        product.ReleasePrefab();
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
                cartButton.UnregisterCallback(cartClick);
                colorDropdown.UnregisterCallback(colorChange);
                if (heart != null)
                    heart.UnregisterCallback(heartClick);
                page.AddToClassList("product-page-off");
            };

            returnArrow.RegisterCallback(returnClick);

            page.RemoveFromClassList("product-page-off");
        }

        private void SetProductGrid(VisualTreeAsset template, ScrollView scrollView, Product product)
        {
            VisualElement newItem = template.CloneTree();

            newItem.Q<Label>("Product-Name").text = product.productName;
            if (product.isSale)
            {
                newItem.Q<Label>("Product-Price").text = $"<s>${product.productPrice} </s>";
                newItem.Q<Label>("Product-Price-Sale").text = $"${product.productSale}";
            }
            else
            {
                newItem.Q<Label>("Product-Price").text = "";
                newItem.Q<Label>("Product-Price-Sale").text = $"${product.productPrice}";
            }
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
                    if (product.prefab == null)
                    {
                        product.LoadPrefab();
                    }
                }
                else
                {
                    heart.RemoveFromClassList("heart-filled");
                    FavoriteManager.Instance.favoriteList.Remove(product);
                    product.ReleasePrefab();
                }
            });
            scrollView.Add(newItem);
        }

        #endregion
        public List<Product> SearchProducts(string searchString)
        {
            searchString = searchString.ToLower();


            var matchingProducts = productList.Where(product =>
                product.productName.ToLower().Contains(searchString) ||
                product.tags.Any(tag => tag.ToLower().Contains(searchString)) ||
                product.productDescription.ToLower().Contains(searchString) ||
                product.productShortDescription.ToLower().Contains(searchString)
            ).ToList();

            return matchingProducts;
        }

        public List<Product> TagProducts(string tagSearch)
        {
            tagSearch = tagSearch.ToLower();

            var matchingProducts = productList.Where(product =>
            product.tags.Any(tag => tag.ToLower().Contains(tagSearch))
            ).ToList();

            return matchingProducts;
        }

        private void SetClassList(Label label, string className, bool condition)
        {
            if (condition)
            {
                if (!label.ClassListContains(className))
                    label.AddToClassList(className);
            }
            else
            {
                if (label.ClassListContains(className))
                    label.RemoveFromClassList(className);
            }
        }
        bool IsAnyNullOrEmpty(params string[] values)
        {
            foreach (string value in values)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return true;
                }
            }
            return false;
        }
    }

}
