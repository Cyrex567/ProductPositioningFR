// MapController.cs - Keep this script as is from before
using UnityEngine;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
    public RectTransform mapPanelRect; // MAKE SURE THIS IS ASSIGNED IN INSPECTOR
    public Vector2 mapDimensions = new Vector2(10f, 10f);

    public Vector2 MapToUIPosition(Vector2 mapCoords)
    {
        if (mapPanelRect == null)
        {
            Debug.LogError("Map Panel Rect is not assigned in MapController!");
            return Vector2.zero; // Avoid errors if not assigned
        }
        float uiX = (mapCoords.x / mapDimensions.x) * mapPanelRect.rect.width;
        float uiY = (mapCoords.y / mapDimensions.y) * mapPanelRect.rect.height;
        // Basic conversion assuming bottom-left anchor for the panel for simplicity now.
        // We might adjust this later if needed.
        return new Vector2(uiX, uiY);
    }

    // Inside MapController.cs
    public Vector2 UIPositionToMap(Vector2 uiPosition) // uiPosition here is the 'adjustedLocalPoint' from InputController
    {
        if (mapPanelRect == null)
        {
            Debug.LogError("Map Panel Rect is not assigned in MapController!");
            return Vector2.zero;
        }

        // --- ADDED LOGGING ---
        Debug.Log($"[MapController] Received UI Position: {uiPosition}");
        Debug.Log($"[MapController] Map Panel Rect Width: {mapPanelRect.rect.width}, Height: {mapPanelRect.rect.height}");
        Debug.Log($"[MapController] Target Map Dimensions: {mapDimensions}");
        // --- END ADDED LOGGING ---


        float mapX = (uiPosition.x / mapPanelRect.rect.width) * mapDimensions.x;
        float mapY = (uiPosition.y / mapPanelRect.rect.height) * mapDimensions.y;

        // --- ADDED LOGGING ---
        Debug.Log($"[MapController] Calculated Raw Map Coords: ({mapX}, {mapY})");
        // --- END ADDED LOGGING ---


        mapX = Mathf.Clamp(mapX, 0f, mapDimensions.x);
        mapY = Mathf.Clamp(mapY, 0f, mapDimensions.y);

        // --- ADDED LOGGING ---
        Debug.Log($"[MapController] Clamped Final Map Coords: ({mapX}, {mapY})");
        // --- END ADDED LOGGING ---


        return new Vector2(mapX, mapY);
    }
}