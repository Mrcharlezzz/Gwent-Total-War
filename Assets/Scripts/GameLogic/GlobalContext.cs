using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalContext
{
    public static GameMaster gameMaster;
    public static GameComponent Board{get => gameMaster.board;}   
    public static GameComponent Hand(Player player)
    {
        return player.hand;
    }
    public static GameComponent Deck(Player player)
    {
        return player.deck;
    }
    public static GameComponent Field(Player player)
    {
        return player.field;
    }
    public static GameComponent Graveyard(Player player)
    {
        return player.graveyard;
    }

    public static Player GetPlayer(int n){
        if(n == 0) return null;
        if(n == 1) return gameMaster.player1;
        return gameMaster.player2;
    }

    
}
