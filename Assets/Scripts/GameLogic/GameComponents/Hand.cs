using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public int size{get{return cards.Count;} }
    public List<Card> cards= new List<Card>();
    public AddCards addCards;
    
    
    void Awake()
    {
        addCards=gameObject.GetComponent<AddCards>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddCard(Card card)
    {
        cards.Add(card);
        addCards.Add(card.id);
    }
    public void RemoveCard(Card card)
    {
        cards.Remove(card);
    }
}
