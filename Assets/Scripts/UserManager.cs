using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using System.Net.Mail;

public class UserManager : MonoBehaviour
{
    public static UserManager Instance { get; private set; } // Singleton

    #region UserTable declaration

    public string currentUser;
    [SerializeField]
    private UserInfo userInfos;

    public List<UserEntry> UserTable => userInfos.userEntries; // Expose userEntries as UserTable

    [Serializable]
    public class UserInfo
    {
        public List<UserEntry> userEntries;
    }
    [Serializable]
    public class UserEntry
    {
        public string Email;
        public string Username;
        public string Password;
        public string Salt;
    }

    #endregion


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeUserTable();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #region Initialize UserTable
    private void InitializeUserTable()
    {
        string jsonString = PlayerPrefs.GetString("usersTable", string.Empty);
        if (string.IsNullOrEmpty(jsonString) || jsonString == "{}")
        {
            Debug.Log("No user data found, initializing with default users...");
            InitializeDefaultUsers();
        }
        else
        {
            Debug.Log("User data found, attempting to load...");
            userInfos = JsonUtility.FromJson<UserInfo>(jsonString);
            if (userInfos == null || userInfos.userEntries == null || userInfos.userEntries.Count == 0)
            {
                Debug.LogError("Deserialization failed or data is empty, initializing with default users...");
                InitializeDefaultUsers();
            }
            else
            {
                Debug.Log("Deserialization successful. Loaded users: " + userInfos.userEntries.Count);

                // Ensure all user entries have a salt
                foreach (var userEntry in userInfos.userEntries)
                {
                    if (string.IsNullOrEmpty(userEntry.Salt))
                    {
                        userEntry.Salt = GenerateSalt();
                        userEntry.Password = HashPassword(userEntry.Password, userEntry.Salt);
                    }
                }

                // Save updated user information
                SaveUserTables();
            }
        }
    }
    #endregion
    #region UserTable functions

    private void InitializeDefaultUsers()
    {
        userInfos = new UserInfo()
        {
            userEntries = new List<UserEntry>()
        };
        AddUserEntry("admin", "admin@vhome.com", "Qwerty1234");
        // Save to PlayerPrefs
        SaveUserTables();
    }

    private void SaveUserTables()
    {
        string json = JsonUtility.ToJson(userInfos);
        PlayerPrefs.SetString("usersTable", json);
        PlayerPrefs.Save();
        Debug.Log("User data saved, total users: " + userInfos.userEntries.Count);
    }

    public void AddUserEntry(string username, string email, string password)
    {
        // Generate a random salt
        string salt = GenerateSalt();
        // Hash the password with the salt
        string hashedPassword = HashPassword(password, salt);

        // Create UserEntry
        UserEntry userEntry = new UserEntry { Username = username, Email = email, Password = hashedPassword, Salt = salt };

        // Ensure userInfos and userEntries are not null
        if (userInfos == null)
        {
            userInfos = new UserInfo()
            {
                userEntries = new List<UserEntry>()
            };
        }
        else if (userInfos.userEntries == null)
        {
            userInfos.userEntries = new List<UserEntry>();
        }

        // Add new entry to userEntries
        userInfos.userEntries.Add(userEntry);

        // Save updated user information
        SaveUserTables();
    }

    public UserEntry ValidateUser(string email, string password)
    {
        if (userInfos != null && userInfos.userEntries != null)
        {
            foreach (var userEntry in userInfos.userEntries)
            {
                if (userEntry.Email == email)
                {
                    string hashedPassword = HashPassword(password, userEntry.Salt);
                    if (userEntry.Password == hashedPassword)
                    {
                        currentUser = userEntry.Username;
                        return userEntry;
                    }
                    break;
                }
            }
        }
        return null;
    }

    public UserEntry GetUserByEmail(string email)
    {
        if (userInfos != null && userInfos.userEntries != null)
        {
            foreach (var userEntry in userInfos.userEntries)
            {
                if (userEntry.Email == email)
                {
                    return userEntry;
                }
            }
        }
        return null; // User with this email not found
    }
    #endregion
    #region Encryption
    private string GenerateSalt()
    {
        byte[] saltBytes = new byte[16];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(saltBytes);
        }
        return Convert.ToBase64String(saltBytes);
    }

    private string HashPassword(string password, string salt)
    {
        byte[] saltBytes = Convert.FromBase64String(salt);
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] saltedPasswordBytes = new byte[saltBytes.Length + passwordBytes.Length];

        Buffer.BlockCopy(saltBytes, 0, saltedPasswordBytes, 0, saltBytes.Length);
        Buffer.BlockCopy(passwordBytes, 0, saltedPasswordBytes, saltBytes.Length, passwordBytes.Length);

        using (var sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(saltedPasswordBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
    #endregion

    public bool IsValidMail(string emailaddress) // Checks if the string is a proper email address format
    {
        try
        {
            MailAddress m = new MailAddress(emailaddress);

            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}