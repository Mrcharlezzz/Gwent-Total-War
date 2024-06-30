using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor.PackageManager;

public class Parser
{
    List<Token> tokens;
    int current=0;
    public Parser(List<Token> tokens){
        this.tokens = tokens;
    }

    //All classes in this region has similar logic to their counterparts in Lexer class
    #region Tools
    public static Exception Error(Token token, string message){
        DSL.Error(token, message);
        return new Exception();
    }
    
    bool Match(List<TokenType> types){
        foreach (TokenType type in types){
            if(Check(type)){
                Advance();
                return true;
            }
        }
        return false;
    }
    bool Match(TokenType type){
        if(Check(type)){
            Advance();
            return true;
        }
        return false;
    }
    bool Check(TokenType type){
        if(IsAtEnd()) return false;
        return Peek().type==type;
    }
    Token Advance(){
        if(!IsAtEnd()) current++;
        return Previous();
    }
    bool IsAtEnd(){
        return Peek().type==TokenType.EOF;
    }
    Token Peek(){
        return tokens[current];
    }
    Token PeekNext(){
        if(IsAtEnd())return tokens[current];
        else return tokens[current+1];
    }
    Token Previous(){
        return tokens[current-1];
    }
    Token Consume(TokenType type, string message){
        if(Check(type)) return Advance();
        throw Error(Peek(),message);
    }
    
    void synchronize() {
        Advance();
        while (!IsAtEnd()) {
            if (Previous().type == TokenType.Semicolon) return;
            switch (Peek().type) {
            case TokenType.effect:
            case TokenType.Card:
            case TokenType.For:
            case TokenType.While:
            //add more types here
            return;
        }
        Advance();
    }
}

    #endregion

    #region Expression Parsing Hierarchy
    public IExpression Expression(){
        return Logic();
    }
    public IExpression Logic(){
        IExpression expr=Equality();
        List<TokenType> types=new List<TokenType>() {TokenType.And,TokenType.Or};
        while(Match(types)){
            Token operation = Previous();
            IExpression right=Equality();
            if(Previous().type==TokenType.And) expr = new And(expr,right,operation);
            else expr = new Or(expr,right,operation);
        }
        return expr;
    }
    public IExpression Equality(){
        IExpression expr=Comparison();
        List<TokenType> types=new List<TokenType>(){TokenType.BangEqual,TokenType.EqualEqual};
        while(Match(types)){
            Token operation = Previous();
            IExpression right=Comparison();
            if(Previous().type==TokenType.BangEqual) expr = new Differ(expr,right,operation);
            else expr = new Equals(expr,right,operation);
        }
        return expr; 
    }
    public IExpression Comparison(){
        IExpression expr=Term();
        List<TokenType> types=new List<TokenType>(){
            TokenType.Greater,TokenType.GreaterEqual,
            TokenType.Less,TokenType.LessEqual,    
        };
        while(Match(types)){
            Token operation = Previous();
            IExpression right=Term();
            switch (Previous().type){
                case TokenType.Greater: expr= new Great(expr,right,operation); break;
                case TokenType.GreaterEqual: expr= new AtMost(expr,right,operation); break;
                case TokenType.Less: expr= new Less(expr,right,operation); break;
                case TokenType.LessEqual: expr= new AtLeast(expr,right,operation); break;
            }
        }
        return expr; 
    }
    public IExpression Term(){
       IExpression expr=Factor();
        List<TokenType> types=new List<TokenType>(){TokenType.Minus,TokenType.Plus};
        while(Match(types)){
            Token operation = Previous();
            IExpression right=Factor();
            if(Previous().type==TokenType.Plus) expr = new Plus(expr,right,operation);
            else expr = new Minus(expr,right,operation);
        }
        return expr; 
    }
    public IExpression Factor(){
        IExpression expr=Power();
        while(Match(TokenType.Pow)){
            Token operation = Previous();
            IExpression right=Power();
            expr = new Power(expr,right,operation);
        }
        return expr; 
    }
    public IExpression Power(){
        IExpression expr=Unary();
        List<TokenType> types=new List<TokenType>(){TokenType.BangEqual,TokenType.EqualEqual};
        while(Match(types)){
            Token operation = Previous();
            IExpression right=Unary();
            if(Previous().type==TokenType.BangEqual) expr = new Differ(expr,right,operation);
            else expr = new Equals(expr,right,operation);
        }
        return expr; 
    }
    public IExpression Unary(){
        List<TokenType> types=new List<TokenType>(){TokenType.Minus,TokenType.Bang};
        if(Match(types)){
            Token operations=Previous();
            IExpression right=Primary();
            if(Previous().type==TokenType.Minus) return new Negative(right);
            else return new Negation(right);
        }
        return Primary(); 
    }
    
    public IExpression Primary(){
        if(Match(TokenType.False)) return new Literal(false);
        if(Match(TokenType.True)) return new Literal(true);
        List<TokenType> types=new List<TokenType>(){ TokenType.NumberLiteral, TokenType.StringLiteral};
        if(Match(types)) return new Literal(Previous().literal);
        if(Match(TokenType.LeftParen)) {
            IExpression expr= Expression();
            Consume(TokenType.RightParen,"Expect ')' after expression");
            return expr;
        }
        if(Match(TokenType.Identifier)){
            IExpression left=new Variable (Previous());
            if(Peek().type==TokenType.Dot) return Access();
            return left;
        }

        if(Check(TokenType.Dot)) throw Error(Peek(), "Invalid property access");
        throw Error(Peek(), "Expect expression");
    }

    public IExpression Access(){
        IExpression left= new Variable(Peek());
        Advance();
        if(Check(TokenType.LeftBracket)) left=Indexer(left);
        while(Match(TokenType.Dot)){
            List<TokenType>types=new List<TokenType>(){ 
                TokenType.HandOfPlayer,TokenType.DeckOfPlayer,TokenType.GraveyardOfPlayer,
                TokenType.FieldOfPlayer,TokenType.Board,
            };
            if(Match(types)){
                if(Previous().type!=TokenType.Board)
                {
                    TokenType aux=Previous().type;
                    Consume(TokenType.LeftParen,"Expected TriggerPlayer Argument");
                    IExpression arg= Expression();
                    Consume(TokenType.RightParen,"Expected ')' after TriggerPlayer Argument");
                    switch(aux){
                        case TokenType.HandOfPlayer: left=Indexer(new HandList(arg)); break;
                        case TokenType.DeckOfPlayer: left=Indexer(new DeckList(arg)); break;
                        case TokenType.GraveyardOfPlayer: left=Indexer(new GraveyardList(arg)); break;
                        case TokenType.FieldOfPlayer: left=Indexer(new FieldList(arg)); break;
                    }
                }
                else left=Indexer(new BoardList());
            }
            else if(Match(TokenType.Find)){
                Consume(TokenType.LeftParen,"Expected '(' after method");
                Consume (TokenType.Identifier, "Invalid predicate argument");
                Token parameter=Previous();
                Consume (TokenType.RightParen,"Expeted ')' after predicate argument");
                Consume (TokenType.Arrow, "Expected predicate function call");
                IExpression predicate= Expression();
                Consume(TokenType.RightParen, "Expected ')' after predicate");
                left= Indexer(new ListFind(left,predicate,parameter));
            }
            else if(Match(TokenType.Pop)){
                Consume(TokenType.LeftParen, "Expected '(' after method");
                Consume(TokenType.RightParen, "Expected ')' after method");
                left=new Pop(left);
            }
            else if(Match(TokenType.TriggerPlayer)) return new TriggerPlayer();
            else if(Match(TokenType.Name)) return new NameAccess(left);
            else if(Match(TokenType.Power)) return new PowerAccess(left);
            else if(Match(TokenType.Faction)) return new FactionAccess(left);
            else if(Match(TokenType.Type)) return new TypeAccess(left);
            else if(Match(TokenType.Owner)) return new OwnerAccess(left);
            TokenType type =Peek().type;
            if(type== TokenType.Push||type== TokenType.Remove||type== TokenType.SendBottom||type== TokenType.Shuffle) return left;
            throw Error(Peek(), "Invalid property access");
        }
        return left;
    }
    public IExpression Indexer(IExpression list){
        if(Match(TokenType.LeftBracket)){
            IExpression index= Expression();
            Consume(TokenType.RightBracket, "Expected ']' after List Indexing");
            return new IndexedCard(index,list);
        }
        else return list;
    }
    #endregion

    #region Statement Parsing   

    
    public IStatement Statement(){
        if(Check(TokenType.Identifier)){
            Token identifier=Peek();
            IExpression expr=Expression();
           
            if(Match(TokenType.Equal)){
                IExpression assignation=Expression();
                Consume(TokenType.Semicolon, "Expected ';' after assignation");
                return new Assignation(expr,assignation);
            }
            if(Match(new List<TokenType>(){TokenType.Increment,TokenType.Decrement})){
                Token operation=Previous();
                Consume(TokenType.Semicolon, "Expected ';' after assignation");
                return new Increment_Decrement(expr,operation);
            }
            if(Match(new List<TokenType>(){TokenType.MinusEqual,TokenType.PlusEqual,TokenType.StarEqual,TokenType.SlashEqual,TokenType.AtSymbolEqual})){
                Token operation=Previous();
                IExpression assignation=Expression();
                return new NumericModification(expr,assignation,operation);
            }
            if(Match(TokenType.Dot)){
                if(expr is List){
                    if(Match(new List<TokenType>(){TokenType.Push,TokenType.SendBottom, TokenType.Remove})){
                        Token method=Previous();
                        Consume(TokenType.LeftParen,"Expeted '(' after method");
                        IExpression card=Expression();
                        Consume(TokenType.RightParen,"Expected ')' after method");
                        Consume(TokenType.Semicolon, "Expected ';' after method");
                        if(method.type==TokenType.Push)return new Push((List)expr,(ICardAtom)card);
                        if(method.type==TokenType.SendBottom) return new SendBottom((List)expr,(ICardAtom)card);
                        return new Remove((List)expr,(ICardAtom)card);
                    }
                    if(Match(TokenType.Shuffle)){
                        Consume(TokenType.LeftParen, "Expected '(' after method");
                        Consume(TokenType.LeftParen, "Expected ')' after method");
                        Consume(TokenType.Semicolon, "Expected ';' after method");
                        return new Shuffle((List)expr);
                    }
                    throw Error(Peek(), "Expected list method");
                }
                else throw Error(Peek(), "Invalid method call");
            }
            if(expr is IStatement) return (IStatement) expr;
            else throw Error(Peek(), "Invalid statement");
        }
        if(Check(TokenType.While)){
            Consume(TokenType.LeftParen, "Expect '(' after 'while'");
            IExpression predicate = Expression();
            Consume(TokenType.RightParen, "Expect ')' after condition.");
            List<IStatement> body = null;
            if(Match(TokenType.LeftBrace)) body=Block();
            else body=new List<IStatement>(){Statement()};
            return new While(body,predicate);
        }
        if(Check(TokenType.For)){
            Token variable =Consume(TokenType.Identifier,"Expected identifier in for statement");
            Consume(TokenType.In, "Expected 'in' in for statement");
            IExpression collection= Expression();
            List<IStatement> body = null;
            if(Match(TokenType.LeftBrace)) body=Block();
            else body=new List<IStatement>(){Statement()};
            return new Foreach(body,collection,variable);
        }
        throw Error(Peek(),"Invalid statement");
    }

    public List<IStatement> Block(){
        List<IStatement> statements=new List<IStatement>();
        while(!Check(TokenType.RightBrace)&&!IsAtEnd()){
            statements.Add(Statement());
        }
        Consume(TokenType.RightBrace, "Expected '}' after block");
        return statements;
    }
    #endregion
    public Action Action(){
        Consume(TokenType.Colon, "Expected ':' after 'Action' in Action construction");
        Consume(TokenType.RightParen, "Invalid Action construction, expected '('");
        Token targetsID=Consume(TokenType.Identifier,"Expected targets argument identifier");
        Consume(TokenType.Comma,"Expected ',' between arguments");
        Token contextID= Consume(TokenType.Identifier,"Expected context argument identifier");
        Consume(TokenType.RightParen, "Invalid Action construction, expected ')'");
        Consume(TokenType.Arrow,"Invalid Action construction, expected '=>'"); 
        List<IStatement> body = null;
        if(Match(TokenType.LeftBrace)) body=Block();
        else body=new List<IStatement>(){Statement()};
        return new Action(body,contextID,targetsID);
    }
    public DefParameters Parameters(){
        Dictionary<string,string> parameters=new Dictionary<string,string>();
        Consume(TokenType.Colon,"Expected ':' after Params construction");
        Consume(TokenType.RightBrace,"Params definition must declare a body");
        while(!Match(TokenType.RightBrace)){
            Token name=Consume(TokenType.Identifier,"Invalid parameter name");
            if(parameters.ContainsKey(name.lexeme)) throw Error(Previous(),$"The effect already contains {name.lexeme} parameter");
            Consume(TokenType.Colon, "Expected ':' after parameter name");
            if(Match(new List<TokenType>(){TokenType.Number,TokenType.Bool,TokenType.String})) parameters[name.lexeme]=Previous().lexeme;
            else throw Error(Peek(), "Invalid parameter type");
        }
        return new DefParameters(parameters);
    }

    public string EffectName(){
        Consume(TokenType.Colon,"Expected ':' after name declaration");
        Token name= Consume(TokenType.StringLiteral,"Expected string in name declaration");
        Consume(TokenType.Comma,"Expected ',' after name declaration");
        return name.lexeme;
    }

    public EffectDefinition EffectDefinition(){
        Token reserved =Previous();
        Consume(TokenType.LeftBrace,"EffectDefinition must declare a body");
        EffectDefinition definition=new EffectDefinition();
        while(!Match(TokenType.LeftBrace)){
            if(Match(TokenType.Name)){
                if(definition.name!=null) throw Error(Previous(),"Name was already declared in this effect");
                definition.name=EffectName();
                continue;
            }
            if(Match(TokenType.Params)){
                if(definition.parameters!=null) throw Error(Previous(),"Params was already declared in this effect");
                definition.parameters=Parameters();
                continue;
            }
            if(Match(TokenType.Action)){
                if(definition.action!=null) throw Error(Previous(),"Action was already declared in this effect");
                definition.action=Action();
                continue;
            }
            throw Error(Peek(), "Expected effect definition field");
        }
        if(definition.name==null||definition.action==null) throw Error(reserved,"There are missing effect arguments in the ocnstruction");
        return definition;
    }

    
}
    



    
  /////////////////////////////////////////////////////////////////////
  
    //  PENDIENTE: HACER COMPOUND PARSING EMPEZANDO POR EFFECT DEFINITON Y VER LO DE HACER LOS CAMPOS NULLABLES PARA TRABAJAR COMODO
    //             IMPLEMENTAR AZUCAR SINTACTICA DE LISTAS DEL CONTEXT
    //  PIN DE IDEA IMPORTANTE:
    //  AGRUPAR MEDIANTE INTERFACES A LAS CLASES DEL AST PARA SABER QUE TIPO DEBERIAN DEVOLVER, PARA EL CHECK