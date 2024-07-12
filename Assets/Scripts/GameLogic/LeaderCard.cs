using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderCard: MonoBehaviour
{
    public bool alreadyused=false;
    public Leader leader;

    
    public void ActivateLeaderEffect()
    {
        if(!alreadyused && GlobalContext.gameMaster.currentplayer == leader.owner)
        {
            alreadyused=true;
            leader.ActivateEffect(leader.owner);
            GlobalContext.gameMaster.NextTurn();
        }
    }
    
}
