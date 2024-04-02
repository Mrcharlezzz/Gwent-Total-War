using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowCard : MonoBehaviour
{
    GameObject gamemaster;
    private GameObject tempCanvas;
    public GameObject showCard;
    private DragandDrop dragandDrop;
    public GameObject cardName;
    public GameObject cardDescription;
    public GameObject cardPower;
    public GameObject cardPosition;
    public GameObject cardFaction;
    public GameObject cardType;

    public void Awake()//References to info console children
    {
        tempCanvas=GameObject.Find("Canvas");
        cardName=GameObject.Find("cardName");
        cardDescription=GameObject.Find("cardDescription");
        cardPower=GameObject.Find("cardPower");
        cardPosition=GameObject.Find("cardPosition");
        cardFaction=GameObject.Find("cardFaction");
        cardType=GameObject.Find("cardType");
        gamemaster=GameObject.Find("gamemaster");
    }
    void Start()
    {
        dragandDrop=GetComponent<DragandDrop>();
    }
    
    public void PointerEnter()
    {
    if(!dragandDrop.dragging&&!gamemaster.GetComponent<GameMaster>().dragging)    
    {
        //show card info in info Console while mouse is on the card
        Card.Type type=gameObject.GetComponent<Carddisplay>().type;
        cardName.GetComponent<TextMeshProUGUI>().text="Name: "+gameObject.GetComponent<Carddisplay>().cardname;
        cardDescription.GetComponent<TextMeshProUGUI>().text="Description: "+gameObject.GetComponent<Carddisplay>().carddescription;
        cardFaction.GetComponent<TextMeshProUGUI>().text="Faction: "+gameObject.GetComponent<Carddisplay>().faction.ToString();
        cardType.GetComponent<TextMeshProUGUI>().text="Type: "+gameObject.GetComponent<Carddisplay>().type.ToString();
        if((type==Card.Type.Golden)||(type==Card.Type.Silver)||(type==Card.Type.Decoy))
        {
        cardPower.GetComponent<TextMeshProUGUI>().text="Power: "+gameObject.GetComponent<Carddisplay>().power.ToString();
        }
        if(type!=Card.Type.Clear)
        {
        cardPosition.GetComponent<TextMeshProUGUI>().text="Position: "+gameObject.GetComponent<Carddisplay>().position.ToString();
        }
        
        
        Vector2 position;

        if(gameObject.GetComponent<Carddisplay>().type==Card.Type.Leader||gameObject.name=="graveyard1"||gameObject.name=="graveyard2")
        {
            position =new Vector2(68,135);
        }
        else{
            position=Input.mousePosition.y>405 ? new Vector2(410,220): new Vector2(410, 415);
        }

        

        // Instantiate the card at the determined position
        showCard=Instantiate(gameObject, position, Quaternion.identity);
        showCard.GetComponent<Carddisplay>().card.power=gameObject.GetComponent<Carddisplay>().power;
        showCard.transform.SetParent(tempCanvas.transform, false);

        // Adjust the scale
        foreach(Transform child in showCard.transform)
        {
            child.localScale=new Vector3(3, 3, 3);
        }
    }
    
        
    }
    public void PointerExit()
    {
        
        //Clear info Console
        cardName.GetComponent<TextMeshProUGUI>().text=""; 
        cardDescription.GetComponent<TextMeshProUGUI>().text="";
        cardFaction.GetComponent<TextMeshProUGUI>().text="";
        cardType.GetComponent<TextMeshProUGUI>().text="";
        cardPower.GetComponent<TextMeshProUGUI>().text="";
        cardPosition.GetComponent<TextMeshProUGUI>().text="";
        
        //Destroy zoom
        Destroy(showCard);
        
        
    }
}
