using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Hand : GameComponent
{
    public int size{get{return cards.Count;} }
    public AddCards addCards;
    public bool player1;
    
    
    void Awake()
    {
        addCards=gameObject.GetComponent<AddCards>();
    }


    public void Add(Card card)
    {
        Push(card);
        addCards.Add(card.id);
    }
    public void RemoveCard(Card card)
    {
        cards.Remove(card);
    }
}
