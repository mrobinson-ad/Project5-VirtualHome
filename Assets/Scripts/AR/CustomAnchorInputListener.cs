using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections.Generic;

public class CustomAnchorInputListener : MonoBehaviour
{
    [System.Serializable]
    public class Vector2Event : UnityEvent<Vector2> { }

    public Vector2Event OnInputReceived;

    private void Update()
    {
        // Mouse input check
        if (Input.GetMouseButton(0))
        {
            if (!IsPointerOverUI(Input.mousePosition))
            {
                Vector2 inputPosition = Input.mousePosition;
                OnInputReceived?.Invoke(inputPosition);
            }
        }

        // Touch input check
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPosition = touch.position;

            if (!IsPointerOverUI(touchPosition))
            {
                // Only trigger the event if the touch is not over a UI element
                if (touch.phase == TouchPhase.Began)
                {
                    OnInputReceived?.Invoke(touchPosition);
                }
            }
        }
    }

    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);
        return results.Count > 0;
    }
}