using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DSLManager : MonoBehaviour
{
    public TextMeshProUGUI SelectionText;
    public TextMeshProUGUI source;
    
    public void CreateCard(){
        string aux=source.text.Substring(0,source.text.Length-1);
        DSL.Compile(aux.Trim());
        Debug.Log("Source: " + aux);
        Debug.Log("last " + aux[aux.Length-1]);
    }

    public void DeckSwitch(){
        DSL.DeckSwitch();
        switch (DSL.destinyDeck) {
            case 0: SelectionText.text = "Deck 1"; break;
            case 1: SelectionText.text = "Deck 2"; break;
        }
    }
}
