using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FieldDropzone : DropZone{
    public bool weatheraffected=false;
    public bool boostaffected=false;

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
            Carddisplay display= child.GetComponent<Carddisplay>();
            int [] powers = (display.card as FieldCard).powers;
            display.powerText.text=powers[3].ToString();
            if(powers[3]>powers[0]) Card.CardPowerImageColorchange(child,Color.green);
            else if(powers[3]<powers[0]) Card.CardPowerImageColorchange(child, Color.red);
            else Card.CardPowerImageColorchange(child, Color.black);
        }
             
        Debug.Log("Updated");
    }
}
