using UnityEngine;
using UnityEngine.UI; // Needed for Image component

public class Customer : MonoBehaviour
{
    // --- Public properties ---
    public Vector2 PreferencePosition { get; private set; } // Where does this customer want to be on the 0-10 map?
    public PlayerProduct AssignedProduct { get; private set; } // Which product did this customer choose? (null if none)

    // --- Private references ---
    private Image visual; // Reference to the Image component for changing color etc.

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        // Get the Image component attached to this GameObject
        visual = GetComponent<Image>();
        if (visual == null)
        {
            Debug.LogError("Customer script needs an Image component on the same GameObject!", this.gameObject);
        }
        // Set a default visual state (e.g., grey)
        AssignToProduct(null);
    }

    // Call this method from GameManager after instantiating the prefab
    public void Initialize(Vector2 preferencePosition)
    {
        PreferencePosition = preferencePosition;
        AssignedProduct = null; // Ensure it starts unassigned

        // The actual visual position on the screen (transform.localPosition)
        // should be set by the GameManager using MapController.MapToUIPosition(preferencePosition)
        // right after instantiating.
    }

    // Call this from GameManager after the assignment logic is complete
    public void AssignToProduct(PlayerProduct product)
    {
        AssignedProduct = product;

        // Update visual appearance based on assignment (optional, but helpful)
        if (visual != null)
        {
            if (product != null && product.GetComponent<Image>() != null)
            {
                // Example: Match customer color to the assigned product's player color, but maybe make it slightly transparent
                Color productClr = product.GetComponent<Image>().color;
                visual.color = new Color(productClr.r, productClr.g, productClr.b, 0.7f); // Make slightly transparent
            }
            else
            {
                // If no product assigned, set to a default color like grey
                visual.color = Color.grey;
            }
        }
    }
}