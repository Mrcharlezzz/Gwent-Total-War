using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TurnUI : MonoBehaviour
{
    public TextMeshProUGUI playerText;
    public TextMeshProUGUI selectiontext;
    public Button pass;

    public void Begin()
    {
        selectiontext.enabled=false;
        gameObject.SetActive(false);
        GlobalContext.gameMaster.currentplayer.hand.gameObject.SetActive(true);
        pass.enabled=true;
        if(GlobalContext.gameMaster.turn<=1)
        {
            pass.enabled=false;
            GlobalContext.gameMaster.StartSelection();
        }
    }
}
