using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Database
{
    static List<Card> allcards;
    static DeckList Deck1;
    static DeckList Deck2;

    public static Card Search(int id)
    {
        return allcards[id];
    }
}


