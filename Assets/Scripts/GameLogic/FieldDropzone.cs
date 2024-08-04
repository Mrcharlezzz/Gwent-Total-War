using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FieldDropZone : DropZone{
    public bool weatheraffected=false;
    public bool boostaffected=false;
    public Dictionary<Card, GameObject> children;
    public int Size{get => children.Count;}

    void Awake(){
        cardlist=new List<Card>();
        children = new Dictionary<Card, GameObject>();
    }

    public void Modify()
    {
        if (boostaffected){
            foreach (FieldCard card  in cardlist){
                card.powers[2] = card.powers[1]*2;
            }
        }
        else foreach (FieldCard card in cardlist){
            card.powers[2]=card.powers[1];
        }
        if(weatheraffected){
            foreach (FieldCard card in cardlist){
                card.powers[3]=1;
            }
        }
        else foreach (FieldCard card in cardlist){
            card.powers[3]=card.powers[2];
        }

        foreach(Transform child in gameObject.transform){
            Carddisplay display= child.gameObject.GetComponent<Carddisplay>();
            int [] powers = (display.card as FieldCard).powers;
            display.powerText.text=powers[3].ToString();
            string debug = display.powerText.text;
            if(powers[3]>powers[0]) Card.CardPowerImageColorchange(child,Color.green);
            else if(powers[3]<powers[0]) Card.CardPowerImageColorchange(child, Color.red);
            else Card.CardPowerImageColorchange(child, Color.white);
        }
             
        Debug.Log("Updated");
    }
}
