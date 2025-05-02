using UnityEngine;
using UnityEngine.InputSystem; // Using the new Input System
using UnityEngine.EventSystems; // Required for checking clicks on UI
using System.Collections.Generic; // Required for List (used in RaycastAll)

public class InputController : MonoBehaviour
{
    // Assign these in the Unity Inspector
    public Camera mainCamera;
    public MapController mapController;
    public Canvas parentCanvas; // Assign the parent Canvas GameObject

    void Update()
    {
        // --- 1. Check for Mouse Click using New Input System ---
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            // No click detected this frame, exit Update
            return;
        }

        // --- 2. Check for Blocking UI Elements ---
        // Create pointer data for the current mouse position
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };
        // Create a list to store results from raycasting
        List<RaycastResult> results = new List<RaycastResult>();

        // Make sure EventSystem exists
        if (EventSystem.current == null)
        {
            Debug.LogError("EventSystem is NULL! Cannot check for UI blocking.");
            return;
        }
        // Perform the raycast against the UI
        EventSystem.current.RaycastAll(pointerData, results);

        // If the raycast hit any UI element, ignore the click for map placement
        if (results.Count > 0)
        {
            // You could optionally log what was hit for debugging:
            // Debug.LogWarning($"Clicked on UI element: {results[0].gameObject.name}. Ignoring click.");
            return; // Stop processing this click
        }
        // --- End UI Check ---


        // --- 3. Check if References are Assigned ---
        if (mapController == null || mapController.mapPanelRect == null || mainCamera == null || parentCanvas == null)
        {
            Debug.LogError("InputController is missing one or more assignments (Camera, MapController, MapPanelRect, or Canvas)!");
            return;
        }
        // --- End Check ---


        // --- 4. Convert Screen Position to Map Coordinates ---
        Vector2 screenPosition = Mouse.current.position.ReadValue();
        // Debug.Log($"Attempting conversion. Screen Click Position: {screenPosition}, Target Rect: {mapController.mapPanelRect.name}"); // Optional debug

        // Determine the correct camera to use based on the Canvas Render Mode
        Camera eventCamera = null; // Default for Overlay mode
        if (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera || parentCanvas.renderMode == RenderMode.WorldSpace)
        {
            eventCamera = parentCanvas.worldCamera; // Use the Canvas's assigned camera
            if (eventCamera == null && parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                Debug.LogError("Canvas Render Mode is ScreenSpaceCamera, but no Render Camera is assigned on the Canvas Component!");
                return; // Stop if camera needed but not assigned
            }
        }
        // Debug.Log($"Canvas Render Mode: {parentCanvas.renderMode}. Using Event Camera: {(eventCamera != null ? eventCamera.name : "null (Overlay)")}"); // Optional debug


        // Attempt the conversion using the determined camera
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mapController.mapPanelRect, // The UI element's RectTransform
                screenPosition,             // The mouse position on screen
                eventCamera,                // The relevant camera (null for Overlay)
                out Vector2 localPoint))    // The output local position within the RectTransform
        {
            // Conversion succeeded, now check if it's within the rect bounds and calculate logical coords

            // Use the actual pivot to adjust the localPoint to a 0,0 bottom-left origin system
            Vector2 pivotAdjustment = mapController.mapPanelRect.pivot;
            Vector2 adjustedLocalPoint = localPoint + mapController.mapPanelRect.rect.size * pivotAdjustment;

            // Use the original localPoint for the Contains check, as it's relative to the pivot
            if (mapController.mapPanelRect.rect.Contains(localPoint))
            {
                // Convert the adjusted local point (0,0 origin) to logical map coordinates (0-10)
                Vector2 mapCoords = mapController.UIPositionToMap(adjustedLocalPoint);
                Debug.Log($"SUCCESS! Click processed. Logical Map Coords: {mapCoords:F2}"); // Log success and coords

                // --- 5. Call GameManager ---
                GameManager gameManager = FindObjectOfType<GameManager>(); // Find the active GameManager
                if (gameManager != null && gameManager.CurrentGameState == GameState.PlayerPlacing)
                {
                    // Tell GameManager to place the product at the calculated coordinates
                    gameManager.PlaceCurrentPlayerProduct(mapCoords);
                }
                else if (gameManager == null)
                {
                    Debug.LogError("InputController couldn't find active GameManager in the scene!");
                }
                else
                {
                    // Log if click happened but it wasn't the right game state
                    Debug.Log($"Click detected but not in PlayerPlacing state (Current state: {gameManager.CurrentGameState}).");
                }
                // --- End Call ---
            }
            else
            {
                // This might happen if clicking just outside the visible bounds but within the RectTransform somehow
                Debug.LogWarning("Click conversion succeeded but point was outside Rect bounds. Local Point: " + localPoint);
            }
        }
        else
        {
            // ScreenPointToLocalPointInRectangle failed - click was likely outside the RectTransform area from the camera's perspective
            Debug.Log("Click was outside the MapPanel area (RectTransformUtility failed)."); // Changed to Log instead of LogError
        }
        // --- End Coordinate Conversion ---

    } // End Update
} // End Class