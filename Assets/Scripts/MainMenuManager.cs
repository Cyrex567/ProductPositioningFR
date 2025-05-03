using UnityEngine;
using UnityEngine.SceneManagement; // Required for loading scenes

public class MainMenuManager : MonoBehaviour
{
    // Call this function when the button is clicked
    public void StartGame()
    {
        // Replace "SampleScene" with the ACTUAL name of your main game scene file if it's different
        Debug.Log("Loading Game Scene...");
        SceneManager.LoadScene("SampleScene");
    }

    // Optional: Add a Quit button function later if needed
     public void QuitGame()
     {
         Debug.Log("Quitting Game...");
         Application.Quit();
     }
}