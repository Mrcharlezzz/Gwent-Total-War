using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;

public class Parser
{
    List<Token> tokens;
    int current=0;
    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    //All classes in this region has similar logic to their counterparts in Lexer class
    #region Tools
    public static Exception Error(Token token, string message)
    {
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
    bool Check(TokenType type)
    {
        if(IsAtEnd()) return false;
        return Peek().type==type;
    }
    Token Advance()
    {
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
    Token Consume(TokenType type, string message)
    {
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
            if(Peek().type==TokenType.Dot) return Access(new CardVariable((string)Previous().literal));
            return new Variable ((string)Previous().literal);
        }
            
        if(Match(TokenType.Context)) return Context();
        if(Match(TokenType.Targets)) return ParseCard(new TargetList());
        types=new List<TokenType>(){ TokenType.Targets,TokenType.Context};

        throw Error(Peek(), "Expect expression");
    }

    public IExpression Context(){
        Consume(TokenType.Dot,"Expected context access");
        if(Match(TokenType.TriggerPlayer)) return new TriggerPlayer();
        List<TokenType>types=new List<TokenType>(){ 
            TokenType.HandOfPlayer,TokenType.DeckOfPlayer,TokenType.GraveyardOfPlayer,
            TokenType.FieldOfPlayer,TokenType.Board,
        };
        if(Match(types)){
            IExpression list=null!;
            if(Previous().type!=TokenType.Board)
            {
                TokenType aux=Previous().type;
                Consume(TokenType.LeftParen,"Expected List Argument");
                IExpression arg= Expression();
                Consume(TokenType.RightParen,"Expected ')' after List Argument");
                switch(aux){
                    case TokenType.HandOfPlayer: list=new HandList((Player) arg); break;
                    case TokenType.DeckOfPlayer: list=new DeckList((Player) arg); break;
                    case TokenType.GraveyardOfPlayer: list=new GraveyardList((Player) arg); break;
                    case TokenType.FieldOfPlayer: list=new FieldList((Player) arg); break;
                }
            }
            else list=new BoardList();
            return ParseCard(list);
        }

        throw Error(Peek(), "Invalid context access");
    }

    public IExpression ParseCard(IExpression list){
        if(Match(TokenType.LeftBracket)){
            IExpression index= Expression();
            Consume(TokenType.RightBracket, "Expected ']' after List Indexing");
            return Access(new IndexedCard( index, (List)list));
        }
        if(Match(TokenType.Dot)){
            if(Match(TokenType.Find)){
                Consume(TokenType.LeftParen,"Expected '(' after method");
                Consume (TokenType.card, "Invalid Predicate");
                Consume (TokenType.RightParen,"Expeted ')' after predicate argument");
                Consume (TokenType.Arrow, "Expected predicate function call");
                IExpression predicate= Expression();
                Consume(TokenType.RightParen, "Expected ')' after predicate");
                return ParseCard(new ListFind((List)list,predicate));
            }
            if(Match(TokenType.Pop)){
                Consume(TokenType.LeftParen, "Expected '(' after method");
                Consume(TokenType.RightParen, "Expected ')' after method");
                return Access(new Pop((List)list));
            }
            throw Error(Peek(), "Expected list access");
        }
        throw Error(Peek(), "Invalid list operand");
    }

    public IExpression Access(ICardAtom card){
        if(Match(TokenType.Dot)){
            if(Match(TokenType.Name)) return new NameAccess(card);
            if(Match(TokenType.Power)) return new PowerAccess(card);
            if(Match(TokenType.Faction)) return new FactionAccess(card);
            if(Match(TokenType.Type)) return new TypeAccess(card);
            if(Match(TokenType.Owner)) return new OwnerAccess(card);
            throw Error(Peek(), "Expected Card property access");
        }
        return card;
    }
    #endregion

    #region Statement Parsing
    
    public IStatement Statement(){
        if(Match(TokenType.Identifier)){
            if(Peek().type==TokenType.Dot){
                var access=(PropertyAccess)Access(new CardVariable((string)Previous().literal));
                Consume(TokenType.Equal, "Expected assignation");
                IExpression assignation=Expression();
                Consume(TokenType.Semicolon, "Expected ';' after statement");
                return new CardPropertyAssignation(access,assignation);
            }
            if(Peek().type==TokenType.Equal){
                Token variable=Previous();
                Advance();
                IExpression assignation=Expression();
                Consume(TokenType.Semicolon, "Expected ';' after statement");
                return new VarAssignation(variable,assignation);
            }
            //plusplus, minusminus
        }
    }
    



    #endregion

}