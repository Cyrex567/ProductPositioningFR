using UnityEngine;
using UnityEngine.UI; // Required for UI elements like Text and Button
using System.Collections.Generic; // Required for Lists
using System.Linq; // Required for LINQ methods like FirstOrDefault

// Define the different states the game can be in
public enum GameState { Setup, PlayerPlacing, Assignment, Results, GameOver }

public class GameManager : MonoBehaviour
{
    [Header("Scene References")] // Assign these in the Unity Inspector
    public MapController mapController;
    public GameObject playerProductPrefab; // Drag your Product Prefab here
    public GameObject customerPrefab;      // Drag your Customer Prefab here
    public Transform mapPanelTransform;    // Drag the parent Transform for products/customers (e.g., your InteractableMap's Transform)
    public TMPro.TextMeshProUGUI infoText;                // Drag a UI Text element here for messages
    public Button nextActionButton;        // Drag your main Button here
    public List<GameObject> playerProductPrefabs;

    [Header("Game Settings")]
    public int numberOfPlayers = 2;        // How many players?
    public int numberOfCustomers = 50;     // How many customers per round?
    public List<Color> playerColors = new List<Color> { Color.red, Color.blue, Color.green, Color.yellow }; // Colors for player markers

    // --- Runtime Data ---
    // List to keep track of products placed in the current round
    private List<PlayerProduct> placedProducts = new List<PlayerProduct>();
    // List to keep track of customers generated in the current round
    private List<Customer> customers = new List<Customer>();
    // Index of the player whose turn it currently is
    private int currentPlayerIndex = 0;
    // The current state of the game
    public GameState CurrentGameState { get; private set; }


    // Called when the script instance is being loaded
    void Start()
    {
       // if (nextActionButton != null)
       // {
        //    nextActionButton.onClick.RemoveAllListeners();
       //     nextActionButton.onClick.AddListener(OnNextActionButtonClick);
       // }
       // else
       // {
       //     Debug.LogError("Next Action Button is not assigned in the GameManager Inspector!");
       // }

       // StartNewGame(); // Make sure this is running again
    }

    // Sets up the game for the very first time or restarts it
    void StartNewGame()
    {
        Debug.Log("Starting New Game...");
        currentPlayerIndex = 0;
        placedProducts.Clear(); // Ensure product list is empty
        ClearBoard(); // Remove any old visuals
        ChangeState(GameState.PlayerPlacing); // Move to the first player's placement turn
    }

    // Removes visual elements (products, customers) from the board
    void ClearBoard()
    {
        // Destroy existing product GameObjects
        foreach (var product in placedProducts)
        {
            if (product != null) Destroy(product.gameObject);
        }
        // Destroy existing customer GameObjects
        foreach (var customer in customers)
        {
            if (customer != null) Destroy(customer.gameObject);
        }
        // Clear the lists holding references to the scripts
        // Note: placedProducts list is cleared in StartNewGame or before PlayerPlacing state usually
        customers.Clear();
    }

    // Central function to change the game state and update UI accordingly
    void ChangeState(GameState newState)
    {
        CurrentGameState = newState;
        Debug.Log($"--- Changing state to: {newState} ---");

        // Reset button state unless specified otherwise in the case
        if (nextActionButton != null) nextActionButton.interactable = false;


        switch (newState)
        {
            case GameState.Setup:
                // Potential state for initial setup if needed later
                if (infoText != null) infoText.text = "Game Setup...";
                break;

            case GameState.PlayerPlacing:
                // Ensure player index is valid
                if (currentPlayerIndex >= playerColors.Count)
                {
                    Debug.LogError($"Not enough colors defined in playerColors list for player {currentPlayerIndex + 1}. Defaulting to white.");
                }
                string colorName = (currentPlayerIndex < playerColors.Count) ? playerColors[currentPlayerIndex].ToString() : "white";

                if (infoText != null) infoText.text = $"Player {currentPlayerIndex + 1} ({colorName}): Place your product!";
                if (nextActionButton != null)
                {
                    // Button stays uninteractable until player clicks map
                    //nextActionButton.GetComponentInChildren<Text>().text = "Confirm Placement";
                    nextActionButton.interactable = false;
                }
                // Optional: Add logic here to hide other products if needed
                break;

            case GameState.Assignment:
                if (infoText != null) infoText.text = "Assigning customers...";
                if (nextActionButton != null) nextActionButton.interactable = false; // Disable button during calculation
                GenerateCustomers();
                AssignCustomers();
                // Use Invoke to wait a tiny moment before showing results, makes it feel less abrupt
                Invoke(nameof(GoToResultsState), 0.1f);
                break;

            case GameState.Results:
                if (nextActionButton != null)
                {
                    nextActionButton.GetComponentInChildren<Text>().text = "Next Round";
                    nextActionButton.interactable = true; // Allow proceeding
                }
                CalculateAndDisplayResults(); // Calculate scores and update text
                break;

            case GameState.GameOver:
                // Potential state if you add win conditions
                if (infoText != null) infoText.text = "Game Over!";
                if (nextActionButton != null)
                {
                    nextActionButton.GetComponentInChildren<Text>().text = "Restart";
                    nextActionButton.interactable = true;
                }
                break;
        }
    }

    // Called by InputController when player clicks the map
    public void PlaceCurrentPlayerProduct(Vector2 mapPosition)
    {
        // Only allow placement if it's the placing state
        if (CurrentGameState != GameState.PlayerPlacing)
        {
            Debug.LogWarning("Cannot place product outside of PlayerPlacing state.");
            return;
        }

        Debug.Log($"GameManager: Received placement click for Player {currentPlayerIndex + 1} at {mapPosition}");

        // --- Check necessary references ---
        if (playerProductPrefab == null || mapPanelTransform == null || mapController == null)
        {
            Debug.LogError("Missing references needed for placing product (Prefab, MapPanel, or MapController)!");
            return;
        }
        // --- End Check ---


        // Remove previous marker for this player this round if one exists
        PlayerProduct existing = placedProducts.FirstOrDefault(p => p.PlayerId == currentPlayerIndex);
        if (existing != null)
        {
            Debug.Log($"Replacing existing marker for player {currentPlayerIndex + 1}.");
            Destroy(existing.gameObject);
            placedProducts.Remove(existing); // Remove from the list too!
        }



        // Set its visual position using the MapController
        // --- Check if we have a prefab for the current player ---
        if (playerProductPrefabs == null || currentPlayerIndex >= playerProductPrefabs.Count || playerProductPrefabs[currentPlayerIndex] == null)
        {
            Debug.LogError($"Player Product Prefab for player index {currentPlayerIndex} is not assigned or list is invalid!");
            return; // Stop if prefab is missing
        }
        // --- End Check ---

        // New line using the list:
        GameObject productGO = Instantiate(playerProductPrefabs[currentPlayerIndex], mapPanelTransform);

        // Get the script component and initialize it
        PlayerProduct product = productGO.GetComponent<PlayerProduct>();
        if (product != null)
        {
            // Get the appropriate color (handle case where not enough colors are defined)
            Color color = Color.white; // Default color
            if (currentPlayerIndex < playerColors.Count)
            {
                color = playerColors[currentPlayerIndex];
            }
            else
            {
                Debug.LogWarning($"Player index {currentPlayerIndex} is out of bounds for playerColors list (Size: {playerColors.Count}). Using default color.");
            }

            product.Initialize(currentPlayerIndex, mapPosition, color);
            placedProducts.Add(product); // Add the new product to our list

            // Update UI text and enable the confirmation button
            if (infoText != null) infoText.text = $"Player {currentPlayerIndex + 1}: Placed at {mapPosition:F2}. Confirm?";
            if (nextActionButton != null) nextActionButton.interactable = true;

            Debug.Log($"Successfully placed product for Player {currentPlayerIndex + 1}.");

        }
        else
        {
            Debug.LogError("Assigned playerProductPrefab is missing the PlayerProduct script component!");
            Destroy(productGO); // Clean up the instantiated object if script is missing
        }
    }

    // --- Customer Generation and Assignment Logic ---

    void GenerateCustomers()
    {
        Debug.Log($"Generating {numberOfCustomers} customers.");
        // Ensure customer list is clear before generating new ones
        foreach (var cust in customers)
        {
            if (cust != null) Destroy(cust.gameObject);
        }
        customers.Clear();

        if (customerPrefab == null || mapPanelTransform == null || mapController == null)
        {
            Debug.LogError("Missing references needed for generating customers (Prefab, MapPanel, or MapController)!");
            return;
        }


        for (int i = 0; i < numberOfCustomers; i++)
        {
            // Generate random preference within map dimensions
            float prefX = Random.Range(0f, mapController.mapDimensions.x);
            float prefY = Random.Range(0f, mapController.mapDimensions.y);
            Vector2 preferencePosition = new Vector2(prefX, prefY);

            // Instantiate prefab and place it visually
            GameObject customerGO = Instantiate(customerPrefab, mapPanelTransform);
            customerGO.transform.localPosition = mapController.MapToUIPosition(preferencePosition);

            Customer customer = customerGO.GetComponent<Customer>();
            if (customer != null)
            {
                customer.Initialize(preferencePosition);
                customers.Add(customer);
            }
            else
            {
                Debug.LogError("Assigned customerPrefab is missing the Customer script component!");
                Destroy(customerGO); // Clean up
            }
        }
        Debug.Log($"Generated {customers.Count} customers.");
    }

    void AssignCustomers()
    {
        Debug.Log("Assigning customers to nearest product...");
        if (placedProducts.Count == 0)
        {
            Debug.LogWarning("No products placed, cannot assign customers.");
            return; // Can't assign if no products exist
        }

        // Reset customer counts on products before assigning
        foreach (var product in placedProducts)
        {
            if (product != null) product.AssignedCustomers = 0;
        }

        foreach (Customer customer in customers)
        {
            if (customer == null) continue; // Skip if customer somehow null

            PlayerProduct closestProduct = null;
            float minDistance = float.MaxValue;

            foreach (PlayerProduct product in placedProducts)
            {
                if (product == null) continue; // Skip if product somehow null

                float distance = Vector2.Distance(customer.PreferencePosition, product.MapPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestProduct = product;
                }
                // Note: Simple tie-breaking: first product checked wins if distances are equal.
            }

            // Assign customer visually and update product's count
            if (closestProduct != null)
            {
                customer.AssignToProduct(closestProduct);
                closestProduct.AssignedCustomers++;
            }
            else
            {
                customer.AssignToProduct(null); // Mark as unassigned if no products found (shouldn't happen if check above passed)
                Debug.LogWarning($"Customer at {customer.PreferencePosition} could not be assigned to any product.");
            }
        }
        Debug.Log("Customer assignment complete.");
    }

    // Helper function to transition state after a short delay
    void GoToResultsState()
    {
        ChangeState(GameState.Results);
    }


    // --- Results Calculation and Display ---
    void CalculateAndDisplayResults()
    {
        Debug.Log("Calculating results...");
        if (infoText == null)
        {
            Debug.LogError("InfoText is not assigned, cannot display results!");
            return;
        }

        string resultsText = "--- Round Results ---\n";

        // Sort products by Player ID for consistent display order
        placedProducts.Sort((p1, p2) => {
            if (p1 == null && p2 == null) return 0;
            if (p1 == null) return -1;
            if (p2 == null) return 1;
            return p1.PlayerId.CompareTo(p2.PlayerId);
        });


        foreach (PlayerProduct product in placedProducts)
        {
            if (product == null) continue;

            // Simple profit calculation: 1 point per customer
            int profit = product.AssignedCustomers;
            string colorName = (product.PlayerId < playerColors.Count) ? playerColors[product.PlayerId].ToString() : "white";
            resultsText += $"Player {product.PlayerId + 1} ({colorName}): {product.AssignedCustomers} customers -> Profit: {profit}\n";
        }

        infoText.text = resultsText; // Update the UI Text element
        Debug.Log(resultsText); // Log results to console too

        // Optional: Add logic here to check for game over condition (e.g., after X rounds)
        // if (roundNumber >= maxRounds) ChangeState(GameState.GameOver);
    }

    // --- Button Click Handler ---

    // Called when the main action button ('nextActionButton') is clicked
    void OnNextActionButtonClick()
    {
        Debug.Log($"Button clicked in state: {CurrentGameState}");

        switch (CurrentGameState)
        {
            case GameState.PlayerPlacing:
                // Player confirmed their placement
                Debug.Log($"Player {currentPlayerIndex + 1} confirmed placement.");
                currentPlayerIndex++; // Move to the next player index

                if (currentPlayerIndex < numberOfPlayers)
                {
                    // Still more players to place
                    ChangeState(GameState.PlayerPlacing);
                }
                else
                {
                    // All players have placed, move to assignment phase
                    ChangeState(GameState.Assignment);
                }
                break;

            case GameState.Results:
                // Player wants to start the next round
                Debug.Log("Starting Next Round...");
                currentPlayerIndex = 0; // Reset to player 1
                ClearBoard(); // Clear customers and old products
                placedProducts.Clear(); // Make sure product list is empty for new round
                ChangeState(GameState.PlayerPlacing); // Go back to placement state
                break;

            case GameState.GameOver:
                // Player wants to restart the game
                StartNewGame();
                break;

            default:
                Debug.LogWarning($"Button click not handled in state: {CurrentGameState}");
                break;
        }
    }
}