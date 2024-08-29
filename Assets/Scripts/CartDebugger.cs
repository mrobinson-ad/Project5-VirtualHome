using UnityEngine;
using System.Text;
using System.Collections.Generic;

public class CartDebugger : MonoBehaviour
{
    // Function to log the contents of the cart dictionary
    public void LogCartContents(Dictionary<CartItem, int> cartDict)
    {
        if (cartDict == null || cartDict.Count == 0)
        {
            Debug.Log("Cart is empty.");
            return;
        }

        // StringBuilder for efficient string concatenation
        StringBuilder logMessage = new StringBuilder();
        logMessage.AppendLine("Cart Contents:");

        foreach (var kvp in cartDict)
        {
            CartItem item = kvp.Key;
            int count = kvp.Value;
            string productName = item.product != null ? item.product.name : "Unknown Product";
            string color = string.IsNullOrEmpty(item.color) ? "No Color" : item.color;

            // Append each item’s details to the log message
            logMessage.AppendLine($"Product: {productName}, Color: {color}, Quantity: {count}");
        }

        Debug.Log(logMessage.ToString());
    }
}