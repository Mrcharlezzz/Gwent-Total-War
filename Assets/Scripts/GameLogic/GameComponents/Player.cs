using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public GameMaster gameMaster;
    public Hand hand;
    public Deck playerdeck;
    public GraveYard graveyard;
    public GraveYard othergraveyard; 
    public Field field;
    public LeaderCard leaderCard;
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


    //playcard fields
    bool isValidType= false;
    bool isValidPosition=false;
    bool availableSlot=false;
    bool playable=false;
    bool isValidField=false;

    
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
            if(!player1&&leaderCard.Alejandro)
            {
                totalpower=totalpower+totalpower/5;
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
        DropZone zone=dropZone.GetComponent<DropZone>();
        Carddisplay display=card.GetComponent<Carddisplay>();
        
        //Checking if playing conditions are satisfied
        Conditions(display,zone);
        
        //Special cards and playing handling
        WeatherCardHandling(display,zone);
        BoostHandling(display,zone);
        PlayOrReturn(display,zone);
        ClearHandling(display,zone);
        DecoyHandling(display,dropZone);


        //Effects handling
        if((display.type==Card.Type.Golden||display.type==Card.Type.Silver)&&playable&&display.effect!=Card.Effect.None)
        {
            SummonBoost(display);
            SummonWeather(display);
            PowerxNTimes(display,dropZone);
            DrawEffect(display);
            DestroyStrong(display);
            DestroyWeak(display);
            Average(display);
            RowCleanUp(display);
        }

        //resetting playing conditions
        isValidType= false;
        isValidPosition=false;
        availableSlot=false;
        playable=false;
        isValidField=false;
    }


    
    void Conditions(Carddisplay display,DropZone zone)
    {
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
    }

    //Special cards and playing handling Methods
    void WeatherCardHandling(Carddisplay display,DropZone zone)     
    {
        if(display.type==Card.Type.Weather&&playable)
        {
            Debug.Log ("weather");
            conditions0=zone.gameObject.GetComponent<BufferLink>().dropzones[0].GetComponent<ModifyingConditions>();
            conditions1=zone.gameObject.GetComponent<BufferLink>().dropzones[1].GetComponent<ModifyingConditions>();
            
            conditions0.modified=true;
            conditions1.modified=true;
            Debug.Log("dropzone modified BBB");
            
            conditions0.weatheraffected=true;
            conditions1.weatheraffected=true;
            Debug.Log("Weather Affected");
            
            if(zone.cardlist.Count!=0)
            foreach (Transform child in zone.gameObject.transform)
            {
                if(child.gameObject.GetComponent<Carddisplay>().position==display.position)
                {
                    zone.cardlist.Remove(zone.cardlist[0]);

                    if(child.gameObject.GetComponent<Carddisplay>().player1==player1)
                    {
                        graveyard.Add(child.gameObject.GetComponent<Carddisplay>().card);
                    }
                    else{
                        othergraveyard.Add(child.gameObject.GetComponent<Carddisplay>().card);
                    }
                    Destroy(child.gameObject);
                }
            }
            conditions0.averaged=false;
            conditions1.averaged=false;
        }
    }

    void BoostHandling(Carddisplay display,DropZone zone)
    {  
        if(display.type==Card.Type.Boost&&playable)
        {
            conditions0=zone.gameObject.GetComponent<BufferLink>().dropzones[0].GetComponent<ModifyingConditions>();
            conditions0.boostaffected=true;
            conditions0.averaged=false;
            conditions0.modified=true;
            conditions0.boostamount=display.power; 
        }
    }

    void PlayOrReturn(Carddisplay display,DropZone zone)       
    {
        //If necessary playing conditions are satisfied then play card, else return card to its original position
        if(playable&&(display.type!=Card.Type.Decoy))
        {
            display.gameObject.GetComponent<DragandDrop>().alreadyplayed=true; Debug.Log("Played succesfully");
            
            hand.RemoveCard(display.card);
            zone.cardlist.Add(display.card);
            if(zone.isUnitRow)
            {
                zone.gameObject.GetComponent<ModifyingConditions>().modified=true;
            }
            
            display.gameObject.transform.SetParent(zone.gameObject.transform,false);

            gameMaster.globalModified=true;
        }
        else
        {
            display.gameObject.transform.position=display.gameObject.GetComponent<DragandDrop>().startPosition;
        }
    }

    void ClearHandling(Carddisplay display, DropZone zone)
    {
        if(display.type==Card.Type.Clear&&playable)
        {
            Debug.Log("Cleared");
            Transform weatherslots=zone.gameObject.transform.parent;
            
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
                    if(child.GetChild(0).gameObject.GetComponent<Carddisplay>().player1==player1)
                    {
                        graveyard.Add(child.GetChild(0).gameObject.GetComponent<Carddisplay>().card);
                    }
                    else{
                        othergraveyard.Add(child.GetChild(0).gameObject.GetComponent<Carddisplay>().card);
                    }
                    Destroy(child.GetChild(0).gameObject);
                }
            }
            
            Destroy(display.gameObject);
            graveyard.Add(display.gameObject.GetComponent<Carddisplay>().card);
        }
    }

    void DecoyHandling(Carddisplay display, GameObject dropzone)
    {
        if(display.type==Card.Type.Decoy)    
        {    
            GameObject card2=dropzone.gameObject;//In this case the decoy collides with a card, not a dropzone due to the layer dispositions
            GameObject parent=card2.transform.parent.gameObject;
            Carddisplay card2display=card2.GetComponent<Carddisplay>();
            if(parent.GetComponent<ModifyingConditions>()!=null&&card2.GetComponent<Carddisplay>().type!=Card.Type.Golden&&parent.GetComponent<DropZone>().player1==player1)
            {
                card2display.power=card2display.basepower;

                CardPowerImageColorchange(card2.transform,Color.white);
                
                hand.AddCard(card2display.card);
                parent.GetComponent<DropZone>().cardlist.Remove(card2display.card);
                Destroy(card2);
                parent.GetComponent<ModifyingConditions>().modified=true;

                parent.GetComponent<DropZone>().cardlist.Add(display.card);
                display.gameObject.transform.SetParent(parent.transform);
                
                if(card2display.effect!=Card.Effect.None)
                {
                    parent.GetComponent<ModifyingConditions>().modified=true;
                    parent.GetComponent<ModifyingConditions>().powerXntimesAffected=true;
                }

                display.gameObject.GetComponent<DragandDrop>().alreadyplayed=true;

                gameMaster.NextTurn();
            }
            else
            {
                display.gameObject.transform.position=display.gameObject.GetComponent<DragandDrop>().startPosition;
            }
        }
    }
    
    
    //Effects
    void SummonBoost(Carddisplay display)
    {
        if(display.effect==Card.Effect.SummonBoost)
        {
            int boostID=0;

            if(display.faction==Card.Faction.Rome)
            boostID=4;
            else
            boostID=21;

            GameObject boostSlots;
            if(player1)
            {
                boostSlots=GameObject.Find("boostSlots");
            }
            else
            {
                boostSlots=GameObject.Find("boostSlots2");
            }
    
                
            DropZone boostslot=GameObject.Find("MeleeBoost1").GetComponent<DropZone>();
            foreach(Transform child in boostSlots.transform)
            {
                DropZone childzone=child.gameObject.GetComponent<DropZone>();
                if(childzone.positionlist[0]==display.effectposition)
                {
                    boostslot=childzone;
                }
            }
            if(boostslot.transform.childCount>0)
            {
                Destroy(boostslot.transform.GetChild(0).gameObject);
                graveyard.Add(boostslot.transform.GetChild(0).gameObject.GetComponent<Carddisplay>().card);
            }
            boostslot.cardlist.Clear();

            GameObject boost= Instantiate(prefab,new Vector3(0,0,0),Quaternion.identity);
            boost.transform.SetParent(boostslot.transform,false);
            boost.GetComponent<DragandDrop>().alreadyplayed=true;
            Carddisplay carddisplay= boost.GetComponent<Carddisplay>();
            carddisplay.displayId=boostID;
            carddisplay.update=true;


            boostslot.cardlist.Add(carddisplay.card);
                    
            conditions0=boostslot.gameObject.GetComponent<BufferLink>().dropzones[0].GetComponent<ModifyingConditions>();

            //carddisplay updates after this line is executed, therefore there must be a call to the original card
            conditions0.boostamount=Resources.Load<Card>($"{boostID}").power; 
                    
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
            int weatherID;

            if(display.faction==Card.Faction.Rome)
            weatherID=24;
            else
            weatherID=23;

            GameObject weatherSlots=GameObject.Find("weatherSlots");
                
            DropZone weatherSlot=GameObject.Find("MeleeWeather").GetComponent<DropZone>();
            foreach(Transform child in weatherSlots.transform)
            {
                DropZone childzone=child.gameObject.GetComponent<DropZone>();
                if(childzone.positionlist[0]==display.effectposition)
                {
                    weatherSlot=childzone;
                }
            }
            if(weatherSlot.transform.childCount>0)
            {
                Destroy(weatherSlot.transform.GetChild(0).gameObject);

                if(weatherSlot.transform.GetChild(0).gameObject.GetComponent<Carddisplay>().player1==player1)
                {
                    graveyard.Add(weatherSlot.transform.GetChild(0).gameObject.GetComponent<Carddisplay>().card);
                }
                else{
                   othergraveyard.Add(weatherSlot.transform.GetChild(0).gameObject.GetComponent<Carddisplay>().card); 
                }
            }
            weatherSlot.cardlist.Clear();

            GameObject weather= Instantiate(prefab,new Vector3(0,0,0),Quaternion.identity);
            weather.transform.SetParent(weatherSlot.transform,false);
            weather.GetComponent<DragandDrop>().alreadyplayed=true;
            Carddisplay carddisplay= weather.GetComponent<Carddisplay>();
            carddisplay.displayId=weatherID;
            carddisplay.update=true;

            weatherSlot.cardlist.Add(carddisplay.card);

            conditions0=weatherSlot.gameObject.GetComponent<BufferLink>().dropzones[0].GetComponent<ModifyingConditions>();
            conditions0.weatheraffected=true;
            conditions0.averaged=false;
            conditions0.modified=true;

            conditions1=weatherSlot.gameObject.GetComponent<BufferLink>().dropzones[1].GetComponent<ModifyingConditions>(); 
            conditions1.weatheraffected=true;
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
            List<GameObject> possiblydestroyed=new List<GameObject>();
            
            
            foreach(Transform dropzone in gameMaster.player1.field.unitRows.transform)
            {
                if(dropzone.childCount>0)
                {   
                    foreach(Transform card in dropzone)
                    {   
                        temporaldisplay=card.gameObject.GetComponent<Carddisplay>();
                        
                        if(temporaldisplay.type!=Card.Type.Golden)
                        {    
                            if(temporaldisplay.power>=max)
                            {
                                max=temporaldisplay.power;
                                possiblydestroyed.Add(card.gameObject);
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
                            if(temporaldisplay.power>=max)
                            {
                                max=temporaldisplay.power;
                                possiblydestroyed.Add(card.gameObject);
                            }
                        }
                    }
                }
            }
            
            if(possiblydestroyed.Count>0)
            {    
                List<GameObject> toRemove = new List<GameObject>();
                foreach(GameObject strongcard in possiblydestroyed)
                {
                    if(strongcard.GetComponent<Carddisplay>().power<max)
                    {
                        toRemove.Add(strongcard);
                    }
                }
                foreach(GameObject item in toRemove)
                {
                    possiblydestroyed.Remove(item);
                }

                int random=Random.Range(0,possiblydestroyed.Count);
                GameObject destroyedcard=possiblydestroyed[random];
                if(destroyedcard!=prefab)
                {
                    destroyedcard.transform.parent.gameObject.GetComponent<DropZone>().cardlist.Remove(destroyedcard.GetComponent<Carddisplay>().card);
                    Destroy(destroyedcard);

                    if(destroyedcard.GetComponent<Carddisplay>().player1==player1)
                    {
                        graveyard.Add(destroyedcard.GetComponent<Carddisplay>().card);
                    }
                    else{
                        othergraveyard.Add(destroyedcard.GetComponent<Carddisplay>().card);
                    }
                    gameMaster.globalModified=true;
                }
            }
        }
    }

    void DestroyWeak(Carddisplay display)
    {
        if(display.effect==Card.Effect.DestroyWeak)
        {
            int min=int.MaxValue;
            List<GameObject> possiblydestroyed= new List<GameObject>();
            
            foreach(Transform dropzone in gameMaster.currentplayer.field.unitRows.transform)
            {
                if(dropzone.childCount>0)
                {   
                    foreach(Transform card in dropzone)
                    {   
                        temporaldisplay=card.gameObject.GetComponent<Carddisplay>();
                        
                        if(temporaldisplay.type!=Card.Type.Golden)
                        {    
                            if(temporaldisplay.power<=min)
                            {
                                min=temporaldisplay.power;
                                possiblydestroyed.Add(card.gameObject);
                            }
                        }
                    }
                }
            }

            List<GameObject> toRemove = new List<GameObject>();
            foreach(GameObject weakcard in possiblydestroyed)
            {
                if(weakcard.GetComponent<Carddisplay>().power>min)
                {
                    toRemove.Add(weakcard);
                }
            }
            foreach(GameObject item in toRemove)
            {
                possiblydestroyed.Remove(item);
            }

            if(possiblydestroyed.Count>0)
            {
                int random=Random.Range(0,possiblydestroyed.Count);
                GameObject destroyedcard=possiblydestroyed[random];

                destroyedcard.transform.parent.gameObject.GetComponent<DropZone>().cardlist.Remove(destroyedcard.GetComponent<Carddisplay>().card);
                Destroy(destroyedcard);  
                
                othergraveyard.Add(destroyedcard.GetComponent<Carddisplay>().card);
                gameMaster.globalModified=true;
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
                            CardPowerImageColorchange(card.transform,Color.yellow);
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
                            temporaldisplay.card.power=average;
                            temporaldisplay.powerText.text=average.ToString();
                            CardPowerImageColorchange(card.transform,Color.yellow);
                            temporaldisplay.averaged=true;
                        }
                    }
                }
            } 
        }
    }

    void RowCleanUp(Carddisplay display)
    {
        if(display.effect==Card.Effect.RowCleanup)
        {
            List<GameObject> possiblecleanzones= new List<GameObject>();
            int min=int.MaxValue;

            foreach (Transform dropzone in gameMaster.player1.field.unitRows.transform)
            {
                int cardcount=0;
                if(dropzone.childCount>0)
                {
                    foreach(Transform card in dropzone)
                    {
                        if(card.gameObject.GetComponent<Carddisplay>().type!=Card.Type.Golden)
                        {
                            cardcount++;
                        }
                    }
                }
                if(cardcount<min&&cardcount>0)
                {
                    min =cardcount;
                    possiblecleanzones.Add(dropzone.gameObject);
                }
            
            }

            foreach (Transform dropzone in gameMaster.player2.field.unitRows.transform)
            {
                int cardcount=0;
                if(dropzone.childCount>0)
                {
                    foreach(Transform card in dropzone)
                    {
                        if(card.gameObject.GetComponent<Carddisplay>().type!=Card.Type.Golden)
                        {
                            cardcount++;
                        }
                    }
                }
                if(cardcount<min&&cardcount>0)
                {
                    min =cardcount;
                    possiblecleanzones.Add(dropzone.gameObject);
                }
            }

            List<GameObject> toRemove= new List<GameObject>();
            foreach(GameObject cleanzone in possiblecleanzones)
            {
                int count=0;
                foreach(Transform card in cleanzone.transform)
                {
                    if(card.gameObject.GetComponent<Carddisplay>().type!=Card.Type.Golden)
                    {
                        count++;
                    }
                }
                if(count>min)
                {
                    toRemove.Add(cleanzone);
                }
            }
            foreach(GameObject item in toRemove)
            {
                possiblecleanzones.Remove(item);
            }
            if(possiblecleanzones.Count>0)
            {
                int random=Random.Range(0,possiblecleanzones.Count);
                GameObject Cleanzone=possiblecleanzones[random];

                
                foreach(Transform card in Cleanzone.transform)
                {
                    temporaldisplay=card.gameObject.GetComponent<Carddisplay>();
                    if(temporaldisplay.type!=Card.Type.Golden)
                    {
                        Cleanzone.GetComponent<DropZone>().cardlist.Remove(temporaldisplay.card);
                        
                        if(temporaldisplay.player1==player1)
                        {
                            graveyard.Add(temporaldisplay.card);
                        }
                        else{
                            othergraveyard.Add(temporaldisplay.card);
                        }
                        Destroy(card.gameObject);

                        gameMaster.globalModified=true;
                    }
                }
            }
        }
    }
    static void CardPowerImageColorchange(Transform child,Color color)
    {
        child.GetChild(1).GetChild(0).GetChild(0).gameObject.GetComponent<Image>().color=color;
    }
}