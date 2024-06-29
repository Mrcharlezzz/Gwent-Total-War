using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
[CreateAssetMenu (fileName = "CardSO", menuName = "Create Card")]


public abstract class Card: ScriptableObject
{
    public int id;
    public Player owner;
    new public string name;
    public Sprite image;
    public Type type;
    public string carddescription;
    public Faction faction;
    public Position position;

    public Onactivation activation;

    public void ActivateEffect()
    {
        activation.triggerplayer=owner;
        activation.Execute();
    }
    

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
        Neutral,
    }
    public bool IsUnit()
    {
        if(!(type==Card.Type.Leader||type==Card.Type.Clear||type==Card.Type.Weather))return true;
        return false;
    }

}


public class Unit: Card{
    //Due to similarities between decoy card and unit cards decoy will also be instantiated as a Unit object with its respective effect
    public IntEffect effect;

    /*
    It is necessary to save the values of different power layers 
    powers[0]: holds de basepower value
    powers[1]: holds the climate affected power
    powers[2]: holds the boostaffected power
    powers[3]: holds any extra modifications resulting power (user-created effects)
    */

    public int[] powers=new int[4]; 
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

public class Onactivation{
    public Player triggerplayer;
    List<EffectActivation> activations;
    public void Execute()
    {
        foreach(EffectActivation activation in activations)
        {
            activation.effect.definition.action.context.triggerplayer = triggerplayer;
            activation.Execute(triggerplayer);
        }
    }
}

public class EffectActivation
{
    public Effect effect;
    public Selector selector;
    public EffectActivation postAction;
    public void Execute(Player triggerplayer)
    {
        var temp=selector.Select();
        if(selector.single&&temp.Count>0)
        {
            List<Card> singlecard=new List<Card>(){temp[0]}; 
            effect.definition.action.targets=singlecard;
        }
        else effect.definition.action.targets=temp;
        effect.Execute(triggerplayer);
        postAction.Execute(triggerplayer);
    }
}

public class Effect
{
    public EffectDefinition definition;
    public Dictionary<string,object> parameters;
    public void Execute(Player triggerplayer)
    {
        Dictionary<string, object> contextParameters=new Dictionary<string, object>(parameters);
        definition.action.context=new Context(triggerplayer,null,contextParameters);
        definition.Execute();
    }
}

//Used ListFind object with predicate based selection Evaluate method
public class Selector
{
    public bool single;
    public ListFind filtre;
    public List<Card> Select()
    {
        return (List<Card>)filtre.Evaluate(new Context(null,null,null), new List<Card>());
    }
}

