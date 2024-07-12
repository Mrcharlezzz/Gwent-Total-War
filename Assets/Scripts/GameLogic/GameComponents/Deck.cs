using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : GameComponent
{
    public override void Push(Card card)
    {
        cards.Add(card);
    }
    public override Card Pop()
    {
        Card removed = cards[^1];
        cards.RemoveAt(Size - 1);
        return removed;
    }

    public override void Remove(Card card)
    {
        cards.Remove(card);
    }

    public override void SendBottom(Card card)
    {
        cards.Insert(0, card);
    }
}
