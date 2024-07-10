using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Database
{
    static List<Card> allcards;
    static List<Card> deck1;
    static List<Card> deck2;

    public static Card Search(int id)
    {
        return allcards[id];
    }
}


