using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalContext
{
    public static GameMaster gameMaster;
    public static GameComponent Board{get => gameMaster.board;}   
    public static GameComponent Hand(int player)
    {
        return GetPlayer(player).hand;
    }
    public static GameComponent Deck(int player)
    {
        return GetPlayer(player).deck;
    }
    public static GameComponent Field(int player)
    {
        return GetPlayer(player).field;
    }
    public static GameComponent Graveyard(int player)
    {
        return GetPlayer(player).graveyard;
    }

    public static Player GetPlayer(int n){
        if(n == 0) return null;
        if(n == 1) return gameMaster.player1;
        return gameMaster.player2;
    }

    
}
