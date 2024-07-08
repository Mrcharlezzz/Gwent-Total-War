using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UIElements;
using JetBrains.Annotations;
using Unity.VisualScripting.FullSerializer;
using Unity.VisualScripting.Antlr3.Runtime;

public class Lexer
{
    string source;
    // Reserved words
    private static Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType> {
        {"card", TokenType.Card},
        {"effect", TokenType.effect},
        {"Name", TokenType.Name},
        {"Params", TokenType.Params},
        {"Action", TokenType.Action},
        {"while", TokenType.While},
        {"for", TokenType.For},
        {"in", TokenType.In},
        {"TriggerPlayer", TokenType.TriggerPlayer},
        {"HandOfPlayer", TokenType.HandOfPlayer},
        {"FieldOfPlayer", TokenType.FieldOfPlayer},
        {"GraveyardOfPlayer", TokenType.GraveyardOfPlayer},
        {"DeckOfPlayer", TokenType.DeckOfPlayer},
        {"Hand", TokenType.Hand},
        {"Field", TokenType.Field},
        {"Graveyard", TokenType.Graveyard},
        {"Deck", TokenType.Deck},
        {"Board", TokenType.Board},
        {"Push", TokenType.Push},
        {"SendBottom", TokenType.SendBottom},
        {"Pop", TokenType.Pop},
        {"Remove", TokenType.Remove},
        {"Shuffle", TokenType.Shuffle},
        {"Find", TokenType.Find},
        {"Type", TokenType.Type},
        {"Faction", TokenType.Faction},
        {"Power", TokenType.Power},
        {"Range", TokenType.Range},
        {"Owner", TokenType.Owner},
        {"OnActivation", TokenType.OnActivation},
        {"Effect", TokenType.Effect},
        {"Selector", TokenType.Selector},
        {"Source", TokenType.Source},
        {"Single", TokenType.Single},
        {"Predicate", TokenType.Predicate},
        {"PostAction", TokenType.PostAction},
        {"Number", TokenType.Number},
        {"String", TokenType.String},
        {"Bool", TokenType.Bool},
    };

    List<Token> tokens = new List<Token>();
    int start = 0;
    int current = 0;
    int line = 1;
    int column = 1;

    // Constructor that initializes the source code to be tokenized
    public Lexer(string source)
    {
        this.source = source;
    }

    // Main method to scan and generate tokens from the source code
    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            start = current;
            ScanToken();
        }
        tokens.Add(new Token(TokenType.EOF, "", null!, line, column));
        return tokens;
    }

    // Checks if the lexer has reached the end of the source code
    public bool IsAtEnd()
    {
        return current >= source.Length;
    }

    // Scans the next token from the source code
    void ScanToken()
    {
        char c = Advance();
        switch (c)
        {
            case '(': AddToken(TokenType.LeftParen); break;
            case ')': AddToken(TokenType.RightParen); break;
            case '[': AddToken(TokenType.LeftBracket); break;
            case ']': AddToken(TokenType.RightBracket); break;
            case '{': AddToken(TokenType.LeftBrace); break;
            case '}': AddToken(TokenType.RightBrace); break;
            case ',': AddToken(TokenType.Comma); break;
            case '.': AddToken(TokenType.Dot); break;
            case ';': AddToken(TokenType.Semicolon); break;
            case ':': AddToken(TokenType.Colon); break;
            case '^': AddToken(TokenType.Pow); break;
            case '%': AddToken(TokenType.Mod); break;
            case '!':
                AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang);
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EqualEqual : Match('>') ? TokenType.Arrow : TokenType.Equal);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                break;
            case '+':
                AddToken(Match('=') ? TokenType.PlusEqual : Match('+') ? TokenType.Increment : TokenType.Plus);
                break;
            case '-':
                AddToken(Match('=') ? TokenType.MinusEqual : Match('-') ? TokenType.Decrement : TokenType.Minus);
                break;
            case '*':
                AddToken(Match('=') ? TokenType.StarEqual :  TokenType.Star);
                break;
            case '@':
                AddToken(Match('=') ? TokenType.AtSymbolEqual : Match('@') ? TokenType.AtSymbolAtSymbol : TokenType.AtSymbol);
                break;
            case '&':
                if (Match('&')) AddToken(TokenType.And);
                else DSL.Report(line, column, "", "Unexpected character: " + c);
                break;
            case '|':
                if (Match('|')) AddToken(TokenType.Or);
                else DSL.Report(line, column, "", "Unexpected character: " + c);
                break;
            case '/':
                if (Match('/'))
                {
                    // A comment goes until the end of the line.
                    while (Peek() != '\n' && !IsAtEnd()) Advance();
                }
                else
                {
                    AddToken(Match('=') ? TokenType.SlashEqual : TokenType.Slash);
                }
                break;
            case '"': String(); break;
            case ' ':
            case '\t':
            case '\r': break;
            case '\n':
                line++;
                column = 1;
                break;
            default:
                if (IsDigit(c))
                {
                    Number();
                }
                else if (IsAlpha(c))
                {
                    Identifier();
                }
                else
                {
                    DSL.Report(line, column, "", "Unexpected character: " + c);
                }
                break;
        }
    }

    // Moves to the next character while returning the current one
    char Advance()
    {
        column++;
        current++;
        return source[current - 1];
    }

    // Adds a token without a literal value
    void AddToken(TokenType type)
    {
        AddToken(type, null);
    }

    // Adds a token with a literal value
    void AddToken(TokenType type, object literal)
    {
        string lexeme = source.Substring(start, current - start);
        tokens.Add(new Token(type, lexeme, literal, line, column - lexeme.Length));
    }

    // Matches the current character with the expected one and advances if they match
    bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (source[current] != expected) return false;
        current++;
        return true;
    }

    // Peeks at the current character without consuming it
    char Peek()
    {
        if (IsAtEnd()) return '\0';
        return source[current];
    }

    // Reads a string literal from the source code
    private void String()
    {
        // Read the string until it reaches the string ending (") or a line change (\n)
        while (Peek() != '"' && Peek() != '\n' && !IsAtEnd())
        {
            Advance();
        }
        if (IsAtEnd() || Peek() == '\n')
        {
            DSL.Report(line, column, "", "Unterminated string");
            // Consume line change
            if (Peek() == '\n')
            {
                line++;
                column = 1;
                Advance();
            }
            return;
        }
        // Consume the last "
        Advance();
        // Trim the surrounding quotes.
        object value = source.Substring(start + 1, current - start - 2);
        AddToken(TokenType.StringLiteral, value);
    }

    // Reads a number literal from the source code
    void Number()
    {
        while (IsDigit(Peek())) Advance();
        // Getting token real value
        object value = int.Parse(source.Substring(start, current - start));
        AddToken(TokenType.NumberLiteral, value);
    }

    // Reads an identifier from the source code
    void Identifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();
        string lexeme = source.Substring(start, current - start);
        // If it is a reserved word save it as its respective type, else save it as identifier
        if (keywords.ContainsKey(lexeme)){
            if(lexeme == "false") AddToken(TokenType.False, false);
            else if(lexeme=="true") AddToken(TokenType.True, true);
            else AddToken(keywords[lexeme]);
        }
        else AddToken(TokenType.Identifier);
    }

    // Checks if the character is a digit
    bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    // Checks if the character is an alphabetical character or underscore
    bool IsAlpha(char c)
    {
        return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
    }

    // Checks if the character is alphanumeric
    bool IsAlphaNumeric(char c)
    {
        return IsAlpha(c) || IsDigit(c);
    }

    // Prints the list of tokens to the console
    public void Print()
    {
        foreach (Token token in tokens) System.Console.WriteLine(token.ToString());
    }
}

public enum TokenType
{
    // Single character tokens
    LeftParen, RightParen, LeftBracket, RightBracket, LeftBrace, RightBrace,
    Comma, Dot, Colon, Semicolon, Mod, Pow,

    // One or two character tokens
    Bang, BangEqual,
    Plus, PlusEqual, Increment,
    Minus, MinusEqual, Decrement,
    Star, StarEqual,
    Slash, SlashEqual,
    Equal, EqualEqual, Arrow,
    Greater, GreaterEqual,
    Less, LessEqual,
    AtSymbol, AtSymbolAtSymbol, AtSymbolEqual,
    Or, And,

    // Literals
    Identifier, StringLiteral, NumberLiteral, True, False,

    // Reserved words

    // Main
    Card, effect,
    // Effect
    Name, Params, Action,
    // Loops
    While, For, In,
    // Context Properties
    TriggerPlayer, HandOfPlayer, FieldOfPlayer, GraveyardOfPlayer,
    DeckOfPlayer, Hand, Field, Graveyard, Deck, Board,
    // Methods
    Push, SendBottom, Pop, Remove, Shuffle, Find,
    // Card
    Type, Faction, Power, Range, Owner, OnActivation,
    // OnActivation
    Effect, Selector, Source, Single, Predicate, PostAction,
    // Types
    Number, String, Bool,

    EOF
}

public class Token
{
    public TokenType type;
    public string lexeme;
    public object literal;
    public int line;
    public int column;

    // Constructor to initialize a token
    public Token(TokenType type, string lexeme, object literal, int line, int column)
    {
        this.type = type;
        this.lexeme = lexeme;
        this.literal = literal;
        this.line = line;
        this.column = column;
    }

    // Overrides the ToString method to provide a string representation of the token
    public override string ToString()
    {
        string res = $"{type.ToString()} {lexeme}";
        if (literal != null) res += " " + literal.ToString();
        res += $" [ln {line}, col {column}]";
        return res;
    }
}