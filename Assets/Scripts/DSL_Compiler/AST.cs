using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

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
    Token token;

    protected BinaryOperator(IExpression left,IExpression right,Token token)
    {
        this.left = left;
        this.right = right;
        this.token=token;
    }
    public abstract object Evaluate(Context context, List<Card> targets);
}
public class Plus: BinaryOperator
{
    public Plus(IExpression left, IExpression right, Token token) : base(left, right, token){}

    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)+(int)right.Evaluate(context, targets);
    }
}
public class Minus: BinaryOperator
{
    public Minus(IExpression left, IExpression right, Token token) : base(left, right, token){}

    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)-(int)right.Evaluate(context, targets);
    }
}
public class Multiplication: BinaryOperator
{
    public Multiplication(IExpression left, IExpression right, Token token) : base(left, right, token){}
    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)*(int)right.Evaluate(context, targets);
    }
}
public class Division: BinaryOperator
{
    public Division(IExpression left, IExpression right, Token token) : base(left, right, token){}
    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)/(int)right.Evaluate(context, targets);
    }
}
public class Power: BinaryOperator
{
    public Power(IExpression left, IExpression right, Token token) : base(left, right, token){}
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
    public Equals(IExpression left, IExpression right, Token token) : base(left, right, token){}
    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)==(int)right.Evaluate(context, targets);
    }
}
public class Differ: BinaryOperator
{
    public Differ(IExpression left, IExpression right, Token token) : base(left, right, token){}
    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)!=(int)right.Evaluate(context, targets);
    }
}
public class AtMost: BinaryOperator
{
    public AtMost(IExpression left, IExpression right, Token token) : base(left, right, token){}
    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)<=(int)right.Evaluate(context, targets);
    }
}
public class AtLeast: BinaryOperator
{
    public AtLeast(IExpression left, IExpression right, Token token) : base(left, right, token){}
    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)>=(int)right.Evaluate(context, targets);
    }
}
public class Less: BinaryOperator
{
    public Less(IExpression left, IExpression right, Token token) : base(left, right, token){}
    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)<(int)right.Evaluate(context, targets);
    }
}
public class Great: BinaryOperator
{
    public Great(IExpression left, IExpression right, Token token) : base(left, right, token){}
    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets)>(int)right.Evaluate(context, targets);
    }
}
public class Or: BinaryOperator
{
    public Or(IExpression left, IExpression right, Token token) : base(left, right, token){}
    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (bool)left.Evaluate(context, targets)||(bool)right.Evaluate(context, targets);
    }
}
public class And: BinaryOperator
{
    public And(IExpression left, IExpression right, Token token) : base(left, right, token){}
    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (bool)left.Evaluate(context, targets)&&(bool)right.Evaluate(context, targets);
    }
}
public class Join: BinaryOperator
{
    public Join(IExpression left, IExpression right, Token token) : base(left, right, token){}
    public override object  Evaluate(Context context, List<Card> targets)
    {
        return (string)left.Evaluate(context, targets)+(string)right.Evaluate(context, targets);
    }
}
public class SpaceJoin: BinaryOperator
{
    public SpaceJoin(IExpression left, IExpression right, Token token) : base(left, right, token){}
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
    public IExpression expr;
    public IndividualList(IExpression expr){
        this.expr = expr;
    }
}

public class HandList: IndividualList
{
    public HandList(IExpression expr):base(expr){}
    public override object Evaluate(Context context, List<Card> targets)
    {
        return GlobalContext.Hand((Player)expr.Evaluate(context, targets));
    }
}
public class DeckList: IndividualList
{
    public DeckList(IExpression expr):base(expr){}
    public override object Evaluate(Context context, List<Card> targets)
    {
        return GlobalContext.Deck((Player)expr.Evaluate(context, targets));
    }
}
public class GraveyardList: IndividualList
{
    public GraveyardList(IExpression expr):base(expr){}
    public override object Evaluate(Context context, List<Card> targets)
    {
        return GlobalContext.Graveyard((Player)expr.Evaluate(context, targets));
    }
}
public class FieldList: IndividualList
{
    public FieldList(IExpression expr):base(expr){}
    public override object Evaluate(Context context, List<Card> targets)
    {
        return GlobalContext.Field((Player)expr.Evaluate(context, targets));
    }
}


public class ListFind: List{
    public ListFind(IExpression list, IExpression predicate){
        this.list = list;
        this.predicate = predicate;
    }
    public IExpression list;
    IExpression predicate;
    public override object Evaluate(Context context,List<Card> targets){
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
            if((bool)predicate.Evaluate(context,targets)) result.Add(listcard);
        }
        if(usedvariable) context.context["card"]=card;
        else context.context.Remove("card");
        return result;
    }
}

public abstract class Unary: Atom{
    protected IExpression right;
    public Unary(IExpression right)
    {
        this.right = right;
    }
}

public class Negation: Unary{
    public Negation(IExpression right): base(right){}
    public override object Evaluate(Context context, List<Card> targets)
    {
        return !(bool)right.Evaluate(context, targets);
    }
}
public class Negative: Unary{
    public Negative(IExpression right): base(right){}
    public override object Evaluate(Context context, List<Card> targets)
    {
        return -(int)right.Evaluate(context, targets);
    }
}
public class Literal: Atom{
    public Literal(object value)
    {
        this.value = value;
    }
    object value;
    public override object Evaluate(Context context, List<Card> targets)
    {
        return value;
    }
}

public interface ICardAtom:IExpression
{
    public void Set(Context context, List<Card> targets, Card card);
}
public class Variable: Atom
{
    public Variable(Token name)
    {
        this.name=name;
    }
    public Token name;
    public override object Evaluate(Context context,List<Card> targets)
    {
        return context.context[name.lexeme];
    }
}
public class CardVariable: Variable,ICardAtom{
    public CardVariable(Token name) : base(name) {}
    
    public void Set(Context context,List<Card> targets, Card card)
    {
        context.context[name.lexeme]=card;
    }
}

public class IndexedCard:  ICardAtom
{
    public IndexedCard(IExpression index, IExpression list){
        this.index=index;
        this.list=list;     
    }
    public IExpression index;
    public IExpression list;
    public  object Evaluate(Context context, List<Card> targets)
    {
        var evaluation=list.Evaluate(context,targets) as List<Card>;
        return evaluation[Math.Max(evaluation.Count,(int)index.Evaluate(context,targets))];
    }
    
    public void Set(Context context,List<Card> targets,Card card)
    {
        var evaluation=list.Evaluate(context,targets) as List<Card>;
        evaluation[Math.Max(evaluation.Count,(int)index.Evaluate(context,targets))]=card;
    }
}

public class Pop: ICardAtom,IStatement{
    public Pop( IExpression list){
        this.list = list;
    }
    public IExpression list;

    public object Evaluate( Context context, List<Card> targets){
        List<Card> evaluation = list.Evaluate(context,targets) as List<Card>;
        if(evaluation.Count==0) throw new Exception("Indexed Empty List");
        Card result=evaluation[evaluation.Count-1];
        Execute(context,targets);
        return result;
    }

    public void Execute(Context context, List<Card> targets)
    {
        List<Card> temp=list.Evaluate(context,targets) as List<Card>;
        if(temp.Count>0)temp.RemoveAt(temp.Count - 1);
    }
    public void Set(Context context, List<Card> targets, Card card){}
}

public abstract class PropertyAccess: Atom
{
    public PropertyAccess(IExpression card){
        this.card = card;
    }
    public IExpression card;
    public abstract void Set(Context context,List<Card> targets, IExpression Iexpression);
}

public class PowerAccess: PropertyAccess
{
    public PowerAccess(IExpression card):base(card){}
    public  override object Evaluate(Context context, List<Card> targets)
    {
        Card aux1=(Card)card.Evaluate(context,targets);
        if(aux1.IsUnit())
        {
            return (card.Evaluate(context,targets) as Unit).powers[3];
        }
        else return 0;
    }
    public override void Set(Context context,List<Card> targets, IExpression Iexpression)
    {
        (card.Evaluate(context,targets) as Unit).powers[3]=(int)Iexpression.Evaluate(context,targets);
    }
}
public class NameAccess: PropertyAccess
{
    public NameAccess(IExpression card):base(card){}
    public override object Evaluate(Context context, List<Card> targets)
    {
        Card aux=(Card)card.Evaluate(context,targets);
        return aux.name;    
    }
    public override void Set(Context context,List<Card> targets, IExpression Iexpression)
    {
        (card.Evaluate(context,targets) as Card).name=(string)Iexpression.Evaluate(context,targets);
    }
}

public class FactionAccess: PropertyAccess
{
    public FactionAccess(IExpression card):base(card){}
    public  override object Evaluate(Context context, List<Card> targets)
    {
        Card aux=(Card)card.Evaluate(context,targets);
        return aux.faction;    
    }
    public override void Set(Context context,List<Card> targets, IExpression Iexpression)
    {
        (card.Evaluate(context,targets) as Card).faction=(Card.Faction)Iexpression.Evaluate(context,targets);
    }
}
public class OwnerAccess: PropertyAccess
{
    public OwnerAccess(IExpression card):base(card){}
    public  override object Evaluate(Context context, List<Card> targets)
    {
        Card aux=(Card)card.Evaluate(context,targets);
        return aux.owner;    
    }
    public override void Set(Context context,List<Card> targets, IExpression Iexpression)
    {
        (card.Evaluate(context,targets) as Card).owner=(Player)Iexpression.Evaluate(context,targets);
    }
}
public class TypeAccess: PropertyAccess
{
    public TypeAccess(IExpression card):base(card){}
    public override object Evaluate(Context context, List<Card> targets)
    {
        Card aux=(Card)card.Evaluate(context,targets);
        return aux.type;    
    }
    public override void Set(Context context,List<Card> targets, IExpression Iexpression)
    {
        (card.Evaluate(context,targets) as Card).type=(Card.Type)Iexpression.Evaluate(context,targets);
    }
} 

public class PositionAccess: PropertyAccess
{
    public PositionAccess(IExpression card):base(card){}
    public override object Evaluate(Context context, List<Card> targets)
    {
        Card aux=(Card)card.Evaluate(context,targets);
        return aux.position;    
    }
    public override void Set(Context context,List<Card> targets, IExpression Iexpression)
    {
        (card.Evaluate(context,targets) as Card).position=(Card.Position)Iexpression.Evaluate(context,targets);
    }
}

public class TriggerPlayer: Atom{
    public override object Evaluate( Context context, List<Card> targets){
        return context.triggerplayer;
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

    public object Get(Token key){
        if(context.ContainsKey(key.lexeme)){
            return context[key.lexeme];
        }
        throw Parser.Error(key,"Undifined variable");
    }
    public void Set(Token key,object value){
        if(context.ContainsKey(key.lexeme)){
            if(context[key.lexeme].GetType().Equals(value.GetType())) throw Parser.Error(key,"Assignation type differs from variable type");
        }
        context[key.lexeme]=value;
    }
}




public abstract class Assignation: IStatement
{
    public Assignation(IExpression assignation){
        this.assignation = assignation;
    }
    public IExpression assignation;
    public abstract void Execute(Context context, List<Card> targets);
}

public class CardAssignation:Assignation
{
    public CardAssignation(ICardAtom card, IExpression assignation): base(assignation){    
        this.card = card;
    }
    public ICardAtom card;
    public override void Execute(Context context, List<Card> targets)
    {
        card.Set(context,targets, assignation.Evaluate(context, targets) as Card);
    }
}

public class CardPropertyAssignation: Assignation
{
    public CardPropertyAssignation(PropertyAccess access, IExpression assignation): base(assignation){    
        this.access = access;
    }
    PropertyAccess access;
    public override void Execute(Context context, List<Card> targets)
    {
        access.Set(context,targets,assignation);
    }
}

public class VarAssignation: Assignation
{
    public VarAssignation(Token variable, IExpression assignation): base(assignation){    
        this.variable = variable;
    }
    public Token variable;
    public override void Execute(Context context, List<Card> targets)
    {
        context.context[variable.lexeme]=assignation.Evaluate(context,targets);
    }
    public virtual void Check()
    {

    }
}
public class PlusPlus: Variable,IStatement{
    public PlusPlus(Token name):base(name) {}
    public override object Evaluate(Context context, List<Card> targets)
    {
        Execute(context, targets);
        return (int)base.Evaluate(context, targets)-1;
    }
    public void Execute(Context context, List<Card> targets){
        context.context[(string)name.literal]=(int)context.context[(string)name.literal]+1;
    }
}
public class MinusMinus: Variable,IStatement{
    public MinusMinus(Token name):base(name) {}
    public override object Evaluate(Context context, List<Card> targets)
    {
        Execute(context, targets);
        return (int)base.Evaluate(context, targets)+1;
    }
    public void Execute(Context context, List<Card> targets){
        context.Set(name,(int)context.context[(string)name.literal]-1);
    }
}

public class PlusEqual: VarAssignation{
    public PlusEqual(Token variable, IExpression assignation):base(variable, assignation){}

    public override void Execute(Context context, List<Card> targets)
    {
        var value = context.Get(variable);
        if(value is int) context.Set(variable,(int)value+(int)assignation.Evaluate(context, targets));
        //else throw Parser.Error(variable,"");
    }
}
public class MinusEqual: VarAssignation{
    public MinusEqual(Token variable, IExpression assignation):base(variable, assignation){}

    public override void Execute(Context context, List<Card> targets)
    {
        var value = context.Get(variable);
        if(value is int) context.Set(variable,(int)value+(int)assignation.Evaluate(context, targets));
        //else throw Parser.Error(variable,"");
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

public abstract class Method: IStatement
{
    public Method(List list){
        this.list=list;
    }
    public List list; 
    public abstract void Execute(Context context,List<Card> targets);
}
public class Push: Method
{
    public Push(List list, ICardAtom card):base(list){
        this.card=card;
    }
    ICardAtom card;
    public override void Execute(Context context, List<Card> targets)
    {
        (list.Evaluate(context,targets) as List<Card>).Add(card.Evaluate(context,targets) as Card);
    }
}
public class SendBottom: Method
{
    public SendBottom(List list, ICardAtom card):base(list){
        this.card=card;
    }
    ICardAtom card;
    public override void Execute(Context context, List<Card> targets)
    {
        (list.Evaluate(context,targets) as List<Card>).Insert(0,card.Evaluate(context,targets) as Card);
    }
}
public class Remove: Method
{
    public Remove(List list, ICardAtom card):base(list){
        this.card=card;
    }
    ICardAtom card;
    public override void Execute(Context context, List<Card> targets)
    {
        (list.Evaluate(context,targets) as List<Card>).Remove(card.Evaluate(context,targets) as Card);
    }
}

public class Shuffle: Method
{
    public Shuffle(List list):base(list){}
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










