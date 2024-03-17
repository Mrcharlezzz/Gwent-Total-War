using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TurnUI : MonoBehaviour
{
    public GameObject turnUI;
    public GameMaster gameMaster;
    public TextMeshProUGUI playerText;
    public GameObject pass;

    public void Begin()
    {
        turnUI.SetActive(false);
        gameMaster.currentplayer.hand.gameObject.SetActive(true);
        pass.GetComponent<Button>().enabled=true;
    }
}
