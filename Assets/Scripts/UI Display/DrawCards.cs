using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AddCards : MonoBehaviour
{
    public GameObject card;
    public GameObject PlayerHand;

    
    public void Add(int n)
    {
        GameObject playercard= Instantiate(card,new Vector3(0,0,0),Quaternion.identity);
        playercard.transform.SetParent(PlayerHand.transform,false);
        Carddisplay carddisplay= playercard.GetComponent<Carddisplay>();
        carddisplay.displayId=n;
        carddisplay.update=true;
        if(gameObject.GetComponent<Hand>().player1)
        {
            carddisplay.player1=true;
        }   
    }
}
