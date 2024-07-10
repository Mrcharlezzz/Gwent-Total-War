using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms.Impl;


public abstract class Card : ScriptableObject
{
    public int id;
    public Player owner;
    public string cardname;
    public Sprite image;
    public Type? type;
    public string description;
    public string faction;
    public List<Position> positions;

    public Onactivation activation;

    public void ActivateEffect(Player triggerplayer)
    {
        activation.Execute(triggerplayer);
    }

    public virtual void Play(Player triggerplayer, GameObject body, GameObject dropzone)
    {
        DropZone zone = dropzone.GetComponent<DropZone>();
        bool ValidType = false, validPosition = false, availableslot = false, validField = false;

        foreach (Type type in zone.typelist)
        {
            if (this.type == type)
            {
                ValidType = true;
                break;
            }
        }
        foreach (Position pos in positions)
        {
            if (pos == zone.position)
            {
                validPosition = true;
                break;
            }
        }
        if (zone.cardlist.Count < zone.maxsize) availableslot = true;
        if (triggerplayer.player1 == zone.player1 || zone.typelist[0] == Type.Weather)
            validField = true;

        if (ValidType && validPosition && availableslot && validField)
        {

            body.GetComponent<DragandDrop>().alreadyplayed = true;
            Debug.Log("Played succesfully");

            triggerplayer.hand.RemoveCard(this);
            zone.cardlist.Add(this);

            body.transform.SetParent(zone.gameObject.transform, false);
            triggerplayer.gameMaster.NextTurn();
        }
        else
        {
            body.transform.position = body.GetComponent<DragandDrop>().startPosition;
        }
    }

    public enum Position
    {
        Melee, Ranged, Siege,
    }


    public enum Type
    {
        Leader,
        Golden,
        Silver,
        Weather,
        Boost,
        Decoy,
        Clear,
    }

    public static void CardPowerImageColorchange(Transform cardbody, Color color)
    {
        cardbody.GetChild(1).GetChild(0).GetChild(0).gameObject.GetComponent<Image>().color = color;
    }
}

public abstract class FieldCard : Card
{
    /*
    It is necessary to save the values of different power layers 
    powers[0]: holds de basepower value
    powers[1]: holds extra modifications resulting power (user-created effects)
    powers[2]: holds the boostaffected power
    powers[3]: holds the climate affected power
    */

    public int[] powers = new int[4];
}

public class Unit : FieldCard { }


public class Decoy : FieldCard
{
    public override void Play(Player triggerplayer, GameObject body, GameObject dropzone)
    {
        //In this case the decoy collides with a card, not a dropzone due to the layer dispositions
        GameObject card2 = dropzone;
        GameObject parent = card2.transform.parent.gameObject;
        Carddisplay card2display = card2.GetComponent<Carddisplay>();
        if (parent.TryGetComponent<FieldDropzone>(out _) && card2.GetComponent<Carddisplay>().card.type != Type.Golden && parent.GetComponent<DropZone>().player1 == triggerplayer.player1)
        {
            for (int i = 1; i < 4; i++)
                (card2display.card as FieldCard).powers[i] = (card2display.card as FieldCard).powers[i - 1];

            CardPowerImageColorchange(card2.transform, Color.white);

            triggerplayer.hand.Add(card2display.card);
            parent.GetComponent<DropZone>().cardlist.Remove(card2display.card);
            Destroy(card2);

            parent.GetComponent<DropZone>().cardlist.Add(this);
            body.transform.SetParent(parent.transform);

            body.GetComponent<DragandDrop>().alreadyplayed = true;
            triggerplayer.gameMaster.globalModified = true;
            triggerplayer.gameMaster.ModifyZones();
            triggerplayer.gameMaster.NextTurn();
        }
        else
        {
            body.transform.position = body.GetComponent<DragandDrop>().startPosition;
        }
    }
}

public class Weather : Card
{
    public override void Play(Player triggerplayer, GameObject body, GameObject dropzone)
    {
        DropZone zone = dropzone.GetComponent<DropZone>();
        base.Play(triggerplayer, body, dropzone);
        if (!body.GetComponent<DragandDrop>().alreadyplayed) return;
        Debug.Log("Weather");

        var buffer = dropzone.GetComponent<BufferLink>();
        var dropzone0 = buffer.dropzones[0].GetComponent<FieldDropzone>();
        var dropzone1 = buffer.dropzones[1].GetComponent<FieldDropzone>();

        dropzone0.weatheraffected = true;
        dropzone1.weatheraffected = true;
        Debug.Log("Weather Affected");

        if (zone.cardlist.Count > 1)
        {
            zone.cardlist[0].owner.graveyard.Add(zone.cardlist[0]);
            zone.cardlist.RemoveAt(0);
            Destroy(zone.transform.GetChild(0).gameObject);
        }

        triggerplayer.gameMaster.ModifyZones();
        triggerplayer.gameMaster.globalModified = true;
    }
}
public class Boost : Card
{
    public override void Play(Player triggerplayer, GameObject body, GameObject dropzone)
    {
        base.Play(triggerplayer, body, dropzone);
        if (!body.GetComponent<DragandDrop>().alreadyplayed) return;
        Debug.Log("Boost");

        var unitrow = dropzone.GetComponent<BufferLink>().dropzones[0].GetComponent<FieldDropzone>();
        unitrow.boostaffected = true;
        Debug.Log("Boost Affected");

        triggerplayer.gameMaster.ModifyZones();
        triggerplayer.gameMaster.globalModified = true;
    }
}
public class Leader : Card {}
public class Clear : Card
{
    public override void Play(Player triggerplayer, GameObject body, GameObject dropzone)
    {
        base.Play(triggerplayer, body, dropzone);
        Transform weatherslots = dropzone.transform.parent;
        foreach (Transform child in weatherslots)
        {
            var buffer = child.gameObject.GetComponent<BufferLink>();
            buffer.dropzones[0].GetComponent<FieldDropzone>().weatheraffected = false;
            buffer.dropzones[1].GetComponent<FieldDropzone>().weatheraffected = false;

            if (child.childCount > 0)
            {
                var childZoneCard = child.gameObject.GetComponent<DropZone>().cardlist[0];
                childZoneCard.owner.graveyard.Add(childZoneCard);
                Destroy(child.GetChild(0).gameObject);
            }
            Destroy(body);
            triggerplayer.graveyard.Add(this);
        }




    }
}

