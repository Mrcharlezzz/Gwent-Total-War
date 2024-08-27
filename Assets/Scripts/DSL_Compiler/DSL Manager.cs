using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DSLManager : MonoBehaviour
{
    public TextMeshProUGUI SelectionText;
    public TextMeshProUGUI source;
    public TMP_InputField outputField;

    void Start(){
        DSL.outputField= outputField;
    }

    /// <summary>
    /// Trims the editor input string and run the compiler
    /// </summary>
    public void CreateCard(){
        string aux=source.text.Substring(0,source.text.Length-1);
        DSL.Compile(aux.Trim());
    }

    /// <summary>
    /// Changes destiny deck
    /// </summary>
    public void DeckSwitch(){
        DSL.DeckSwitch();
        switch (DSL.destinyDeck) {
            case 0: SelectionText.text = "Deck 1"; break;
            case 1: SelectionText.text = "Deck 2"; break;
        }
    }
}
