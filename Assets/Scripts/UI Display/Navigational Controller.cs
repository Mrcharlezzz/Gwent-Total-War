using UnityEngine;
using UnityEngine.SceneManagement;

public class NavigationController : MonoBehaviour
{
    public void CargarEscena(string Faction)
    {
        SceneManager.LoadScene(Faction);
    }
    
}

