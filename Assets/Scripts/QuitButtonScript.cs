using UnityEngine; 
public class QuitButtonScript : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("Quit button pressed!"); 
        Application.Quit();
 
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}