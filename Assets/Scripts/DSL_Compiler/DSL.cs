using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public static class DSL{
    static bool hadError = false;

    public static void Report(int line,int column, string where, string message) {
        System.Console.WriteLine($"[Ln {line}, Col {column}] {where} Error: " + message);
        hadError = true;
    }
    public static void Error(Token token, string message) {
        if(token.type==TokenType.EOF) Report(token.line, token.column,"at end",message);
        else Report(token.line,token.column, $"at'{token.lexeme}'",message);
    }
}
