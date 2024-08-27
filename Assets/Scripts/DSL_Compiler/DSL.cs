using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;

public static class DSL
{
    public static TMP_InputField outputField;
    static bool hadError = false;
    public static int destinyDeck = 0;

    public static void DeckSwitch(){
        destinyDeck = (destinyDeck + 1) % 2;
    }

    /// <summary>
    /// Prints the error in the console
    /// </summary>
    /// <param name="line"> error line</param>
    /// <param name="column">error column</param>
    /// <param name="where"> error token</param>
    /// <param name="message"> error message</param>
    public static void Report(int line, int column, string where, string message)
    {
        Debug.LogError($"[Ln {line}, Col {column}] {where} Error: " + message);
        outputField.text+=$"[Ln {line}, Col {column}] {where} Error: " + message+'\n';
        hadError = true;
    }

    public static void Error(Token token, string message)
    {
        if (token.type == TokenType.EOF) Report(token.line, token.column, "at end", message);
        else Report(token.line, token.column, $"at'{token.lexeme}'", message);
    }

    /// <summary>
    /// Informs that compilation ended and the given code is invalid
    /// </summary>
    public static void CancelCompilation(){
        Debug.Log("Invalid code\n");
        outputField.text+= "Invalid code\n";
        return;
    }

    /// <summary>
    /// Compile user code
    /// </summary>
    /// <param name="source"> source code</param>
    public static void Compile(string source)
    {
        hadError=false;
        outputField.text= "";

        if(source==""){
            Debug.LogError("Empty source");
            CancelCompilation();
            return;
        }
        // Create an instance of the Lexer with the input source code
        Lexer lexer= new Lexer(source);
        // Tokenize the input source code
        var tokens = lexer.ScanTokens();
        if(hadError){
            CancelCompilation();
            return;
        }
        // Create an instance of the Parser with the tokens
        Parser parser = new Parser(tokens);
        // Parse the tokens to generate the AST
        var nodes= parser.Program();
        if(hadError){
            CancelCompilation();
            return;
        }
        SemanticCheck check = new SemanticCheck(nodes);
        check.CheckProgram(check.AST);
        if(hadError){
            CancelCompilation();
            return;
        }

        Debug.Log("Successfull Compilation");
        outputField.text+="Successfull Compilation\n";

        foreach(var effect in nodes.nodes.Where(n => n is EffectDefinition).Select(n => (EffectDefinition)n)) {
            
            GlobalEffects.effects[effect.name] = effect;
        }

        foreach(var card in nodes.nodes.Where(n => n is CardNode).Select(n => (CardNode)n)){
            Card newcard = null;
            switch(card.type){
                case Card.Type.Silver:
                case Card.Type.Golden:
                    newcard = new Unit(Database.Count, 0, card.name, "DefaultImage", card.type, "Created Card", card.faction, Tools.GetCardPositions(card.position), card.activation, (int)card.power);
                    break;
                case Card.Type.Decoy:
                    newcard = new Decoy(Database.Count, 0, card.name, "DefaultImage", card.type, "Created Card", card.faction, Tools.GetCardPositions(card.position), card.activation, 0);
                    break;
                case Card.Type.Boost:
                    newcard = new Boost(Database.Count, 0, card.name, "DefaultImage", card.type, "Created Card", card.faction, Tools.GetCardPositions(card.position), card.activation);
                    break;
                case Card.Type.Weather:
                    newcard = new Weather(Database.Count, 0, card.name, "DefaultImage", card.type, "Created Card", card.faction, Tools.GetCardPositions(card.position), card.activation);
                    break;
                case Card.Type.Leader:
                    newcard = new Leader(Database.Count, 0, card.name, "DefaultImage", card.type, "Created Card", card.faction, Tools.GetCardPositions(card.position), card.activation);
                    break;
                case Card.Type.Clear:
                    newcard = new Clear(Database.Count, 0, card.name, "DefaultImage", card.type, "Created Card", card.faction, Tools.GetCardPositions(card.position), card.activation);
                    break;
            }
            if(destinyDeck == 0) Database.deck1.Add(newcard);
            if(destinyDeck == 1) Database.deck2.Add(newcard);
        }
    }
}
