using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraveYard : MonoBehaviour
{
    public List<Card> cards= new List<Card>();
    public void Add(Card card)
    {
        cards.Add(card);
    }
}
