using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Field : GameComponent
{
    public GameObject unitRows;
    public GameObject boostSlots;
    public Dictionary<Card, GameObject> bodies;
    public Dictionary<GameObject, FieldDropZone> bodiesRows;

    void Awake(){
        bodies =new Dictionary<Card, GameObject>();
        bodiesRows= new Dictionary<GameObject, FieldDropZone>();
        cards=new List<Card>();
    }

    public void Clear()
    {
        foreach(Transform child in unitRows.transform)
        {
            child.gameObject.GetComponent<DropZone>().ZoneClear();
        }
        foreach(Transform child in boostSlots.transform)
        {
            child.gameObject.GetComponent<DropZone>().ZoneClear();
        }
        bodies.Clear();
        bodiesRows.Clear();
        cards.Clear();
    }

    // Assigning unit row to any card, for cases where card-additive methods in the field have no specified row
    FieldDropZone AssignRow(Card card){
        //First we fill a list with all posible dropzones where the card can be instantiated
        List<GameObject> PossibleRows = new List<GameObject>();
        foreach(Card.Position position in card.positions){
            switch(position){
                case Card.Position.Melee: PossibleRows.Add(unitRows.transform.GetChild(0).gameObject); break;
                case Card.Position.Ranged: PossibleRows.Add(unitRows.transform.GetChild(1).gameObject); break;
                case Card.Position.Siege: PossibleRows.Add(unitRows.transform.GetChild(2).gameObject); break;
            }
        }
        //get the row with less cards
        return PossibleRows.OrderBy(r => r.GetComponent<FieldDropZone>().Size).Select(r => r.GetComponent<FieldDropZone>()).First();
    }

    public void Push(Card card, FieldDropZone row){
        if(!(card is FieldCard)) throw new InvalidOperationException("Card can't be placed in field");
        cards.Add(card);
        row.cardlist.Add(card);
        GameObject body = GameTools.CreateCardInObject(card, row.gameObject, owner);
        bodiesRows[body] = row;
        row.children[card] = body;
        bodies[card] = body;
        //Add the card to the board as well and assigning the card this field
        GlobalContext.gameMaster.board.cards.Add(card);
        GlobalContext.gameMaster.board.cardField[card]=this;
    }

    public override void Push(Card card)
    {
        Push(card, AssignRow(card));
    }

    public override Card Pop()
    {
        if(Size==0) throw new IndexOutOfRangeException("Cannot apply pop method to an empty list");
        Card removed = cards[Size - 1];
        cards.RemoveAt(Size - 1);
        bodiesRows[bodies[removed]].children.Remove(removed);
        Destroy(bodies[removed]);
        bodies.Remove(removed);
        return removed;
    }

    public override void Remove(Card card)
    {
        cards.Remove(card);
        bodiesRows[bodies[card]].children.Remove(card);
        Destroy(bodies[card]);
        bodies.Remove(card);
    }

    public override void SendBottom(Card card)
    {
        if(!(card is FieldCard)) throw new InvalidOperationException("Card can't be placed in field");
        cards.Insert(0, card);
        FieldDropZone row = AssignRow(card);
        GameObject body = GameTools.CreateCardInObject(card, row.gameObject, owner);
        bodiesRows[body] = row;
        row.children[card] = body;
        bodies[card] = body;
        GlobalContext.gameMaster.board.cards.Add(card);
        GlobalContext.gameMaster.board.cardField[card]=this;
    }
}

