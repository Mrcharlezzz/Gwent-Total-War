using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class GraveYard : GameComponent
{
    public void Modify(){
        if(Size==0){ 
            Hide();
            return;
        }
        Show();
        gameObject.GetComponent<Carddisplay>().displayId=cards[^1].id;
        gameObject.GetComponent<Carddisplay>().update=true;

    }

    void Show(){
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        gameObject.transform.GetChild(1).gameObject.SetActive(true);
    }

    void Hide(){
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        gameObject.transform.GetChild(1).gameObject.SetActive(false);
    }

    public override void Push(Card card)
    {
        cards.Add(card);
    }
    public override void Remove(Card card)
    {
        cards.Remove(card);
    }
    public override Card Pop()
    {
        if (Size == 0) throw new IndexOutOfRangeException("Cannot apply pop method to an empty list");
        Card removed = cards[Size - 1];
        cards.RemoveAt(Size - 1);
        return removed;
    }

    public override void SendBottom(Card card)
    {
        cards.Insert(0, card);
    }
}
