using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropZone : MonoBehaviour
{
    public List<Card.Type> typelist = new List<Card.Type>();
    public Card.Position position;
    public List<Card> cardlist;
    public int maxsize;
    public bool player1; // side of the field of the row (false means it is player2's)
    GameMaster gameMaster;
    


    public void ZoneClear()
    {
        cardlist.Clear();
        if (gameObject.transform.childCount > 0)
        {
            foreach (Transform child in gameObject.transform)
            {
                GraveYard graveYard;
                if (player1)
                {
                    graveYard = gameMaster.player1.graveyard;
                }
                else
                {
                    graveYard = gameMaster.player2.graveyard;
                }
                graveYard.Add(child.GetComponent<Carddisplay>().card);
                Destroy(child.gameObject);
            }
        }
    }

    void Awake()
    {
        gameMaster = GameObject.Find("gamemaster").GetComponent<GameMaster>();
    }
}

