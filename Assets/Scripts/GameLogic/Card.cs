using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
[CreateAssetMenu (fileName = "CardSO", menuName = "Create Card")]


public class Card: ScriptableObject
{
    public int id;
    public string cardname;
    public Sprite image;
    public Type type;
    public string carddescription;
    public Faction faction;
    public Effect effect;
    public int basepower; //power stands for attack
    public int power; 
    public Position position;

    public enum Position
    {
        L,C,M,R,S,MR,MS,RS,MRS //For units,boosters,weather,decoy: Rows where the unit can be played M(Melee) R(Range) S(Siege) L(Leader) C(Clear)
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
    public enum Faction
    {
        Rome,
        Greece,
        Egypt,
        Neutral,
    }

    public enum Effect
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



    
 


