using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Abstract Sintax Tree for card and effect compiler


#region ExpressionNodes
    
public interface IExpression
{
    public object Evaluate(Context context, List<Card> targets);
}
public abstract class BinaryOperator :IExpression
{
    protected IExpression left;
    protected IExpression right;

    protected BinaryOperator(IExpression left,IExpression right)
    {
        this.left = left;
        this.right = right;
    }
    public abstract object Evaluate(Context context, List<Card> targets);
}
public class Plus: BinaryOperator
{
    public Plus(IExpression left, IExpression right) : base(left, right){}

    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)+(int)right.Evaluate(context, targets);
    }
}
public class Minus: BinaryOperator
{
    public Minus(IExpression left, IExpression right) : base(left, right){}

    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)-(int)right.Evaluate(context, targets);
    }
}
public class Multiplication: BinaryOperator
{
    public Multiplication(IExpression left, IExpression right) : base(left, right){}
    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)*(int)right.Evaluate(context, targets);
    }
}
public class Division: BinaryOperator
{
    public Division(IExpression left, IExpression right) : base(left, right){}
    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)/(int)right.Evaluate(context, targets);
    }
}
public class Power: BinaryOperator
{
    public Power(IExpression left, IExpression right) : base(left, right){}
    public override object  Evaluate(Context context, List<Card> targets)
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
    public Equals(IExpression left, IExpression right) : base(left, right){}
    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)==(int)right.Evaluate(context, targets);
    }
}
public class Differ: BinaryOperator
{
    public Differ(IExpression left, IExpression right) : base(left, right){}
    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)!=(int)right.Evaluate(context, targets);
    }
}
public class AtMost: BinaryOperator
{
    public AtMost(IExpression left, IExpression right) : base(left, right){}
    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)<=(int)right.Evaluate(context, targets);
    }
}
public class AtLeast: BinaryOperator
{
    public AtLeast(IExpression left, IExpression right) : base(left, right){}
    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)>=(int)right.Evaluate(context, targets);
    }
}
public class Less: BinaryOperator
{
    public Less(IExpression left, IExpression right) : base(left, right){}
    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)<(int)right.Evaluate(context, targets);
    }
}
public class Great: BinaryOperator
{
    public Great(IExpression left, IExpression right) : base(left, right){}
    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)>(int)right.Evaluate(context, targets);
    }
}
public class Or: BinaryOperator
{
    public Or(IExpression left, IExpression right) : base(left, right){}
    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (bool)left.Evaluate(context, targets)||(bool)right.Evaluate(context, targets);
    }
}
public class And: BinaryOperator
{
    public And(IExpression left, IExpression right) : base(left, right){}
    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (bool)left.Evaluate(context, targets)&&(bool)right.Evaluate(context, targets);
    }
}
public class Join: BinaryOperator
{
    public Join(IExpression left, IExpression right) : base(left, right){}
    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (string)left.Evaluate(context, targets)+(string)right.Evaluate(context, targets);
    }
}
public class SpaceJoin: BinaryOperator
{
    public SpaceJoin(IExpression left, IExpression right) : base(left, right){}
    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (string)left.Evaluate(context, targets)+" "+(string)right.Evaluate(context, targets);
    }
}

public abstract class Atom:IExpression{
    public abstract object Evaluate(Context context, List<Card> targets);
}

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
    IExpression predicate;
    public bool Evaluate(Context context, List<Card> targets)
    {
        return (bool)predicate.Evaluate(context, targets);
    }
}
}
public class Literal: Atom{
    object value;
    public override object Evaluate(Context context, List<Card> targets)
    {
        return value;
    }
}
public class Negation: Atom{
    public Literal literal;
    public override object Evaluate(Context context, List<Card> targets)
    {
        return !(bool)literal.Evaluate(context, targets);
    }
}
public class Negative: Atom{
    public Literal literal;
    public override object Evaluate(Context context, List<Card> targets)
    {
        return -(int)literal.Evaluate(context, targets);
    }
}

public interface ICardAtom
{
    public object IEvaluate(Context context, List<Card> targets);
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
    public object IEvaluate(Context context, List<Card> targets)
    {
        return Evaluate(context, targets);
    }
    public void Set(Context context,Card card)
    {
        context.context[name]=card;
    }
}

public class IndexedCard: Atom, ICardAtom
{
    public int index{get => Math.Max(list.Count,index); set{}}
    public List<Card> list;
    public  object IEvaluate(Context context, List<Card> targets)
    {
        return list[index];
    }
    public override object Evaluate(Context context, List<Card> targets)
    {
        return IEvaluate(context, targets);
    }
    public void Set(Context context,Card card)
    {
        list[index]=card;
    }
}

public abstract class PropertyAccess: Atom
{
    public ICardAtom card;
    public abstract void Set(Context context,List<Card> targets, IExpression Iexpression);
}

public class PowerAccess: PropertyAccess
{
    public  override object Evaluate(Context context, List<Card> targets)
    {
        Card aux1=(Card)card.IEvaluate(context,targets);
        if(aux1.IsUnit())
        {
            return (card.IEvaluate(context,targets) as Unit).powers[3];
        }
        else return 0;
    }
    public override void Set(Context context,List<Card> targets, IExpression Iexpression)
    {
        (card.IEvaluate(context,targets) as Unit).powers[3]=(int)Iexpression.Evaluate(context,targets);
    }
}
public class NameAccess: PropertyAccess
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        Card aux=(Card)card.IEvaluate(context,targets);
        return aux.name;    
    }
    public override void Set(Context context,List<Card> targets, IExpression Iexpression)
    {
        (card.IEvaluate(context,targets) as Card).name=(string)Iexpression.Evaluate(context,targets);
    }
}

public class FactionAccess: PropertyAccess
{
    public  override object Evaluate(Context context, List<Card> targets)
    {
        Card aux=(Card)card.IEvaluate(context,targets);
        return aux.faction;    
    }
    public override void Set(Context context,List<Card> targets, IExpression Iexpression)
    {
        (card.IEvaluate(context,targets) as Card).faction=(Card.Faction)Iexpression.Evaluate(context,targets);
    }
}
public class OwnerAccess: PropertyAccess
{
    public  override object Evaluate(Context context, List<Card> targets)
    {
        Card aux=(Card)card.IEvaluate(context,targets);
        return aux.owner;    
    }
    public override void Set(Context context,List<Card> targets, IExpression Iexpression)
    {
        (card.IEvaluate(context,targets) as Card).owner=(Player)Iexpression.Evaluate(context,targets);
    }
}
public class TypeAccess: PropertyAccess
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        Card aux=(Card)card.IEvaluate(context,targets);
        return aux.type;    
    }
    public override void Set(Context context,List<Card> targets, IExpression Iexpression)
    {
        (card.IEvaluate(context,targets) as Card).type=(Card.Type)Iexpression.Evaluate(context,targets);
    }
} 

public class PositionAccess: PropertyAccess
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        Card aux=(Card)card.IEvaluate(context,targets);
        return aux.position;    
    }
    public override void Set(Context context,List<Card> targets, IExpression Iexpression)
    {
        (card.IEvaluate(context,targets) as Card).position=(Card.Position)Iexpression.Evaluate(context,targets);
    }
}

#endregion


#region StatementNodes

public interface IStatement
{
    public void Execute(Context context, List<Card> targets);
}
public class Action: IStatement
{
    
    public Context context;
    public List<Card> targets;
    public List<IStatement> statements;
    public virtual void Execute(Context context, List<Card> targets)
    {
        foreach(IStatement statement in statements)
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



public abstract class Assignation: IStatement
{
    public IExpression assignation;
    public abstract void Execute(Context context, List<Card> targets);
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
    IExpression predicate;
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

public abstract class Methods: IStatement
{
    public abstract void Execute(Context context,List<Card> targets);
    public List list; 
}
public class Push: Methods
{
    ICardAtom card;
    public override void Execute(Context context, List<Card> targets)
    {
        (list.Evaluate(context,targets) as List<Card>).Add(card.IEvaluate(context,targets) as Card);
    }
}
public class SendBottom: Methods
{
    ICardAtom card;
    public override void Execute(Context context, List<Card> targets)
    {
        (list.Evaluate(context,targets) as List<Card>).Insert(0,card.IEvaluate(context,targets) as Card);
    }
}
public class Remove: Methods
{
    ICardAtom card;
    public override void Execute(Context context, List<Card> targets)
    {
        (list.Evaluate(context,targets) as List<Card>).Remove(card.IEvaluate(context,targets) as Card);
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










