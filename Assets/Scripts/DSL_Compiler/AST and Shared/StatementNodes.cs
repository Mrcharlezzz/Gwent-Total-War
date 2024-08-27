using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

// Interface for statements in the AST
public interface IStatement : IASTNode
{
    public void Execute(Context context, List<Card> targets);
}

/// <summary>
/// Represents a group of statements
/// </summary>
public abstract class Block : IStatement
{
    public readonly static List<TokenType> synchroTypes = new List<TokenType>() { TokenType.For, TokenType.While, TokenType.RightBrace };
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


/// <summary>
/// It contains the behaviour of the effect coded by the user
/// </summary>
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
        context.variables[targetsID.lexeme] = targets;
        foreach (IStatement statement in statements)
        {
            statement.Execute(context, targets);
        }
    }
}

/// <summary>
/// Represent variable or property value assignations
/// </summary>

public class Assignation : IStatement
{
    public Assignation(IExpression operand, IExpression assignation, Token operation)
    {
        this.operand = operand;
        this.assignation = assignation;
        this.operation = operation;
    }

    public IExpression operand;
    public IExpression assignation;
    public Token operation;

    public virtual void Execute(Context context, List<Card> targets)
    {
        if (operand is ICardAtom) (operand as ICardAtom).Set(context, targets, assignation.Evaluate(context, targets) as Card);
        else if (operand is PropertyAccess) (operand as PropertyAccess).Set(context, targets, assignation.Evaluate(context, targets));
        else if (operand is Variable) context.Set((operand as Variable).name, assignation.Evaluate(context, targets));
    }
}

/// <summary>
/// Increment and decrement operations
/// </summary>

public class Increment_Decrement : Assignation, IExpression
{
    public Increment_Decrement(IExpression operand, Token operation) : base(operand, null, operation) { }

    public object Evaluate(Context context, List<Card> targets)
    {
        int result = (int)operand.Evaluate(context, targets);
        Execute(context, targets);
        return result;
    }

    public override void Execute(Context context, List<Card> targets)
    {
        int result = 0;
        if (operation.type == TokenType.Increment) result = (int)operand.Evaluate(context, targets) + 1;
        else result = (int)operand.Evaluate(context, targets) - 1;
        if (operand is PowerAccess) (operand as PowerAccess).Set(context, targets, result);
        else if (operand is Variable) context.Set((operand as Variable).name, result);
    }
}

/// <summary>
/// Numeric modification operations (e.g., +=, -=, etc.)
/// </summary>

public class NumericModification : Assignation
{
    public NumericModification(IExpression operand, IExpression assignation, Token operation) : base(operand, assignation, operation) { }

    public override void Execute(Context context, List<Card> targets)
    {
        object result = null;
        switch (operation.type)
        {
            case TokenType.PlusEqual: result = (int)operand.Evaluate(context, targets) + (int)assignation.Evaluate(context, targets); break;
            case TokenType.MinusEqual: result = (int)operand.Evaluate(context, targets) - (int)assignation.Evaluate(context, targets); break;
            case TokenType.SlashEqual: result = (int)operand.Evaluate(context, targets) * (int)assignation.Evaluate(context, targets); break;
            case TokenType.StarEqual: result = (int)operand.Evaluate(context, targets) / (int)assignation.Evaluate(context, targets); break;
            case TokenType.AtSymbolEqual: result = (string)operand.Evaluate(context, targets) + (string)assignation.Evaluate(context, targets); break;
        }
        if (operand is PowerAccess) (operand as PowerAccess).Set(context, targets, result);
        else if (operand is Variable) context.Set((operand as Variable).name, result);
    }
}

/// <summary>
/// Foreach loop statement
/// </summary>

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

        var evaluation = collection.Evaluate(context, targets);
        if (evaluation is GameComponent component) evaluation = component.cards;

        foreach (Card card in (List<Card>)evaluation)
        {
            this.context.Set(variable, card);
            foreach (IStatement statement in statements)
            {
                statement.Execute(this.context, targets);
            }
        }
    }
}

/// <summary>
/// While loop statement
/// </summary>

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
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        while ((bool)predicate.Evaluate(context, targets))
        {
            if (stopwatch.ElapsedMilliseconds >= 5000) // Check if 5 seconds have passed
            {
                throw new TimeoutException("Potential infinite loop detected.");
            }
            foreach (IStatement statement in statements)
            {
                statement.Execute(this.context, targets);
            }
        }
    }
}

/// <summary>
/// Abstract class for list methods
/// </summary>

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

/// <summary>
/// Pop operation on lists
/// </summary>

public class Pop : Method, ICardAtom
{
    public Pop(IExpression list, Token accessToken) : base(list, accessToken) { }

    public object Evaluate(Context context, List<Card> targets)
    {
        var evaluation = list.Evaluate(context, targets);
        if (evaluation is GameComponent component) evaluation = component.cards;
        List<Card> safeList = (List<Card>)evaluation;

        if (safeList.Count == 0) throw new InvalidOperationException("Cannot Apply Pop method to empty list");
        Card result = safeList[safeList.Count - 1];
        Execute(context, targets);
        return result;
    }

    public override void Execute(Context context, List<Card> targets)
    {
        var evaluation = list.Evaluate(context, targets);
        if (evaluation is GameComponent component) component.Pop();
        else
        {
            List<Card> safeList = (List<Card>)evaluation;
            if (safeList.Count == 0) throw new InvalidOperationException("Cannot Apply Pop method to empty list");
            safeList.RemoveAt(safeList.Count - 1);
        }
    }

    public void Set(Context context, List<Card> targets, Card card) { }
}

/// <summary>
/// Shuffle method (shuffles the list of cards)
/// </summary>

public class Shuffle : Method
{
    public Shuffle(IExpression list, Token accessToken) : base(list, accessToken) { }

    public override void Execute(Context context, List<Card> targets)
    {
        var evaluation = list.Evaluate(context, targets);
        if (evaluation is GameComponent component) component.Shuffle();
        else
        {
            List<Card> safeList = (List<Card>)evaluation;
            for (int i = safeList.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                Card container = safeList[i];
                safeList[i] = safeList[randomIndex];
                safeList[randomIndex] = container;
            }
        }
    }
}


public abstract class ArgumentMethod : Method
{
    public ArgumentMethod(IExpression list, IExpression card, Token accessToken) : base(list, accessToken)
    {
        this.card = card;
    }
    public IExpression card;
}

/// <summary>
/// Push method (adds card to list)
/// </summary>

public class Push : ArgumentMethod
{
    public Push(IExpression list, IExpression card, Token accessToken) : base(list, card, accessToken) { }

    public override void Execute(Context context, List<Card> targets)
    {
        var evaluation = list.Evaluate(context, targets);
        if (evaluation is GameComponent component) component.Push((Card)card.Evaluate(context, targets));
        else{
            List<Card> safeList = (List<Card>)evaluation;
            safeList.Add((Card)card.Evaluate(context, targets));
        }
    }
}

/// <summary>
/// SendBottom method (adds card to the bottom of the list)
/// </summary>

public class SendBottom : ArgumentMethod
{
    public SendBottom(IExpression list, IExpression card, Token accessToken) : base(list, card, accessToken) { }


    public override void Execute(Context context, List<Card> targets)
    {
        var evaluation = list.Evaluate(context, targets);
        if (evaluation is GameComponent component) component.SendBottom((Card)card.Evaluate(context, targets));
        else{
            List<Card> safeList = (List<Card>)evaluation;
            safeList.Insert(0,(Card)card.Evaluate(context, targets));
        }
    }
}

/// <summary>
/// Remove method (removes card from list)
/// </summary>

public class Remove : ArgumentMethod
{
    public Remove(IExpression list, IExpression card, Token accessToken) : base(list, card, accessToken) { }


    public override void Execute(Context context, List<Card> targets)
    {
        var evaluation = list.Evaluate(context, targets);
        if (evaluation is GameComponent component) component.Remove((Card)card.Evaluate(context, targets));
        else{
            List<Card> safeList = (List<Card>) evaluation;
            safeList.Remove((Card)card.Evaluate(context,targets));
        }
    }
}