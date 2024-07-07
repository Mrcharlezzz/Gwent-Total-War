using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using System.Reflection.Emit;

public class SemanticCheck
{
    ProgramNode AST;
    public SemanticCheck(ProgramNode node)
    {
        AST = node;
        expressiontypes = new Dictionary<IExpression, ExpressionType>();
    }
    public static void SemanticError(Token token, string message)
    {
        DSL.Error(token, message);
    }

    public Dictionary<IExpression, ExpressionType> expressiontypes;



    public void CheckExpression(IExpression expression)
    {
        switch (expression)
        {
            case Plus:
            case Minus:
            case Product:
            case Division:
            case Power: CheckBinaryOperator((BinaryOperator)expression, ExpressionType.Number, ExpressionType.Number); break;
            case Equal:
            case Differ:
            case AtMost:
            case AtLeast:
            case Less:
            case Greater: CheckBinaryOperator((BinaryOperator)expression, ExpressionType.Number, ExpressionType.Bool); break;
            case And:
            case Or: CheckBinaryOperator((BinaryOperator)expression, ExpressionType.Bool, ExpressionType.Bool); break;
            case Join: CheckBinaryOperator((BinaryOperator)expression, ExpressionType.String, ExpressionType.String); break;
            case List: CheckList((List)expression); break;
            case Negative negative: CheckUnary(negative, ExpressionType.Number); break;
            case Negation negation: CheckUnary(negation, ExpressionType.Bool); break;
            case Literal literal: CheckLiteral(literal); break;
            case ICardAtom: break;



        }
    }

    public void CheckBinaryOperator(BinaryOperator binaryOp, ExpressionType operandtypes, ExpressionType operatortype)
    {
        CheckExpression(binaryOp.left);
        CheckExpression(binaryOp.right);
        if (expressiontypes[binaryOp.left] != operandtypes) SemanticError(binaryOp.operation, $"Left operand must be a {operandtypes}");
        if (expressiontypes[binaryOp.right] != operandtypes) SemanticError(binaryOp.operation, $"Right operand must be a {operandtypes}");
        expressiontypes[binaryOp] = operatortype;
    }

    public void CheckList(List list)
    {
        expressiontypes[list] = ExpressionType.List;
        if (list is IndividualList indlist)
        {
            CheckExpression(indlist.player);
            if (expressiontypes[indlist] != ExpressionType.Player) SemanticError(indlist.playertoken, "Individual list argument must be a player");
            return;
        }
        if (list is ListFind find)
        {
            CheckExpression(find.list);
            CheckExpression(find.predicate);
            if (expressiontypes[find.list] != ExpressionType.List) SemanticError(find.accessToken, "Find method must be called by a list");
            if (expressiontypes[find.list] != ExpressionType.Bool) SemanticError(find.argumentToken, "Find method predicate must be a boolean expression");
            return;
        }
    }

    public void CheckUnary(Unary unary, ExpressionType type)
    {
        CheckExpression(unary.right);
        if (expressiontypes[unary.right] != type) SemanticError(unary.operation, $"Right operand must be a {type}");
        expressiontypes[unary.right] = type;
    }

    public void CheckLiteral(Literal literal)
    {
        expressiontypes[literal] = SemanticTools.GetValueType(literal.value);
    }

    public void CheckCardAtom(ICardAtom card)
    {
        expressiontypes[card] = ExpressionType.Card;
        if (card is IndexedCard indexed)
        {
            CheckExpression(indexed.list);
            CheckExpression(indexed.index);
            if (expressiontypes[indexed.list] != ExpressionType.List) SemanticError(indexed.indexToken, $"Cannot apply indexing to a non-list expression");
            if (expressiontypes[indexed.index] != ExpressionType.Number) SemanticError(indexed.indexToken, $"Indexer must be a Number");
            return;
        }
        if(card is Pop pop){
            CheckExpression(pop.list);
            if(expressiontypes[pop.list] != ExpressionType.List) SemanticError(pop.accessToken, $"Pop method must be called by a List");
            return;
        }
    }

    public void CheckPropertyAccess(PropertyAccess access){
        CheckExpression(access.card);
        if(expressiontypes[access.card] != ExpressionType.Card) SemanticError(access.accessToken, $"Card property access must be called by a Card");
        ExpressionType type=ExpressionType.Null;
        switch(access){
            case PowerAccess: type=ExpressionType.Number; break;
            case TypeAccess:
            case FactionAccess:
            case NameAccess: type=ExpressionType.String; break;
            ///////PENDIENTE, CREAR UN SISTEMA QUE ACEPTE LA LISTA DE RANGO, PERO QUE NO PERMITA REALIZAR 
            ///////OPERACIONES DE LISTAS DE CARTAS SOBRE EL
            ///////PREGUNTAR ACERCA DE MANEJO CORRECTO DEL SYNCHRONIZE EN EL PARSER

        }
    }




}



// Static class for semantic tools and utilities
public static class SemanticTools
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
}

public enum ExpressionType
{
    Number, Bool, String, Card, List, Player, Context, Targets, Null,
}

public class Scope
{
    Scope enclosing;
    public Dictionary<string, ExpressionType> types;

    // Gets a variable's type from the scope
    public ExpressionType Get(Token key)
    {
        if (types.ContainsKey(key.lexeme))
        {
            return types[key.lexeme];
        }
        if (enclosing != null) return enclosing.Get(key);
        SemanticCheck.SemanticError(key, "Undefined variable");
        return ExpressionType.Null;
    }

    // Sets a variable's type in the scope
    public void Set(Token key, ExpressionType type)
    {
        key.literal = type;
        if (types.ContainsKey(key.lexeme) && types[key.lexeme] != type) SemanticCheck.SemanticError(key, "Assignation type differs from variable type");
        if (enclosing.types.ContainsKey(key.lexeme))
        {
            if (enclosing.types[key.lexeme] != type) SemanticCheck.SemanticError(key, "Assignation type differs from variable type");
            else return;
        }
        types[key.lexeme] = type;
    }
}
