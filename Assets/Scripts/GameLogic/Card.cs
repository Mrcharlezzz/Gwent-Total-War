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

public class EffectDefinition
{
    public Effect effect;
    public Selector selector;
    public EffectDefinition postAction;
}

public class Effect
{
    public string name;
    public Dictionary<string,object> parameters;
}
public class Selector
{
    List<Card> source = new List<Card>();
    bool single;
    Predicate filtre;
}

public class Predicate
{
    Card card;
    Expression predicate;
    bool
}
public class Effects
{
// Pincha para mannana >:(
}
public abstract class Expression
{
    public abstract object Evaluate();
}
public abstract class BinaryOperator : Expression
{
    protected Expression left;
    protected Expression right;
    protected object leftvalue{get{return left.Evaluate();}}
    protected object rightvalue{get{return right.Evaluate();}}

}
public class PlusNode: BinaryOperator
{
    public override object Evaluate()
    {
        return (int)leftvalue+(int)rightvalue;
    }
}
public class MinusNode: BinaryOperator
{
    public override object Evaluate()
    {
        return (int)leftvalue-(int)rightvalue;
    }
}
public class MultiplicationNode: BinaryOperator
{
    public override object Evaluate()
    {
        return (int)leftvalue*(int)rightvalue;
    }
}
public class DivisionNode: BinaryOperator
{
    public override object Evaluate()
    {
        return (int)leftvalue/(int)rightvalue;
    }
}
public class PowerNode: BinaryOperator
{
    public override object Evaluate()
    {
        return OptimizedPower((int)leftvalue,(int)rightvalue);
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
    public override object Evaluate()
    {
        return (int)leftvalue==(int)rightvalue;
    }
}
public class AtMostNode: BinaryOperator
{
    public override object Evaluate()
    {
        return (int)leftvalue<=(int)rightvalue;
    }
}
public class AtLeastNode: BinaryOperator
{
    public override object Evaluate()
    {
        return (int)leftvalue>=(int)rightvalue;
    }
}
public class LessNode: BinaryOperator
{
    public override object Evaluate()
    {
        return (int)leftvalue<(int)rightvalue;
    }
}
public class GreatNode: BinaryOperator
{
    public override object Evaluate()
    {
        return (int)leftvalue>(int)rightvalue;
    }
}
public class OrNode: BinaryOperator
{
    public override object Evaluate()
    {
        return (bool)leftvalue||(bool)rightvalue;
    }
}
public class AndNode: BinaryOperator
{
    public override object Evaluate()
    {
        return (bool)leftvalue&&(bool)rightvalue;
    }
}
public class JoinNode: BinaryOperator
{
    public override object Evaluate()
    {
        return (string)leftvalue+(string)rightvalue;
    }
}
public class SpaceJoinNode: BinaryOperator
{
    public override object Evaluate()
    {
        return (string)leftvalue+" "+(string)rightvalue;
    }
}
public class Atom: Expression
{
    protected object atom;
    public override object Evaluate()
    {
        return atom;
    }
}