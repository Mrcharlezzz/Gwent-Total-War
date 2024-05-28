using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Abstract Sintax Tree for card and effect compiler


#region ExpressionNodes
    
public abstract class Expression
{
    public abstract object Evaluate(Context context, List<Card> targets);
}
public abstract class BinaryOperator : Expression
{
    protected Expression left;
    protected Expression right;
}
public class Plus: BinaryOperator
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)+(int)right.Evaluate(context, targets);
    }
}
public class Minus: BinaryOperator
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)-(int)right.Evaluate(context, targets);
    }
}
public class Multiplication: BinaryOperator
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)*(int)right.Evaluate(context, targets);
    }
}
public class Division: BinaryOperator
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)/(int)right.Evaluate(context, targets);
    }
}
public class Power: BinaryOperator
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        return OptimizedPower((int)left.Evaluate(context, targets),(int)right.Evaluate(context, targets));
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
public class Equals: BinaryOperator
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)==(int)right.Evaluate(context, targets);
    }
}
public class AtMost: BinaryOperator
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)<=(int)right.Evaluate(context, targets);
    }
}
public class AtLeast: BinaryOperator
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)>=(int)right.Evaluate(context, targets);
    }
}
public class Less: BinaryOperator
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)<(int)right.Evaluate(context, targets);
    }
}
public class Great: BinaryOperator
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)>(int)right.Evaluate(context, targets);
    }
}
public class Or: BinaryOperator
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        return (bool)left.Evaluate(context, targets)||(bool)right.Evaluate(context, targets);
    }
}
public class And: BinaryOperator
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        return (bool)left.Evaluate(context, targets)&&(bool)right.Evaluate(context, targets);
    }
}
public class Join: BinaryOperator
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        return (string)left.Evaluate(context, targets)+(string)right.Evaluate(context, targets);
    }
}
public class SpaceJoin: BinaryOperator
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        return (string)left.Evaluate(context, targets)+" "+(string)right.Evaluate(context, targets);
    }
}

public abstract class Atom: Expression{}

public abstract class List:Atom{}


public class TargetList:List{
    public override object Evaluate(Context context, List<Card> targets)
    {
        return targets;
    }
}
public class BoardList:List{
    public override object Evaluate(Context context, List<Card> targets)
    {
        return GlobalContext.board;
    }
}
public abstract class IndividualList: List
{
    public Player owner;
}

public class HandList: IndividualList
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        return GlobalContext.Hand(owner);
    }
}
public class DeckList: IndividualList
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        return GlobalContext.Deck(owner);
    }
}
public class GraveyardList: IndividualList
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        return GlobalContext.Graveyard(owner);
    }
}
public class FieldList: IndividualList
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        return GlobalContext.Field(owner);
    }
}

public class ListFind: List
{
    public List list;
    Predicate predicate;
    public override object Evaluate(Context context,List<Card> targets)
    {
        /*
            If "card" is already used as a variable in this scope context this piece of code will save the variable value
        in order to use it for the predicate based selection of cards from the list, and after the find method has executed
        the variable is assigned its original value 
        */
        object card=0;
        List<Card> result= new List<Card>();
        bool usedvariable = false;
        if(context.context.ContainsKey("card"))
        {
           card=context.context["card"];
           usedvariable=true;
        }
        foreach (Card listcard in (List<Card>)list.Evaluate(context, targets))
        {
            context.context["card"]=listcard;
            if(predicate.Evaluate(context,targets)) result.Add(listcard);
        }
        if(usedvariable) context.context["card"]=card;
        else context.context.Remove("card");
        return result;
    }
public class Predicate
{
    Expression predicate;
    public bool Evaluate(Context context, List<Card> targets)
    {
        return (bool)predicate.Evaluate(context, targets);
    }
}
}
public class Constant: Atom
{
    public object literal;
    public override object Evaluate(Context context, List<Card> targets)
    {
        return literal;
    }
}

public interface ICardAtom
{
    public object Evaluate(Context context, List<Card> targets);
    public void Set(Context context, Card card);
}
public class Variable: Atom
{
    public string name;
    public override object Evaluate(Context context,List<Card> targets)
    {
        return context.context[name];
    }
}
public class CardVariable: Variable,ICardAtom{
    public void Set(Context context,Card card)
    {
        context.context[name]=card;
    }
}

public class IndexedCard: Atom, ICardAtom
{
    public int index{get => Math.Max(list.Count,index); set{}}
    public List<Card> list;
    public override object Evaluate(Context context, List<Card> targets)
    {
        return list[index];
    }
    public void Set(Context context,Card card)
    {
        list[index]=card;
    }
}

public abstract class PropertyAccess: Atom
{
    public ICardAtom card;
    public abstract void Set(Context context,List<Card> targets, Expression expression);
}

public class PowerAccess: PropertyAccess
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        Card aux1=(Card)card.Evaluate(context,targets);
        if(aux1.IsUnit())
        {
            return (card.Evaluate(context,targets) as Unit).powers[4];
        }
        else return 0;
    }
    public override void Set(Context context,List<Card> targets, Expression expression)
    {
        (card.Evaluate(context,targets) as Unit).powers[4]=(int)expression.Evaluate(context,targets);
    }
}
public class NameAccess: PropertyAccess
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        Card aux=(Card)card.Evaluate(context,targets);
        return aux.name;    
    }
    public override void Set(Context context,List<Card> targets, Expression expression)
    {
        (card.Evaluate(context,targets) as Card).name=(string)expression.Evaluate(context,targets);
    }
}

public class FactionAccess: PropertyAccess
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        Card aux=(Card)card.Evaluate(context,targets);
        return aux.faction;    
    }
    public override void Set(Context context,List<Card> targets, Expression expression)
    {
        (card.Evaluate(context,targets) as Card).faction=(Card.Faction)expression.Evaluate(context,targets);
    }
}
public class OwnerAccess: PropertyAccess
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        Card aux=(Card)card.Evaluate(context,targets);
        return aux.owner;    
    }
    public override void Set(Context context,List<Card> targets, Expression expression)
    {
        (card.Evaluate(context,targets) as Card).owner=(Player)expression.Evaluate(context,targets);
    }
}
public class TypeAccess: PropertyAccess
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        Card aux=(Card)card.Evaluate(context,targets);
        return aux.type;    
    }
    public override void Set(Context context,List<Card> targets, Expression expression)
    {
        (card.Evaluate(context,targets) as Card).type=(Card.Type)expression.Evaluate(context,targets);
    }
} 

public class PositionAccess: PropertyAccess
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        Card aux=(Card)card.Evaluate(context,targets);
        return aux.position;    
    }
    public override void Set(Context context,List<Card> targets, Expression expression)
    {
        (card.Evaluate(context,targets) as Card).position=(Card.Position)expression.Evaluate(context,targets);
    }
}

#endregion


#region StatementNodes

public abstract class Statement
{
    public abstract void Execute(Context context, List<Card> targets);
}
public class Action: Statement
{
    
    public Context context;
    public List<Card> targets;
    public List<Statement> statements;
    public override void Execute(Context context, List<Card> targets)
    {
        foreach(Statement statement in statements)
        {
            statement.Execute(context,targets);
        }
    }
}
public class Context
{
    public Player triggerplayer;
    public  Dictionary <string, object> context;
}



public abstract class Assignation: Statement
{
    public Expression assignation;
}

public class CardAssignation:Assignation
{
    public ICardAtom card;
    public override void Execute(Context context, List<Card> targets)
    {
        card.Set(context, assignation.Evaluate(context, targets) as Card);
    }
}

public class CardPropertyAssignation: CardAssignation
{
    PropertyAccess access;
    public override void Execute(Context context, List<Card> targets)
    {
        access.card=card;
        access.Set(context,targets,assignation);
    }
}

public class VarAssignation: Assignation
{
    public string variable;
    public override void Execute(Context context, List<Card> targets)
    {
        context.context[variable]=assignation.Evaluate(context,targets);
    }
    public virtual void Check()
    {

    }
}

public class VarDeclaration: VarAssignation
{
    public override void Check()
    {
        base.Check();
    }
}


public class Foreach: Action
{
    string variable;
    List<Card> collection;

    public override void Execute(Context context, List<Card> targets)
    {
        foreach(string key in context.context.Keys)
        {
            this.context.context[key]=context.context[key];
        }

        foreach (Card card in collection)
        {
            this.context.context[variable]=card;
            base.Execute(this.context,targets);
        }
    }
}

public class While: Action
{
    Expression predicate;
    public override void Execute(Context context,List<Card> targets)
    {
        foreach(string key in context.context.Keys)
        {
            this.context.context[key]=context.context[key];
        }
        while((bool)predicate.Evaluate(context,targets))
        {
            base.Execute(this.context,targets);
        }
    }
}

public abstract class Methods: Statement
{
    public List list; 
}
public class Push: Methods
{
    ICardAtom card;
    public override void Execute(Context context, List<Card> targets)
    {
        (list.Evaluate(context,targets) as List<Card>).Add(card.Evaluate(context,targets) as Card);
    }
}
public class SendBottom: Methods
{
    ICardAtom card;
    public override void Execute(Context context, List<Card> targets)
    {
        (list.Evaluate(context,targets) as List<Card>).Insert(0,card.Evaluate(context,targets) as Card);
    }
}
public class Remove: Methods
{
    ICardAtom card;
    public override void Execute(Context context, List<Card> targets)
    {
        (list.Evaluate(context,targets) as List<Card>).Remove(card.Evaluate(context,targets) as Card);
    }
}
public class Pop: Methods
{
    public override void Execute(Context context, List<Card> targets)
    {
        List<Card> temp=list.Evaluate(context,targets) as List<Card>;
        temp.RemoveAt(temp.Count - 1);
    }
}
public class Shuffle: Methods
{
    public override void Execute(Context context, List<Card> targets)
    {
        List<Card> temp=list.Evaluate(context,targets) as List<Card>;

        for (int i=temp.Count-1;i>0;i--)
        {
            int randomIndex=UnityEngine.Random.Range(0,i+1);
            Card container=temp[i];
            temp[i]=temp[randomIndex];
            temp[randomIndex]=container;
        }
    }
}

#endregion










