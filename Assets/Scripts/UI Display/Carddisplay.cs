using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Carddisplay : MonoBehaviour
{
    public Card card;
    public Unit auxcard;
    public int displayId; // this will allow me to display different cards from the inspector in the same Card GameObject
    //References to class Card
    public int id;
    public string cardname;
    public Sprite image;
    public Card.Type type;
    public string carddescription;
    public Unit.IntEffect effect;
    public Card.Faction faction;
    public int power;
    public Card.Position position;
    public bool update=false;
    public bool player1=false;
    public bool averaged=false;

    //References to GameObject

    public Image CardImage;
    public TextMeshProUGUI posText;
    public TextMeshProUGUI powerText;
    public GameObject powerborder;

    
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
            card=Database.Search(displayId);
            
            /*
            When the card is drawn, the power modifications of previous games
            (these may remain if the execution was interrupted) must be reverted
            */ 


            id= card.id;
            cardname= card.cardname;
            image= card.image;
            type= card.type;
            carddescription= card.carddescription;
            faction= card.faction;

            if(card.IsUnit())
            {
                auxcard=(Unit)card;
                power= auxcard.powers[4];
                position= auxcard.position;
                effect=auxcard.effect;
            }

            //Layer use for decoy collisions

            if(type==Card.Type.Decoy)
            {
                int decoyLayerIndex = LayerMask.NameToLayer("Decoy");
                gameObject.layer=decoyLayerIndex;
                gameObject.GetComponent<BoxCollider2D>().size=new Vector2(20.0f,30.0f);
            }

            powerText.text=power.ToString();
            posText.text=position.ToString();
            CardImage.sprite=image;

            //Not showing card power
            if(card.IsUnit())
            {
                powerborder.SetActive(false);
            }
        }
    }
    
}
