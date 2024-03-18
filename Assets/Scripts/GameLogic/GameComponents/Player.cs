using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameMaster gameMaster;
    public Hand hand;
    public Deck playerdeck;
    public GraveYard graveyard;
    public Field field;
    public Card leaderCard;
    public int totalpower=0;
    public int roundpoints=0;
    public TextMeshProUGUI totalPowerText;
    public bool player1;
    public bool fieldModified=false;
    public bool roundupdate=false;
    public GameObject point1;
    public GameObject point2;

    
    void Update()
    {
        if(roundupdate)
        {
            if(roundpoints==1)
            {
                point1.SetActive(true);
            }
            if(roundpoints==2)
            {
                point2.SetActive(true);
            }
            roundupdate=false;
        }
        
        
        if(fieldModified)
        {
            Transform unitrows=field.unitRows.transform;
            totalpower=0;
            foreach(Transform child in unitrows)
            {
                if(child.childCount>0)
                {    
                    foreach(Transform grandchild in child)
                    {
                        totalpower+=grandchild.gameObject.GetComponent<Carddisplay>().power;
                    }
                }
            }

            totalPowerText.text=totalpower.ToString();
            fieldModified=false;
        }
    }
    
    public void PlayerClear()
    {
        field.Clear();
        totalpower=0;
        totalPowerText.text="0";
    }

    public void DrawCard()
    {
        
        if(hand.size==10)
        {
            graveyard.Add(playerdeck.deck[playerdeck.deck.Count-1]);
            playerdeck.DrawCard();
        }
        else
        {
            
            Debug.Log(playerdeck.deck.Count-1); //DO NOT ERASE :)
            hand.AddCard(playerdeck.deck[playerdeck.deck.Count-1]);
            playerdeck.DrawCard();
        }
    }
    public void PlayCard(GameObject card,GameObject dropZone)
    
    {
        bool isValidType= false;
        bool isValidPosition=false;
        bool availableSlot=false;
        bool playable=false;
        bool isValidField=false;
        

        DropZone zone=dropZone.GetComponent<DropZone>();
        Carddisplay display=card.GetComponent<Carddisplay>();
        
        Debug.Log("Play");
        
        

        #region Conditions
        if(display.type!=Card.Type.Decoy)
        {
        foreach (Card.Type type in zone.typelist)
        {
            if(display.type==type)
            {
                isValidType=true;
            }
        }
        foreach (Card.Position position in zone.positionlist)
        {
            if(display.position==position)
            {
                isValidPosition=true;
            }
        }
        if(zone.cardlist.Count<zone.maxsize)
        {
            availableSlot=true;
        }

        if(zone.typelist[0]==Card.Type.Weather||player1==zone.player1)
        {
            isValidField=true;
        }


        if(isValidType&&isValidPosition&&availableSlot&&isValidField)
        {
            playable=true;
            gameMaster.NextTurn();
        }
        }
        #endregion

        #region Weather card substitution
            
        if(display.type==Card.Type.Weather&&playable)
        {
            ModifyingConditions conditions0=dropZone.GetComponent<BufferLink>().dropzones[0].GetComponent<ModifyingConditions>();
            ModifyingConditions conditions1=dropZone.GetComponent<BufferLink>().dropzones[1].GetComponent<ModifyingConditions>();
            
            conditions0.modified=true;
            conditions1.modified=true;
            Debug.Log("dropzone modified BBB");
            
            conditions0.weatheraffected=true;
            conditions1.weatheraffected=true;
            Debug.Log("Weather Affected");
            
            if(zone.cardlist.Count!=0)
            foreach (Transform child in dropZone.transform)
            {
                if(child.gameObject.GetComponent<Carddisplay>().position==display.position)
                {
                    
                    Debug.Log("Weather Card Destroyed");
                    zone.cardlist.Remove(zone.cardlist[0]);
                    Debug.Log(zone.cardlist.Count);
                    graveyard.Add(child.gameObject.GetComponent<Carddisplay>().card);
                    Destroy(child.gameObject);
                }
            }
        }

        #endregion

        #region Boost Handling
        
        if(display.type==Card.Type.Boost&&playable)
        {
        ModifyingConditions conditions0=dropZone.GetComponent<BufferLink>().dropzones[0].GetComponent<ModifyingConditions>();
        conditions0.modified=true;
        conditions0.boostaffected=true;
        conditions0.boostamount=display.power; 
 

        Debug.Log("Boostaffected and modidied");  
        }

            
        #endregion

        #region Play or return
            
        
        //If necessary playing conditions are satisfied then play card, else return card to its original position
        if(playable&&(display.type!=Card.Type.Decoy))
        {
            card.GetComponent<DragandDrop>().alreadyplayed=true; Debug.Log("Played succesfully");
            
            hand.RemoveCard(display.card);//Tratar de mejorar, pasos innecesarios carddisplay tiene un componente card con la carta en cuestion
            zone.cardlist.Add(display.card);
            if(zone.isUnitRow)
            {
            dropZone.GetComponent<ModifyingConditions>().modified=true;
            Debug.Log("dropzone modifiedAAA");
            }
            card.transform.SetParent(dropZone.transform,false);

            gameMaster.globalModified=true;
        }
        else
        {
            card.transform.position=card.GetComponent<DragandDrop>().startPosition;
        }
        #endregion
        

        #region Clear Handling
        if(display.type==Card.Type.Clear&&playable)
        {
            Debug.Log("Cleared");
            Transform weatherslots=dropZone.transform.parent;
            
            foreach(Transform child in weatherslots)
            {
                ModifyingConditions conditions0=child.gameObject.GetComponent<BufferLink>().dropzones[0].GetComponent<ModifyingConditions>();
                ModifyingConditions conditions1=child.gameObject.GetComponent<BufferLink>().dropzones[1].GetComponent<ModifyingConditions>();
                
                conditions0.weatheraffected=false;
                conditions0.weatheroff=true;
                conditions0.modified=true;                    

                conditions1.weatheraffected=false;
                conditions1.weatheroff=true;
                conditions1.modified=true;                    

                child.gameObject.GetComponent<DropZone>().cardlist.Clear();
                if(child.childCount>0)
                {
                    Destroy(child.GetChild(0).gameObject);
                }
            }
            
            Destroy(card);
        }
        #endregion


        #region Decoy Handling
        if(display.type==Card.Type.Decoy)    
        {    
            Debug.Log("Is Working");
            
            GameObject card2=dropZone;//In this case the decoy collides with a card, not a dropzone due to the layer dispositions
            GameObject parent=card2.transform.parent.gameObject;
            Carddisplay card2display=card2.GetComponent<Carddisplay>();
            if(parent.GetComponent<ModifyingConditions>()!=null&&card2.GetComponent<Carddisplay>().type!=Card.Type.Golden&&parent.GetComponent<DropZone>().player1==player1)
            {
                card2display.power=card2display.basepower;
                card2display.powerText.color=Color.black;

                
                hand.AddCard(card2display.card);
                parent.GetComponent<DropZone>().cardlist.Remove(card2display.card);
                Destroy(card2);

                parent.GetComponent<DropZone>().cardlist.Add(display.card);
                parent.GetComponent<DropZone>().cardlist.Add(card2display.card);
                card.transform.SetParent(parent.transform);
                
                //Implementar condicional para caso de q la carta devuelta tenga determinado effecto constant
                if(card2display.effect!=Card.Effect.None)
                {
                    parent.GetComponent<ModifyingConditions>().modified=true;
                    parent.GetComponent<ModifyingConditions>().powerXntimesAffected=true;
                }

                gameMaster.NextTurn();
            }
            else
            {
                card.transform.position=card.GetComponent<DragandDrop>().startPosition;
            }
        }
        #endregion

        #region Effect Handling
            
            
            if((display.type==Card.Type.Golden||display.type==Card.Type.Silver)&&playable&&display.effect!=Card.Effect.None)
            {
                Debug.Log("Effectstep1");
                //Summon Boost
                if(display.effect==Card.Effect.SummonBoost)
                {
                    GameObject boostSlots =GameObject.Find("boostSlots");
                    DropZone boostslot=GameObject.Find("MeleeBoost1").GetComponent<DropZone>();
                    foreach(Transform child in boostSlots.transform)
                    {
                        DropZone childzone=child.gameObject.GetComponent<DropZone>();
                        if(childzone.positionlist[0]==display.position&&(childzone.player1==player1))
                        {
                            boostslot=childzone;
                        }
                    }
                    if(boostslot.transform.childCount>0)
                    {
                        Destroy(boostslot.transform.GetChild(0).gameObject);
                    }
                    boostslot.cardlist.Clear();

                    GameObject boost= Instantiate(card,new Vector3(0,0,0),Quaternion.identity);
                    boost.transform.SetParent(boostslot.transform,false);
                    Carddisplay carddisplay= boost.GetComponent<Carddisplay>();
                    carddisplay.displayId=4;
                    
                    Debug.Log(carddisplay.name);
                    boostslot.cardlist.Add(carddisplay.card);
                    
                    ModifyingConditions conditions=boostslot.GetComponent<BufferLink>().dropzones[0].GetComponent<ModifyingConditions>();
                    conditions.boostamount=carddisplay.card.basepower;
                    conditions.boostaffected=true;
                    conditions.modified=true;


                }

            
            
            
            
                //Power X n times
                if(display.effect==Card.Effect.PowerXntimes)
                {
                    ModifyingConditions conditions=dropZone.GetComponent<ModifyingConditions>();
                    conditions.powerXntimesAffected=true;
                    conditions.modified=true;
                }   
            }

            

        #endregion
    }
}
