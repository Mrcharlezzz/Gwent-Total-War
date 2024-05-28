using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalContext
{
    public static List<Card> board;   
    public static List<Card> Hand(Player player)
    {
        return player.hand.cards;
    }
    public static List<Card> Deck(Player player)
    {
        return player.playerdeck.cards;
    }
    public static List<Card> Field(Player player)
    {
        return player.field.cards;
    }
    public static List<Card> Graveyard(Player player)
    {
        return player.graveyard.cards;
    }
}
