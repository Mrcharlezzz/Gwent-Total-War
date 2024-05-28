using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class GameComponent : MonoBehaviour
{
    public Player owner;
    public List<Card> cards;
    public void Push(Card card)
    {
        cards.Add(card);
    }
    public void Pop()
    {
        cards.RemoveAt(cards.Count - 1);
    }
    public void SendBottom(Card card)
    {
        cards.Insert(0, card);
    }
    public void Remove(Card card)
    {
        cards.Remove(card);
    }
    public void Shuffle()
    {
        for (int i=cards.Count-1;i>0;i--)
        {
            int randomIndex=Random.Range(0,i+1);
            Card container=cards[i];
            cards[i]=cards[randomIndex];
            cards[randomIndex]=container;
        }
    }
}
