using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DSLManager : MonoBehaviour
{
    public TextMeshProUGUI SelectionText;
    void Start (){
        Database.Initialize();
    }
    public void CreateCard(){
        DSL.Compile(CompilationSource.Source);
        Database.SaveData();
    }

    public void DeckSwitch(){
        DSL.DeckSwitch();
        switch (DSL.destinyDeck) {
            case 0: SelectionText.text = "Storage"; break;
            case 1: SelectionText.text = "Deck 1"; break;
            case 2: SelectionText.text = "Deck 2"; break;
        }
    }
}
