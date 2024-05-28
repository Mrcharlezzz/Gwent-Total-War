using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : GameComponent
{
    public void DrawCard()
    {
        Debug.Log($"2- {cards.Count}");
        cards.Remove(cards[cards.Count-1]);
    }
}
