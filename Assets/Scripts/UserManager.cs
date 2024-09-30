using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace VirtualHome
{
    public class UserManager : MonoBehaviour
    {
        public static UserManager Instance { get; private set; } // Singleton
        public string currentUser;
        public string currentID;

        public int currentRole;

        public string currentAddress;

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

        public void StartRegisterUser(string firstName, string lastName, string userName, string email, string password, System.Action<string> callback)
        {
            StartCoroutine(RegisterUser(firstName, lastName, userName, email, password, callback));
        }

        public void StartLogin(string email, string password, System.Action<string> callback)
        {
            StartCoroutine(Login(email, password, callback));
        }
        private IEnumerator RegisterUser(string firstName, string lastName, string userName, string email, string password, System.Action<string> callback)
        {

            WWWForm form = new WWWForm();
            form.AddField("action", "register");
            form.AddField("firstName", firstName);
            form.AddField("lastName", lastName);
            form.AddField("username", userName);
            form.AddField("email", email);
            form.AddField("password", password);

            using (UnityWebRequest webRequest = UnityWebRequest.Post("http://localhost/MYG/api/index.php", form))
            {
                yield return webRequest.SendWebRequest();
                Debug.Log("webSent");
                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error registering user: " + webRequest.error);
                    callback("connection");
                    yield break;
                }

                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Response from server: " + jsonResponse);

                if (jsonResponse.Contains("\"success\":\"User registered successfully.\""))
                {
                    callback("success");
                }
                else if (jsonResponse.Contains("\"error\":\"Email already registered.\""))
                {
                    callback("email");
                }
                else
                {
                    callback("fail");
                }
            }
        }

        private IEnumerator Login(string email, string password, Action<string> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("action", "login");
            form.AddField("email", email);
            form.AddField("password", password);

            using (UnityWebRequest webRequest = UnityWebRequest.Post("http://localhost/MYG/api/index.php", form))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error logging in: " + webRequest.error);
                    callback("connection");
                    yield break;
                }

                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Response: " + jsonResponse);

                var response = JObject.Parse(jsonResponse);

                if (response["success"] != null)
                {
                    currentID = response["userID"].ToString();
                    currentUser = response["username"].ToString();
                    currentRole = (int)response["role"];
                    StartCoroutine(GetAddress());
                    callback("success");
                }
                else if (response["error"] != null)
                {
                    callback(response["error"].ToString());
                }
            }
        }

        public void StartSetAddress(string address, string city, string state, string postal)
        {
            StartCoroutine(SetAddress(address, city, state, postal));
        }
        private IEnumerator SetAddress(string address, string city, string state, string postal)
        {
            WWWForm form = new WWWForm();
            form.AddField("action", "setaddress");
            form.AddField("id", currentID);
            form.AddField("address", address);
            form.AddField("city", city);
            form.AddField("state", state);
            form.AddField("postal", postal);

            using (UnityWebRequest webRequest = UnityWebRequest.Post("http://localhost/MYG/api/index.php", form))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error setting address: " + webRequest.error);
                    yield break;
                }
                currentAddress = $"{currentUser}'s address in {city}";
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Response: " + jsonResponse);
            }
        }

        public IEnumerator GetAddress()
        {
            string url = $"http://localhost/MYG/API/getaddress/{currentID}"; // Construct the URL

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest(); // Wait for the response

                // Check for errors
                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Error fetching address for user {currentUser}: {webRequest.error}");
                }
                else
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    Debug.Log(jsonResponse);
                    try
                    {
                        var city = JsonConvert.DeserializeObject<string>(jsonResponse);
                        if (city.Contains("error"))
                        {
                            Debug.Log("No address Found");
                            yield break;
                        }
                        Debug.Log(city);
                        currentAddress = $"{currentUser}'s address in {city}";
                        Debug.Log($"Fetched address in {city} for {currentUser}.");
                    }
                    catch (JsonReaderException)
                    {
                        Debug.Log($"JSON Parsing Error:");
                    }
                }
            }
        }
    }

    [Serializable]
    public class UserAddress
    {
        public string address;
        public string city;
        public string state;
        public string postal;
        public UserAddress(string address, string city, string state, string postal)
        {
            this.address = address;
            this.city = city;
            this.state = state;
            this.postal = postal;
            UserManager.Instance.StartSetAddress(address, city, state, postal);
        }
    }
}
