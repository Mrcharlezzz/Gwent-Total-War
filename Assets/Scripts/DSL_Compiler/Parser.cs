using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

/// <summary>
/// Receive a list of tokens from the lexer and turns them into AST nodes
/// </summary>
public class Parser
{
    List<Token> tokens;
    int current = 0;
    string CurrentTokenName { get { return tokens[current].lexeme; } }
    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    //All classes in this region has similar logic to their counterparts in Lexer class
    #region Tools

    /// <summary>
    /// Raises an error and reports it
    /// </summary>
    /// <param name="token">error token</param>
    /// <param name="message">error message</param>
    /// <param name="synchroTypes"> Token types that when encountered
    /// will stop the synchronization proccess</param>
    public void Error(Token token, string message, List<TokenType> synchroTypes)
    {
        Synchronize(synchroTypes);
        DSL.Error(token, message);
    }

    /// <summary>
    /// Tries to match the current token with a list of types and advances if it matches with any of the tokens in the list
    /// </summary>
    /// <param name="types">list of types</param>
    /// <returns>whether the current token matches the list or not</returns>
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

    /// <summary>
    /// Tries to match the current token with the given type and advances if it matches
    /// </summary>
    bool Match(TokenType type)
    {
        if (Check(type))
        {
            Advance();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if the current token matches the given type
    /// </summary>
    bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().type == type;
    }

    /// <summary>
    /// Checks if the current token matches an element from a list of types 
    /// </summary>
  
    bool Check(List<TokenType> types)
    {
        foreach (TokenType type in types)
        {
            if (Check(type)) return true;
        }
        return false;
    }

    /// <summary>
    /// Advances to the next token
    /// </summary>
    Token Advance()
    {
        if (!IsAtEnd()) current++;
        return Previous();
    }

    /// <summary>
    /// Check if parser has reached the end of the token list
    /// </summary>
    bool IsAtEnd()
    {
        return Peek().type == TokenType.EOF;
    }

    /// <summary>
    /// Returns the current token
    /// </summary>
    Token Peek()
    {
        return tokens[current];
    }

    /// <summary>
    /// Returns the next token
    /// </summary>
    Token PeekNext()
    {
        if (IsAtEnd()) return tokens[current];
        else return tokens[current + 1];
    }

    /// <summary>
    /// Returns the previous token
    /// </summary>
    Token Previous()
    {
        return tokens[current - 1];
    }

    /// <summary>
    /// Consumes the current token if it matches the given type, otherwise thwros an error
    /// </summary>
    Token Consume(TokenType type, string message, List<TokenType> synchroTypes)
    {
        if (Check(type)) return Advance();
        Error(Peek(), message, synchroTypes);
        return null;
    }

    /// <summary>
    /// Sinchronizes the parser state after an error based on a given list
    /// of tokens that determine where to stop synchronization
    /// </summary>
    void Synchronize(List<TokenType> synchroTypes)
    {
        if (synchroTypes == null) return;
        while (!IsAtEnd())
        {
            if (Check(synchroTypes)) return;
            Advance();
            if (synchroTypes == Block.synchroTypes && Previous().type == TokenType.Semicolon) return;
            if (synchroTypes == RangeAccess.synchrotypes && Previous().type == TokenType.Comma) return;
        }
    }

    #endregion




    #region Expression Parsing

    //Parses an expression
    public IExpression Expression()
    {
        return Logic();
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
            if (operation.type == TokenType.And) expr = new And(expr, right, operation);
            else expr = new Or(expr, right, operation);
        }
        return expr;
    }

    //Parses equality expressions
    public IExpression Equality()
    {
        IExpression expr = Stringy();
        List<TokenType> types = new List<TokenType>() { TokenType.BangEqual, TokenType.EqualEqual };
        while (Match(types))
        {
            Token operation = Previous();
            IExpression right = Stringy();
            if (operation.type == TokenType.BangEqual) expr = new Differ(expr, right, operation);
            else expr = new Equal(expr, right, operation);
        }
        return expr;
    }

    //Parses string expressions
    public IExpression Stringy()
    {
        IExpression expr = Comparison();
        List<TokenType> types = new List<TokenType>() { TokenType.AtSymbol, TokenType.AtSymbolAtSymbol };
        while (Match(types))
        {
            Token operation = Previous();
            IExpression right = Comparison();
            if (operation.type == TokenType.AtSymbol) expr = new Join(expr, right, operation);
            else expr = new SpaceJoin(expr, right, operation);
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
            switch (operation.type)
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
            if (operation.type == TokenType.Plus) expr = new Plus(expr, right, operation);
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
            if (operation.type == TokenType.Slash) expr = new Division(expr, right, operation);
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
            Token operation = Previous();
            IExpression right = Primary();
            if (operation.type == TokenType.Minus) return new Negative(right, Previous());
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
            Token check = Consume(TokenType.RightParen, "Expect ')' after expression", Atom.synchrotypes);
            if (check == null) return null;
            return expr;
        }
        if (Match(TokenType.Identifier))
        {
            //Indexing managing
            IExpression left = Indexer(new Variable(Previous()));

            //Indexer and Access methods will return their arguments if 
            //they dont match a left bracket or dot respectively

            if (Check(TokenType.Dot)) left = Access(left);
            types = new List<TokenType>() { TokenType.Increment, TokenType.Decrement };
            if (Match(types)) left = new Increment_Decrement(left, Previous());
            return left;
        }

        if (Check(TokenType.Dot)) Error(Peek(), "Invalid property access", Atom.synchrotypes);
        else Error(Peek(), "Expect expression", Atom.synchrotypes);
        return null;
    }

    //Parses access expressions (property and pop method access)
    public IExpression Access(IExpression left)
    {
        Token check;
        //Store dot token in order to report posible semantic errors later in the Access

        while (Match(TokenType.Dot))
        {
            Token dot = Previous();
            List<TokenType> types = new List<TokenType>(){
                TokenType.HandOfPlayer, TokenType.DeckOfPlayer,
                TokenType.GraveyardOfPlayer, TokenType.FieldOfPlayer,
                TokenType.Hand, TokenType.Deck,
                TokenType.Graveyard, TokenType.Field,
            };
            if (Check(types))
            {
                types = new List<TokenType>(){
                    TokenType.HandOfPlayer, TokenType.DeckOfPlayer,
                    TokenType.GraveyardOfPlayer, TokenType.FieldOfPlayer,
                };
                //store list token
                Token aux = Peek();
                if (Match(types))
                {

                    Token player = Consume(TokenType.LeftParen, "Expected Player Argument", Atom.synchrotypes);
                    if (player == null) return null;

                    //store player argument
                    IExpression arg = Expression();
                    check = Consume(TokenType.RightParen, "Expected ')' after Player Argument", Atom.synchrotypes);
                    if (check == null) return null;

                    switch (aux.type)
                    {
                        case TokenType.HandOfPlayer: left = Indexer(new HandList(left,arg, dot, player)); break;
                        case TokenType.DeckOfPlayer: left = Indexer(new DeckList(left,arg, dot, player)); break;
                        case TokenType.GraveyardOfPlayer: left = Indexer(new GraveyardList(left, arg, dot, player)); break;
                        case TokenType.FieldOfPlayer: left = Indexer(new FieldList(left, arg, dot, player)); break;
                    }
                }
                else
                {
                    Advance();
                    //Syntax sugar for player lists
                    switch (aux.type)
                    {
                        case TokenType.Hand: left = Indexer(new HandList(left, new TriggerPlayer(), dot, aux)); break;
                        case TokenType.Deck: left = Indexer(new DeckList(left, new TriggerPlayer(), dot, aux)); break;
                        case TokenType.Field: left = Indexer(new FieldList(left, new TriggerPlayer(), dot, aux)); break;
                        case TokenType.Graveyard: left = Indexer(new GraveyardList(left, new TriggerPlayer(), dot, aux)); break;
                    }
                }
            }
            else if (Match(TokenType.Board)) left = Indexer(new BoardList(left, dot));

            else if (Match(TokenType.Find))
            {
                //Consume all expected tokens in a Find method
                Token argument = Consume(TokenType.LeftParen, "Expected '(' after method", Atom.synchrotypes);
                if (argument == null) return null;

                check = Consume(TokenType.LeftParen, "Expected '('", Atom.synchrotypes);
                if (check == null) return null;

                Token parameter = Consume(TokenType.Identifier, "Invalid predicate argument", Atom.synchrotypes);
                if (parameter == null) return null;

                check = Consume(TokenType.RightParen, "Expeted ')' after predicate argument", Atom.synchrotypes);
                if (check == null) return null;

                check = Consume(TokenType.Arrow, "Expected predicate function call", Atom.synchrotypes);
                if (check == null) return null;

                //store predicate 
                IExpression predicate = Expression();
                check = Consume(TokenType.RightParen, "Expected ')' after predicate", Atom.synchrotypes);
                if (check == null) return null;

                left = Indexer(new ListFind(left, predicate, parameter, dot, argument));
            }
            else if (Match(TokenType.Pop))
            {
                check = Consume(TokenType.LeftParen, "Expected '(' after method", Atom.synchrotypes);
                if (check == null) return null;

                check = Consume(TokenType.RightParen, "Expected ')' after method", Atom.synchrotypes);
                if (check == null) return null;

                left = new Pop(left, dot);
            }
            else if (Match(TokenType.TriggerPlayer)) left = Indexer(new TriggerPlayer());
            //Card property access
            else if (Match(TokenType.Name)) left = Indexer(new NameAccess(left, dot));
            else if (Match(TokenType.Power)) left = Indexer(new PowerAccess(left, dot));
            else if (Match(TokenType.Faction)) left = Indexer(new FactionAccess(left, dot));
            else if (Match(TokenType.Type)) left = Indexer(new TypeAccess(left, dot));
            else if (Match(TokenType.Owner)) left = Indexer(new OwnerAccess(left, dot));
            else if (Match(TokenType.Range)) left = Indexer(new RangeAccess(left, dot));
            if (Check(TokenType.Dot)) continue;
            types = new List<TokenType>() { TokenType.Push, TokenType.Remove, TokenType.SendBottom, TokenType.Shuffle };
            if (Previous().type == TokenType.Dot)
            {
                if (Check(types))
                {
                    current--;
                    return left;
                }
                else
                {
                    Error(Peek(), "Invalid property access", Atom.synchrotypes);
                    return null;
                }
            }
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
            Token check = Consume(TokenType.RightBracket, "Expected ']' after List Indexing", Atom.synchrotypes);
            if (check == null) return null;
            if (list is RangeAccess)
            {
                return new IndexedRange(list, index, indexToken);
            }
            else return new IndexedCard(index, list, indexToken);
        }
        else return list;
    }
    #endregion

    #region Statement Parsing   

    //Parses a statement
    public IStatement Statement()
    {
        

        Token statementHead=Peek();
        
        //This token is for checking if Consume method had a non-null return, in order
        //to check if an error was found to reset the parser to a good state
        Token check;
        //Handle assignations, increments, decrements, numeric modifications and methods
        if (Check(TokenType.Identifier))
        {
            Token identifier = Peek();
            IExpression expr = Expression();

            if (Match(TokenType.Equal))
            {
                Token equal = Previous();
                IExpression assignation = Expression();
                check = Consume(TokenType.Semicolon, "Expected ';' after assignation", Block.synchroTypes);
                if (check == null) return null;

                return new Assignation(expr, assignation, equal);
            }
            if (Match(new List<TokenType>() { TokenType.Increment, TokenType.Decrement }))
            {
                Token operation = Previous();
                check = Consume(TokenType.Semicolon, "Expected ';' after assignation", Block.synchroTypes);
                if (check == null) return null;

                return new Increment_Decrement(expr, operation);
            }
            if (Match(new List<TokenType>() { TokenType.MinusEqual, TokenType.PlusEqual, TokenType.StarEqual, TokenType.SlashEqual, TokenType.AtSymbolEqual }))
            {
                Token operation = Previous();
                IExpression assignation = Expression();
                check = Consume(TokenType.Semicolon, "Expected ';' after assignation", Block.synchroTypes);
                if (check == null) return null;
                return new NumericModification(expr, assignation, operation);
            }
            if (Match(TokenType.Dot))
            {
                Token dot = Previous();

                if (Match(new List<TokenType>() { TokenType.Push, TokenType.SendBottom, TokenType.Remove }))
                {
                    Token method = Previous();
                    check = Consume(TokenType.LeftParen, "Expeted '(' after method", Block.synchroTypes);
                    if (check == null) return null;

                    IExpression card = Expression();
                    check = Consume(TokenType.RightParen, "Expected ')' after method", Block.synchroTypes);
                    if (check == null) return null;

                    check = Consume(TokenType.Semicolon, "Expected ';' after method", Block.synchroTypes);
                    if (check == null) return null;

                    if (method.type == TokenType.Push) return new Push(expr, card, dot);
                    if (method.type == TokenType.SendBottom) return new SendBottom(expr, card, dot);
                    return new Remove(expr, card, dot);
                }
                if (Match(TokenType.Shuffle))
                {
                    check = Consume(TokenType.LeftParen, "Expected '(' after method", Block.synchroTypes);
                    if (check == null) return null;

                    check = Consume(TokenType.RightParen, "Expected ')' after method", Block.synchroTypes);
                    if (check == null) return null;

                    check = Consume(TokenType.Semicolon, "Expected ';' after method", Block.synchroTypes);
                    if (check == null) return null;

                    return new Shuffle(expr, dot);
                }


                else Error(Peek(), "Invalid method call", Block.synchroTypes);
                return null;
            }
            if (expr is IStatement){
                check=Consume(TokenType.Semicolon, "Expected ';' after statement", Block.synchroTypes);
                if(check==null) return null;
                return (IStatement)expr;
            } 
            else
            {
                Error(Peek(), "Invalid statement", Block.synchroTypes);
                return null;
            }
        }
        //Parses while loop
        if (Match(TokenType.While))
        {
            check = Consume(TokenType.LeftParen, "Expect '(' after 'while'", Block.synchroTypes);
            IExpression predicate = Expression();
            check = Consume(TokenType.RightParen, "Expect ')' after condition.", Block.synchroTypes);
            List<IStatement> body = null;
            if (Match(TokenType.LeftBrace))
            {
                body = ParseBlock(Block.synchroTypes);
            }
            //One-liner handling
            else body = new List<IStatement>() { Statement() };
            return new While(body, predicate, statementHead);
        }
        //Parses for loop
        if (Match(TokenType.For))
        {
            check = Consume(TokenType.LeftParen, "Expected '('", Block.synchroTypes);
            if (check == null) return null;
            Token variable = Consume(TokenType.Identifier, "Expected identifier in for statement", Block.synchroTypes);
            if (variable == null) return null;
            check = Consume(TokenType.In, "Expected 'in' in for statement", Block.synchroTypes);
            if (check == null) return null;
            IExpression collection = Expression();
            check = Consume(TokenType.RightParen, "Expected ')'", Block.synchroTypes);
            if (check == null) return null;
            List<IStatement> body = null;
            if (Match(TokenType.LeftBrace))
            {
                body = ParseBlock(Block.synchroTypes);
            }
            //One-liner handling
            else body = new List<IStatement>() { Statement() };
            return new Foreach(body, collection, variable, statementHead);
        }
        Error(Peek(), "Invalid statement", Block.synchroTypes);
        return null;
    }

    /// <summary>
    /// Parses a block of statements
    /// </summary>
    public List<IStatement> ParseBlock(List<TokenType> synchroTypes)
    {
        List<IStatement> statements = new List<IStatement>();
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            statements.Add(Statement());
        }
        Token check = Consume(TokenType.RightBrace, "Expected '}' after block", synchroTypes);
        if (check == null) return null;
        return statements;
    }
    #endregion

    #region Compounds Parsing

    // Parsing method for all fields that consist of the form:
    // <Field>: "literal",
    public string StringField(List<TokenType> synchroTypes)
    {
        Token check = Consume(TokenType.Colon, "Expected ':' after name declaration", synchroTypes);
        if (check == null) return null;

        Token name = Consume(TokenType.StringLiteral, "Expected string in name declaration", synchroTypes);
        if (check == null) return null;

        check = Consume(TokenType.Comma, "Expected ',' after name declaration", synchroTypes);
        if (check == null) return null;

        return (string)name.literal;
    }

    //EffectDefinition Fields Parsing

    //Parses parameters definition for an Effect Definition
    ParameterDef ParametersDefinition()
    {
        Dictionary<string, ExpressionType> parameters = new Dictionary<string, ExpressionType>();
        Token check = Consume(TokenType.Colon, "Expected ':' after Params construction", EffectDefinition.synchroTypes);
        if (check == null) return null;
        check = Consume(TokenType.LeftBrace, "Params definition must declare a body", EffectDefinition.synchroTypes);
        if (check == null) return null;
        while (!Match(TokenType.RightBrace))
        {
            Token name = Consume(TokenType.Identifier, "Invalid parameter name", ParameterDef.synchroTypes);
            if (name == null) continue;
            if (parameters.ContainsKey(name.lexeme))
            {
                Error(Previous(), $"The effect already contains {name.lexeme} parameter", ParameterDef.synchroTypes);
                continue;
            }
            check = Consume(TokenType.Colon, "Expected ':' after parameter name", ParameterDef.synchroTypes);
            if (check == null) continue;
            if (Match(new List<TokenType>() { TokenType.Number, TokenType.Bool, TokenType.String }))
                parameters[name.lexeme] = Tools.GetStringType(Previous().lexeme);
            else Error(Peek(), "Invalid parameter type", ParameterDef.synchroTypes);
        }
        return new ParameterDef(parameters);
    }

    //Parses an Action of an Effect Definition
    public Action Action()
    {
        Token action= Previous();
        Token check = Consume(TokenType.Colon, "Expected ':' after 'Action' in Action construction", EffectDefinition.synchroTypes);
        if (check == null) return null;

        check = Consume(TokenType.LeftParen, "Invalid Action construction, expected '('", EffectDefinition.synchroTypes);
        if (check == null) return null;

        Token targetsID = Consume(TokenType.Identifier, "Expected targets argument identifier", EffectDefinition.synchroTypes);
        if (check == null) return null;

        check = Consume(TokenType.Comma, "Expected ',' between arguments", EffectDefinition.synchroTypes);
        if (check == null) return null;

        Token contextID = Consume(TokenType.Identifier, "Expected context argument identifier", EffectDefinition.synchroTypes);
        if (check == null) return null;

        check = Consume(TokenType.RightParen, "Invalid Action construction, expected ')'", EffectDefinition.synchroTypes);
        if (check == null) return null;

        check = Consume(TokenType.Arrow, "Invalid Action construction, expected '=>'", EffectDefinition.synchroTypes);
        if (check == null) return null;

        List<IStatement> body = null;
        if (Match(TokenType.LeftBrace)) body = ParseBlock(EffectDefinition.synchroTypes);
        else body = new List<IStatement>() { Statement() };
        return new Action(body, contextID, targetsID, action);
    }

    //EffectDefinition Parsing

    //Parses an Effect Definition    
    public EffectDefinition ParseEffect()
    {
        Token keyword = Previous();
        Token check = Consume(TokenType.LeftBrace, "EffectDefinition must declare a body", ProgramNode.synchroTypes);
        if (check == null) return null;
        EffectDefinition definition = new EffectDefinition();
        definition.keyword= keyword;
        while (!Match(TokenType.RightBrace))
        {
            if (Match(TokenType.Name))
            {
                if (definition.name != null)
                {
                    Error(Previous(), "Name was already declared in this effect", EffectDefinition.synchroTypes);
                    continue;
                }
                definition.name = StringField(EffectDefinition.synchroTypes);
                continue;
            }
            if (Match(TokenType.Params))
            {
                if (definition.parameterdefs != null)
                {
                    Error(Previous(), "Params was already declared in this effect", EffectDefinition.synchroTypes);
                    continue;
                }
                definition.parameterdefs = ParametersDefinition();
                continue;
            }
            if (Match(TokenType.Action))
            {
                if (definition.action != null)
                {
                    Error(Previous(), "Action was already declared in this effect", EffectDefinition.synchroTypes);
                    continue;
                }
                definition.action = Action();
                continue;
            }
            Error(Peek(), "Expected effect definition field", EffectDefinition.synchroTypes);
        }
        if (definition.name == null || definition.action == null)
        {
            Error(keyword, "There are missing effect arguments in the ocnstruction", ProgramNode.synchroTypes);
            return null;
        }
        return definition;
    }

    // Effect fields parsing

    //Parses the parameters for an effect
    public Parameters ParseParameter()
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>();
        while (!Check(TokenType.RightBrace) && !Check(TokenType.Name))
        {
            Token name = Consume(TokenType.Identifier, "Invalid parameter name", Parameters.synchroTypes);
            if (name == null) continue;
            if (parameters.ContainsKey(name.lexeme))
            {
                Error(Previous(), $"The effect already contains {name.lexeme} parameter", Parameters.synchroTypes);
                continue;
            }
            Token check = Consume(TokenType.Colon, "Expected ':' after parameter name", Parameters.synchroTypes);
            if (check == null) continue;
            if (Match(new List<TokenType>() { TokenType.NumberLiteral, TokenType.True, TokenType.False, TokenType.StringLiteral }))
                parameters[name.lexeme] = Previous().literal;
            else
            {
                Error(Peek(), "Invalid parameter type", Parameters.synchroTypes);
                continue;
            }
            check = Consume(TokenType.Comma, "Expected ',' after parameter", Parameters.synchroTypes);
            if (check == null) continue;
        }
        return new Parameters(parameters);
    }

    //Selector fields Parsing

    //Parses the Source field of a selector

    public Token Source()
    {
        Token check = Consume(TokenType.Colon, "Expected ':' after Source declaration", Selector.synchroTypes);
        if (check == null) return null;

        Token source = Consume(TokenType.StringLiteral, "Expected string in Source declaration", Selector.synchroTypes);
        if (source == null) return null;

        check = Consume(TokenType.Comma, "Expected ',' after Source declaration", Selector.synchroTypes);
        if (check == null) return null;

        return source;
    }

    //Parses the Single field of a selector
    public bool? Single()
    {
        Token check = Consume(TokenType.Colon, "Expected ':' after Single declaration", Selector.synchroTypes);
        if (!Match(new List<TokenType>() { TokenType.True, TokenType.False }))
        {
            Error(Peek(), "Expected Bool in Single declaration", Selector.synchroTypes);
            return null;
        }
        Token boolean = Previous();
        check = Consume(TokenType.Comma, "Expected ',' after Single declaration", Selector.synchroTypes);
        return (bool)boolean.literal;
    }

    //Parses the Predicate field of a selector
    public (IExpression, Token) ParsePredicate()
    {
        Token check = Consume(TokenType.Colon, "Expected ':' after Predicate definition", Selector.synchroTypes);
        if (check == null) return (null, null);

        check = Consume(TokenType.LeftParen, "Expected '('", Selector.synchroTypes);
        if (check == null) return (null, null);
        Token argument = Consume(TokenType.Identifier, "Expected Predicate argument", Selector.synchroTypes);
        if (argument == null) return (null, null);

        check = Consume(TokenType.RightParen, "Expected ')'", Selector.synchroTypes);
        if (check == null) return (null, null);

        check = Consume(TokenType.Arrow, "Invalid Predicate construction, expected '=>'", Selector.synchroTypes);
        if (check == null) return (null, null);

        IExpression predicate = Expression();
        return (predicate, argument);
    }

    //EffectActivation Fields Parsing

    //Parses an effect
    public Effect ParseActivationEffect()
    {
        Token keyword = Previous();
        Effect effect = new Effect();
        effect.keyword=keyword;
        Token check = Consume(TokenType.Colon, "Expected ':' after Effect declaration", EffectActivation.synchroTypes);
        if (check == null) return null;
        if (Match(TokenType.StringLiteral))
        {
            effect.definition = (string)Previous().literal;
            check = Consume(TokenType.Comma, "Expected ',' after Name declaration", EffectActivation.synchroTypes);
            if (check == null) return null;
            return effect;
        }
        check = Consume(TokenType.LeftBrace, "Effect must declare a body", EffectActivation.synchroTypes);
        if (check == null) return null;
        while (!Match(TokenType.RightBrace))
        {
            if (Match(TokenType.Name))
            {
                if (effect.definition != null)
                {
                    Error(Previous(), "Params was already declared in this effect", Effect.synchroTypes);
                    continue;
                }
                effect.definition = StringField(Effect.synchroTypes);
                continue;
            }
            if (Check(TokenType.Identifier))
            {
                if (effect.parameters != null)
                {
                    Error(Previous(), "Params was already declared in this effect", Effect.synchroTypes);
                    continue;
                }
                effect.parameters = ParseParameter();
                continue;
            }
            Error(Peek(), "Expected effect field", Effect.synchroTypes);
        }
        if (effect.definition == null)
        {
            Error(keyword, "There are missing effect arguments in the ocnstruction", EffectActivation.synchroTypes);
            return null;
        }
        return effect;
    }

    //Parses a Selector
    public Selector ParseSelector()
    {
        Token keyword = Previous();
        Token check = Consume(TokenType.Colon, "Expected ':' after Selector declaration", EffectActivation.synchroTypes);
        if (check == null) return null;
        check = Consume(TokenType.LeftBrace, "Selector must declare a body", EffectActivation.synchroTypes);
        if (check == null) return null;
        Selector selector = new Selector();
        selector.filtre = new ListFind();
        while (!Match(TokenType.RightBrace))
        {
            if (Match(TokenType.Source))
            {
                if (selector.source != null)
                {
                    Error(Previous(), "Source was already declared in this Selector", Selector.synchroTypes);
                    continue;
                }
                selector.source = Source();
                continue;
            }
            if (Match(TokenType.Single))
            {
                if (selector.single != null)
                {
                    Error(Previous(), "Single was already declared in this Selector", Selector.synchroTypes);
                    continue;
                }
                selector.single = Single();
                continue;
            }
            if (Match(TokenType.Predicate))
            {
                if (selector.filtre.predicate != null)
                {
                    Error(Previous(), "Predicate was already declared in this Selector", Selector.synchroTypes);
                    continue;
                }
                var aux = ParsePredicate();
                selector.filtre.predicate = aux.Item1;
                selector.filtre.parameter = aux.Item2;
                continue;
            }
            Error(Peek(), "Expected effect field", Selector.synchroTypes);
        }
        if (selector.single == null) selector.single = false;
        if (selector.source == null || selector.filtre.parameter == null || selector.filtre.predicate == null)
        {
            Error(keyword, "There are missing fields in Selector construction", EffectActivation.synchroTypes);
            return null;
        }
        return selector;
    }


    //Parses an Effect Activation
    public EffectActivation ParseEffectActivation()
    {
        Token check = Consume(TokenType.LeftBrace, "Expected '{'", Onactivation.synchroTypes);
        if (check == null) return null;
        EffectActivation activation = new EffectActivation();
        while (!Match(TokenType.RightBrace))
        {
            if (Match(TokenType.Effect))
            {
                if (activation.effect != null) Error(Previous(), "Effect was already declared in this EffectActivation", EffectActivation.synchroTypes);
                activation.effect = ParseActivationEffect();
                continue;
            }
            if (Match(TokenType.Selector))
            {
                if (activation.selector != null) Error(Previous(), "Selector was already declared in this EffectActivation", EffectActivation.synchroTypes);
                activation.selector = ParseSelector();
                continue;
            }
            if (Match(TokenType.PostAction))
            {
                if (activation.postAction != null) Error(Previous(), "PostAction was already declared in this EffectActivation", EffectActivation.synchroTypes);
                check = Consume(TokenType.Colon, "Expected ':' after PostAction declaration", EffectActivation.synchroTypes);
                if (check == null) continue;
                activation.postAction = ParseEffectActivation();
                continue;
            }
            Error(Peek(), "Expected EffectActivation field", EffectActivation.synchroTypes);
        }
        if (activation.effect == null)
        {
            Error(Previous(), "There are missing fields in EffectActivation", Onactivation.synchroTypes);
            return null;
        }
        return activation;
    }

    //Card Fields Parsing

    // Parses the onactivation field of a card
    public Onactivation ParseOnactivation()
    {
        List<EffectActivation> activations = new List<EffectActivation>();
        Token check = Consume(TokenType.Colon, "Expected ':' after Onactivation declaration", CardNode.synchroTypes);
        if (check == null) return null;
        check = Consume(TokenType.LeftBracket, "Expected '['", CardNode.synchroTypes);
        if (check == null) return null;
        while (!Match(TokenType.RightBracket))
        {
            activations.Add(ParseEffectActivation());
            if (!Check(TokenType.RightBracket))
            {
                Consume(TokenType.Comma, "Expected ',' between EffectActivations", Onactivation.synchroTypes);
            }
        }
        return new Onactivation(activations);
    }

    // Parses the range field of a card
    public List<string> ParseRange()
    {

        Token check = Consume(TokenType.Colon, "Expected ':' after Range definition", CardNode.synchroTypes);
        if (check == null) return null;
        check = Consume(TokenType.LeftBracket, "Expected Range list", CardNode.synchroTypes);
        if (check == null) return null;

        List<string> positions = new List<string>();
        bool melee = false, ranged = false, siege = false;
        while (!Match(TokenType.RightBracket))
        {
            Token pos = Consume(TokenType.StringLiteral, "Expected string in Range list", RangeAccess.synchroTypes);
            if (pos == null) continue;
            switch (pos.literal)
            {
                case "Melee":
                    if (melee)
                    {
                        Error(Previous(), "Melee Range is already in this Range List", RangeAccess.synchroTypes);
                        break;
                    }
                    melee = true; positions.Add((string)pos.literal); break;
                case "Ranged":
                    if (ranged)
                    {
                        Error(Previous(), "Ranged Range is already in this Range List", RangeAccess.synchroTypes);
                        break;
                    }
                    ranged = true; positions.Add((string)pos.literal); break;
                case "Siege":
                    if (siege)
                    {
                        Error(Previous(), "Siege Range is already in this Range List", RangeAccess.synchroTypes);
                        break;
                    }
                    siege = true; positions.Add((string)pos.literal); break;
                default: Error(Previous(), "Invalid Range", RangeAccess.synchroTypes); break;
            }
            if (!Check(TokenType.RightBracket))
            {
                check = Consume(TokenType.Comma, "Expected ',' between ranges", RangeAccess.synchroTypes);
                if (check == null) continue;
            }
        }
        check = Consume(TokenType.Comma, "Expected ',' after Range declaration", CardNode.synchroTypes);
        if (check == null) return null;
        return positions;
    }

    //Parses a card
    public CardNode ParseCard()
    {
        Token keyword=Previous();
        Token check = Consume(TokenType.LeftBrace, "Card must declare a body", ProgramNode.synchroTypes);
        if (check is null) return null;
        CardNode card = new CardNode();
        card.keyword=keyword;

        while (!Match(TokenType.RightBrace))
        {
            if (Match(TokenType.Name))
            {
                if (card.name != null)
                {
                    Error(Peek(), "Name was already declared in this Card", CardNode.synchroTypes);
                    continue;
                }
                card.name = StringField(CardNode.synchroTypes);
                continue;
            }
            if (Match(TokenType.Type))
            {
                if (card.type != null)
                {
                    Error(Peek(), "Type was already declared in this Card", CardNode.synchroTypes);
                    continue;
                }
                string aux = StringField(CardNode.synchroTypes);
                card.type = Tools.GetCardType(aux);
                if (card.type == null) Error(keyword, "Invalid Type", CardNode.synchroTypes);
                continue;
            }
            if (Match(TokenType.Faction))
            {
                if (card.faction != null)
                {
                    Error(Peek(), "Faction was already declared in this Card", CardNode.synchroTypes);
                    continue;
                }
                card.faction = StringField(CardNode.synchroTypes);
                continue;
            }
            if (Match(TokenType.Power))
            {
                if (card.power != null)
                {
                    Error(Peek(), "Power was already declared in this card", CardNode.synchroTypes);
                    continue;
                }
                check = Consume(TokenType.Colon, "Expected ':' after Power declaration", CardNode.synchroTypes);
                if (check is null) continue;
                Token number = Consume(TokenType.NumberLiteral, "Expected Number in power declaration", CardNode.synchroTypes);
                check = Consume(TokenType.Comma, "Expected ',' after Power declaration", CardNode.synchroTypes);
                if (check is null) continue;
                card.power = (int)number.literal;
                continue;
            }
            if (Match(TokenType.Range))
            {
                if (card.position != null)
                {
                    Error(Peek(), "Range was already declared in this Card", CardNode.synchroTypes);
                    continue;
                }
                card.position = ParseRange();
                continue;
            }
            if (Match(TokenType.OnActivation))
            {
                if (card.activation != null)
                {
                    Error(Peek(), "Onactivation was already declared in this card", CardNode.synchroTypes);
                    continue;
                }
                card.activation = ParseOnactivation();
                continue;
            }
            Error(Peek(), "Invalid Card field", CardNode.synchroTypes);
        }
        if (card.name == null || card.type == null || card.faction == null || card.activation == null|| card.position==null)
        {
            Error(Peek(), "There are missing fields in Card construction", ProgramNode.synchroTypes);
            return null;
        }
        return card;
    }

    // Parses the entire program
    public ProgramNode Program()
    {
        List<IASTNode> nodes = new List<IASTNode>();
        ProgramNode program = new ProgramNode(nodes);
        while (!IsAtEnd())
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
            Error(Peek(), "Expected Card or Effect", ProgramNode.synchroTypes);
        }
        return program;
    }
    #endregion

}