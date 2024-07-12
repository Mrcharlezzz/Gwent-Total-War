using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Message : MonoBehaviour
{
    public TextMeshProUGUI text;
    public GameObject winnertext;
    public Button pass;
    public bool gameended=false;

    public void Continue()
    {
        if(gameended)
        {
            gameObject.GetComponent<NavigationController>().LoadScene("UI");
        }
        gameended=false;
        gameObject.SetActive(false);
        winnertext.SetActive(true);
        GlobalContext.gameMaster.currentplayer.hand.gameObject.SetActive(true);
        pass.enabled=true;
    }
}
