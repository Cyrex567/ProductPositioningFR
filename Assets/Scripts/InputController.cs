// InputController.cs - TEMPORARY TEST VERSION
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class InputController : MonoBehaviour
{
    public Camera mainCamera; // Assign your Main Camera in Inspector
    public MapController mapController; // Assign your MapController GameObject in Inspector

    void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return; // Exit if no mouse or no click this frame
        }

        // Check if clicking on a UI element (like the button itself)
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Clicked on UI, ignoring for map placement test.");
            return; // Don't process click if it's on UI
        }

        // Ensure mapController and its rect are assigned
        if (mapController == null || mapController.mapPanelRect == null)
        {
            Debug.LogError("MapController or its MapPanelRect is not assigned in InputController!");
            return;
        }

        Vector2 screenPosition = Mouse.current.position.ReadValue();

        // Try to convert screen point to local point in the MapPanel's rectangle
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mapController.mapPanelRect, screenPosition, mainCamera, out Vector2 localPoint))
        {
            // Check if the click was actually inside the visual bounds of the panel
            // Note: localPoint origin depends on pivot. Let's assume center pivot for calculation.
            Vector2 adjustedLocalPoint = localPoint + mapController.mapPanelRect.rect.size * 0.5f; // Adjust for center pivot

            // Check if the adjusted point is within the 0 to width/height range
            if (adjustedLocalPoint.x >= 0 && adjustedLocalPoint.x <= mapController.mapPanelRect.rect.width &&
               adjustedLocalPoint.y >= 0 && adjustedLocalPoint.y <= mapController.mapPanelRect.rect.height)
            {
                // Convert the valid local point to logical map coordinates
                Vector2 mapCoords = mapController.UIPositionToMap(adjustedLocalPoint); // Use adjusted point
                Debug.Log($"Map Click Detected! Local UI Point: {adjustedLocalPoint}, Logical Map Coords: {mapCoords}");
            }
            else
            {
                Debug.Log("Clicked inside RectTransform bounds, but outside logical 0-width/0-height range (check pivot/calculation). Local Point: " + localPoint);
            }
        }
        else
        {
            Debug.Log("Clicked outside the MapPanel area.");
        }
    }
}