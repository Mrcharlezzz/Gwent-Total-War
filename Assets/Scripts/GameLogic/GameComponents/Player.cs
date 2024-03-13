using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Hand hand;
    public Deck playerdeck;
    public GraveYard graveyard;
    public LeaderCard leaderCard;
    public int totalpower;

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
        if(isValidType&&isValidPosition&&availableSlot)
        {
            playable=true;
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
            if(parent.GetComponent<ModifyingConditions>()!=null&&card2.GetComponent<Carddisplay>().type!=Card.Type.Golden)
            {
                card2display.power=card2display.basepower;
                card2display.powerText.color=Color.black;
                
                hand.AddCard(card2display.card);
                parent.GetComponent<DropZone>().cardlist.Remove(card2display.card);
                Destroy(card2);
                
                parent.GetComponent<DropZone>().cardlist.Add(display.card);
                card.transform.SetParent(parent.transform);
            }
            else
            {
                card.transform.position=card.GetComponent<DragandDrop>().startPosition;
            }
            
        
        }
        #endregion
    }
}
