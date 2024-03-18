using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

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
                        if(tempcard.type!=Card.Type.Golden)
                        {
                            tempcard.power=tempcard.basepower;
                            tempcard.powerText.text=tempcard.power.ToString();
                            tempcard.powerText.color=Color.black;
                        }
                    }
                }
                conditions.weatheroff=false;
                conditions.powerXntimesAffected=true;
            }
    }

    IEnumerator CheckPowerXnTimesEffectWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
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
                            display0.powerText.color=Color.green;
                        }
                    }
                }
            }
            conditions.powerXntimesAffected=false;
        }
            
    }

    void CheckBoostAffectedCondition()
    {
        if(conditions.boostaffected)
        {
            Debug.Log("aaaa");
            if(gameObject.GetComponent<DropZone>().cardlist.Count!=0)
            {
                foreach (Transform child in gameObject.transform)
                {
                Carddisplay tempcard=child.gameObject.GetComponent<Carddisplay>();
                    if(tempcard.type!=Card.Type.Golden)
                    {
                        if(tempcard.effect==Card.Effect.PowerXntimes)
                        {
                            Debug.Log($"Mult {multiplePower}");
                            tempcard.power=multiplePower+conditions.boostamount;
                            Debug.Log("Measure");
                            Debug.Log(multiplePower);
                            Debug.Log(conditions.boostamount);
                        }
                        else{
                            tempcard.power=tempcard.basepower+conditions.boostamount;   
                        }
                    
                        tempcard.powerText.text=tempcard.power.ToString();
                        if(tempcard.power>tempcard.basepower)
                        {
                            tempcard.powerText.color=Color.green;
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
                    if(tempcard.type!=Card.Type.Golden)
                    {
                        if(tempcard.power>1)
                        {
                        tempcard.power=1;
                        tempcard.powerText.color=Color.red; 
                        tempcard.powerText.text=tempcard.power.ToString();  
                        }
                        
                    }
                }
            }
        } 
        else
        {
            CheckWeatherOffCondition();
            StartCoroutine(CheckPowerXnTimesEffectWithDelay(0.05f));//0.05 seconds delay in order to let unity update scene data
            CheckBoostAffectedCondition();
        }
        Debug.Log("Updated");
    }
}
}
