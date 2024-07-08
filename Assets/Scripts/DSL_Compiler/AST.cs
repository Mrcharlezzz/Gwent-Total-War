using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor.U2D.Aseprite;

// Abstract Syntax Tree (AST) for card and effect compiler

public interface IASTNode { }

#region ExpressionNodes

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

    protected BinaryOperator(IExpression left, IExpression right, Token token)
    {
        this.left = left;
        this.right = right;
        this.operation = token;
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
            if (power % 2 == 1) result = (result * argument);
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
        return (int)left.Evaluate(context, targets) == (int)right.Evaluate(context, targets);
    }
}

// Inequality operator
public class Differ : BinaryOperator
{
    public Differ(IExpression left, IExpression right, Token token) : base(left, right, token) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        return (int)left.Evaluate(context, targets) != (int)right.Evaluate(context, targets);
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
    public abstract object Evaluate(Context context, List<Card> targets);
}

// Abstract class for lists of cards
public abstract class List : Atom {
    public List(Token accesToken){
        this.accessToken = accesToken;
    }
    public Token accessToken;
}

// List of cards on the board
public class BoardList : List
{
    public BoardList(IExpression context, Token accessToken) : base(accessToken){
        this.context=context;
    }
    public IExpression context;
    public override object Evaluate(Context context, List<Card> targets)
    {
        return GlobalContext.board;
    }
}

// Abstract class for lists specific to a player
public abstract class IndividualList : List
{
    //This field isn't used in the evaluation method, it is only for the semnatic check
    //This is why in cases where a semantic check isn't needed it will have null value
    public IExpression context;
    public Token playertoken;
    public IExpression player;
    public IndividualList(IExpression context, IExpression player, Token accessToken, Token playertoken) : base(accessToken)
    {
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
        return GlobalContext.Hand((Player)player.Evaluate(context, targets));
    }
}

// List of cards in a player's deck
public class DeckList : IndividualList
{
    public DeckList(IExpression context,IExpression player, Token accessToken, Token playertoken) : base(context, player, accessToken, playertoken) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        return GlobalContext.Deck((Player)player.Evaluate(context, targets));
    }
}

// List of cards in a player's graveyard
public class GraveyardList : IndividualList
{
    public GraveyardList(IExpression context,IExpression player, Token accessToken, Token playertoken) : base(context, player, accessToken, playertoken) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        return GlobalContext.Graveyard((Player)player.Evaluate(context, targets));
    }
}

// List of cards in a player's field
public class FieldList : IndividualList
{
    public FieldList(IExpression context,IExpression player, Token accessToken, Token playertoken) : base(context, player, accessToken, playertoken) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        return GlobalContext.Field((Player)player.Evaluate(context, targets));
    }
}

// List of cards filtered by a predicate
public class ListFind : List
{
    public ListFind() : base(null){ }

    public ListFind(IExpression list, IExpression predicate, Token parameter, Token accessToken, Token argumentToken) : base(accessToken)
    {
        this.list = list;
        this.predicate = predicate;
        this.parameter = parameter;
        this.argumentToken = argumentToken;
    }

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
        if (context.variables.ContainsKey(parameter.lexeme))
        {
            card = context.variables[parameter.lexeme];
            usedvariable = true;
        }

        // Evaluate the predicate for each card in the list
        foreach (Card listcard in (List<Card>)list.Evaluate(context, targets))
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
        var evaluation = list.Evaluate(context, targets) as List<Card>;
        return evaluation[Math.Max(evaluation.Count, (int)index.Evaluate(context, targets))];
    }

    public void Set(Context context, List<Card> targets, Card card)
    {
        var evaluation = list.Evaluate(context, targets) as List<Card>;
        evaluation[Math.Max(evaluation.Count, (int)index.Evaluate(context, targets))] = card;
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
        Card aux1 = (Card)card.Evaluate(context, targets);
        if (aux1.IsUnit())
        {
            return (card.Evaluate(context, targets) as Unit).powers[3];
        }
        else return 0;
    }

    public override void Set(Context context, List<Card> targets, object value)
    {
        (card.Evaluate(context, targets) as Unit).powers[3] = (int)value;
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

    public override void Set(Context context, List<Card> targets, object value)
    {
        (card.Evaluate(context, targets) as Card).owner = (Player)value;
    }
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
    public static readonly List<TokenType> synchroTypes = new List<TokenType>() {TokenType.Comma, TokenType.RightBracket};
    public RangeAccess(IExpression card, Token accessToken) : base(card, accessToken) { }

    public override object Evaluate(Context context, List<Card> targets)
    {
        Card aux = (Card)card.Evaluate(context, targets);
        return Tools.GetCardPositions(aux.position);
    }

    public override void Set(Context context, List<Card> targets, object value)
    {
        (card.Evaluate(context, targets) as Card).position = (List<Card.Position>)value;
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
        return context.triggerplayer;
    }
}

#endregion

#region StatementNodes

// Interface for statements in the AST
public interface IStatement : IASTNode
{
    public void Execute(Context context, List<Card> targets);
}

// Abstract class for blocks of statements
public abstract class Block : IStatement
{
    public readonly static List<TokenType> synchroTypes = new List<TokenType>() {TokenType.For, TokenType.While, TokenType.RightBrace};
    public Block(List<IStatement> statements, Token keyword)
    {
        this.statements = statements;
        this.keyword = keyword;
    }
    public Token keyword;
    public Context context;
    public List<IStatement> statements;
    public abstract void Execute(Context context, List<Card> targets);
}

// Action block
public class Action : Block
{
    public Action(List<IStatement> statements, Token contextID, Token targetsID, Token keyword) : base(statements, keyword)
    {
        this.statements = statements;
        this.contextID = contextID;
        this.targetsID = targetsID;
    }

    public List<Card> targets;
    public Token contextID;
    public Token targetsID;

    public override void Execute(Context context, List<Card> targets)
    {
        this.context.variables[contextID.lexeme] = new ID(contextID, ExpressionType.Context);
        this.context.variables[targetsID.lexeme] = new ID(targetsID, ExpressionType.Targets);
        foreach (IStatement statement in statements)
        {
            statement.Execute(context, targets);
        }
    }
}

// Assignment statement
public class Assignation : IStatement
{
    public Assignation(IExpression assignation, IExpression container, Token operation)
    {
        this.container = container;
        this.assignation = assignation;
        this.operation = operation;
    }

    public IExpression container;
    public IExpression assignation;
    public Token operation;

    public virtual void Execute(Context context, List<Card> targets)
    {
        if (container is ICardAtom) (container as ICardAtom).Set(context, targets, assignation.Evaluate(context, targets) as Card);
        else if (container is PropertyAccess) (container as PropertyAccess).Set(context, targets, assignation.Evaluate(context,targets));
        else if (container is Variable) context.Set((container as Variable).name, assignation.Evaluate(context, targets));
    }
}

// Increment and decrement operations
public class Increment_Decrement : Assignation, IExpression
{
    public Increment_Decrement(IExpression assignation, Token operation) : base(assignation, null, operation){}
    
    public object Evaluate(Context context, List<Card> targets)
    {
        Execute(context, targets);
        if (operation.type == TokenType.Increment) return (int)container.Evaluate(context, targets) + 1;
        else return (int)container.Evaluate(context, targets) - 1;
    }

    public override void Execute(Context context, List<Card> targets)
    {
        int result = 0;
        if (operation.type == TokenType.Increment) result = (int)container.Evaluate(context, targets) + 1;
        else result = (int)container.Evaluate(context, targets) - 1;
        if (container is PowerAccess) (container as PowerAccess).Set(context, targets, result);
        else if (container is Variable) context.Set((container as Variable).name, result);
    }
}

// Numeric modification operations (e.g., +=, -=, etc.)
public class NumericModification : Assignation
{
    public NumericModification(IExpression container, IExpression assignation, Token operation) : base(container, assignation ,operation){}

    public override void Execute(Context context, List<Card> targets)
    {
        object result = null;
        switch (operation.type)
        {
            case TokenType.PlusEqual: result = (int)container.Evaluate(context, targets) + (int)assignation.Evaluate(context, targets); break;
            case TokenType.MinusEqual: result = (int)container.Evaluate(context, targets) - (int)assignation.Evaluate(context, targets); break;
            case TokenType.SlashEqual: result = (int)container.Evaluate(context, targets) * (int)assignation.Evaluate(context, targets); break;
            case TokenType.StarEqual: result = (int)container.Evaluate(context, targets) / (int)assignation.Evaluate(context, targets); break;
            case TokenType.AtSymbolEqual: result = (string)container.Evaluate(context, targets) + (string)assignation.Evaluate(context, targets); break;
        }
        if (container is PowerAccess) (container as PowerAccess).Set(context, targets, result);
        else if (container is Variable) context.Set((container as Variable).name, result);
    }
}

// Foreach loop statement
public class Foreach : Block
{
    public Foreach(List<IStatement> statements, IExpression collection, Token variable, Token keyword) : base(statements, keyword)
    {
        this.collection = collection;
        this.variable = variable;
    }

    public Token variable;
    public IExpression collection;

    public override void Execute(Context context, List<Card> targets)
    {
        this.context = new Context(context.triggerplayer, context, new Dictionary<string, object>());

        foreach (Card card in (List<Card>)collection)
        {
            this.context.Set(variable, card);
            foreach (IStatement statement in statements)
            {
                statement.Execute(this.context, targets);
            }
        }
    }
}

// While loop statement
public class While : Block
{
    public While(List<IStatement> statements, IExpression predicate, Token keyword) : base(statements, keyword)
    {
        this.predicate = predicate;
    }

    public IExpression predicate;

    public override void Execute(Context context, List<Card> targets)
    {
        this.context = new Context(context.triggerplayer, context, new Dictionary<string, object>());
        while ((bool)predicate.Evaluate(context, targets))
        {
            foreach (IStatement statement in statements)
            {
                statement.Execute(this.context, targets);
            }
        }
    }
}

// Abstract class for list methods
public abstract class Method : IStatement
{
    public Method(IExpression list, Token accessToken)
    {
        this.list = list;
        this.accessToken = accessToken;
    }

    public Token accessToken;
    public IExpression list;
    public abstract void Execute(Context context, List<Card> targets);
}

// Pop operation on lists
public class Pop : Method, ICardAtom
{
    public Pop(IExpression list, Token accessToken) : base(list, accessToken) {}

    public object Evaluate(Context context, List<Card> targets)
    {
        List<Card> evaluation = list.Evaluate(context, targets) as List<Card>;
        if (evaluation.Count == 0) throw new Exception("Indexed Empty List");
        Card result = evaluation[evaluation.Count - 1];
        Execute(context, targets);
        return result;
    }

    public override void Execute(Context context, List<Card> targets)
    {
        List<Card> temp = list.Evaluate(context, targets) as List<Card>;
        if (temp.Count > 0) temp.RemoveAt(temp.Count - 1);
    }

    public void Set(Context context, List<Card> targets, Card card) { }
}

// Shuffle method (shuffles the list of cards)
public class Shuffle : Method
{
    public Shuffle(IExpression list, Token accessToken) : base(list,accessToken) {}

    public override void Execute(Context context, List<Card> targets)
    {
        List<Card> temp = list.Evaluate(context, targets) as List<Card>;

        for (int i = temp.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            Card container = temp[i];
            temp[i] = temp[randomIndex];
            temp[randomIndex] = container;
        }
    }
}

public abstract class ArgumentMethod: Method{
    public ArgumentMethod(IExpression list,IExpression card, Token accessToken) : base(list,accessToken){
        this.card = card;
    }
    public IExpression card;
}

// Push method (adds card to list)
public class Push : ArgumentMethod
{
    public Push(IExpression list, IExpression card, Token accessToken) : base(list,card,accessToken){}

    public override void Execute(Context context, List<Card> targets)
    {
        (list.Evaluate(context, targets) as List<Card>).Add(card.Evaluate(context, targets) as Card);
    }
}

// SendBottom method (adds card to the bottom of the list)
public class SendBottom : ArgumentMethod
{
    public SendBottom(IExpression list, IExpression card, Token accessToken) : base(list,card,accessToken){}


    public override void Execute(Context context, List<Card> targets)
    {
        (list.Evaluate(context, targets) as List<Card>).Insert(0, card.Evaluate(context, targets) as Card);
    }
}

// Remove method (removes card from list)
public class Remove : ArgumentMethod
{
    public Remove(IExpression list, IExpression card, Token accessToken) : base(list,card,accessToken){}


    public override void Execute(Context context, List<Card> targets)
    {
        (list.Evaluate(context, targets) as List<Card>).Remove(card.Evaluate(context, targets) as Card);
    }
}


#endregion

#region Card Nodes

// Represents a card in the AST
public class CardNode : IASTNode
{
    public static readonly List<TokenType> synchroTypes = new List<TokenType>() {
        TokenType.Name, TokenType.Type , TokenType.Faction, TokenType.Power,
        TokenType.Range, TokenType.OnActivation, TokenType.RightBrace
    };

    public string name;
    public string faction;
    public Card.Type? type;
    public int? power;
    public List<string> position;
    public Onactivation activation;
    public Token keyword;
}

// Represents the onactivation field of a card
public class Onactivation : IASTNode
{
    public static readonly List<TokenType> synchroTypes= new List<TokenType>() {TokenType.LeftBrace, TokenType.RightBracket};
    public Onactivation(List<EffectActivation> activations)
    {
        this.activations = activations;
    }

    public List<EffectActivation> activations;

    public void Execute(Player triggerplayer)
    {
        foreach (EffectActivation activation in activations)
        {
            GlobalEffects.effects[activation.effect.definition].action.context.triggerplayer = triggerplayer;
            activation.Execute(triggerplayer);
        }
    }
}

// Represents an effect activation in the AST
public class EffectActivation : IASTNode
{
    public static readonly List<TokenType> synchroTypes= new List<TokenType>() {TokenType.Effect, TokenType.Selector, TokenType.PostAction, TokenType.RightBrace};
    public Effect effect;
    public Selector selector;
    public EffectActivation postAction;

    public void Execute(Player triggerplayer)
    {
        switch (selector.source.literal)
        {
            case "board": selector.filtre.list = new BoardList(null,null); break;
            case "hand": selector.filtre.list = new HandList(null,new Literal(triggerplayer), null, null); break;
            case "otherHand": selector.filtre.list = new HandList(null,new Literal(triggerplayer.Other()), null, null); break;
            case "deck": selector.filtre.list = new DeckList(null,new Literal(triggerplayer), null, null); break;
            case "otherDeck": selector.filtre.list = new DeckList(null,new Literal(triggerplayer.Other()), null, null); break;
            case "graveyard": selector.filtre.list = new GraveyardList(null,new Literal(triggerplayer), null, null); break;
            case "otherGraveyard": selector.filtre.list = new GraveyardList(null,new Literal(triggerplayer.Other()), null, null); break;
            case "field": selector.filtre.list = new FieldList(null,new Literal(triggerplayer), null, null); break;
            case "otherField": selector.filtre.list = new FieldList(null,new Literal(triggerplayer.Other()), null, null); break;
        }
        if (postAction.selector == null) postAction.selector = selector;
        else if ((string)postAction.selector.source.literal == "parent") postAction.selector.filtre.list = selector.filtre;
        var temp = selector.Select(triggerplayer);
        if ((bool)selector.single && temp.Count > 0)
        {
            List<Card> singlecard = new List<Card>() { temp[0] };
            GlobalEffects.effects[effect.definition].action.targets = singlecard;
        }
        else GlobalEffects.effects[effect.definition].action.targets = temp;
        effect.Execute(triggerplayer);
        postAction.Execute(triggerplayer);
    }
}

// Represents an effect definition in the AST
public class EffectDefinition : IASTNode
{
    public static readonly List<TokenType> synchroTypes= new List<TokenType>() {
        TokenType.Name, TokenType.Params,
        TokenType.Action, TokenType.RightBrace
    };
    public string name;
    public ParameterDef parameterdefs;
    public Action action;
    public Token keyword;

    public EffectDefinition() { }
    public void Execute()
    {
        action.Execute(action.context, action.targets);
    }
}

public class ParameterDef{
    public static readonly List<TokenType> synchroTypes= new List<TokenType>() {TokenType.Identifier, TokenType.RightBrace};
    public Dictionary<string, ExpressionType> parameters;
    public ParameterDef(Dictionary<string, ExpressionType> parameters){
        this.parameters=parameters;
    }

}

// Represents an effect in the AST
public class Effect : IASTNode
{
    public static readonly List<TokenType> synchroTypes= new List<TokenType>() {TokenType.Identifier, TokenType.Name, TokenType.RightBrace};
    public string definition;
    public Parameters parameters;
    public Token keyword;

    public void Execute(Player triggerplayer)
    {
        Dictionary<string, object> contextParameters = parameters.parameters;
        GlobalEffects.effects[definition].action.context = new Context(triggerplayer, null, contextParameters);
        GlobalEffects.effects[definition].Execute();
    }
}

public class Parameters{
    public static readonly List<TokenType> synchroTypes;
    public Dictionary<string, object> parameters;
    public Parameters(Dictionary<string,object> parameters){
        this.parameters=parameters;
    }
}

// Used ListFind object with predicate based selection Evaluate method
public class Selector : IASTNode
{
    public static readonly List<TokenType> synchroTypes = new List<TokenType> {TokenType.Source, TokenType.Single, TokenType.Predicate, TokenType.RightBrace,};
    public Selector() { }
    public Token source;
    public bool? single;
    public ListFind filtre;

    public List<Card> Select(Player triggerplayer)
    {
        return (List<Card>)filtre.Evaluate(new Context(), new List<Card>());
    }
}

#endregion

public class ProgramNode
{
    public static readonly List<TokenType> synchroTypes = new List<TokenType>() {TokenType.effect, TokenType.Card , TokenType.EOF};
    public List<IASTNode> nodes;
    public ProgramNode(List<IASTNode> nodes)
    {
        this.nodes = nodes;
    }
}

// Represents the execution context in the AST
public class Context : IASTNode
{
    public Context() { }

    public Context(Player triggerplayer, Context enclosing, Dictionary<string, object> variables)
    {
        this.triggerplayer = triggerplayer;
        this.enclosing = enclosing;
        this.variables = variables;
    }

    public Player triggerplayer;
    public Context enclosing;
    public Dictionary<string, object> variables;

    // Gets a variable's value from the context
    public object Get(Token key)
    {
        if (variables.ContainsKey(key.lexeme))
        {
            return variables[key.lexeme];
        }
        if (enclosing != null) return enclosing.Get(key);
        throw new Exception("variable was not found context");
    }

    // Sets a variable's value in the context
    public void Set(Token key, object value)
    {
        if (variables.ContainsKey(key.lexeme))
        {
            variables[key.lexeme] = value;
            return;
        }
        if (enclosing.variables.ContainsKey(key.lexeme))
        {
            enclosing.variables[key.lexeme] = value;
            return;
        }
        variables[key.lexeme] = value;
    }
}

// Represents an identifier in the AST
public class ID
{
    public ID(Token token, ExpressionType type)
    {
        this.token = token;
        this.type = type;
    }

    public Token token;
    public ExpressionType type;
}