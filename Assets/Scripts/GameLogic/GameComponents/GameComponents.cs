using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class GameComponent : MonoBehaviour
{
    public Player owner;
    public List<Card> cards;
    public abstract void Push(Card card);
    public abstract Card Pop();
    public abstract void SendBottom(Card card);
    public abstract void Remove(Card card);
    public int Size {get => cards.Count;}
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
