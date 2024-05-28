using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraveYard : GameComponent
{
    public void Add(Card card)
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        gameObject.transform.GetChild(1).gameObject.SetActive(true);
        
        //When the card is destroyed all power modifications must be reverted 
        //card.power=card.basepower;
        Push(card);
        gameObject.GetComponent<Carddisplay>().displayId=card.id;
        gameObject.GetComponent<Carddisplay>().update=true;
    }
}
