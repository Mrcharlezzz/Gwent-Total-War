using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Board: GameComponent{
    public Dictionary<Card, Field> cardField;

    void Awake(){
        cardField=new Dictionary<Card, Field>();
        cards=new List<Card>();
    }

    public override void Push(Card card)
    {
        cardField[card] = GlobalContext.GetPlayer(card.owner).field;
        cardField[card].Push(card);
        cards.Add(card);
    }

    public override Card Pop(){
        Card removed = cards[Size - 1];
        cards.RemoveAt(Size - 1);
        Card result=cardField[removed].Pop();
        cardField.Remove(removed);
        return result;
    }

    public override void Remove(Card card)
    {
        cards.Remove(card);
        cardField[card].Remove(card);
        cardField.Remove(card);
    }

    public override void SendBottom(Card card)
    {
        cardField[card] = GlobalContext.GetPlayer(card.owner).field;
        cardField[card].SendBottom(card);
        cards.Insert(0, card);
    }
}