using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using System.Reflection.Emit;
using UnityEditor;
using Unity.Properties;
using UnityEngine.UIElements;
using System.Linq;

// Class responsible for performing semantic checks on the abstract syntax tree (AST)
public class SemanticCheck
{
    // Stores the root of the abstract syntax tree (AST)
    ProgramNode AST;
    
    // Constructor for the SemanticCheck class, initializing the AST and expression types dictionary
    public SemanticCheck(ProgramNode node)
    {
        AST = node;
        expressiontypes = new Dictionary<IExpression, ExpressionType>();
    }
    
    // Method to report semantic errors, taking a token and an error message
    public static void SemanticError(Token token, string message)
    {
        DSL.Error(token, message);
    }

    // Dictionary to store the types of expressions encountered during parsing
    public Dictionary<IExpression, ExpressionType> expressiontypes;

    // Region containing methods for checking individual expressions
    #region Expression Check
    public void CheckExpression(IExpression expression, Scope scope)
    {
        // Switch statement to handle different types of expressions
        switch (expression)
        {
            case Plus:
            case Minus:
            case Product:
            case Division:
            case Power: CheckBinaryOperator((BinaryOperator)expression, ExpressionType.Number, ExpressionType.Number,scope); break;
            case Equal:
            case Differ:
            case AtMost:
            case AtLeast:
            case Less:
            case Greater: CheckBinaryOperator((BinaryOperator)expression, ExpressionType.Number, ExpressionType.Bool,scope); break;
            case And:
            case Or: CheckBinaryOperator((BinaryOperator)expression, ExpressionType.Bool, ExpressionType.Bool,scope); break;
            case SpaceJoin:
            case Join: CheckBinaryOperator((BinaryOperator)expression, ExpressionType.String, ExpressionType.String,scope); break;
            case List list: CheckList(list,scope); break;
            case Negative negative: CheckUnary(negative, ExpressionType.Number,scope); break;
            case Negation negation: CheckUnary(negation, ExpressionType.Bool,scope); break;
            case Literal literal: CheckLiteral(literal); break;
            case Variable variable: CheckVariable(variable, scope); break;
            case ICardAtom card: CheckCardAtom(card,scope); break;
            case IndexedRange indrange: CheckIndexedRange(indrange,scope); break;
            case TriggerPlayer player: CheckTriggerPlayer(player); break;
            default: expressiontypes[expression]= ExpressionType.Null; break;
        }
    }

    // Checks binary operators, ensuring both operands match the expected types
    public void CheckBinaryOperator(BinaryOperator binaryOp, ExpressionType operandtypes, ExpressionType operatortype, Scope scope)
    {
        CheckExpression(binaryOp.left,scope);
        CheckExpression(binaryOp.right,scope);
        if (expressiontypes[binaryOp.left] != operandtypes) 
            SemanticError(binaryOp.operation, $"Left operand must be a {operandtypes}");
        if (expressiontypes[binaryOp.right] != operandtypes) 
            SemanticError(binaryOp.operation, $"Right operand must be a {operandtypes}");
        expressiontypes[binaryOp] = operatortype;
    }

    // Checks lists, handling specific cases like IndividualList and ListFind
    public void CheckList(List list,Scope scope)
    {
        expressiontypes[list] = ExpressionType.List;
        if (list is IndividualList indlist)
        {
            CheckExpression(indlist.context,scope);
            if(expressiontypes[indlist.context] != ExpressionType.Context)
                SemanticError(indlist.accessToken, "Invalid individual list, must be called by context");
            CheckExpression(indlist.player,scope);
            if (expressiontypes[indlist] != ExpressionType.Player) 
                SemanticError(indlist.playertoken, "Individual list argument must be a player");
            return;
        }
        if (list is ListFind find)
        {
            CheckExpression(find.list,scope);
            CheckExpression(find.predicate,scope);
            if (expressiontypes[find.list] != ExpressionType.List && expressiontypes[find.list] !=  ExpressionType.Targets) 
                SemanticError(find.accessToken, "Find method must be called by a list or targets");
            if (expressiontypes[find.list] != ExpressionType.Bool) 
                SemanticError(find.argumentToken, "Find method predicate must be a boolean expression");
            return;
        }
    }

    // Checks unary operations, ensuring the right operand matches the expected type
    public void CheckUnary(Unary unary, ExpressionType type, Scope scope)
    {
        CheckExpression(unary.right,scope);
        if (expressiontypes[unary.right] != type) 
            SemanticError(unary.operation, $"Right operand must be a {type}");
        expressiontypes[unary.right] = type;
    }

    // Checks literals, setting their type based on the value
    public void CheckLiteral(Literal literal)
    {
        expressiontypes[literal] = Tools.GetValueType(literal.value);
    }

    // Checks variables against the scope
    public void CheckVariable(Variable variable, Scope scope){
        expressiontypes[variable]=scope.Get(variable.name);
    }

    // Checks card atoms, handling special cases like IndexedCard and Pop
    public void CheckCardAtom(ICardAtom card, Scope scope)
    {
        expressiontypes[card] = ExpressionType.Card;
        if (card is IndexedCard indexed)
        {
            CheckExpression(indexed.list,scope);
            CheckExpression(indexed.index,scope);
            if (expressiontypes[indexed.list] != ExpressionType.List&& expressiontypes[indexed.list] != ExpressionType.Targets) 
                SemanticError(indexed.indexToken, $"Cannot apply indexing to a non-list expression");
            if (expressiontypes[indexed.index] != ExpressionType.Number) 
                SemanticError(indexed.indexToken, $"Indexer must be a Number");
            return;
        }
        if(card is Pop pop){
            CheckExpression(pop.list,scope);
            if(expressiontypes[pop.list] != ExpressionType.List && expressiontypes[pop.list] != ExpressionType.Targets) 
                SemanticError(pop.accessToken, $"Pop method must be called by a List or targets");
            return;
        }
    }

    // Checks property accesses, ensuring the accessed card is of the correct type
    public void CheckPropertyAccess(PropertyAccess access, Scope scope){
        CheckExpression(access.card, scope);
        if(expressiontypes[access.card] != ExpressionType.Card) 
            SemanticError(access.accessToken, $"Card property access must be called by a Card");
        ExpressionType type=ExpressionType.Null;
        switch(access){
            case PowerAccess: type=ExpressionType.Number; break;
            case TypeAccess:
            case FactionAccess:
            case NameAccess: type=ExpressionType.String; break;
            case RangeAccess: type= ExpressionType.RangeList; break;
        }
        expressiontypes[access]=type;
    }

    // Checks indexed ranges, ensuring the range and index are of the correct types
    public void CheckIndexedRange(IndexedRange indexedRange, Scope scope){
        CheckExpression(indexedRange.range,scope);
        CheckExpression(indexedRange.index,scope);
        if(expressiontypes[indexedRange.range] != ExpressionType.RangeList) 
            SemanticError(indexedRange.indexedToken, $"Cannot apply range indexing to non-range expression");
        if(expressiontypes[indexedRange.index] != ExpressionType.Number) 
            SemanticError(indexedRange.indexedToken, $"Indexer must be a Number");
        expressiontypes[indexedRange]= ExpressionType.String;
    }

    // Checks triggerplayernodes, setting their type to Player
    public void CheckTriggerPlayer(TriggerPlayer player){
        expressiontypes[player] = ExpressionType.Player;
    }
    #endregion

    // Region containing methods for checking statements
    #region Statement Check

    // Checks statements, handling different types of statements like Assignation, Method and Loops
    public void CheckStatement(IStatement statement, Scope scope){
        switch(statement){
            case Assignation assignation: CheckAssignation(assignation,scope); break;
            case Method method: CheckMethod(method, scope); break; 
            case While loop: CheckWhile(loop,scope); break;
            case Foreach loop: CheckForEach(loop,scope); break;
        }
    }

    // Checks assignations, ensuring the left and right sides match in type and support the operation
    public void CheckAssignation(Assignation assignation, Scope scope){
        CheckExpression(assignation.container,scope);
        CheckExpression(assignation.assignation,scope);
        switch(assignation.container){
            case Variable: CheckVariableAssignation(assignation, scope); break;
            case PropertyAccess: CheckPropertyAssignation(assignation, scope); break;
            case ICardAtom: CheckCardAssignation(assignation,scope); break;
            default: SemanticError(assignation.operation, "Invalid assignation container"); break;
        }
    }

    // Checks card assignations, ensuring the operation is valid and the value is a card
    public void CheckCardAssignation(Assignation assignation, Scope scope){
        if(assignation.operation.type!= TokenType.Equal) 
            SemanticError(assignation.operation, "Invalid card assignation operation");
        if(expressiontypes[assignation.assignation]!= ExpressionType.Card)
            SemanticError(assignation.operation, "Invalid assignation, assignation value must be card");
    }

    // Checks property assignations, ensuring the operation is valid and the value matches the property type
    public void CheckPropertyAssignation(Assignation assignation,Scope scope){
        switch (assignation.container)
        {
            case NameAccess:
            case FactionAccess: 
                if(assignation.operation.type!= TokenType.Equal&&assignation.operation.type!= TokenType.AtSymbolEqual)
                    SemanticError(assignation.operation, "Invalid String property assignation");
                if(expressiontypes[assignation.assignation]!=ExpressionType.String) 
                    SemanticError(assignation.operation, "Invalid assignation, assignation value must be a String");
                break;
            case TypeAccess:
                if(assignation.operation.type!=TokenType.Equal) 
                    SemanticError(assignation.operation, "Invalid Type property assignation");
                if(expressiontypes[assignation.assignation]!=ExpressionType.String) 
                    SemanticError(assignation.operation, "Invalid assignation, assignation value must be a String");
                break;
            case PowerAccess:
                if(assignation.operation.type==TokenType.AtSymbolEqual)
                    SemanticError(assignation.operation, "Invalid Power property assignation");
                if(!(assignation is Increment_Decrement) &&expressiontypes[assignation.assignation]!=ExpressionType.Number) 
                    SemanticError(assignation.operation, "Invalid assignation, assignation value must be a String");
                break;
            case RangeAccess: SemanticError(assignation.operation, "Range access cannot be an assignment container"); break;
            default: break;
        }
    }
    
    // Checks variable assignations, ensuring the types match and handling declarations
    public void CheckVariableAssignation(Assignation assignation, Scope scope){
        if(expressiontypes[assignation.container] == ExpressionType.Null){
            if(assignation.operation.type!=TokenType.Equal){
                SemanticError(assignation.operation, "Invalid operation in non-declared variable");
                return;
            }
            scope.Set((assignation.container as Variable).name, expressiontypes[assignation.assignation]);
        }
        else{
            if(expressiontypes[assignation.container] != expressiontypes[assignation.assignation]){
                SemanticError(assignation.operation, $"Invalid assignation, types mismatch, expected {expressiontypes[assignation.container]} value");
            }
        }
    }

    // Checks method lists and arguments and ensures they have the correct type
    public void CheckMethod(Method method, Scope scope){
        CheckExpression(method.list, scope);
        if(expressiontypes[method.list] != ExpressionType.List && expressiontypes[method.list] != ExpressionType.Targets)
            SemanticError(method.accessToken, "Method must be called by a List or targets");
        if(method is ArgumentMethod argMethod){
            CheckExpression(argMethod.card, scope);
            if(expressiontypes[argMethod.card]!= ExpressionType.Card)
                SemanticError(method.accessToken, "Invalid method argument, expected card argument");
        }
    }

    public void CheckForEach(Foreach loop, Scope scope){
        CheckExpression(loop.collection,scope);
        if(expressiontypes[loop.collection] != ExpressionType.List && expressiontypes[loop.collection] != ExpressionType.Targets && expressiontypes[loop.collection]!=ExpressionType.RangeList)
            SemanticError(loop.keyword, "Invalid collection, expected card range or card list");
        foreach(IStatement statement in loop.statements){
            if(statement is Foreach || statement is While) CheckStatement(statement,new Scope(scope));
            else CheckStatement(statement,scope);
        }
    }
    public void CheckWhile(While loop, Scope scope){
        CheckExpression(loop.predicate,scope);
        if(expressiontypes[loop.predicate] != ExpressionType.Bool)
            SemanticError(loop.keyword, "Invalid predicate, expected Bool");
        foreach(IStatement statement in loop.statements){
            if(statement is Foreach || statement is While) CheckStatement(statement,new Scope(scope));
            else CheckStatement(statement,scope);
        }
    }

    #endregion

    #region Compound Nodes Check

    public void CheckAction(Action action, Scope scope){
        scope.Set(action.contextID, ExpressionType.Context);
        scope.Set(action.targetsID, ExpressionType.Targets);
        foreach(IStatement statement in action.statements){
            if(statement is Foreach || statement is While) CheckStatement(statement,new Scope(scope));
            else CheckStatement(statement,scope);
        }
    }

    public void CheckEffectDefinition(EffectDefinition definition){
        CheckAction(definition.action, new Scope(null));
    }

    public void CheckCardNode(CardNode card, Dictionary<string,EffectDefinition> effects){

        switch(card.type){
            case Card.Type.Boost:
                if(card.power!=null) SemanticError(card.keyword, "Invalid card, boost card cannot have power field");
                if(card.position.Count==0) SemanticError(card.keyword, "Invalid card, boost card position field cannot be empty");
                break;
            case Card.Type.Clear:
                if(card.power!=null) SemanticError(card.keyword, "Invalid card, clear card cannot have power field");
                if(card.position.Count!=0) SemanticError(card.keyword, "Invalid card, clear card position field must be empty");
                break;
            case Card.Type.Decoy:
                if(card.power!=0) SemanticError(card.keyword, "Invalid card, decoy must have 0 power");
                if(card.position.Count==0) SemanticError(card.keyword, "Invalid card, boost card position field cannot be empty");
                break;
            case Card.Type.Golden:
            case Card.Type.Silver:
                if(card.power==null) SemanticError(card.keyword, "Invalid card, unit card must have power field");
                if(card.position.Count==0) SemanticError(card.keyword, "Invalid card, unit card position field cannot be empty");
                break;
            case Card.Type.Leader:
                if(card.power!=null) SemanticError(card.keyword, "Invalid card, leader card cannot have power field");
                if(card.position.Count!=0) SemanticError(card.keyword, "Invalid card, leader card position field must be empty");
                break;
            case Card.Type.Weather:
                if(card.power!=null) SemanticError(card.keyword, "Invalid card, weather card cannot have power field");
                if(card.position.Count==0) SemanticError(card.keyword, "Invalid card, weather card position field cannot be empty");
                break;
        }
        foreach (EffectActivation activation in card.activation.activations){
            CheckEffectActivation(activation, effects, true);
        }
        
    }

    public void CheckEffectActivation(EffectActivation activation, Dictionary<string,EffectDefinition> effects, bool IsRoot=false){
        EffectDefinition Acteffect = GetEffect(effects,activation.effect.definition,activation.effect.keyword);
        if(Acteffect != null){
            if(activation.effect.parameters!=null){
                bool correct=true;
                var parameters=activation.effect.parameters.parameters;
                foreach(var pair in Acteffect.parameterdefs.parameters){
                    if(parameters.ContainsKey(pair.Key)&& Tools.GetValueType(parameters[pair.Key])==pair.Value) continue;
                    correct=false; break;
                }
                if(!correct) SemanticError(activation.effect.keyword, "Effects parameters differ");
            }
            else SemanticError(activation.effect.keyword, "Effect definition doesn't have parameters, but effect activation does");
        }
        else if(activation.effect.parameters!=null) SemanticError(activation.effect.keyword, "Effect activation doesn't have parameters, but effect definition does");
        if(activation.selector!=null){
            switch(activation.selector.source.literal){
                case "board":
                case "hand":
                case"otherHand":
                case "deck":
                case "otherDeck":
                case "graveyard":
                case "otherGraveyard":
                case "field":
                case "otherField": break;
                case "parent":
                    //If its the root of the activation hierarchy its source can't be parent
                    if(IsRoot) SemanticError(activation.selector.source, "Invalid source, an effect activation can't have \"parent\" source");
                break;
                default: SemanticError(activation.selector.source, "Invalid source"); break;
            }
            CheckExpression(activation.selector.filtre.predicate, new Scope(null));
            if(expressiontypes[activation.selector.filtre.predicate] != ExpressionType.Bool)
                SemanticError(activation.selector.filtre.argumentToken, "Invalid predicate, predicate must be bool");
        }

        if(activation.postAction!=null) CheckEffectActivation(activation.postAction, effects);
    }

    public void CheckProgram(ProgramNode program){
        Dictionary<string,EffectDefinition> effects= new Dictionary<string, EffectDefinition>();
        foreach(EffectDefinition definition in program.nodes.Where(n => n is EffectDefinition))
        {
            if(GlobalEffects.effects.ContainsKey(definition.name)||effects.ContainsKey(definition.name))
                SemanticError(definition.keyword, "There is an effect with the same name");
            else effects[definition.name]=definition;
            CheckEffectDefinition(definition);
        }

        foreach(CardNode card in program.nodes.Where(n => n is CardNode)){
            CheckCardNode(card,effects);
        }
    }

    public EffectDefinition GetEffect(Dictionary<string,EffectDefinition> effects, string name, Token errorPointer){
        if(effects.ContainsKey(name)) return effects[name];
        if(GlobalEffects.effects.ContainsKey(name)) return  GlobalEffects.effects[name];
        SemanticError(errorPointer, "There aren't any existing effects with this name");
        return null;
    }


    #endregion
}


// Static class for semantic tools and utilities
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
                case Card.Position.siege: add="Siege"; break;
                default: throw new ArgumentException("Invalid string position");
            }
            result.Add(add);
        }
        return result;
    }
}

// Enum representing the types of expressions that can appear in the DSL
public enum ExpressionType
{
    Number, Bool, String, Card, List, RangeList, Player, Context, Targets, Null,
}

// Class representing a scope within the DSL, tracking variable types
public class Scope
{
    public Scope(Scope enclosing){
        this.enclosing = enclosing;
        types= new Dictionary<string, ExpressionType>();
    }
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


    // Pending implementation notes
    ///////PENDIENTE, CREAR UN SISTEMA QUE ACEPTE LA LISTA DE RANGO, PERO QUE NO PERMITA REALIZAR 
    ///////OPERACIONES DE LISTAS DE CARTAS SOBRE EL
    ///////PENDIENTE MANEJAR QUE NO SE PARSEE +-
    ///////MANEJAR DUPLICADO DE ERRORES EN EL ERROR DE ADD DEL PARSER
    ///////MANEJAR ACCESOS Y CONVERSIONES DE POSITION A STRING Y DE STRING A POSITION

    

