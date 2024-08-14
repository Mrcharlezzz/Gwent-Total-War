using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;


public static class Tools
{
    // Determines the type of a value
    public static ExpressionType GetValueType(object value)
    {
        if (value is int) return ExpressionType.Number;
        if (value is string) return ExpressionType.String;
        if (value is bool) return ExpressionType.Bool;
        if (value is Card) return ExpressionType.Card;
        if (value is List<Card>) return ExpressionType.List;
        return ExpressionType.Null;
    }
    public static ExpressionType GetStringType(string s)
    {
        switch (s)
        {
            case "Number": return ExpressionType.Number;
            case "String": return ExpressionType.String;
            case "Bool": return ExpressionType.Bool;
            default: return ExpressionType.Null;
        }
    }
    public static Card.Type? GetCardType(string s){
        switch (s){
            case "Oro": return Card.Type.Golden;
            case "Plata": return Card.Type.Silver;
            case "Clima": return Card.Type.Weather;
            case "Aumento": return Card.Type.Boost;
            case "Líder": return Card.Type.Leader;
            case "Señuelo": return Card.Type.Decoy;
            case "Despeje": return Card.Type.Clear;
            default: return null;
        }
    }

    public static List<string> GetCardPositions(List<Card.Position> positions){
        List<string> result =new List<string>();
        foreach (Card.Position position in positions){
            string add="";
            switch(position){
                case Card.Position.Melee: add="Melee"; break;
                case Card.Position.Ranged: add="Ranged"; break;
                case Card.Position.Siege: add="Siege"; break;
                default: throw new ArgumentException("Invalid  position");
            }
            result.Add(add);
        }
        return result;
    }

    public static List<Card.Position> GetCardPositions(List<string> positions){
        List<Card.Position> result =new List<Card.Position>();
        foreach (string position in positions.OrderBy(p => p)){
            switch(position){
                case "Melee": result.Add(Card.Position.Melee); break;
                case "Ranged": result.Add(Card.Position.Ranged); break;;
                case "Siege": result.Add(Card.Position.Siege); break;
                default: throw new ArgumentException("Invalid string position");
            }
        }
        return result;
    }
    
    public static Dictionary<TKey, TValue> CopyDictionary<TKey,TValue>(Dictionary<TKey, TValue> dict){
        Dictionary<TKey,TValue> copy = new Dictionary<TKey,TValue>();
        if(dict != null )foreach(var pair in dict){
            copy.Add(pair.Key, pair.Value);
        }
        return copy;
    }
    
}

// Enum representing the types of expressions that can appear in the DSL
[Serializable]
public enum ExpressionType
{
    Number, Bool, String, Card, List, RangeList, Player, Context, Targets, Null,
}

public class ErrorBlock
{
    public string message;
    public List<Token> tokens;

    public ErrorBlock(string message, List<Token> tokens)
    {
        this.message = message;
        this.tokens = tokens;
    }

    public override string ToString()
    {
        return $"{message}: {String.Join(" ", tokens.Select(t => t.literal))}";
    }
}