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
    public string carddescription;
    public Card.Faction faction;
    public bool Displaypower {get{
        if(power<0)
            return false;
        else
            return true;
    }}

    public int basepower;
    public int power;
    public Card.Position position;

    //References to GameObject

    public Image CardImage;
    public TextMeshProUGUI posText;
    public TextMeshProUGUI powerText;
    public Image borderseparation;

    
    // Start is called before the first frame update
    void Start()
    {
        
        card=Resources.Load<Card>($"{displayId}");
        Debug.Log($"1- {displayId}");
        id= card.id;
        cardname= card.cardname;
        image= card.image;
        type= card.type;
        carddescription= card.carddescription;
        faction= card.faction;
        basepower=card.basepower;
        power= card.power;
        position= card.position;

        if(type==Card.Type.Decoy)
        {
            int decoyLayerIndex = LayerMask.NameToLayer("Decoy");
            gameObject.layer=decoyLayerIndex;
            gameObject.GetComponent<BoxCollider2D>().size=new Vector2(20.0f,30.0f);
        }

        powerText.text=power.ToString();
        posText.text=position.ToString();
        CardImage.sprite=image;
        if(Displaypower)// Chequear estas condicionales
        {
            powerText.gameObject.SetActive(true);
            borderseparation.gameObject.SetActive(true);
        }
        else
        {
            powerText.gameObject.SetActive(false);
            borderseparation.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Displaypower)
        {
            powerText.gameObject.SetActive(true);
            borderseparation.gameObject.SetActive(true);
        }
        else
        {
            powerText.gameObject.SetActive(false);
            borderseparation.gameObject.SetActive(false);
        }
    }
}
