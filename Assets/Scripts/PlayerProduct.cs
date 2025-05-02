using UnityEngine;
using UnityEngine.UI; // Needed for Image component

public class PlayerProduct : MonoBehaviour
{
    // --- Public properties to access product data ---
    public int PlayerId { get; private set; }        // Which player owns this product?
    public Vector2 MapPosition { get; private set; } // Where is it on the 0-10 logical map?
    public int AssignedCustomers { get; set; }       // How many customers chose this product this round?

    // --- Private references ---
    private Image visual; // Reference to the Image component for changing color etc.

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        // Get the Image component attached to this GameObject
        visual = GetComponent<Image>();
        if (visual == null)
        {
            Debug.LogError("PlayerProduct script needs an Image component on the same GameObject!", this.gameObject);
        }
        // Initialize customer count
        AssignedCustomers = 0;
    }

    // Call this method from GameManager after instantiating the prefab
    public void Initialize(int playerId, Vector2 mapPosition, Color playerColor)
    {
        PlayerId = playerId;
        MapPosition = mapPosition;
        AssignedCustomers = 0; // Reset count when initialized

        // Set the visual color if the Image component exists
        if (visual != null)
        {
            visual.color = playerColor;
        }

        // The actual visual position on the screen (transform.localPosition)
        // should be set by the GameManager using MapController.MapToUIPosition(mapPosition)
        // right after instantiating.
    }
}