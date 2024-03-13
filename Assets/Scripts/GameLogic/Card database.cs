using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEditor.VersionControl;
using UnityEngine;

public class CardDatabase: MonoBehaviour
{   
    
    public static List<Card> leadercardlist =new List<Card>()
    {
        new Card(-1,"Julio Cesar",Resources.Load<Sprite>("Ramses2"),Card.Type.Leader,"Padre del imperio romano\n Efecto: +2 de poder para el ejército y en caso de empate garantiza la victoria",Card.Faction.Rome,-1,-1,Card.Position.L),
        new Card(-2,"Alejandro Magno",Resources.Load<Sprite>("Ramses2"),Card.Type.Leader,"Lider de los griegos(cambiar)\n Efecto: 1 vez por ronda roba una carta del mazo",Card.Faction.Greece,-1,-1,Card.Position.L),
        new Card(-3,"Ramses II",Resources.Load<Sprite>("Ramses2"),Card.Type.Leader,"Lider de los Egipcios(cambiar)\n Efecto: Crear",Card.Faction.Greece,-1,-1,Card.Position.L),           
    };
    public static List<Card> cardlist =new List<Card>()
    {
        new Card(0,null,null,Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(1,null,null,Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(2,null,null,Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(3,"Espacartaco",Resources.Load<Sprite>("Ramses2"),Card.Type.Golden,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(4,"Marco Aurelio",Resources.Load<Sprite>("Ramses2"),Card.Type.Golden,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(5,"Máximo Décimo Meridio",Resources.Load<Sprite>("Ramses2"),Card.Type.Golden,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(6,"Cayo Mario",Resources.Load<Sprite>("Ramses2"),Card.Type.Golden,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(7,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(8,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(9,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(10,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(11,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(12,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(13,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(14,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(15,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(16,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(17,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(18,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(19,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(20,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(21,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(22,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(23,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(24,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(25,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(26,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(27,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(28,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(29,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(30,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
        new Card(31,null,Resources.Load<Sprite>("Ramses2"),Card.Type.Silver,null,Card.Faction.Neutral,0,0,Card.Position.MRS),
    };
    
  
    
}
