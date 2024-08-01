using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Board: GameComponent{
    public Dictionary<Card, Field> cardField;

    public override void Push(Card card)
    {
        cardField[card] = GlobalContext.GetPlayer(card.owner).field;
        cardField[card].Push(card);
        cards.Add(card);
    }

    public override Card Pop(){
        Card removed = cards[Size - 1];
        cards.RemoveAt(Size - 1);
        return cardField[removed].Pop();
    }

    public override void Remove(Card card)
    {
        cards.Remove(card);
        cardField[card].Remove(card);
    }

    public override void SendBottom(Card card)
    {
        cardField[card] = GlobalContext.GetPlayer(card.owner).field;
        cardField[card].SendBottom(card);
        cards.Insert(0, card);
    }
}