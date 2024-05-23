using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Database
{
    static int lastID;
    static Dictionary<int,Card> allcards;
    static DeckList Rome;
    static DeckList Greece;

    public static Card Search(int id)
    {
        if(id>lastID) return null;
        else return allcards[id];
    }
    
}

class DeckList
{
}
