using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderCard: MonoBehaviour
{
    public GameMaster gameMaster;
    public bool alreadyused=false;
    public bool Alejandro;
    
    public void Julio()
    {
        if(gameMaster.currentplayer==gameMaster.player1&&!alreadyused)
        {
            alreadyused=true;
            gameMaster.player1.DrawCard();
            gameMaster.NextTurn();
        }
    }
    
}
