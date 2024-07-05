using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.Collections;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor.PackageManager;

public class Parser
{
    List<Token> tokens;
    int current = 0;
    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    //All classes in this region has similar logic to their counterparts in Lexer class
    #region Tools

    //Raises an error and reports it
    public static Exception Error(Token token, string message)
    {
        DSL.Error(token, message);
        return new Exception();
    }

    //Tries to match the current token with a list of types and advances if it matches with any of the tokens in the list
    bool Match(List<TokenType> types)
    {
        foreach (TokenType type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }

    //Tries to match the current token with the given type and advances if it matches
    bool Match(TokenType type)
    {
        if (Check(type))
        {
            Advance();
            return true;
        }
        return false;
    }

    //Checks if the current token matches the given type
    bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().type == type;
    }

    //Advances to the next token
    Token Advance()
    {
        if (!IsAtEnd()) current++;
        return Previous();
    }

    //Check if parser has reached the end of the token list
    bool IsAtEnd()
    {
        return Peek().type == TokenType.EOF;
    }

    //Returns the current token
    Token Peek()
    {
        return tokens[current];
    }

    //Returns the next token
    Token PeekNext()
    {
        if (IsAtEnd()) return tokens[current];
        else return tokens[current + 1];
    }

    //Returns the previous token
    Token Previous()
    {
        return tokens[current - 1];
    }

    //Consumes the current token if it matches the given type, otherwise thwros an error
    Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw Error(Peek(), message);
    }

    //Sinchronizes the parser state after an error
    void synchronize()
    {
        Advance();
        while (!IsAtEnd())
        {
            if (Previous().type == TokenType.Semicolon) return;
            switch (Peek().type)
            {
                case TokenType.effect:
                case TokenType.Card:
                case TokenType.For:
                case TokenType.While:
                case TokenType.Params:
                case TokenType.Action:
                case TokenType.OnActivation:
                case TokenType.Effect:
                case TokenType.Selector:
                case TokenType.Source:
                case TokenType.Single:
                case TokenType.Predicate:
                case TokenType.PostAction:
                    return;
                case TokenType.Name:
                case TokenType.Type:
                case TokenType.Faction:
                case TokenType.Power:
                case TokenType.Range:
                    if (Previous().type != TokenType.Dot) return;
                    break;
                default: break;
            }
            Advance();

        }
    }

    #endregion




    #region Expression Parsing

    //Parses an expression
    public IExpression Expression()
    {
        try
        {
            return Logic();
        }
        catch
        {
            return null;
        }

    }

    //Parses logical expressions
    public IExpression Logic()
    {
        IExpression expr = Equality();
        List<TokenType> types = new List<TokenType>() { TokenType.And, TokenType.Or };
        while (Match(types))
        {
            Token operation = Previous();
            IExpression right = Equality();
            if (Previous().type == TokenType.And) expr = new And(expr, right, operation);
            else expr = new Or(expr, right, operation);
        }
        return expr;
    }

    //Parses equality expressions
    public IExpression Equality()
    {
        IExpression expr = Comparison();
        List<TokenType> types = new List<TokenType>() { TokenType.BangEqual, TokenType.EqualEqual };
        while (Match(types))
        {
            Token operation = Previous();
            IExpression right = Comparison();
            if (Previous().type == TokenType.BangEqual) expr = new Differ(expr, right, operation);
            else expr = new Equal(expr, right, operation);
        }
        return expr;
    }

    //Parses comparison expressions
    public IExpression Comparison()
    {
        IExpression expr = Term();
        List<TokenType> types = new List<TokenType>(){
            TokenType.Greater,TokenType.GreaterEqual,
            TokenType.Less,TokenType.LessEqual,
        };
        while (Match(types))
        {
            Token operation = Previous();
            IExpression right = Term();
            switch (Previous().type)
            {
                case TokenType.Greater: expr = new Greater(expr, right, operation); break;
                case TokenType.GreaterEqual: expr = new AtMost(expr, right, operation); break;
                case TokenType.Less: expr = new Less(expr, right, operation); break;
                case TokenType.LessEqual: expr = new AtLeast(expr, right, operation); break;
            }
        }
        return expr;
    }

    //Parses term expressions (addition and substraction)
    public IExpression Term()
    {
        IExpression expr = Factor();
        List<TokenType> types = new List<TokenType>() { TokenType.Minus, TokenType.Plus };
        while (Match(types))
        {
            Token operation = Previous();
            IExpression right = Factor();
            if (Previous().type == TokenType.Plus) expr = new Plus(expr, right, operation);
            else expr = new Minus(expr, right, operation);
        }
        return expr;
    }

    //Parses factor expressions (product and division)
    public IExpression Factor()
    {
        IExpression expr = Power();
        List<TokenType> types = new List<TokenType>() { TokenType.Slash, TokenType.Star };
        while (Match(types))
        {
            Token operation = Previous();
            IExpression right = Unary();
            if (Previous().type == TokenType.Slash) expr = new Division(expr, right, operation);
            else expr = new Product(expr, right, operation);
        }
        return expr;
    }

    //Parses power expressions (exponentiation)
    public IExpression Power()
    {
        IExpression expr = Unary();
        while (Match(TokenType.Pow))
        {
            Token operation = Previous();
            IExpression right = Unary();
            expr = new Power(expr, right, operation);
        }
        return expr;
    }

    //Parses unary expressions (negation and logical NOT)
    public IExpression Unary()
    {
        List<TokenType> types = new List<TokenType>() { TokenType.Minus, TokenType.Bang };
        if (Match(types))
        {
            Token operations = Previous();
            IExpression right = Primary();
            if (Previous().type == TokenType.Minus) return new Negative(right, Previous());
            else return new Negation(right, Previous());
        }
        return Primary();
    }

    //Parses primary expressions (literals, identifiers, and grouped expressions)
    public IExpression Primary()
    {
        if (Match(TokenType.False)) return new Literal(false);
        if (Match(TokenType.True)) return new Literal(true);
        List<TokenType> types = new List<TokenType>() { TokenType.NumberLiteral, TokenType.StringLiteral };
        if (Match(types)) return new Literal(Previous().literal);
        if (Match(TokenType.LeftParen))
        {
            IExpression expr = Expression();
            Consume(TokenType.RightParen, "Expect ')' after expression");
            return expr;
        }
        if (Match(TokenType.Identifier))
        {
            IExpression left = new Variable(Previous());
            if (Peek().type == TokenType.Dot) return Access();
            return left;
        }

        if (Check(TokenType.Dot)) throw Error(Peek(), "Invalid property access");
        throw Error(Peek(), "Expect expression");
    }

    //Parses access expressions (property and method access)
    public IExpression Access()
    {
        Token dot = Previous();
        IExpression left = new Variable(Peek());
        Advance();
        if (Check(TokenType.LeftBracket)) left = Indexer(left);
        while (Match(TokenType.Dot))
        {
            List<TokenType> types = new List<TokenType>(){
                TokenType.HandOfPlayer,TokenType.DeckOfPlayer,TokenType.GraveyardOfPlayer,
                TokenType.FieldOfPlayer,TokenType.Board,
            };
            if (Match(types))
            {
                if (Previous().type != TokenType.Board)
                {
                    TokenType aux = Previous().type;
                    Token player = Consume(TokenType.LeftParen, "Expected Player Argument");
                    IExpression arg = Expression();
                    Consume(TokenType.RightParen, "Expected ')' after Player Argument");
                    switch (aux)
                    {
                        case TokenType.HandOfPlayer: left = Indexer(new HandList(arg, player)); break;
                        case TokenType.DeckOfPlayer: left = Indexer(new DeckList(arg, player)); break;
                        case TokenType.GraveyardOfPlayer: left = Indexer(new GraveyardList(arg, player)); break;
                        case TokenType.FieldOfPlayer: left = Indexer(new FieldList(arg, player)); break;
                    }
                }
                else left = Indexer(new BoardList());
            }
            else if (Match(TokenType.Find))
            {
                Token argument = Consume(TokenType.LeftParen, "Expected '(' after method");
                Token parameter = Consume(TokenType.Identifier, "Invalid predicate argument");
                Consume(TokenType.RightParen, "Expeted ')' after predicate argument");
                Consume(TokenType.Arrow, "Expected predicate function call");
                IExpression predicate = Expression();
                Consume(TokenType.RightParen, "Expected ')' after predicate");
                left = Indexer(new ListFind(left, predicate, parameter, dot, argument));
                continue;
            }
            else if (Match(TokenType.Pop))
            {
                Consume(TokenType.LeftParen, "Expected '(' after method");
                Consume(TokenType.RightParen, "Expected ')' after method");
                left = new Pop(left, dot);
            }
            else if (Match(TokenType.TriggerPlayer)) return new TriggerPlayer();
            else if (Match(TokenType.Name)) return new NameAccess(left, dot);
            else if (Match(TokenType.Power)) return new PowerAccess(left, dot);
            else if (Match(TokenType.Faction)) return new FactionAccess(left, dot);
            else if (Match(TokenType.Type)) return new TypeAccess(left, dot);
            else if (Match(TokenType.Owner)) return new OwnerAccess(left, dot);
            TokenType type = Peek().type;
            if (type == TokenType.Push || type == TokenType.Remove || type == TokenType.SendBottom || type == TokenType.Shuffle) return left;
            throw Error(Peek(), "Invalid property access");
        }
        return left;
    }

    //Parses indexer expressions (list indexing)
    public IExpression Indexer(IExpression list)
    {
        if (Match(TokenType.LeftBracket))
        {
            Token indexToken = Previous();
            IExpression index = Expression();
            Consume(TokenType.RightBracket, "Expected ']' after List Indexing");
            return new IndexedCard(index, list, indexToken);
        }
        else return list;
    }
    #endregion

    #region Statement Parsing   

    //Parses a statement
    public IStatement Statement()
    {
        try
        {
            if (Check(TokenType.Identifier))
            {
                Token identifier = Peek();
                IExpression expr = Expression();

                if (Match(TokenType.Equal))
                {
                    IExpression assignation = Expression();
                    Consume(TokenType.Semicolon, "Expected ';' after assignation");
                    return new Assignation(expr, assignation);
                }
                if (Match(new List<TokenType>() { TokenType.Increment, TokenType.Decrement }))
                {
                    Token operation = Previous();
                    Consume(TokenType.Semicolon, "Expected ';' after assignation");
                    return new Increment_Decrement(expr, operation);
                }
                if (Match(new List<TokenType>() { TokenType.MinusEqual, TokenType.PlusEqual, TokenType.StarEqual, TokenType.SlashEqual, TokenType.AtSymbolEqual }))
                {
                    Token operation = Previous();
                    IExpression assignation = Expression();
                    return new NumericModification(expr, assignation, operation);
                }
                if (Match(TokenType.Dot))
                {
                    if (expr is List)
                    {
                        if (Match(new List<TokenType>() { TokenType.Push, TokenType.SendBottom, TokenType.Remove }))
                        {
                            Token method = Previous();
                            Consume(TokenType.LeftParen, "Expeted '(' after method");
                            IExpression card = Expression();
                            Consume(TokenType.RightParen, "Expected ')' after method");
                            Consume(TokenType.Semicolon, "Expected ';' after method");
                            if (method.type == TokenType.Push) return new Push((List)expr, (ICardAtom)card);
                            if (method.type == TokenType.SendBottom) return new SendBottom((List)expr, (ICardAtom)card);
                            return new Remove((List)expr, (ICardAtom)card);
                        }
                        if (Match(TokenType.Shuffle))
                        {
                            Consume(TokenType.LeftParen, "Expected '(' after method");
                            Consume(TokenType.LeftParen, "Expected ')' after method");
                            Consume(TokenType.Semicolon, "Expected ';' after method");
                            return new Shuffle((List)expr);
                        }
                        throw Error(Peek(), "Expected list method");
                    }
                    else throw Error(Peek(), "Invalid method call");
                }
                if (expr is IStatement) return (IStatement)expr;
                else throw Error(Peek(), "Invalid statement");
            }
            if (Check(TokenType.While))
            {
                Consume(TokenType.LeftParen, "Expect '(' after 'while'");
                IExpression predicate = Expression();
                Consume(TokenType.RightParen, "Expect ')' after condition.");
                List<IStatement> body = null;
                if (Match(TokenType.LeftBrace)) body = Block();
                else body = new List<IStatement>() { Statement() };
                return new While(body, predicate);
            }
            if (Check(TokenType.For))
            {
                Token variable = Consume(TokenType.Identifier, "Expected identifier in for statement");
                Consume(TokenType.In, "Expected 'in' in for statement");
                IExpression collection = Expression();
                List<IStatement> body = null;
                if (Match(TokenType.LeftBrace)) body = Block();
                else body = new List<IStatement>() { Statement() };
                return new Foreach(body, collection, variable);
            }
            throw Error(Peek(), "Invalid statement");
        }
        catch
        {
            synchronize();
            return null;
        }
    }

    //Parses a block of statements
    public List<IStatement> Block()
    {
        try
        {
            List<IStatement> statements = new List<IStatement>();
            while (!Check(TokenType.RightBrace) && !IsAtEnd())
            {
                statements.Add(Statement());
            }
            Consume(TokenType.RightBrace, "Expected '}' after block");
            return statements;
        }
        catch
        {
            return null;
        }
    }
    #endregion

    #region Compounds Parsing

    // Parsing method for all fields that consist of the form:
    // <Field>: "literal",
    public string StringField()
    {
        try
        {
            Consume(TokenType.Colon, "Expected ':' after name declaration");
            Token name = Consume(TokenType.StringLiteral, "Expected string in name declaration");
            Consume(TokenType.Comma, "Expected ',' after name declaration");
            return (string)name.literal;
        }
        catch
        {
            synchronize();
            return null;
        }
    }

    //EffectDefinition Fields Parsing

    //Parses parameters definition for an Effect Definition
    public Dictionary<string, ExpressionType> ParametersDefinition()
    {
        try
        {
            Dictionary<string, ExpressionType> parameters = new Dictionary<string, ExpressionType>();
            Consume(TokenType.Colon, "Expected ':' after Params construction");
            Consume(TokenType.LeftBrace, "Params definition must declare a body");
            while (!Match(TokenType.RightBrace))
            {
                Token name = Consume(TokenType.Identifier, "Invalid parameter name");
                if (parameters.ContainsKey(name.lexeme)) throw Error(Previous(), $"The effect already contains {name.lexeme} parameter");
                Consume(TokenType.Colon, "Expected ':' after parameter name");
                if (Match(new List<TokenType>() { TokenType.Number, TokenType.Bool, TokenType.String }))
                    parameters[name.lexeme] = SemanticTools.GetStringType(Previous().lexeme);
                else throw Error(Peek(), "Invalid parameter type");
            }
            return parameters;
        }
        catch
        {
            synchronize();
            return null;
        }
    }

    //Parses an Action of an Effect Definition
    public Action Action()
    {
        try
        {
            Consume(TokenType.Colon, "Expected ':' after 'Action' in Action construction");
            Consume(TokenType.LeftParen, "Invalid Action construction, expected '('");
            Token targetsID = Consume(TokenType.Identifier, "Expected targets argument identifier");
            Consume(TokenType.Comma, "Expected ',' between arguments");
            Token contextID = Consume(TokenType.Identifier, "Expected context argument identifier");
            Consume(TokenType.RightParen, "Invalid Action construction, expected ')'");
            Consume(TokenType.Arrow, "Invalid Action construction, expected '=>'");
            List<IStatement> body = null;
            if (Match(TokenType.LeftBrace)) body = Block();
            else body = new List<IStatement>() { Statement() };
            return new Action(body, contextID, targetsID);
        }
        catch
        {
            synchronize();
            return null;
        }
    }

    //EffectDefinition Parsing

    //Parses an Effect Definition    
    public EffectDefinition ParseEffect()
    {
        try
        {
            Token reserved = Previous();
            Consume(TokenType.LeftBrace, "EffectDefinition must declare a body");
            EffectDefinition definition = new EffectDefinition();
            while (!Match(TokenType.LeftBrace))
            {
                if (Match(TokenType.Name))
                {
                    if (definition.name != null) throw Error(Previous(), "Name was already declared in this effect");
                    definition.name = StringField();
                    continue;
                }
                if (Match(TokenType.Params))
                {
                    if (definition.parameters != null) throw Error(Previous(), "Params was already declared in this effect");
                    definition.parameters = ParametersDefinition();
                    continue;
                }
                if (Match(TokenType.Action))
                {
                    if (definition.action != null) throw Error(Previous(), "Action was already declared in this effect");
                    definition.action = Action();
                    continue;
                }
                throw Error(Peek(), "Expected effect definition field");
            }
            if (definition.name == null || definition.action == null) throw Error(reserved, "There are missing effect arguments in the ocnstruction");
            return definition;
        }
        catch
        {
            synchronize();
            return null;
        }
    }

    // Effect fields parsing

    //Parses the parameters for an effect
    public Dictionary<string, object> Parameters()
    {
        try
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            while (!Match(TokenType.RightBrace) || !Match(TokenType.Name))
            {
                Token name = Consume(TokenType.Identifier, "Invalid parameter name");
                if (parameters.ContainsKey(name.lexeme)) throw Error(Previous(), $"The effect already contains {name.lexeme} parameter");
                Consume(TokenType.Colon, "Expected ':' after parameter name");
                if (Match(new List<TokenType>() { TokenType.NumberLiteral, TokenType.True, TokenType.False, TokenType.StringLiteral })) parameters[name.lexeme] = Previous().literal;
                else throw Error(Peek(), "Invalid parameter type");
            }
            return parameters;
        }
        catch
        {
            synchronize();
            return null;
        }
    }

    //Selector fields Parsing

    //Parses the Source field of a selector

    public Token Source()
    {
        try
        {
            Consume(TokenType.Colon, "Expected ';' after Source declaration");
            Token source = Consume(TokenType.StringLiteral, "Expected string in Source declaration");
            Consume(TokenType.Comma, "Expected ',' after Source declaration");
            return source;
        }
        catch
        {
            synchronize();
            return null;
        }
    }

    //Parses the Single field of a selector
    public bool? Single()
    {
        try
        {
            Consume(TokenType.Colon, "Expected ':' after Single declaration");
            if (!Match(new List<TokenType>() { TokenType.True, TokenType.False })) throw Error(Peek(), "Expected Bool in Single declaration");
            Token boolean = Previous();
            Consume(TokenType.Comma, "Expected ',' after Single declaration");
            return (bool)boolean.literal;
        }
        catch
        {
            synchronize();
            return null;
        }
    }

    //Parses the Predicate field of a selector
    public (IExpression, Token) Predicate()
    {
        try
        {
            Consume(TokenType.Colon, "Expected ':' after Predicate definition");
            Consume(TokenType.LeftParen, "Expected '('");
            Token argument = Consume(TokenType.Identifier, "Expected Predicate argument");
            Consume(TokenType.RightParen, "Expected ')'");
            Consume(TokenType.Arrow, "Invalid Predicate construction, expected '=>'");
            IExpression predicate = Expression();
            Consume(TokenType.RightBrace, "Expected '}'");
            return (predicate, argument);
        }
        catch
        {
            synchronize();
            return (null, null);
        }
    }

    //EffectActivation Fields Parsing

    //Parses an effect
    public Effect Effect()
    {
        try
        {
            Token reserved = Previous();
            Effect effect = new Effect();
            Consume(TokenType.Colon, "Expected ':' after Effect declaration");
            if (Match(TokenType.StringLiteral))
            {
                effect.definition = (string)Previous().literal;
                return effect;
            }
            Consume(TokenType.LeftBrace, "Effect must declare a body");
            while (!Match(TokenType.RightBrace))
            {
                if (Match(TokenType.Name))
                {
                    if (effect.definition != null) throw Error(Previous(), "Params was already declared in this effect");
                    effect.definition = StringField();
                    continue;
                }
                if (Match(TokenType.Identifier))
                {
                    if (effect.parameters != null) throw Error(Previous(), "Params was already declared in this effect");
                    effect.parameters = Parameters();
                    continue;
                }
                throw Error(Peek(), "Expected effect field");
            }
            if (effect.definition == null) throw Error(reserved, "There are missing effect arguments in the ocnstruction");
            return effect;
        }
        catch
        {
            synchronize();
            return null;
        }
    }

    //Parses a Selector
    public Selector Selector()
    {
        try
        {
            Token reserved = Previous();
            Consume(TokenType.Colon, "Expected ':' after Selector declaration");
            Consume(TokenType.LeftBrace, "Selector must declare a body");
            Selector selector = new Selector();
            selector.filtre = new ListFind();
            while (!Match(TokenType.RightBrace))
            {
                if (Match(TokenType.Source))
                {
                    if (selector.source != null) throw Error(Previous(), "Source was already declared in this Selector");
                    selector.source = Source();
                    continue;
                }
                if (Match(TokenType.Single))
                {
                    if (selector.single != null) throw Error(Previous(), "Single was already declared in this Selector");
                    selector.single = Single();
                    continue;
                }
                if (Match(TokenType.Predicate))
                {
                    if (selector.filtre.predicate != null) throw Error(Previous(), "Predicate was already declared in this Selector");
                    var aux = Predicate();
                    selector.filtre.predicate = aux.Item1;
                    selector.filtre.parameter = aux.Item2;
                }
                throw Error(Peek(), "Expected effect field");
            }
            if (selector.single == null) selector.single = false;
            if (selector.source == null || selector.filtre.parameter == null || selector.filtre.predicate == null) throw Error(reserved, "There are missing fields in Selector construction");
            return selector;
        }
        catch
        {
            synchronize();
            return null;
        }
    }


    //Parses an Effect Activation
    public EffectActivation EffectActivation()
    {
        try
        {
            Consume(TokenType.LeftBrace, "Expected '{'");
            EffectActivation activation = new EffectActivation();
            while (!Match(TokenType.RightBrace))
            {
                if (Match(TokenType.Effect))
                {
                    if (activation.effect != null) throw Error(Previous(), "Effect was already declared in this EffectActivation");
                    activation.effect = Effect();
                    continue;
                }
                if (Match(TokenType.Selector))
                {
                    if (activation.selector != null) throw Error(Previous(), "Selector was already declared in this EffectActivation");
                    activation.selector = Selector();
                    continue;
                }
                if (Match(TokenType.PostAction))
                {
                    if (activation.postAction != null) throw Error(Previous(), "PostAction was already declared in this EffectActivation");
                    activation.postAction = EffectActivation();
                    continue;
                }
            }
            if (activation.effect == null) throw Error(Previous(), "There are missing fields in EffectActivation");
            return activation;
        }
        catch
        {
            synchronize();
            return null;
        }
    }

    //Card Fields Parsing

    // Parses the onactivation field of a card
    public Onactivation Onactivation()
    {
        try
        {
            List<EffectActivation> activations = new List<EffectActivation>();
            Consume(TokenType.Colon, "Expected ':' after Onactivation declaration");
            Consume(TokenType.LeftBracket, "Expected '['");
            while (!Match(TokenType.RightBracket))
            {
                activations.Add(EffectActivation());
                if (!Check(TokenType.RightBracket)) Consume(TokenType.Comma, "Expected ',' between EffectActivations");
            }
            return new Onactivation(activations);
        }
        catch
        {
            synchronize();
            return null;
        }
    }

    // Parses the range field of a card
    public List<Card.Position> ParseRange()
    {
        try
        {
            Consume(TokenType.Colon, "Expected ':' after Range definition");
            Consume(TokenType.LeftBracket, "Expected Range list");
            List<Card.Position> positions = new List<Card.Position>();
            bool melee = false, ranged = false, siege = false;
            while (!Match(TokenType.RightBracket))
            {
                Token pos = Consume(TokenType.StringLiteral, "Expected string in Range list");
                switch (pos.literal)
                {
                    case "Melee":
                        if (melee) throw Error(Previous(), "Melee Range is already in this Range List");
                        melee = true; positions.Add(Card.Position.Melee); break;
                    case "Ranged":
                        if (ranged) throw Error(Previous(), "Ranged Range is already in this Range List");
                        ranged = true; positions.Add(Card.Position.Ranged); break;
                    case "Siege":
                        if (siege) throw Error(Previous(), "Siege Range is already in this Range List");
                        siege = true; positions.Add(Card.Position.siege); break;
                    default: throw Error(Previous(), "Invalid Range");
                }
            }
            return positions;
        }
        catch
        {
            synchronize();
            return null;
        }
    }

    //Parses a card
    public CardNode ParseCard()
    {
        try
        {
            Consume(TokenType.LeftBrace, "Card must declare a body");
            CardNode card = new CardNode();

            while (!Match(TokenType.RightBrace))
            {
                if (Match(TokenType.Name))
                {
                    if (card.name != null) throw Error(Peek(), "Name was already declared in this Card");
                    card.name = StringField();
                    continue;
                }
                if (Match(TokenType.Type))
                {
                    Token reserved = Previous();
                    if (card.type != null) throw Error(Peek(), "Type was already declared in this Card");
                    string aux = StringField();
                    switch (aux)
                    {
                        case "Oro": card.type = Card.Type.Golden; break;
                        case "Plata": card.type = Card.Type.Silver; break;
                        case "Clima": card.type = Card.Type.Weather; break;
                        case "Aumento": card.type = Card.Type.Boost; break;
                        case "Líder": card.type = Card.Type.Leader; break;
                        case "Señuelo": card.type = Card.Type.Decoy; break;
                        case "Despeje": card.type = Card.Type.Clear; break;
                        default: throw Error(reserved, "Invalid Type");
                    }
                }
                if (Match(TokenType.Faction))
                {
                    if (card.faction != null) throw Error(Peek(), "Faction was already declared in this Card");
                    card.faction = StringField();
                    continue;
                }
                if (Match(TokenType.Power))
                {
                    if (card.power != null) throw Error(Peek(), "Power was already declared in this card");
                    Consume(TokenType.Colon, "Expected ':' after Power declaration");
                    Token number = Consume(TokenType.NumberLiteral, "Expected Number in power declaration");
                    Consume(TokenType.Comma, "Expected ',' after Power declaration");
                    card.power = (int)number.literal;
                    continue;
                }
                if (Match(TokenType.Range))
                {
                    if (card.position != null) throw Error(Peek(), "Range was already declared in this Card");
                    card.position = ParseRange();
                    continue;
                }
                if (Match(TokenType.OnActivation))
                {
                    if (card.activation != null) throw Error(Peek(), "Onactivation was already declared in this card");
                    card.activation = Onactivation();
                    continue;
                }
                throw Error(Peek(), "Invalid Card field");
            }
            if (card.name == null || card.type == null || card.faction == null || card.activation == null) throw Error(Peek(), "There are missing fields in Card construction");
            return card;

        }
        catch
        {
            synchronize();
            return null;
        }
    }

    // Parses the entire program
    public ProgramNode Program()
    {
        List<IASTNode> nodes = new List<IASTNode>();
        while (!Match(TokenType.EOF))
        {
            try
            {
                if (Match(TokenType.effect))
                {
                    nodes.Add(ParseEffect());
                    continue;
                }
                if (Match(TokenType.Card))
                {
                    nodes.Add(ParseCard());
                    continue;
                }
                throw Error(Peek(), "Expected Card or Effect");
            }
            catch
            {
                synchronize();
            }
        }
        return new ProgramNode(nodes);
    }
    #endregion

}



