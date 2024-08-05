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


    void Awake(){
        cardlist=new List<Card>();
    }
    public virtual void ZoneClear()
    {
        cardlist.Clear();
        if (gameObject.transform.childCount > 0)
        {
            foreach (Transform child in gameObject.transform)
            {
                GraveYard graveYard;
                if (player1)
                {
                    graveYard = GlobalContext.gameMaster.player1.graveyard;
                }
                else
                {
                    graveYard = GlobalContext.gameMaster.player2.graveyard;
                }
                graveYard.Push(child.GetComponent<Carddisplay>().card);
                Destroy(child.gameObject);
            }
        }
    }
}

