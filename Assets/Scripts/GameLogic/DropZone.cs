using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

public class DropZone : MonoBehaviour
{
    public List<Card.Type> typelist= new List<Card.Type>();
    public List<Card.Position> positionlist= new List<Card.Position>();
    public List<Card> cardlist;
    public int maxsize;
    public int multiplePower=0; //for power x n times effect
    public bool isUnitRow;
    public bool player1; // side of the field of the row (false means it is player2's)
    GameMaster gameMaster;
     
    ModifyingConditions conditions;

    
    public void ZoneClear()
    {
        cardlist.Clear();
        multiplePower=0;
        if(gameObject.transform.childCount>0)
        {
            foreach(Transform child in gameObject.transform)
            {
                GraveYard graveYard;
                if(player1)
                {
                    graveYard=gameMaster.player1.graveyard;
                }
                else
                {
                    graveYard=gameMaster.player2.graveyard;
                }
                graveYard.Add(child.GetComponent<Carddisplay>().card);
                Destroy(child.gameObject);
            }
        }
        
        if(conditions!=null)
        {    
            conditions.modified=false;
            conditions.weatheraffected=false;
            conditions.weatheroff= false;
            conditions.boostaffected=false;
            conditions.powerXntimesAffected=false;
            conditions.nTimes=0;
            conditions.boostamount=0;
        }
    }
    
    void Awake()
    {
        conditions=gameObject.GetComponent<ModifyingConditions>();
        gameMaster=GameObject.Find("gamemaster").GetComponent<GameMaster>();
    }
    void Update()
    {        

        if(isUnitRow&&conditions.modified)
        {
        conditions.modified=false;
        CheckConditions();
        
        gameMaster.globalModified=true;
    }

    
    void CheckWeatherOffCondition()
    {
        if(conditions.weatheroff)
            {
                if(cardlist.Count!=0)
                {
                    foreach (Transform child in gameObject.transform)
                    {
                        Carddisplay tempcard=child.gameObject.GetComponent<Carddisplay>();
                        if(tempcard.type!=Card.Type.Golden&&!(conditions.averaged&&tempcard.averaged))
                        {
                            tempcard.power=tempcard.basepower;
                            tempcard.powerText.text=tempcard.power.ToString();
                            CardPowerImageColorchange(child,Color.white);
                        }
                    }
                }
                conditions.weatheroff=false;
                conditions.powerXntimesAffected=true;
            }
    }

    void CheckPowerXnTimesEffect()
    {   
        if(conditions.powerXntimesAffected)
        {
            Debug.Log("modified");
            if(gameObject.transform.childCount>0)
            {    
                conditions.nTimes=0;
                foreach(Transform child in gameObject.transform)
                {
                    
                    Carddisplay display0=child.gameObject.GetComponent<Carddisplay>();
                    if(display0.effect==Card.Effect.PowerXntimes)
                    {
                        conditions.nTimes++;
                        Debug.Log($"ntimes {conditions.nTimes}");
                    }
                }
                foreach(Transform child in gameObject.transform)
                {
                    
                    Carddisplay display0=child.gameObject.GetComponent<Carddisplay>();
                    if(display0.effect==Card.Effect.PowerXntimes)
                    {
                        multiplePower=display0.basepower*conditions.nTimes;
                        display0.power=multiplePower;
                        display0.powerText.text=display0.power.ToString();
                        if(conditions.nTimes!=1)
                        {
                            CardPowerImageColorchange(child,Color.green);
                        }
                    }
                }
            }
            conditions.powerXntimesAffected=false;
        }
        gameMaster.globalModified=true;
            
    }

    void CheckBoostAffectedCondition()
    {
        if(conditions.boostaffected)
        {
            Debug.Log("boostaffected");
            if(gameObject.GetComponent<DropZone>().cardlist.Count!=0)
            {
                foreach (Transform child in gameObject.transform)
                {
                Carddisplay tempcard=child.gameObject.GetComponent<Carddisplay>();
                    if(tempcard.type!=Card.Type.Golden&&!(conditions.averaged&&tempcard.averaged))
                    {
                        if(tempcard.effect==Card.Effect.PowerXntimes)
                        {
                            tempcard.power=multiplePower+conditions.boostamount;
                        }
                        else{
                            tempcard.power=tempcard.basepower+conditions.boostamount;   
                        }
                    
                        tempcard.powerText.text=tempcard.power.ToString();
                        if(tempcard.power>=tempcard.basepower)
                        {
                            CardPowerImageColorchange(child,Color.green);
                        }    
                    }
                }
            }
        }
    }

    void CheckConditions()
    {
        if(conditions.weatheraffected)
        {
            if(gameObject.GetComponent<DropZone>().cardlist.Count!=0)
            {
                foreach (Transform child in gameObject.transform)
                {
                    Carddisplay tempcard=child.gameObject.GetComponent<Carddisplay>();
                    if(tempcard.type!=Card.Type.Golden&&!(conditions.averaged&&tempcard.averaged))
                    {
                        if(tempcard.power>1)
                        {
                        tempcard.power=1;
                        
                        CardPowerImageColorchange(child,Color.red);
                        tempcard.powerText.text=tempcard.power.ToString();  
                        }
                        
                    }
                }
            }
        } 
        else
        {
            CheckWeatherOffCondition();
            CheckPowerXnTimesEffect();
            CheckBoostAffectedCondition();
        }
        Debug.Log("Updated");
    }

    void CardPowerImageColorchange(Transform child,Color color)
    {
        child.GetChild(1).GetChild(0).GetChild(0).gameObject.GetComponent<Image>().color=color;
    }
}
}
