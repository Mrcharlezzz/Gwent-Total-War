using UnityEngine;
using UnityEngine.SceneManagement;

public class NavigationController : MonoBehaviour
{
    public void LoadScene(string nextscene)
    {
        SceneManager.LoadScene(nextscene);
    }
    
}

