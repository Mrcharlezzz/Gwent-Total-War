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
    public GameObject prefab;
    private ModifyingConditions conditions0;
    private ModifyingConditions conditions1;
    private Carddisplay temporaldisplay;

    
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
            conditions0=dropZone.GetComponent<BufferLink>().dropzones[0].GetComponent<ModifyingConditions>();
            conditions1=dropZone.GetComponent<BufferLink>().dropzones[1].GetComponent<ModifyingConditions>();
            
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
            conditions0.averaged=false;
            conditions1.averaged=false;
        }

        #endregion

        #region Boost Handling
        
        if(display.type==Card.Type.Boost&&playable)
        {
        conditions0=dropZone.GetComponent<BufferLink>().dropzones[0].GetComponent<ModifyingConditions>();
        conditions0.boostaffected=true;
        conditions0.averaged=false;
        conditions0.modified=true;
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
                conditions0=child.gameObject.GetComponent<BufferLink>().dropzones[0].GetComponent<ModifyingConditions>();
                conditions1=child.gameObject.GetComponent<BufferLink>().dropzones[1].GetComponent<ModifyingConditions>();
                
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
                graveyard.Add(card2display.card);

                //Pendiente mandar para el cementerio



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
                SummonBoost(display);
                SummonWeather(display);
                PowerxNTimes(display,dropZone);
                DrawEffect(display);
                DestroyStrong(display);
                DestroyWeak(display);
                Average(display);
            }

            

        #endregion
    }

    void SummonBoost(Carddisplay display)
    {
        if(display.effect==Card.Effect.SummonBoost)
        {
            int weatherID=0;

            if(display.faction==Card.Faction.Rome)
            weatherID=4;
            else
            weatherID=0;

            GameObject boostSlots;
            if(player1)
            {
                boostSlots=GameObject.Find("boostSlots");
            }
            else
            {
                boostSlots=GameObject.Find("boostSlots2");
            }
            //OJO crear forma de update el carddisplay
                
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

            GameObject boost= Instantiate(prefab,new Vector3(0,0,0),Quaternion.identity);
            boost.transform.SetParent(boostslot.transform,false);
            Carddisplay carddisplay= boost.GetComponent<Carddisplay>();
            carddisplay.displayId=weatherID;
            carddisplay.update=true;

            boostslot.cardlist.Add(carddisplay.card);
                    
            conditions0=boostslot.gameObject.GetComponent<BufferLink>().dropzones[0].GetComponent<ModifyingConditions>();
            Debug.Log($"boostamount {carddisplay.power}"); 
            conditions0.boostamount=Resources.Load<Card>($"{weatherID}").power; // carddisplay updates after this line is executed, therefore there must be a call to the original card
                    
            conditions0.boostaffected=true;
            conditions0.averaged=false;
            conditions0.modified=true;
        }
    }

    void PowerxNTimes(Carddisplay display, GameObject dropZone)
    {
    if(display.effect==Card.Effect.PowerXntimes)
        {
            conditions0=dropZone.GetComponent<ModifyingConditions>();
            conditions0.powerXntimesAffected=true;
            conditions0.modified=true;
        }
    }

    void SummonWeather(Carddisplay display)
    {
        if(display.effect==Card.Effect.SummonWeather)
        {
            int weatherID=0;

            if(display.faction==Card.Faction.Rome)
            weatherID=23;
            else
            weatherID=23;

            GameObject weatherSlots=GameObject.Find("weatherSlots");
                
            DropZone weatherSlot=GameObject.Find("MeleeWeather").GetComponent<DropZone>();
            foreach(Transform child in weatherSlots.transform)
            {
                DropZone childzone=child.gameObject.GetComponent<DropZone>();
                if(childzone.positionlist[0]==display.position&&(childzone.player1==player1))
                {
                    weatherSlot=childzone;
                }
            }
            if(weatherSlot.transform.childCount>0)
            {
                Destroy(weatherSlot.transform.GetChild(0).gameObject);
            }
            weatherSlot.cardlist.Clear();

            GameObject boost= Instantiate(prefab,new Vector3(0,0,0),Quaternion.identity);
            boost.transform.SetParent(weatherSlot.transform,false);
            Carddisplay carddisplay= boost.GetComponent<Carddisplay>();
            carddisplay.displayId=weatherID;
            carddisplay.update=true;

            weatherSlot.cardlist.Add(carddisplay.card);
                    
            //Hacerlo para conditions0 y conditions1
            
            

            // carddisplay updates after this line is executed, therefore there must be a call to the original card

            conditions0=weatherSlot.gameObject.GetComponent<BufferLink>().dropzones[0].GetComponent<ModifyingConditions>();
            conditions0.boostamount=Resources.Load<Card>($"{weatherID}").power; 
            conditions0.boostaffected=true;
            conditions0.averaged=false;
            conditions0.modified=true;

            conditions1=weatherSlot.gameObject.GetComponent<BufferLink>().dropzones[1].GetComponent<ModifyingConditions>();
            conditions1.boostamount=Resources.Load<Card>($"{weatherID}").power; 
            conditions1.boostaffected=true;
            conditions1.averaged=false;
            conditions1.modified=true;
        }
    }
    void DrawEffect(Carddisplay display)
    {
        if(display.effect==Card.Effect.Draw)
        {
            DrawCard();
        }
    }

    void DestroyStrong(Carddisplay display) //
    {
        if(display.effect==Card.Effect.DestroyStrong)
        {    
            int max=0;
            GameObject destroyedcard=prefab;
            
            
            foreach(Transform dropzone in gameMaster.player1.field.unitRows.transform)
            {
                if(dropzone.childCount>0)
                {   
                    foreach(Transform card in dropzone)
                    {   
                        temporaldisplay=card.gameObject.GetComponent<Carddisplay>();
                        
                        if(temporaldisplay.type!=Card.Type.Golden)
                        {    
                            if(temporaldisplay.power>max)
                            {
                                max=temporaldisplay.power;
                                destroyedcard=card.gameObject;
                            }
                        }
                    }
                }
            }
            foreach(Transform dropzone in gameMaster.player2.field.unitRows.transform)
            {
                if(dropzone.childCount>0)
                {    
                    foreach(Transform card in dropzone)
                    {   
                        temporaldisplay=card.gameObject.GetComponent<Carddisplay>();
                        if(temporaldisplay.type!=Card.Type.Golden)
                        {    
                            if(temporaldisplay.power>max)
                            {
                                max=temporaldisplay.power;
                                destroyedcard=card.gameObject;
                            }
                        }
                    }
                }
            }
            if(destroyedcard!=prefab)
            {
                destroyedcard.transform.parent.gameObject.GetComponent<DropZone>().cardlist.Remove(destroyedcard.GetComponent<Carddisplay>().card);
                Destroy(destroyedcard);
            }
        }
    }

    void DestroyWeak(Carddisplay display)
    {
        if(display.effect==Card.Effect.DestroyWeak)
        {
            Debug.Log("destroyweak");
            int min=int.MaxValue;
            GameObject destroyedcard=prefab;
            
            Debug.Log($"effectplayer{gameMaster.currentplayer.gameObject.name}");
            foreach(Transform dropzone in gameMaster.currentplayer.field.unitRows.transform)
            {
                if(dropzone.childCount>0)
                {   
                    foreach(Transform card in dropzone)
                    {   
                        temporaldisplay=card.gameObject.GetComponent<Carddisplay>();
                        
                        if(temporaldisplay.type!=Card.Type.Golden)
                        {    
                            if(temporaldisplay.power<min)
                            {
                                min=temporaldisplay.power;
                                destroyedcard=card.gameObject;
                            }
                        }
                    }
                }
            }
            Debug.Log($"min {min}");

            if(min!=int.MaxValue)
            {
                Debug.Log($"card {destroyedcard.GetComponent<Carddisplay>().cardname}");
                destroyedcard.transform.parent.gameObject.GetComponent<DropZone>().cardlist.Remove(destroyedcard.GetComponent<Carddisplay>().card);
                Destroy(destroyedcard);  
            }
        }
    }

    void Average(Carddisplay display)
    {
        if(display.effect==Card.Effect.Average)
        {
            int unitamount=0;
            foreach(Transform dropzone in gameMaster.player1.field.unitRows.transform)
            {
                unitamount+=dropzone.gameObject.GetComponent<DropZone>().cardlist.Count;
            }
            foreach(Transform dropzone in gameMaster.player2.field.unitRows.transform)
            {
                unitamount+=dropzone.gameObject.GetComponent<DropZone>().cardlist.Count;
            }
            int average=(gameMaster.player1.totalpower+gameMaster.player2.totalpower)/unitamount;

            foreach(Transform dropzone in gameMaster.player1.field.unitRows.transform)
            {
                if(dropzone.childCount>0)
                {   
                    dropzone.GetComponent<ModifyingConditions>().averaged=true;
                    foreach(Transform card in dropzone)
                    {   
                        temporaldisplay=card.gameObject.GetComponent<Carddisplay>();
                        
                        if(temporaldisplay.type!=Card.Type.Golden)
                        {    
                            temporaldisplay.power=average;
                            temporaldisplay.powerText.text=average.ToString();
                            temporaldisplay.powerText.color=Color.yellow;
                            temporaldisplay.averaged=true;
                        }
                    }
                }
            }
            foreach(Transform dropzone in gameMaster.player2.field.unitRows.transform)
            {
                if(dropzone.childCount>0)
                {
                    dropzone.GetComponent<ModifyingConditions>().averaged=true;
                    foreach(Transform card in dropzone)
                    {   
                        temporaldisplay=card.gameObject.GetComponent<Carddisplay>();
                        if(temporaldisplay.type!=Card.Type.Golden)
                        {    
                            temporaldisplay.power=average;
                            temporaldisplay.powerText.text=average.ToString();
                            temporaldisplay.powerText.color=Color.yellow;
                            temporaldisplay.averaged=true;
                        }
                    }
                }
            } 
        }
    }




}
