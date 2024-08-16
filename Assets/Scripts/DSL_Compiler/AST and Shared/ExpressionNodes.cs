using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


// Abstract Syntax Tree (AST) for card and effect compiler

public interface IASTNode {}


// Interface for expressions in the AST

public interface IExpression : IASTNode
{
    public object Evaluate(Context context, List<Card> targets);
}


// Abstract class for binary operators in expressions
public abstract class BinaryOperator : IExpression
{
    public IExpression left;
    public IExpression right;
    public Token operation;

    protected BinaryOperator(IExpression left, IExpression right, Token operation)
    {
        this.left = left;
        this.right = right;
        this.operation = operation;
    }
    public abstract object Evaluate(Context context, List<Card> targets);
}

// Addition operator
public class Plus : BinaryOperator
{
    public Plus(IExpression left, IExpression right, Token token) : base(left, right, token) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets) + (int)right.Evaluate(context, targets);
    }
}

// Subtraction operator
public class Minus : BinaryOperator
{
    public Minus(IExpression left, IExpression right, Token token) : base(left, right, token) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets) - (int)right.Evaluate(context, targets);
    }
}

// Product operator
public class Product : BinaryOperator
{
    public Product(IExpression left, IExpression right, Token token) : base(left, right, token) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets) * (int)right.Evaluate(context, targets);
    }
}

// Division operator
public class Division : BinaryOperator
{
    public Division(IExpression left, IExpression right, Token token) : base(left, right, token) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets) / (int)right.Evaluate(context, targets);
    }
}

// Power operator (exponentiation)
public class Power : BinaryOperator
{
    public Power(IExpression left, IExpression right, Token token) : base(left, right, token) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        return OptimizedPower((int)left.Evaluate(context, targets), (int)right.Evaluate(context, targets));
    }

    // Helper method to calculate power efficiently
    static int OptimizedPower(int argument, int power)
    {
        int result = 1;
        for (; power >= 0; power /= 2, argument = argument * argument)
        {
            if (power % 2 == 1) result = result * argument;
        }
        return result;
    }
}

// Equality operator
public class Equal : BinaryOperator
{
    public Equal(IExpression left, IExpression right, Token token) : base(left, right, token) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        return left.Evaluate(context, targets).Equals(right.Evaluate(context, targets));
    }
}

// Inequality operator
public class Differ : BinaryOperator
{
    public Differ(IExpression left, IExpression right, Token token) : base(left, right, token) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        return left.Evaluate(context, targets) != right.Evaluate(context, targets);
    }
}

// Less than or equal operator
public class AtMost : BinaryOperator
{
    public AtMost(IExpression left, IExpression right, Token token) : base(left, right, token) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets) <= (int)right.Evaluate(context, targets);
    }
}

// Greater than or equal operator
public class AtLeast : BinaryOperator
{
    public AtLeast(IExpression left, IExpression right, Token token) : base(left, right, token) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets) >= (int)right.Evaluate(context, targets);
    }
}

// Less than operator
public class Less : BinaryOperator
{
    public Less(IExpression left, IExpression right, Token token) : base(left, right, token) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets) < (int)right.Evaluate(context, targets);
    }
}

// Greater than operator
public class Greater : BinaryOperator
{
    public Greater(IExpression left, IExpression right, Token token) : base(left, right, token) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets) > (int)right.Evaluate(context, targets);
    }
}

// Logical OR operator
public class Or : BinaryOperator
{
    public Or(IExpression left, IExpression right, Token token) : base(left, right, token) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        return (bool)left.Evaluate(context, targets) || (bool)right.Evaluate(context, targets);
    }
}

// Logical AND operator
public class And : BinaryOperator
{
    public And(IExpression left, IExpression right, Token token) : base(left, right, token) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        return (bool)left.Evaluate(context, targets) && (bool)right.Evaluate(context, targets);
    }
}

// String concatenation operator
public class Join : BinaryOperator
{
    public Join(IExpression left, IExpression right, Token token) : base(left, right, token) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        return (string)left.Evaluate(context, targets) + (string)right.Evaluate(context, targets);
    }
}

// String concatenation with space operator
public class SpaceJoin : BinaryOperator
{
    public SpaceJoin(IExpression left, IExpression right, Token token) : base(left, right, token) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        return (string)left.Evaluate(context, targets) + " " + (string)right.Evaluate(context, targets);
    }
}

// Abstract class for atomic expressions
public abstract class Atom : IExpression
{
    public static readonly List<TokenType> synchrotypes = new List<TokenType>(){
        TokenType.RightBracket,TokenType.RightBrace, TokenType.Arrow, TokenType.Semicolon,
        TokenType.Colon,TokenType.Comma, TokenType.Equal, TokenType.PlusEqual, TokenType.StarEqual,
        TokenType.For, TokenType.While, TokenType.MinusEqual, TokenType.SlashEqual,
    };
    public abstract object Evaluate(Context context, List<Card> targets);
}

// Abstract class for lists of cards
public abstract class List : Atom {
    public GameComponent gameComponent;
}

// List of cards on the board
public class BoardList : List
{
    public BoardList(IExpression context, Token accessToken){
        this.accessToken = accessToken;
        this.context=context;
    }
    public Token accessToken;
    public IExpression context;
    public override object Evaluate(Context context, List<Card> targets)
    {
        return GlobalContext.Board;
    }
}

// Abstract class for lists specific to a player
public abstract class IndividualList : List
{
    //This field isn't used in the evaluation method, it is only for the semnatic check
    //This is why in cases where a semantic check isn't needed it will have null value
    public Token accessToken;
    public IExpression context;
    public Token playertoken;
    public IExpression player;
    public IndividualList(IExpression context, IExpression player, Token accessToken, Token playertoken){
        this.accessToken = accessToken;
        this.context=context;
        this.player = player;
        this.playertoken = playertoken;
    }
}

// List of cards in a player's hand
public class HandList : IndividualList
{
    public HandList(IExpression context, IExpression player, Token accessToken, Token playertoken) : base(context, player, accessToken, playertoken) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        int targetPlayer = (int)player.Evaluate(context, targets);
        return GlobalContext.Hand(targetPlayer);
    }
}

// List of cards in a player's deck
public class DeckList : IndividualList
{
    public DeckList(IExpression context,IExpression player, Token accessToken, Token playertoken) : base(context, player, accessToken, playertoken) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        int targetPlayer = (int)player.Evaluate(context, targets);
        return GlobalContext.Deck(targetPlayer);
    }
}

// List of cards in a player's graveyard
public class GraveyardList : IndividualList
{
    public GraveyardList(IExpression context,IExpression player, Token accessToken, Token playertoken) : base(context, player, accessToken, playertoken) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        int targetPlayer = (int)player.Evaluate(context, targets);
        return GlobalContext.Graveyard(targetPlayer); 
    }
}

// List of cards in a player's field
public class FieldList : IndividualList
{
    public FieldList(IExpression context,IExpression player, Token accessToken, Token playertoken) : base(context, player, accessToken, playertoken) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        int targetPlayer = (int)player.Evaluate(context, targets);
        return GlobalContext.Field(targetPlayer);
    }
}

// List of cards filtered by a predicate
public class ListFind : List
{
    public ListFind(){}

    public ListFind(IExpression list, IExpression predicate, Token parameter, Token accessToken, Token argumentToken)
    {
        this.accessToken = accessToken;
        this.list = list;
        this.predicate = predicate;
        this.parameter = parameter;
        this.argumentToken = argumentToken;
    }
    public Token accessToken;
    public IExpression list;
    public IExpression predicate;
    public Token parameter;
    public Token argumentToken;

    public override object Evaluate(Context context, List<Card> targets)
    {
        // Save the variable value if it exists in the context
        object card = 0;
        List<Card> result = new List<Card>();
        bool usedvariable = false;
        if (context.Contains(parameter))
        {
            card = context.Get(parameter);
            usedvariable = true;
        }

        //List evaluation may return a gamecomponent or a list of cards depending on wether its a list
        //reffering to the game state or its a logical list used for making modifications to its content
        List<Card> safeList;
        var aux = list.Evaluate(context, targets);
        if(aux is GameComponent component) safeList=component.cards;
        else safeList = (List<Card>)aux;

        // Evaluate the predicate for each card in the list
        foreach (Card listcard in safeList)
        {
            context.Set(parameter, listcard);
            if ((bool)predicate.Evaluate(context, targets)) result.Add(listcard);
        }

        // Restore the original variable value if it was used
        if (usedvariable) context.Set(parameter, card);
        else context.variables.Remove(parameter.lexeme);

        return result;
    }
}



// Abstract class for unary operators in expressions
public abstract class Unary : Atom
{
    public Token operation;
    public IExpression right;
    public Unary(IExpression right, Token operation)
    {
        this.right = right;
        this.operation = operation;
    }
}

// Logical negation operator
public class Negation : Unary
{
    public Negation(IExpression right, Token operation) : base(right, operation) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        return !(bool)right.Evaluate(context, targets);
    }
}

// Arithmetic negation operator
public class Negative : Unary
{
    public Negative(IExpression right, Token operation) : base(right, operation) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        return -(int)right.Evaluate(context, targets);
    }
}



// Literal values in expressions
public class Literal : Atom
{
    public Literal(object value)
    {
        this.value = value;
    }
    

    public object value;

    public override object Evaluate(Context context, List<Card> targets)
    {
        return value;
    }
}

// Interface for card-related expressions
public interface ICardAtom : IExpression
{
    public void Set(Context context, List<Card> targets, Card card);
}

// Variable expressions
public class Variable : Atom
{
    public Variable(Token name)
    {
        this.name = name;
    }

    public Token name;

    public override object Evaluate(Context context, List<Card> targets)
    {
        return context.Get(name);
    }
}

// Card variable expressions
public class CardVariable : Variable, ICardAtom
{
    public CardVariable(Token name) : base(name) { }

    public void Set(Context context, List<Card> targets, Card card)
    {
        context.Set(name, card);
    }
}

// Indexed card expressions
public class IndexedCard : ICardAtom
{
    public IndexedCard(IExpression index, IExpression list, Token indexToken)
    {
        this.index = index;
        this.list = list;
        this.indexToken = indexToken;
    }

    public IExpression index;
    public IExpression list;
    public Token indexToken;

    public object Evaluate(Context context, List<Card> targets)
    {
        var evaluation = list.Evaluate(context, targets);
        if(evaluation is GameComponent component) evaluation=component.cards;
        List<Card> safeList= (List<Card>) evaluation;

        return safeList[Math.Max(safeList.Count, (int)index.Evaluate(context, targets))];
    }

    public void Set(Context context, List<Card> targets, Card card)
    {
        var evaluation = list.Evaluate(context, targets);
        if(evaluation is GameComponent component) evaluation = component.cards;
        List<Card> safeList = (List<Card>) evaluation;
        safeList[Math.Max(safeList.Count, (int)index.Evaluate(context, targets))] = card;
    }
}


// Abstract class for property access expressions
public abstract class PropertyAccess : Atom
{
    public PropertyAccess(IExpression card, Token accessToken)
    {
        this.card = card;
        this.accessToken = accessToken;
    }

    public IExpression card;
    public Token accessToken;

    public abstract void Set(Context context, List<Card> targets, object value);
}

// Access card power property
public class PowerAccess : PropertyAccess
{
    public PowerAccess(IExpression card, Token accessToken) : base(card, accessToken) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        Card aux = (Card)card.Evaluate(context, targets);
        if (aux is FieldCard)
        {
            return (card.Evaluate(context, targets) as FieldCard).powers[1];
        }
        else throw new InvalidOperationException("Card doesn't contain power field");
    }

    public override void Set(Context context, List<Card> targets, object value)
    {
        (card.Evaluate(context, targets) as FieldCard).powers[1] = (int)value;
    }
}

// Access card name property
public class NameAccess : PropertyAccess
{
    public NameAccess(IExpression card, Token accessToken) : base(card, accessToken) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        Card aux = (Card)card.Evaluate(context, targets);
        return aux.name;
    }

    public override void Set(Context context, List<Card> targets, object value)
    {
        (card.Evaluate(context, targets) as Card).name = (string)value;
    }
}

// Access card faction property
public class FactionAccess : PropertyAccess
{
    public FactionAccess(IExpression card, Token accessToken) : base(card, accessToken) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        Card aux = (Card)card.Evaluate(context, targets);
        return aux.faction;
    }

    public override void Set(Context context, List<Card> targets, object value)
    {
        (card.Evaluate(context, targets) as Card).faction = (string)value;
    }
}

// Access card owner property
public class OwnerAccess : PropertyAccess
{
    public OwnerAccess(IExpression card, Token accessToken) : base(card, accessToken) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        Card aux = (Card)card.Evaluate(context, targets);
        return aux.owner;
    }

    public override void Set(Context context, List<Card> targets, object value){}
}

// Access card type property
public class TypeAccess : PropertyAccess
{
    public TypeAccess(IExpression card, Token accessToken) : base(card, accessToken) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        Card aux = (Card)card.Evaluate(context, targets);
        return aux.type;
    }

    public override void Set(Context context, List<Card> targets, object value)
    {
        (card.Evaluate(context, targets) as Card).type = Tools.GetCardType((string)value);
    }
}

// Access card position property
public class RangeAccess : PropertyAccess
{
    public static readonly List<TokenType> synchroTypes = new List<TokenType>() {TokenType.RightBracket, TokenType.RightBrace};
    public RangeAccess(IExpression card, Token accessToken) : base(card, accessToken) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        Card aux = (Card)card.Evaluate(context, targets);
        return Tools.GetCardPositions(aux.positions);
    }

    public override void Set(Context context, List<Card> targets, object value)
    {
        (card.Evaluate(context, targets) as Card).positions = (List<Card.Position>)value;
    }
}

public class IndexedRange: IExpression{
    public IExpression range;
    public IExpression index;   
    public Token indexedToken;
    public IndexedRange(IExpression range, IExpression index, Token indexedToken){
        this.range = range;
        this.index = index;
        this.indexedToken = indexedToken;
    }

    public object Evaluate(Context context, List<Card> targets){
        return (range.Evaluate(context,targets) as List<Card.Position>)[(int)index.Evaluate(context,targets)];
    }
}

// Access trigger player
public class TriggerPlayer : Atom
{
    public override object Evaluate(Context context, List<Card> targets)
    {
        int triggerplayer = context.triggerplayer;
        return triggerplayer;
    }
}