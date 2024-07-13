using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
[Serializable]
public class Onactivation : IASTNode
{
    public static readonly List<TokenType> synchroTypes= new List<TokenType>() {TokenType.LeftBrace, TokenType.RightBracket, TokenType.Comma};
    public Onactivation(List<EffectActivation> activations)
    {
        this.activations = activations;
    }

    public List<EffectActivation> activations;

    public void Execute(Player triggerplayer)
    {
        foreach (EffectActivation activation in activations)
        {
            activation.Execute(triggerplayer);
        }
    }
}

// Represents an effect activation in the AST
[Serializable]
public class EffectActivation : IASTNode
{
    public static readonly List<TokenType> synchroTypes= new List<TokenType>() {TokenType.Effect, TokenType.Selector, TokenType.PostAction, TokenType.RightBrace, TokenType.RightBracket};
    public Effect effect;
    public Selector selector;
    public EffectActivation postAction;

    public void Execute(Player triggerplayer)
    {
        if(selector != null){
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
        }
        else GlobalEffects.effects[effect.definition].action.targets=new List<Card>();
        effect.Execute(triggerplayer);
        postAction.Execute(triggerplayer);
    }
}

// Represents an effect definition in the AST
[Serializable]
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

    public EffectDefinition() {}
    public void Execute()
    {
        action.Execute(action.context, action.targets);
    }
}

public class ParameterDef : IASTNode{
    public static readonly List<TokenType> synchroTypes= new List<TokenType>() {TokenType.Identifier, TokenType.RightBrace};
    public Dictionary<string, ExpressionType> parameters;
    public ParameterDef(Dictionary<string, ExpressionType> parameters){
        this.parameters=parameters;
    }
}

// Represents an effect in the AST
public class Effect : IASTNode
{
    public static readonly List<TokenType> synchroTypes= new List<TokenType>() {TokenType.Identifier, TokenType.Name, TokenType.RightBrace, TokenType.RightBracket};
    public string definition;
    public Parameters parameters;
    public Token keyword;

    public void Execute(Player triggerplayer)
    {
        Dictionary<string, object> copy = Tools.CopyDictionary(parameters.parameters);
        Context rootContext = new Context(triggerplayer, null, copy);
        GlobalEffects.effects[definition].action.context = new Context(triggerplayer, rootContext, new Dictionary<string, object>());
        GlobalEffects.effects[definition].Execute();
    }
}

public class Parameters{
    public static readonly List<TokenType> synchroTypes= new List<TokenType>() {TokenType.Identifier, TokenType.RightBrace};
    public Dictionary<string, object> parameters;
    public Parameters(Dictionary<string,object> parameters){
        this.parameters=parameters;
    }
}

// Used ListFind object with predicate based selection Evaluate method
public class Selector : IASTNode
{
    public static readonly List<TokenType> synchroTypes = new List<TokenType> {TokenType.Source, TokenType.Single, TokenType.Predicate, TokenType.RightBrace, TokenType.LeftBracket};
    public Selector() { }
    public Token source;
    public bool? single;
    public ListFind filtre;

    public List<Card> Select(Player triggerplayer)
    {
        return (List<Card>)filtre.Evaluate(new Context(), new List<Card>());
    }
}

public class ProgramNode : IASTNode
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

