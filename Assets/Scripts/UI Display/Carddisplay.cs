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
    //References to class Card
    public int id;
    public string cardname;
    public Sprite image;
    public Card.Type type;
    public Card.Effect effect;
    public Card.Faction faction;
    public string carddescription;
    public bool Displaypower {get{
        if(power<0)
            return false;
        else
            return true;
    }}

    public int basepower;
    public int power;
    public Card.Position position;
    public Card.Position effectposition;
    public bool update=false;
    public bool averaged=false;
    public bool player1=false;

    //References to GameObject

    public Image CardImage;
    public TextMeshProUGUI posText;
    public TextMeshProUGUI powerText;
    public Image borderseparation;

    
    // Start is called before the first frame update
    void Start()
    {
        update=true;
    }
    void Update()
    {
        if(update)
        {    
            update=false;
            card=Resources.Load<Card>($"{displayId}");
            id= card.id;
            cardname= card.cardname;
            image= card.image;
            type= card.type;
            carddescription= card.carddescription;
            faction= card.faction;
            basepower=card.basepower;
            power= card.power;
            position= card.position;
            effectposition=card.effectposition;
            effect=card.effect;

            if(type==Card.Type.Decoy)
            {
                int decoyLayerIndex = LayerMask.NameToLayer("Decoy");
                gameObject.layer=decoyLayerIndex;
                gameObject.GetComponent<BoxCollider2D>().size=new Vector2(20.0f,30.0f);
            }

            powerText.text=power.ToString();
            posText.text=position.ToString();
            CardImage.sprite=image;
        }
    }
}
