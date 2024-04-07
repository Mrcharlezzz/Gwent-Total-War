using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TurnUI : MonoBehaviour
{
    public GameMaster gameMaster;
    public TextMeshProUGUI playerText;
    public TextMeshProUGUI selectiontext;
    public Button pass;

    public void Begin()
    {
        selectiontext.enabled=false;
        gameObject.SetActive(false);
        gameMaster.currentplayer.hand.gameObject.SetActive(true);
        pass.enabled=true;
        if(gameMaster.turn<=1)
        {
            pass.enabled=false;
            gameMaster.StartSelection();
        }
    }
}
