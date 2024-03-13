using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropZone : MonoBehaviour
{
    public List<Card.Type> typelist= new List<Card.Type>();
    public List<Card.Position> positionlist= new List<Card.Position>();
    public List<Card> cardlist;
    public int maxsize;
    public bool isUnitRow;
    public bool player1; // side of the field of the row (false means it is player2's)
     
    
    

    void Update()
    {        
        ModifyingConditions conditions=gameObject.GetComponent<ModifyingConditions>();
        if(isUnitRow&&conditions.modified)
        {
        conditions.modified=false;

        
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
            if(conditions.weatheroff)
            {
                if(cardlist.Count!=0)
                {
                    foreach (Transform child in gameObject.transform)
                    {
                        Carddisplay tempcard=child.gameObject.GetComponent<Carddisplay>();
                        if(tempcard.type!=Card.Type.Golden)
                        {
                            tempcard.power=tempcard.basepower+conditions.boostamount;
                            tempcard.powerText.text=tempcard.power.ToString();
                            tempcard.powerText.color=Color.black;
                            if(tempcard.power>tempcard.basepower)
                            {
                                tempcard.powerText.color=Color.green;
                            }
                               

                        }
                    }
                }
                conditions.weatheroff=false;
            }
            
            if(conditions.boostaffected)
            {
                if(gameObject.GetComponent<DropZone>().cardlist.Count!=0)
                {
                    foreach (Transform child in gameObject.transform)
                    {
                        Carddisplay tempcard=child.gameObject.GetComponent<Carddisplay>();
                        if(tempcard.type!=Card.Type.Golden)
                        {
                            tempcard.power=tempcard.basepower+conditions.boostamount;
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
        Debug.Log("Updated");
        }
    }


}
