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

[Serializable]
public abstract class Card 
{
    public Card(int id, int owner, string name, string image, Type? type, string description, string faction, List<Position> positions, Onactivation activation){
        this.id = id;
        this.owner = owner;
        this.name = name;
        this.image = image;
        this.type = type;
        this.description = description;
        this.faction = faction;
        this.positions = positions;
        this.activation = activation;
    }
    public int id;
    public int owner;
    public string name;
    public string image;
    public Type? type;
    public string description;
    public string faction;
    public List<Position> positions;

    public Onactivation activation;

    public void ActivateEffect(Player triggerplayer)
    {
        try{
            activation.Execute(triggerplayer);
        }
        catch{
            GlobalContext.gameMaster.invalidEffectSign.SetActive(true);
            return;
        }
    }

    protected bool Playable(Player triggerplayer,  GameObject dropzone){
        DropZone zone = dropzone.GetComponent<DropZone>();
        if(zone == null) zone = dropzone.GetComponent<FieldDropZone>();
        bool ValidType = false, validPosition = false, availableslot = false, validField = false;

        foreach (Type type in zone.typelist)
        {
            if (this.type == type)
            {
                ValidType = true;
                break;
            }
        }
        if(this is Clear) validPosition=true;
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

        if (ValidType && validPosition && availableslot && validField) return true;
        else return false;
    }

    public abstract void Play(Player triggerplayer, GameObject body, GameObject dropzone);

    [Serializable]
    public enum Position
    {
        Melee, Ranged, Siege,
    }

    [Serializable]
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

    public static void CardPowerImageColorchange(Transform body, Color color)
    {
        body.GetChild(1).GetChild(0).GetChild(0).gameObject.GetComponent<Image>().color = color;
    }
}
[Serializable]
public abstract class FieldCard : Card
{
    public FieldCard(int id, int owner, string name, string image, Type? type, string description, string faction, List<Position> positions, Onactivation activation, int power):
        base(id, owner, name, image, type, description, faction, positions, activation){
            for (int i = 0; i<4; i++) powers[i] = power;
        }
    /*
    It is necessary to save the values of different power layers 
    powers[0]: holds de basepower value
    powers[1]: holds extra modifications resulting power (user-created effects)
    powers[2]: holds the boostaffected power
    powers[3]: holds the climate affected power
    */

    public int[] powers = new int[4];
}
[Serializable]
public class Unit : FieldCard {
    public Unit(int id, int owner, string name, string image, Type? type, string description, string faction, List<Position> positions, Onactivation activation, int power):
        base(id,owner, name, image, type, description, faction,positions, activation, power){}
    public override void Play(Player triggerplayer, GameObject body, GameObject dropzone)
    {
        DropZone zone = dropzone.GetComponent<DropZone>();
        if (Playable(triggerplayer, dropzone))
        {
            Debug.Log("Played succesfully");

            triggerplayer.hand.Remove(this);
            triggerplayer.field.Push(this, (FieldDropZone)zone);

            GlobalContext.gameMaster.ModifyZones();
            ActivateEffect(GlobalContext.gameMaster.currentplayer);
            GlobalContext.gameMaster.ModifyZones();
            
            GlobalContext.gameMaster.globalModified=true;
            GlobalContext.gameMaster.NextTurn();
        }
        else
        {
            body.transform.position = body.GetComponent<DragandDrop>().startPosition;
        }
    }
}

[Serializable]
public class Decoy : FieldCard
{
    public Decoy(int id, int owner, string name, string image, Type? type, string description, string faction, List<Position> positions, Onactivation activation, int power):
        base(id,owner, name, image, type, description, faction,positions, activation, power){}
    public override void Play(Player triggerplayer, GameObject body, GameObject dropzone)
    {
        //In this case the decoy collides with a card, not a dropzone due to the layer dispositions
        GameObject card2 = dropzone;
        GameObject parent = card2.transform.parent.gameObject;
        Carddisplay card2display = card2.GetComponent<Carddisplay>();
        if (parent.TryGetComponent<FieldDropZone>(out _) && card2.GetComponent<Carddisplay>().card.type != Type.Golden && parent.GetComponent<DropZone>().player1 == triggerplayer.player1)
        {
            for (int i = 1; i < 4; i++)
                (card2display.card as FieldCard).powers[i] = (card2display.card as FieldCard).powers[i - 1];

            CardPowerImageColorchange(card2.transform, Color.white);

            triggerplayer.hand.Remove(this);
            triggerplayer.hand.Push(card2display.card);
            triggerplayer.field.Remove(card2display.card);
            triggerplayer.field.Push(this, parent.GetComponent<FieldDropZone>());

            body.GetComponent<DragandDrop>().alreadyplayed = true;
            GlobalContext.gameMaster.ModifyZones();
            ActivateEffect(GlobalContext.gameMaster.currentplayer);
            GlobalContext.gameMaster.ModifyZones();

            GlobalContext.gameMaster.globalModified = true;
            GlobalContext.gameMaster.NextTurn();
        }
        else
        {
            body.transform.position = body.GetComponent<DragandDrop>().startPosition;
        }
    }
}
[Serializable]
public class Weather : Card
{
    public Weather(int id, int owner, string name, string image, Type? type, string description, string faction, List<Position> positions, Onactivation activation):
        base(id,owner, name, image, type, description, faction,positions, activation){}
    public override void Play(Player triggerplayer, GameObject body, GameObject dropzone)
    {
        DropZone zone = dropzone.GetComponent<DropZone>();
        if (!Playable(triggerplayer, dropzone)){
            body.transform.position = body.GetComponent<DragandDrop>().startPosition;
            return;
        }
        Debug.Log("Weather");
        
        var buffer = dropzone.GetComponent<BufferLink>();
        var dropzone0 = buffer.dropzones[0].GetComponent<FieldDropZone>();
        var dropzone1 = buffer.dropzones[1].GetComponent<FieldDropZone>();

        dropzone0.weatheraffected = true;
        dropzone1.weatheraffected = true;
        Debug.Log("Weather Affected");

        if (zone.cardlist.Count > 0)
        {
            GlobalContext.GetPlayer(zone.cardlist[0].owner).graveyard.Push(zone.cardlist[0]);
            zone.cardlist.RemoveAt(0);
            MonoBehaviour.Destroy(zone.transform.GetChild(0).gameObject);
        }

        body.GetComponent<DragandDrop>().alreadyplayed = true;
        Debug.Log("Played succesfully");

        triggerplayer.hand.cards.Remove(this);
        triggerplayer.hand.bodies.Remove(this);
        zone.cardlist.Add(this);

        body.transform.SetParent(zone.gameObject.transform, false);

        GlobalContext.gameMaster.ModifyZones();
        ActivateEffect(GlobalContext.gameMaster.currentplayer);
        GlobalContext.gameMaster.ModifyZones();

        GlobalContext.gameMaster.globalModified = true;
        GlobalContext.gameMaster.NextTurn();
    }
}
[Serializable]
public class Boost : Card
{
    public Boost(int id, int owner, string name, string image, Type? type, string description, string faction, List<Position> positions, Onactivation activation):
        base(id,owner, name, image, type, description, faction,positions, activation){}
    public override void Play(Player triggerplayer, GameObject body, GameObject dropzone)
    {
        DropZone zone = dropzone.GetComponent<DropZone>();
        if (!Playable(triggerplayer, dropzone)){
            body.transform.position = body.GetComponent<DragandDrop>().startPosition;
            return;
        }
        Debug.Log("Boost");

        var unitrow = dropzone.GetComponent<BufferLink>().dropzones[0].GetComponent<FieldDropZone>();
        unitrow.boostaffected = true;
        Debug.Log("Boost Affected");

        body.GetComponent<DragandDrop>().alreadyplayed = true;
        Debug.Log("Played succesfully");

        triggerplayer.hand.cards.Remove(this);
        triggerplayer.hand.bodies.Remove(this);
        zone.cardlist.Add(this);

        body.transform.SetParent(zone.gameObject.transform, false);

        GlobalContext.gameMaster.ModifyZones();
        ActivateEffect(GlobalContext.gameMaster.currentplayer);
        GlobalContext.gameMaster.ModifyZones();

        GlobalContext.gameMaster.globalModified = true;
        GlobalContext.gameMaster.NextTurn();
    }
}
[Serializable]
public class Leader : Card {
    public Leader(int id, int owner, string name, string image, Type? type, string description, string faction, List<Position> positions, Onactivation activation):
        base(id,owner, name, image, type, description, faction,positions, activation){}
    public override void Play(Player triggerplayer, GameObject body, GameObject dropzone){}
}
[Serializable]
public class Clear : Card
{
    public Clear(int id, int owner, string name, string image, Type? type, string description, string faction, List<Position> positions, Onactivation activation):
        base(id,owner, name, image, type, description, faction,positions, activation){}
    public override void Play(Player triggerplayer, GameObject body, GameObject dropzone)
    {
        if (!Playable(triggerplayer, dropzone)){
            body.transform.position = body.GetComponent<DragandDrop>().startPosition;
            return;
        }

        Transform weatherslots = dropzone.transform.parent;
        foreach (Transform child in weatherslots)
        {
            var buffer = child.gameObject.GetComponent<BufferLink>();
            buffer.dropzones[0].GetComponent<FieldDropZone>().weatheraffected = false;
            buffer.dropzones[1].GetComponent<FieldDropZone>().weatheraffected = false;

            if (child.childCount > 0)
            {
                var childZoneCard = child.gameObject.GetComponent<DropZone>().cardlist[0];
                GlobalContext.GetPlayer(childZoneCard.owner).graveyard.Push(childZoneCard);
                MonoBehaviour.Destroy(child.GetChild(0).gameObject);
            }
            MonoBehaviour.Destroy(body);
            triggerplayer.graveyard.Push(this);
        }

        GlobalContext.gameMaster.ModifyZones();
        ActivateEffect(GlobalContext.gameMaster.currentplayer);
        GlobalContext.gameMaster.ModifyZones();

        GlobalContext.gameMaster.globalModified=true;
        GlobalContext.gameMaster.NextTurn();
    }
}