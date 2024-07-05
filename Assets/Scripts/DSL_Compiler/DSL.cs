using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public static class DSL
{
    static bool hadError = false;

    public static void Report(int line, int column, string where, string message)
    {
        System.Console.WriteLine($"[Ln {line}, Col {column}] {where} Error: " + message);
        hadError = true;
    }
    public static void Error(Token token, string message)
    {
        if (token.type == TokenType.EOF) Report(token.line, token.column, "at end", message);
        else Report(token.line, token.column, $"at'{token.lexeme}'", message);
    }

    public static void Compile(string source)
    {
        hadError=false;
        // Create an instance of the Lexer with the input source code
        Lexer lexer= new Lexer(source);
        // Tokenize the input source code
        var tokens = lexer.ScanTokens();
        if(hadError) return;
        // Create an instance of the Parser with the tokens
        Parser parser = new Parser(tokens);
        // Parse the tokens to generate the AST
        var Nodes= parser.Program();
        if(hadError) return;

        // Optional: Semantic checking
        // SemanticChecker semanticChecker = new SemanticChecker(programNode);
        // semanticChecker.CheckSemantic();



    }


}
