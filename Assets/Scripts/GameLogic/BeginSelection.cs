using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeginSelection : MonoBehaviour
{
    public GameMaster gameMaster;



    void Awake()
    {
        gameMaster=GameObject.Find("gamemaster").GetComponent<GameMaster>();
    }
    public void Substitution()
    {
        Destroy(gameObject.GetComponent<ShowCard>().showCard);
        if(gameMaster.selectionCount<2) 
        {    
            gameMaster.currentplayer.playerdeck.deck.Add(gameObject.GetComponent<Carddisplay>().card);
            
            gameMaster.currentplayer.hand.cards.Remove(gameObject.GetComponent<Carddisplay>().card);
            Destroy(gameObject);
            gameMaster.selectionCount++;

            if(gameMaster.selectionCount==2)
            {
                gameMaster.selectionCount=0;
                gameMaster.EndSelection();
            }
        }
    }
}