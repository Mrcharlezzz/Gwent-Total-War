using UnityEngine;
using UnityEngine.SceneManagement;

public class NavigationController : MonoBehaviour
{
    public void LoadScene(string nextscene)
    {
        SceneManager.LoadScene(nextscene);
    }
    public void Quit()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
    
}

