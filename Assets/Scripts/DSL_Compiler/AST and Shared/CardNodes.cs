using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



/// <summary>
/// Represents a card in the AST
/// </summary>
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


/// <summary>
/// This parameters contains the behaviour of the effects executed when a card is played
/// </summary>
public class Onactivation : IASTNode
{
    /// <summary>
    /// This list contains the types that will stop the
    /// synchronization of the parser when an error occurs
    /// </summary>
    public static readonly List<TokenType> synchroTypes= new List<TokenType>() {TokenType.LeftBrace, TokenType.RightBracket, TokenType.Comma};
    public Onactivation(List<EffectActivation> activations)
    {
        this.activations = activations;
    }

    public List<EffectActivation> activations;

    /// <summary>
    /// Executes each effect activation in activations
    /// </summary>
    /// <param name="triggerplayer"> player who played the card and triggered the effect</param>
    public void Execute(Player triggerplayer)
    {
        foreach (EffectActivation activation in activations)
        {
            activation.Execute(triggerplayer);
        }
    }
}

/// <summary>
/// Represents the behaviour of one of the effects in the onactivation list
/// </summary>

public class EffectActivation : IASTNode
{
    /// <summary>
    /// This list contains the types that will stop the
    /// synchronization of the parser when an error occurs
    /// </summary>
    public static readonly List<TokenType> synchroTypes= new List<TokenType>() {TokenType.Effect, TokenType.Selector, TokenType.PostAction, TokenType.RightBrace, TokenType.RightBracket};
    
    public Effect effect;

    /// <summary>
    /// Selects the target list that will be used in the effect definition
    /// </summary>
    public Selector selector;
    public EffectActivation postAction;

    /// <summary>
    /// Selects the target list, executes the effect definition and then executes the postAction
    /// </summary>
    /// <param name="triggerplayer"> player who play the card and triggered the effect</param>
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
            if(postAction != null){
                if (postAction.selector == null) postAction.selector = selector;
                else if ((string)postAction.selector.source.literal == "parent") postAction.selector.filtre.list = selector.filtre;
            }
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
        if(postAction !=null )postAction.Execute(triggerplayer);
    }
}

/// <summary>
/// Represent the behaviour of the effect after the parameters are set
/// </summary>
public class EffectDefinition : IASTNode
{
    /// <summary>
    /// This list contains the types that will stop the
    /// synchronization of the parser when an error occurs
    /// </summary>
    public static readonly List<TokenType> synchroTypes= new List<TokenType>() {
        TokenType.Name, TokenType.Params,
        TokenType.Action, TokenType.RightBrace
    };
    public string name;

    /// <summary>
    /// List of defined parameters
    /// </summary>
    public ParameterDef parameterdefs;

    /// <summary>
    /// Block ofm statements which contain the effect behaviour
    /// </summary>
    public Action action;
    public Token keyword;

    public EffectDefinition() {}
    public void Execute()
    {
        action.Execute(action.context, action.targets);
    }
}

/// <summary>
/// Represent the type of each parameter
/// </summary>
public class ParameterDef : IASTNode{

    /// <summary>
    /// This list contains the types that will stop the
    /// synchronization of the parser when an error occurs
    /// </summary>
    public static readonly List<TokenType> synchroTypes= new List<TokenType>() {TokenType.Identifier, TokenType.RightBrace};
    public Dictionary<string, ExpressionType> parameters;
    public ParameterDef(Dictionary<string, ExpressionType> parameters){
        this.parameters=parameters;
    }
}

/// <summary>
/// Represents the name of the definition and the parameters that will be copied to it
/// </summary>

public class Effect : IASTNode
{
    /// <summary>
    /// This list contains the types that will stop the
    /// synchronization of the parser when an error occurs
    /// </summary>
    public static readonly List<TokenType> synchroTypes= new List<TokenType>() {TokenType.Identifier, TokenType.Name, TokenType.RightBrace, TokenType.RightBracket};
    public string definition;
    public Parameters parameters;
    public Token keyword;

    public void Execute(Player triggerplayer)
    {
        Dictionary<string, object> copy;
        if(parameters is null) copy = new Dictionary<string, object>();
        else copy = Tools.CopyDictionary<string,object>(parameters.parameters);
        Context rootContext = new Context(triggerplayer, null, copy);
        GlobalEffects.effects[definition].action.context = new Context(triggerplayer, rootContext, new Dictionary<string, object>());
        GlobalEffects.effects[definition].Execute();
    }
}

/// <summary>
/// Represents the value of each parameter
/// </summary>
public class Parameters{
    public static readonly List<TokenType> synchroTypes= new List<TokenType>() {TokenType.Identifier, TokenType.RightBrace};
    public Dictionary<string, object> parameters;
    public Parameters(Dictionary<string,object> parameters){
        this.parameters=parameters;
    }
}

/// <summary>
/// Filtre that selects the target list
/// </summary>
public class Selector : IASTNode
{
    public static readonly List<TokenType> synchroTypes = new List<TokenType> {TokenType.Source, TokenType.Single, TokenType.Predicate, TokenType.RightBrace, TokenType.LeftBracket};
    public Selector() { }
    public Token source;
    public bool? single;
    /// <summary>
    /// Used ListFind object with predicate based selection Evaluate method
    /// </summary>a
    public ListFind filtre;

    public List<Card> Select(Player triggerplayer)
    {
        return (List<Card>)filtre.Evaluate(new Context(triggerplayer), new List<Card>());
    }
}

/// <summary>
/// Its the result of the parsing proccess, contains
/// a list of card nodes and effect definition nodes
/// </summary>
public class ProgramNode : IASTNode
{
    public static readonly List<TokenType> synchroTypes = new List<TokenType>() {TokenType.effect, TokenType.Card , TokenType.EOF};
    public List<IASTNode> nodes;
    public ProgramNode(List<IASTNode> nodes)
    {
        this.nodes = nodes;
    }
}

/// <summary>
/// Represents the scopes for the values of variables during
/// execution, it has a nested structure and auxiliar methods 
/// for finding and setting elements
/// </summary>

public class Context : IASTNode
{
    public Context(Player triggerplayer) {
        this.triggerplayer = triggerplayer;
        variables = new Dictionary<string, object>();
    }

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
        if (enclosing != null && enclosing.Contains(key))
        {
            enclosing.Set(key,value);
            return;
        }
        variables[key.lexeme] = value;
    }
    public bool Contains(Token key){
        if(variables.ContainsKey(key.lexeme))return true;
        if(enclosing != null) return enclosing.Contains(key);
        else return false;
    }
}