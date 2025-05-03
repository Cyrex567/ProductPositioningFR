using UnityEngine;
using UnityEngine.UI; // Required for UI elements like Text and Button
using System.Collections.Generic; // Required for Lists
using System.Linq;
using TMPro; // Required for LINQ methods like FirstOrDefault

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

    [Header("ScoreDisplay")]
    // Assign these two TextMeshPro objects in the Inspector
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;
    // Variables to hold the scores
    private int player1Score = 0;
    private int player2Score = 0;

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
    void Awake()
    {
        // Force the state as early as possible
        CurrentGameState = GameState.PlayerPlacing;
        Debug.Log($"GameManager Awake: Initial state FORCED to {CurrentGameState}");
        // IMPORTANT: Do NOT call ChangeState or UI updates here,
        // as other objects (like UI elements) might not be ready yet.


    }

    void Start()
    {
        if (nextActionButton != null)
        {
            nextActionButton.onClick.RemoveAllListeners();
            nextActionButton.onClick.AddListener(OnNextActionButtonClick);
        }

        else
        {
            Debug.LogError("Next Action Button is not assigned in the GameManager Inspector!");
        }

        void Start()
        {
            // ... existing Start code ...

            player1Score = 0;
            player2Score = 0;
            UpdateScoreDisplay(); // Update display for both at start

            void UpdateScoreDisplay()
            {
                // Update Player 1's Text
                if (player1ScoreText != null)
                {
                    player1ScoreText.text = $"P1 Profit: {player1Score}"; // Using $ for easy formatting
                }
                else
                {
                    Debug.LogError("player1ScoreText not assigned in Inspector!");
                }

                // Update Player 2's Text
                if (player2ScoreText != null)
                {
                    player2ScoreText.text = $"P2 Profit: {player2Score}"; // Using $ for easy formatting
                }
                else
                {
                    Debug.LogError("player2ScoreText not assigned in Inspector!");
                }
            }
        }

        StartNewGame(); // Make sure this is running again
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

        void UpdateScoreDisplay()
{
    // Update Player 1's Text
    if (player1ScoreText != null)
    {
        player1ScoreText.text = $"P1 Profit: {player1Score}"; // Using $ for easy formatting
    }
    else
    {
        Debug.LogError("player1ScoreText not assigned in Inspector!");
    }

    // Update Player 2's Text
    if (player2ScoreText != null)
    {
        player2ScoreText.text = $"P2 Profit: {player2Score}"; // Using $ for easy formatting
    }
    else
    {
        Debug.LogError("player2ScoreText not assigned in Inspector!");
    }
}


        switch (newState)
        {
            case GameState.Setup:
                // Potential state for initial setup if needed later
                if (infoText != null) infoText.text = "Game Setup...";
                break;

            case GameState.PlayerPlacing:
                // --- Prepare info for the current player ---

                // Check if enough colors are defined for safety
                if (currentPlayerIndex >= playerColors.Count)
                {
                    Debug.LogError($"Not enough colors defined in playerColors list for player {currentPlayerIndex + 1}. Defaulting to white.");
                }
                // Determine the color name (handle potential out-of-bounds index)
                string colorName = "white"; // Default
                if (currentPlayerIndex >= 0 && currentPlayerIndex < playerColors.Count)
                {
                    colorName = playerColors[currentPlayerIndex].ToString();
                }

                // --- Update UI Elements ---

                // Update the main info text
                if (infoText != null)
                {
                    infoText.text = $"Player {currentPlayerIndex + 1} ({colorName}): Place your product!";
                }
                else
                {
                    Debug.LogError("InfoText reference not set in GameManager!");
                }

                // Setup the confirmation button
                if (nextActionButton != null)
                {
                    // --- Make sure you use the correct Component type for YOUR button's text! ---

                    // Option A: If your button uses standard UI Text
                    // var buttonText = nextActionButton.GetComponentInChildren<UnityEngine.UI.Text>();
                    // if(buttonText != null) buttonText.text = "Confirm Placement"; else Debug.LogError("Button Text component not found!");

                    // Option B: If your button uses TextMeshPro Text (RECOMMENDED)
                    var buttonTextTMP = nextActionButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (buttonTextTMP != null) buttonTextTMP.text = "Confirm Placement"; else Debug.LogError("Button TextMeshProUGUI component not found!");
                    // If using TextMeshPro, make sure you have 'using TMPro;' at the top of GameManager.cs

                    // --- End of Component Choice ---

                    // Button starts disabled - it gets enabled ONLY when the player clicks the map (in PlaceCurrentPlayerProduct)
                    nextActionButton.interactable = false;
                }
                else
                {
                    Debug.LogError("NextActionButton reference not set in GameManager!");
                }

                // Optional: Add logic here later if you want to hide opponent markers during placement
                break; // Important: End the case

            case GameState.Assignment:
                if (infoText != null) infoText.text = "Assigning customers...";
                if (nextActionButton != null) nextActionButton.interactable = false; // Disable button during calculation
                GenerateCustomers();
                AssignCustomers();
                // Use Invoke to wait a tiny moment before showing results, makes it feel less abrupt
                Invoke(nameof(GoToResultsState), 0.1f);
                break;

            case GameState.Results:
                if (nextActionButton != null) // Good, keep this check for the button itself
                {
                    // --- Replace the lines below this with the corrected code ---

                    // 1. FIND the component and STORE the result in a variable
                    //    (Make sure this is TextMeshProUGUI if that's what your button uses)
                    var buttonText = nextActionButton.GetComponentInChildren<TextMeshProUGUI>();

                    // 2. CHECK if the variable is null (meaning the component wasn't found)
                    if (buttonText != null)
                    {
                        // 3. If it's NOT null (component found), THEN set the text
                        buttonText.text = "Next Round";
                    }
                    else
                    {
                        // 4. If it IS null (component not found), log an error to help you debug
                        Debug.LogError("TextMeshProUGUI component missing in children of nextActionButton!", nextActionButton);
                    }

                    // --- End of corrected code ---

                    // This line was okay and can stay
                    nextActionButton.interactable = true;
                }
                CalculateAndDisplayResults();


                break; // Don't forget the break statement for the case

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
        // Debug.Log($"==== PlaceCurrentPlayerProduct method entered for map position {mapPosition} ===="); // Keep this if you still have it

        if (CurrentGameState != GameState.PlayerPlacing) return;

        Debug.Log($"GameManager: Placing product for Player {currentPlayerIndex + 1} at {mapPosition}");

        // --- DETAILED REFERENCE CHECK ---
        bool referencesOk = true;
        if (mapController == null)
        {
            Debug.LogError("PlaceCurrentPlayerProduct ERROR: mapController reference is NULL!");
            referencesOk = false;
        }
        if (mapPanelTransform == null)
        {
            Debug.LogError("PlaceCurrentPlayerProduct ERROR: mapPanelTransform reference is NULL!");
            referencesOk = false;
        }
        if (playerProductPrefabs == null)
        {
            Debug.LogError("PlaceCurrentPlayerProduct ERROR: playerProductPrefabs LIST is NULL!");
            referencesOk = false;
        }
        // Check list bounds and element only if list itself isn't null
        else if (currentPlayerIndex < 0 || currentPlayerIndex >= playerProductPrefabs.Count)
        {
            Debug.LogError($"PlaceCurrentPlayerProduct ERROR: currentPlayerIndex {currentPlayerIndex} is out of bounds for prefabs list (Size: {playerProductPrefabs.Count})!");
            referencesOk = false;
        }
        else if (playerProductPrefabs[currentPlayerIndex] == null)
        {
            // Use $ for easy variable inclusion in the string
            Debug.LogError($"PlaceCurrentPlayerProduct ERROR: playerProductPrefabs[{currentPlayerIndex}] ELEMENT is NULL!");
            referencesOk = false;
        }

        // If any check failed, stop the method here
        if (!referencesOk)
        {
            Debug.LogError("Aborting PlaceCurrentPlayerProduct due to missing references.");
            return;
        }
        // --- END REFERENCE CHECK ---


        // If we get here, all references should be good!
        Debug.Log("All references checked OK. Proceeding with Instantiate...");

        // Remove previous marker if exists...
        PlayerProduct existing = placedProducts.FirstOrDefault(p => p.PlayerId == currentPlayerIndex);
        if (existing != null)
        {
            Debug.Log($"Replacing existing marker for player {currentPlayerIndex + 1}.");
            Destroy(existing.gameObject);
            placedProducts.Remove(existing);
        }

        // Instantiate the product prefab...
        GameObject productGO = Instantiate(playerProductPrefabs[currentPlayerIndex], mapPanelTransform);

        if (productGO != null)
        {
            Debug.Log($"--- Instantiated {productGO.name} successfully! Active: {productGO.activeInHierarchy} ---");
        }
        else
        {
            Debug.LogError("--- Instantiation FAILED!!! productGO is null! ---");
        }

        // Set its visual position...
        productGO.transform.localPosition = mapController.MapToUIPosition(mapPosition);

        // Get the script component and initialize it...
        PlayerProduct product = productGO.GetComponent<PlayerProduct>();
        // ... (rest of the method as before: initialize product, add to list, SetAsLastSibling, update UI) ...

        // Example remainder:
        if (product != null)
        {
            Color color = Color.white;
            if (currentPlayerIndex < playerColors.Count) { color = playerColors[currentPlayerIndex]; }
            else { Debug.LogWarning($"Player index out of bounds for playerColors list."); }
            product.Initialize(currentPlayerIndex, mapPosition, color);
            placedProducts.Add(product);
            productGO.transform.SetAsLastSibling();
            if (infoText != null) infoText.text = $"Player {currentPlayerIndex + 1}: Placed at {mapPosition:F2}. Confirm?";
            if (nextActionButton != null) nextActionButton.interactable = true;
            Debug.Log($"Successfully placed product for Player {currentPlayerIndex + 1}.");
        }
        else
        {
            Debug.LogError("Assigned playerProductPrefab is missing PlayerProduct script!");
            Destroy(productGO);
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
        // Keep check for the detailed info text UI
        if (infoText == null)
        {
            Debug.LogError("InfoText is not assigned, cannot display detailed results!");
            // Consider if you should return or just skip updating infoText
        }

        string resultsText = "--- Round Results ---\n";
        int p1CustomerCountThisRound = 0; // Initialize P1 customer count for the round
        int p2CustomerCountThisRound = 0; // Initialize P2 customer count for the round

        // Optional Sort (keep if you want consistent order in infoText)
        placedProducts.Sort((p1, p2) => {
            if (p1 == null && p2 == null) return 0;
            if (p1 == null) return -1; // Handle nulls safely
            if (p2 == null) return 1;
            return p1.PlayerId.CompareTo(p2.PlayerId);
        });

        // Loop through products to build details string AND count customers per player
        foreach (PlayerProduct product in placedProducts)
        {
            if (product == null) continue;

            // Your existing logic for per-product display
            int profit = product.AssignedCustomers; // Per-product customer count
            string colorName = "Unknown";
            // Basic color name guessing - adjust logic if needed, e.g., use player index
            if (playerColors != null && product.PlayerId >= 0 && product.PlayerId < playerColors.Count)
            {
                colorName = playerColors[product.PlayerId].ToString();
            }
            resultsText += $"Player {product.PlayerId + 1} ({colorName}): {product.AssignedCustomers} customers -> Profit: {profit}\n";

            // --- Aggregate customer counts per player for the round ---
            if (product.PlayerId == 0) // Assuming Player 1 uses ID 0
            {
                p1CustomerCountThisRound += product.AssignedCustomers;
            }
            else if (product.PlayerId == 1) // Assuming Player 2 uses ID 1
            {
                p2CustomerCountThisRound += product.AssignedCustomers;
            }
            // Add more 'else if' blocks here if you support more than 2 players
        }

        // --- Compare total counts and update OVERALL score ---
        Debug.Log($"Total Customers This Round - P1: {p1CustomerCountThisRound}, P2: {p2CustomerCountThisRound}");

        if (p1CustomerCountThisRound > p2CustomerCountThisRound)
        {
            player1Score++; // Increment P1's total game score by 1
            resultsText += "\nPlayer 1 wins the round (+1 Score)!"; // Add to detailed display
            Debug.Log("Player 1 wins round (+1 overall score)");
        }
        else if (p2CustomerCountThisRound > p1CustomerCountThisRound)
        {
            player2Score++; // Increment P2's total game score by 1
            resultsText += "\nPlayer 2 wins the round (+1 Score)!"; // Add to detailed display
            Debug.Log("Player 2 wins round (+1 overall score)");
        }
        else
        {
            resultsText += "\nRound is a tie!"; // Add to detailed display
            Debug.Log("Round is a tie (no overall score change)");
        }
        // --- End score update logic ---

        // Update the separate P1/P2 overall score UI
        UpdateScoreDisplay(); // Make sure this function updates player1ScoreText & player2ScoreText

        // Update the detailed results text box
        if (infoText != null)
        {
            infoText.text = resultsText;
        }
        Debug.Log(resultsText); // Log details to console

        // Optional: Check for game over condition
        // if (roundNumber >= maxRounds) ChangeState(GameState.GameOver);
    }

    // Ensure this function exists and updates the correct UI elements
    void UpdateScoreDisplay()
    {
        if (player1ScoreText != null)
            player1ScoreText.text = $"P1 Score: {player1Score}"; // Changed text slightly for clarity
        if (player2ScoreText != null)
            player2ScoreText.text = $"P2 Score: {player2Score}"; // Changed text slightly for clarity
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
                if (nextActionButton != null)
                {
                    // This line is likely causing the error if the component type is wrong!
                    nextActionButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Next Round"; // Or UnityEngine.UI.Text
                    nextActionButton.interactable = true;
                }

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