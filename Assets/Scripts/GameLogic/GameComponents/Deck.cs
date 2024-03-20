using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public List<Card> deck = new List<Card>();
    public List<Card> container = new List<Card>();
   
    public int decksize;
    

    public void Shuffle()
    {
        for (int i=0;i<decksize;i++)
        {
            container[0]=deck[i];
            int randomindex=Random.Range(i,decksize);
            deck[i]=deck[randomindex];
            deck[randomindex]=container[0];
            
        }
    }
    
    
    public void DrawCard()
    {
        Debug.Log($"2- {deck.Count}");
        deck.Remove(deck[deck.Count-1]);
    }

    public void AA()
    {
        
    }
}
