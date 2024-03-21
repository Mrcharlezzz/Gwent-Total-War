using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public List<Card> deck = new List<Card>();
    public int decksize;
    

    public void Shuffle()
    {
        for (int i=deck.Count-1;i>0;i--)
        {
            int randomIndex=Random.Range(0,i+1);
            Card container=deck[i];
            deck[i]=deck[randomIndex];
            deck[randomIndex]=container;
        }
    
    }
    
    public void DrawCard()
    {
        Debug.Log($"2- {deck.Count}");
        deck.Remove(deck[deck.Count-1]);
    }

}
