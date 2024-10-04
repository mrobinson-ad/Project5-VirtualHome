using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace VirtualHome
{
    public class PayPalManager : MonoBehaviour
    {
        // Paypal API credentials
        string clientID = "AY31GuGWlSsmQT6prt04rnlm0uYt-Zk4TBJrAlz56IqPA3BqjE9vMLFevnDKyXw6rP2ypxNU9a9KMwsv";
        string secret = "EFVSuSYcD0Xnu4qDLcFBMKk6NMiCuGTScb7BgXWwYYQ7272z_0yTbWUGQto972iRYNc2T9gpKrp1AZIT";
        string _access_token;
        string _payURL = "https://api-m.sandbox.paypal.com/v2/checkout/orders";

        // Variables to store order response details
        OrderResponse _currentOrderResponse;
        string _verifyLink;
        string _approvalLink;
        string _executeLink;

        [Header("PayPal Order Details")]
        public PayPalOrder payPalOrder;

        public float orderPrice;

        // Start the process by requesting the PayPal token
        public IEnumerator StartOrder()
        {
            yield return RequestTokenRoutine();
        }

        // Routine to request an access token from PayPal
        IEnumerator RequestTokenRoutine()
        {
            WWWForm formData = new WWWForm();
            formData.AddField("grant_type", "client_credentials");

            using (var request = UnityWebRequest.Post("https://api-m.sandbox.paypal.com/v1/oauth2/token", formData))
            {
                // Base64 encode clientID and secret for Basic Authorization
                string base64 = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(clientID + ":" + secret));

                // Set headers for the request
                request.SetRequestHeader("Accept", "application/json");
                request.SetRequestHeader("Accept-Language", "en_US");
                request.SetRequestHeader("Authorization", "Basic " + base64);
                request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

                // Send the request and wait for the response
                yield return request.SendWebRequest();

                // Check for errors
                if (request.result == UnityWebRequest.Result.ConnectionError || !string.IsNullOrEmpty(request.error))
                {
                    Debug.LogError("Authentication Error: " + request.error);
                }
                else
                {
                    _access_token = JsonUtility.FromJson<AuthorizationResponse>(request.downloadHandler.text).access_token;
                    StartCoroutine(CreateOrderRoutine());
                }
            }
        }

        // Routine to create an order using PayPal API
        IEnumerator CreateOrderRoutine()
        {
            // Convert the PayPal order object to JSON
            string jsonOrder = JsonConvert.SerializeObject(payPalOrder);
            var jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonOrder);

            // Send request to create order
            using (var request = new UnityWebRequest(_payURL, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                request.downloadHandler = new DownloadHandlerBuffer();

                // Set headers
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", "Bearer " + _access_token);

                // Send the request and wait for the response
                yield return request.SendWebRequest();

                // Check for errors
                if (request.result == UnityWebRequest.Result.ConnectionError || !string.IsNullOrEmpty(request.error))
                {
                    Debug.LogError("Order Request Error: " + request.downloadHandler.text);
                }
                else
                {
                    _currentOrderResponse = JsonUtility.FromJson<OrderResponse>(request.downloadHandler.text);

                    // Extract PayPal order approval and execution links
                    _verifyLink = _currentOrderResponse.links[0].href;
                    _approvalLink = _currentOrderResponse.links[1].href;
                    _executeLink = _currentOrderResponse.links[2].href + "/capture";

                    // Proceed to payment approval
                    StartCoroutine(GetPaymentApprovalRoutine());
                }
            }
        }

        // Routine to handle payment approval by opening a browser with PayPal approval URL
        IEnumerator GetPaymentApprovalRoutine()
        {
            Application.OpenURL(_approvalLink);  // Open the PayPal approval page for the user

            bool orderApproved = false;
            while (!orderApproved)
            {
                using (var request = UnityWebRequest.Get(_verifyLink))
                {
                    request.SetRequestHeader("Content-Type", "application/json");
                    request.SetRequestHeader("Authorization", "Bearer " + _access_token);

                    // Send the verification request and wait for the response
                    yield return request.SendWebRequest();

                    // Handle request errors
                    if (request.result == UnityWebRequest.Result.ConnectionError || !string.IsNullOrEmpty(request.error))
                    {
                        Debug.LogError("URL Request Error: " + request.downloadHandler.text);
                        yield return new WaitForSeconds(5f);  // Retry after 5 seconds
                    }
                    else
                    {
                        // Parse the verification response
                        VerifyResponse _verification = JsonUtility.FromJson<VerifyResponse>(request.downloadHandler.text);

                        // Check if the order is approved
                        if (_verification != null && _verification.status == "APPROVED")
                        {
                            orderApproved = true;
                            StartCoroutine(ExecutePaymentRoutine());
                        }
                        else
                        {
                            yield return new WaitForSeconds(5f);  // Retry after 5 seconds
                        }
                    }
                }
            }
        }

        // Routine to execute the payment after approval
        IEnumerator ExecutePaymentRoutine()
        {
            if (string.IsNullOrEmpty(_executeLink) || string.IsNullOrEmpty(_access_token))
            {
                Debug.LogError("Execute link or access token is missing.");
                yield break;
            }

            // Send request to execute the payment
            using (var request = new UnityWebRequest(_executeLink, "POST"))
            {
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", "Bearer " + _access_token);

                // Send an empty payload for payment execution
                string emptyJson = "{}";
                byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(emptyJson);
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                request.downloadHandler = new DownloadHandlerBuffer();

                // Send request and wait for the response
                yield return request.SendWebRequest();

                // Check for errors
                if (request.result == UnityWebRequest.Result.ConnectionError || !string.IsNullOrEmpty(request.error))
                {
                    Debug.LogError("Execute Payment Error: " + request.error);
                }
                else
                {
                    PayPalExecutionResponse response = JsonUtility.FromJson<PayPalExecutionResponse>(request.downloadHandler.text);

                    // Log the response details if the payment is successfully executed
                    if (response != null)
                    {
                        Debug.Log("Payment status: " + response.status);
                        Debug.Log("Payment capture ID: " + response.id);
                        StartCoroutine(SaveOrder(UserManager.Instance.currentID, response.id, orderPrice.ToString()));
                    }
                    else
                    {
                        Debug.LogError("Failed to parse PayPal execution response.");
                    }
                }
            }
        }

        private IEnumerator SaveOrder(string userID, string paypalID, string price)
        {


            WWWForm form = new WWWForm();
            form.AddField("action", "placeorder");
            form.AddField("userID", userID);
            form.AddField("paypalID", paypalID);
            form.AddField("price", price);

            using (UnityWebRequest webRequest = UnityWebRequest.Post("http://localhost/MYG/api/index.php", form))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log("Error saving order: " + webRequest.error);
                    yield break;
                }
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Response: " + jsonResponse);
            }
        }
    }

    // Classes representing PayPal API responses
    [System.Serializable]
    public class AuthorizationResponse
    {
        public string access_token;
    }

    [System.Serializable]
    public class OrderResponse
    {
        public Link[] links;
    }

    [System.Serializable]
    public class VerifyResponse
    {
        public string status;
        public Link[] links;
    }

    [System.Serializable]
    public class Link
    {
        public string href;
        public string rel;
        public string method;
    }

    [System.Serializable]
    public class PayPalExecutionResponse
    {
        public string id;
        public string status;
        public List<PurchaseUnit> purchase_units;
    }

    [System.Serializable]
    public class PurchaseUnit
    {
        public string reference_id;
        public Amount amount;
    }

    [System.Serializable]
    public class Amount
    {
        public string currency_code;
        public string value;
    }

    // Class to hold PayPal order details for editor
    [System.Serializable]
    public class PayPalOrder
    {
        public string intent { get; set; }  
        public List<PurchaseUnit> purchase_units { get; set; }  // List of purchase units

        
        public PayPalOrder(string intent, string referenceId, string currencyCode, string amountValue)
        {
            this.intent = intent;  
            purchase_units = new List<PurchaseUnit>
            {
                // Add a new purchase unit with the given parameters
                new PurchaseUnit
                {
                    reference_id = referenceId,  // Reference ID of the order
                    amount = new Amount { currency_code = currencyCode, value = amountValue }  // Order amount
                }
            };  
        }
    }

}