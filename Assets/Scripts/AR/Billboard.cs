using UnityEngine;

public class Billboard : MonoBehaviour
{
    void Update()
    {
        ApplyBillboard();
    }

    private void ApplyBillboard()
    {
        transform.rotation = Camera.main.transform.rotation;
    }
}
