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
    public Button pass;

    public void Begin()
    {
        gameObject.SetActive(false);
        gameMaster.currentplayer.hand.gameObject.SetActive(true);
        pass.enabled=true;
    }
}
