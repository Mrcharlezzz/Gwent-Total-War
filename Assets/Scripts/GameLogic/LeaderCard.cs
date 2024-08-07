using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderCard: MonoBehaviour
{
    public bool alreadyused=false;
    public Leader leader;

    public void ShowLeader(){
        gameObject.GetComponent<Carddisplay>().displayId=leader.id;
        gameObject.GetComponent<Carddisplay>().DisplayUpdate();
    }
    
    public void ActivateLeaderEffect()
    {
        if(!alreadyused && GlobalContext.gameMaster.currentplayer == GlobalContext.GetPlayer(leader.owner))
        {
            alreadyused=true;
            leader.ActivateEffect(GlobalContext.GetPlayer(leader.owner));
            GlobalContext.gameMaster.NextTurn();
        }
    }
    
}
