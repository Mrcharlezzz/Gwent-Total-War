using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeginSelection : MonoBehaviour
{
    public void Substitution()
    {
        Destroy(gameObject.GetComponent<ShowCard>().showCard);
        if(GlobalContext.gameMaster.selectionCount<2) 
        {    
            GlobalContext.gameMaster.currentplayer.deck.cards.Add(gameObject.GetComponent<Carddisplay>().card);
            
            GlobalContext.gameMaster.currentplayer.hand.cards.Remove(gameObject.GetComponent<Carddisplay>().card);
            Destroy(gameObject);
            GlobalContext.gameMaster.selectionCount++;

            if(GlobalContext.gameMaster.selectionCount==2)
            {
                GlobalContext.gameMaster.selectionCount=0;
                GlobalContext.gameMaster.EndSelection();
            }
        }
    }
}