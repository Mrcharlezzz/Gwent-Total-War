using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Carddisplay : MonoBehaviour
{
    public Card card;
    public int displayId; // this will allow me to display different cards from the inspector in the same Card GameObject
    public bool update=false;
    public bool player1=false;
    public bool averaged=false;

    //References to GameObject

    public Image CardImage;
    public TextMeshProUGUI posText;
    public TextMeshProUGUI powerText;
    public GameObject powerborder;

    
    // Start is called before the first frame update
    
    public void DisplayUpdate()
    {
        
        card=Database.Search(displayId);
            
        //Layer use for decoy collisions
        if(card.type==Card.Type.Decoy)
        {
            int decoyLayerIndex = LayerMask.NameToLayer("Decoy");
            gameObject.layer=decoyLayerIndex;
            gameObject.GetComponent<BoxCollider2D>().size=new Vector2(20.0f,30.0f);
        }

        /*
        When the card is drawn, the power modifications of previous games
        (these may remain if the execution was interrupted) must be reverted
        */ 
        Debug.Log("POWERS RESET");
        if(card is FieldCard fieldCard){
            for(int i=1; i<4 ;i++) fieldCard.powers[i]=fieldCard.powers[i-1];
            powerText.text=fieldCard.powers[3].ToString();
        }
        else{
            powerText.text="";
            powerborder.SetActive(false);
        }

        posText.text=PositionString(card.positions);
        CardImage.sprite=Resources.Load<Sprite>($"CardImages/{card.image}");      
    }
    public static string PositionString(List<Card.Position> positions){
        bool melee=false, ranged=false, siege=false;
        foreach(Card.Position pos in positions){
            switch(pos){
                case Card.Position.Melee: melee=true; break;
                case Card.Position.Ranged: ranged=true; break;
                case Card.Position.Siege: siege=true; break;
            }
        }
        string result="";
        if(melee) result+="Melee ";
        if(ranged) result+=", Ranged ";
        if(siege) result+=" Siege";
        return result;
    }
    
}
