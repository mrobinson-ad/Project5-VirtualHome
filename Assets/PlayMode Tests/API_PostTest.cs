using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine;
using System;
using VirtualHome;

    public class API_PostTest
    {
        private UserManager userManager;

        [SetUp]
        public void SetUp()
        {
            userManager = new GameObject().AddComponent<UserManager>();
        }

        // Test registration success by checking the response JSON
        [UnityTest]
        public IEnumerator Test1_UserRegistration_success()
        {
            string jsonResponse = null;

            // Call the RegisterUser method and capture the response in the callback
            yield return userManager.RegisterUser(
                "Test",
                "User",
                "testuser",
                "testuser@example.com",
                "Password123",
                (response) => jsonResponse = response
            );

            Assert.IsNotNull(jsonResponse, "Response should not be null");

            Assert.AreEqual("success", jsonResponse, "The user should be registered successfully");
        }
                // Test registration success by checking the response JSON
        [UnityTest]
        public IEnumerator Test2_UserRegistration_failure()
        {
            string jsonResponse = null;

            // Call the RegisterUser method and capture the response in the callback
            yield return userManager.RegisterUser(
                "Test",
                "User",
                "testuser",
                "testuser@example.com",
                "Password123",
                (response) => jsonResponse = response
            );

            Assert.IsNotNull(jsonResponse, "Response should not be null");

            Assert.AreEqual("email", jsonResponse, "The registration should fail because of a duplicate email");
        }

        [UnityTest]
        public IEnumerator Test3_UserLogin_Success()
        {
            string jsonResponse = null;

            // Call the Login method and capture the response in the callback
            yield return userManager.Login(
                "testuser@example.com",
                "Password123",
                (response) => jsonResponse = response
            );

            Assert.IsNotNull(jsonResponse, "Response should not be null");

            Assert.AreEqual("success", jsonResponse, "The user should be registered successfully");
        }

        [UnityTest]
        public IEnumerator Test4_UserLogin_IncorrectPass()
        {
            string jsonResponse = null;

            // Call the Login method and capture the response in the callback
            yield return userManager.Login(
                "testuser@example.com",
                "Password",
                (response) => jsonResponse = response
            );

            Assert.IsNotNull(jsonResponse, "Response should not be null");

            Assert.AreEqual("Incorrect password.", jsonResponse, "The user should not be logged due to an incorrect password");
        }

        [UnityTest]
        public IEnumerator Test5_UserLogin_IncorrectMail()
        {
            string jsonResponse = null;

            // Call the Login method and capture the response in the callback
            yield return userManager.Login(
                "testfail@example.com",
                "Password123",
                (response) => jsonResponse = response
            );

            Assert.IsNotNull(jsonResponse, "Response should not be null");


            Assert.AreEqual("Email not registered.", jsonResponse, "The user should not be logged due to an unknown email");
        }


    }
