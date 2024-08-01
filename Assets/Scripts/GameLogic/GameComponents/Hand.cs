using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class Hand : GameComponent
{
    public Dictionary<Card, GameObject> bodies = new Dictionary<Card, GameObject>();


    public override void Push(Card card)
    {
        cards.Add(card);
        GameObject body = GameTools.CreateCardInObject(card, gameObject, owner);
        bodies[card] = body;
    }

    public override void Remove(Card card)
    {
        cards.Remove(card);
        Destroy(bodies[card]);
        bodies.Remove(card);
    }

    public override Card Pop()
    {
        if (Size == 0) throw new IndexOutOfRangeException("Cannot apply pop method to an empty list");
        Card removed = cards[Size - 1];
        cards.RemoveAt(Size - 1);
        Destroy(bodies[removed]);
        bodies.Remove(removed);
        return removed;
    }

    public override void SendBottom(Card card)
    {
        cards.Insert(0, card);
        GameObject body = GameTools.CreateCardInObject(card, gameObject, owner);
        bodies[card] = body;
    }
}
