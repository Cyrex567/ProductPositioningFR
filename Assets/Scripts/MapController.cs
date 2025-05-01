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

    public Vector2 UIPositionToMap(Vector2 uiPosition)
    {
        if (mapPanelRect == null)
        {
            Debug.LogError("Map Panel Rect is not assigned in MapController!");
            return Vector2.zero;
        }
        // Adjust for pivot/anchor if necessary - assuming simple case for now
        float mapX = (uiPosition.x / mapPanelRect.rect.width) * mapDimensions.x;
        float mapY = (uiPosition.y / mapPanelRect.rect.height) * mapDimensions.y;

        mapX = Mathf.Clamp(mapX, 0f, mapDimensions.x);
        mapY = Mathf.Clamp(mapY, 0f, mapDimensions.y);

        return new Vector2(mapX, mapY);
    }
}