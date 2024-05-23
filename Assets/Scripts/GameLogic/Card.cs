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
    public Position position;

    public Onactivation activation;
    

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
    powers[1]: holds the multiplicated power for units with powerxNtimes effect
    powers[2]: holds the climate affected power
    powers[3]: holds the boostaffected power
    powers[4]: holds any extra modifications resulting power (user-created effects)
    */

    public int[] powers=new int[5]; 
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
    List<EffectActivation> activations;
    public void Execute()
    {
        foreach(EffectActivation activation in activations)
        {
            activation.Execute();
        }
    }
}

public class EffectActivation
{
    public Effect effect;
    public Selector selector;
    public EffectActivation postAction;
    public void Execute()
    {
        effect.definition.action.target=selector.Select();
        effect.Execute();
        postAction.Execute();
    }
}

public class Effect
{
    public EffectDefinition definition;
    public Dictionary<string,object> parameters;
    public void Execute()
    {
        foreach(string key in parameters.Keys)
        {
            definition.parameters[key]=parameters[key];
        }
        definition.Execute();
    }
}
public class Selector
{
    List<Card> source = new List<Card>();
    bool single;
    Predicate filtre;
    public List<Card> Select()
    {
        List<Card> select=new List<Card>();
                
        foreach(Card card in source)
        {
            if(filtre.Evaluate(new GlobalContext(),card))
            {
                select.Add(card);
                if(single) break;
            }
        }
        return select;
    }
}

public class Predicate
{
    Expression predicate;
    public bool Evaluate(GlobalContext context, Card card)
    {
        return (bool)predicate.Evaluate(context, card);
    }
}

public abstract class Expression
{
    public abstract object Evaluate(GlobalContext context, Card card);
}
public abstract class BinaryOperator : Expression
{
    protected Expression left;
    protected Expression right;
}
public class PlusNode: BinaryOperator
{
    public override object Evaluate(GlobalContext context, Card card)
    {
        return (int)left.Evaluate(context, card)+(int)right.Evaluate(context, card);
    }
}
public class MinusNode: BinaryOperator
{
    public override object Evaluate(GlobalContext context, Card card)
    {
        return (int)left.Evaluate(context, card)-(int)right.Evaluate(context, card);
    }
}
public class MultiplicationNode: BinaryOperator
{
    public override object Evaluate(GlobalContext context, Card card)
    {
        return (int)left.Evaluate(context, card)*(int)right.Evaluate(context, card);
    }
}
public class DivisionNode: BinaryOperator
{
    public override object Evaluate(GlobalContext context, Card card)
    {
        return (int)left.Evaluate(context, card)/(int)right.Evaluate(context, card);
    }
}
public class PowerNode: BinaryOperator
{
    public override object Evaluate(GlobalContext context, Card card)
    {
        return OptimizedPower((int)left.Evaluate(context, card),(int)right.Evaluate(context, card));
    }
    static int OptimizedPower(int argument, int power)
    {
        int result=1;
        for(;power>=0;power/=2,argument=argument*argument)
        {
            if(power%2==1) result=(result*argument);
        }
        return result;
    }
}
public class EqualsNode: BinaryOperator
{
    public override object Evaluate(GlobalContext context, Card card)
    {
        return (int)left.Evaluate(context, card)==(int)right.Evaluate(context, card);
    }
}
public class AtMostNode: BinaryOperator
{
    public override object Evaluate(GlobalContext context, Card card)
    {
        return (int)left.Evaluate(context, card)<=(int)right.Evaluate(context, card);
    }
}
public class AtLeastNode: BinaryOperator
{
    public override object Evaluate(GlobalContext context, Card card)
    {
        return (int)left.Evaluate(context, card)>=(int)right.Evaluate(context, card);
    }
}
public class LessNode: BinaryOperator
{
    public override object Evaluate(GlobalContext context, Card card)
    {
        return (int)left.Evaluate(context, card)<(int)right.Evaluate(context, card);
    }
}
public class GreatNode: BinaryOperator
{
    public override object Evaluate(GlobalContext context, Card card)
    {
        return (int)left.Evaluate(context, card)>(int)right.Evaluate(context, card);
    }
}
public class OrNode: BinaryOperator
{
    public override object Evaluate(GlobalContext context, Card card)
    {
        return (bool)left.Evaluate(context, card)||(bool)right.Evaluate(context, card);
    }
}
public class AndNode: BinaryOperator
{
    public override object Evaluate(GlobalContext context, Card card)
    {
        return (bool)left.Evaluate(context, card)&&(bool)right.Evaluate(context, card);
    }
}
public class JoinNode: BinaryOperator
{
    public override object Evaluate(GlobalContext context, Card card)
    {
        return (string)left.Evaluate(context, card)+(string)right.Evaluate(context, card);
    }
}
public class SpaceJoinNode: BinaryOperator
{
    public override object Evaluate(GlobalContext context, Card card)
    {
        return (string)left.Evaluate(context, card)+" "+(string)right.Evaluate(context, card);
    }
}
public class Atom: Expression
{
    protected object atom;
    public override object Evaluate(GlobalContext context, Card card)
    {
        return atom;
    }
}
