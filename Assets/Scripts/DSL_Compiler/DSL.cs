using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;

public class DSL
{
    static bool hadError = false;
    static int destinyDeck = 0;
    public static TextMeshProUGUI SelectionText;

    public static void DeckSwitch(){
        destinyDeck = (destinyDeck + 1) % 3;
        switch (destinyDeck) {
            case 0: SelectionText.text = "Storage"; break;
            case 1: SelectionText.text = "Deck 1"; break;
            case 2: SelectionText.text = "Deck 2"; break;
        }
    }

    public static void Report(int line, int column, string where, string message)
    {
        Debug.Log($"[Ln {line}, Col {column}] {where} Error: " + message);
        hadError = true;
    }
    public static void Error(Token token, string message)
    {
        if (token.type == TokenType.EOF) Report(token.line, token.column, "at end", message);
        else Report(token.line, token.column, $"at'{token.lexeme}'", message);
    }

    public static void CancelCompilation(){
        Debug.Log("Invalid code\n");
        return;
    }

    public static void Compile(string source)
    {
        hadError=false;
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

        foreach(var effect in nodes.nodes.Where(n => n is EffectDefinition).Select(n => (EffectDefinition)n)) {
            
            GlobalEffects.effects[effect.name] = effect;
        }

        foreach(var card in nodes.nodes.Where(n => n is CardNode).Select(n => (CardNode)n)){
            Card newcard = null;
            switch(card.type){
                case Card.Type.Silver:
                case Card.Type.Golden:
                    newcard = new Unit(Database.Count, null, card.name, Resources.Load<Sprite>("DefaultImage"), card.type, "Created Card", card.faction, Tools.GetCardPositions(card.position), card.activation, (int)card.power);
                    break;
                case Card.Type.Decoy:
                    newcard = new Decoy(Database.Count, null, card.name, Resources.Load<Sprite>("DefaultImage"), card.type, "Created Card", card.faction, Tools.GetCardPositions(card.position), card.activation, 0);
                    break;
                case Card.Type.Boost:
                    newcard = new Boost(Database.Count, null, card.name, Resources.Load<Sprite>("DefaultImage"), card.type, "Created Card", card.faction, Tools.GetCardPositions(card.position), card.activation);
                    break;
                case Card.Type.Weather:
                    newcard = new Weather(Database.Count, null, card.name, Resources.Load<Sprite>("DefaultImage"), card.type, "Created Card", card.faction, Tools.GetCardPositions(card.position), card.activation);
                    break;
                case Card.Type.Leader:
                    newcard = new Leader(Database.Count, null, card.name, Resources.Load<Sprite>("DefaultImage"), card.type, "Created Card", card.faction, Tools.GetCardPositions(card.position), card.activation);
                    break;
                case Card.Type.Clear:
                    newcard = new Clear(Database.Count, null, card.name, Resources.Load<Sprite>("DefaultImage"), card.type, "Created Card", card.faction, Tools.GetCardPositions(card.position), card.activation);
                    break;
            }
            Database.storage.Add(newcard);
            if(destinyDeck == 1) Database.deck1.Add(newcard);
            if(destinyDeck == 2) Database.deck2.Add(newcard);
        }
    }
}
