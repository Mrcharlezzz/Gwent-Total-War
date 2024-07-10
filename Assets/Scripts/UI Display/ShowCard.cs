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
    public GameObject cardFaction;
    public GameObject cardType;
    public GameObject RangeRow1;
    public GameObject RangeRow2;
    public GameObject PassButton;

    public void Awake()//References to info console children
    {
        tempCanvas=GameObject.Find("Canvas");
        cardName=GameObject.Find("cardName");
        cardDescription=GameObject.Find("cardDescription");
        cardFaction=GameObject.Find("cardFaction");
        cardType=GameObject.Find("cardType");
        gamemaster=GameObject.Find("gamemaster");
        RangeRow1=GameObject.Find("RangeRow");
        RangeRow2=GameObject.Find("RangeRow2");
        PassButton=GameObject.Find("Pass");
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
        Card.Type? type=gameObject.GetComponent<Carddisplay>().card.type;
        cardName.GetComponent<TextMeshProUGUI>().text="Nombre: "+ gameObject.GetComponent<Carddisplay>().card.name;
        cardDescription.GetComponent<TextMeshProUGUI>().text="Descripción: "+ gameObject.GetComponent<Carddisplay>().card.description;
        cardFaction.GetComponent<TextMeshProUGUI>().text="Facción: "+ SpanishTranslator(gameObject.GetComponent<Carddisplay>().card.faction.ToString());
        cardType.GetComponent<TextMeshProUGUI>().text="Tipo: "+ SpanishTranslator(gameObject.GetComponent<Carddisplay>().card.type.ToString());
        

        //Different instantiation position for leaders and graveyards particular cases
        Vector2 position;
        
        position=Input.mousePosition.y>PassButton.transform.position.y ? RangeRow1.transform.position : RangeRow2.transform.position;

        // Instantiate the card at the determined position
        showCard=Instantiate(gameObject, new Vector2(0,0), Quaternion.identity);
        
        showCard.transform.SetParent(tempCanvas.transform, false);
        showCard.transform.position=position;
        
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
        
        //Destroy zoom
        Destroy(showCard);
    }

    public string SpanishTranslator(string a)
    {
        switch(a)
        {
            case "Rome": return"Roma";
            case "Greece": return"Grecia";
            case "Silver": return"Unidad de Plata";
            case "Golden": return"Unidad de Oro";
            case "Boost": return"Aumento";
            case "Weather": return"Clima";
            case "Decoy": return"Señuelo";
            case "Clear": return"Despeje";
            case "Leader": return"Líder";
            default: return a;
        }
    }
}
