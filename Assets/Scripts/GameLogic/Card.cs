using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;


public class Card
{
    public Card() { }
    public int? id;
    public Player owner;
    public string name;
    public Sprite image;
    public Type? type;
    public string carddescription;
    public string faction;
    public List<Position> position;

    public Onactivation activation;

    public void ActivateEffect(Player triggerplayer)
    {
        activation.Execute(triggerplayer);
    }

    public enum Position
    {
        Melee, Ranged, siege,
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

    public bool IsUnit()
    {
        if (type == Type.Leader || type == Type.Clear || type == Type.Weather) return true;
        return false;
    }

}

public class Unit : Card
{
    //Due to similarities between decoy card and unit cards decoy will also be instantiated as a Unit object with its respective effect
    public IntEffect effect;

    /*
    It is necessary to save the values of different power layers 
    powers[0]: holds de basepower value
    powers[1]: holds the climate affected power
    powers[2]: holds the boostaffected power
    powers[3]: holds any extra modifications resulting power (user-created effects)
    */

    public int[] powers = new int[4];
    public Position effectposition;

    public enum IntEffect
    {
        None,
        SummonBoost,
        SummonWeather,
        DestroyStrong,
        DestroyWeak,
        Draw,
        PowerXntimes,
        RowCleanup,
        Average,
    }
}

