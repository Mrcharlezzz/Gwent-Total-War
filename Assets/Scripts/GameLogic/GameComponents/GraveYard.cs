using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraveYard : MonoBehaviour
{
    public List<Card> cards= new List<Card>();

    public void Add(Card card)
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        gameObject.transform.GetChild(1).gameObject.SetActive(true);
        cards.Add(card);
        gameObject.GetComponent<Carddisplay>().displayId=card.id;
        gameObject.GetComponent<Carddisplay>().update=true;
    }
}
