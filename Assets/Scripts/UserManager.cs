using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections;
using Newtonsoft.Json.Linq;

namespace VirtualHome
{
    public class UserManager : MonoBehaviour
    {
        public static UserManager Instance { get; private set; } // Singleton
        public string currentUser;
        public string currentID;

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

        private IEnumerator Login(string email, string password, System.Action<string> callback)
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

                    callback("success");
                }
                else if (response["error"] != null)
                {
                    callback(response["error"].ToString());
                }
            }
        }
    }

    [Serializable]
    public class UserPayment
    {
        public string firstName;
        public string lastName;
        public string card;
        public string cvv;
        public string date;
        public string cardType;

        public UserPayment(string firstName, string lastName, string card, string cvv, string date, string cardType)
        {
            this.firstName = firstName;
            this.lastName = lastName;
            this.card = card;
            this.cvv = cvv;
            this.date = date;
            this.cardType = cardType;
        }
    }

    [Serializable]
    public class UserAddress
    {
        public string firstName;
        public string lastName;
        public string address;
        public string city;
        public string state;
        public string postal;
        public UserAddress(string firstName, string lastName, string address, string city, string state, string postal)
        {
            this.firstName = firstName;
            this.lastName = lastName;
            this.address = address;
            this.city = city;
            this.state = state;
            this.postal = postal;
        }

    }
}
