using System.Collections;
using System.Collections.Generic;
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
    bool Match(List<TokenType> types){
        foreach (TokenType type in types){
            if(Check(type)){
                Advance();
                return true;
            }
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
    Token Previous(){
        return tokens[current-1];
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
            IExpression right=Equality();
            if(Previous().type==TokenType.And) expr = new And(expr,right);
            else expr = new Or(expr,right);
        }
        return expr;
    }
    public IExpression Equality(){
        IExpression expr=Comparison();
        List<TokenType> types=new List<TokenType>(){TokenType.BangEqual,TokenType.EqualEqual};
        while(Match(types)){
            IExpression right=Comparison();
            if(Previous().type==TokenType.BangEqual) expr = new Differ(expr,right);
            else expr = new Equals(expr,right);
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
            IExpression right=Term();
            switch (Previous().type){
                case TokenType.Greater: expr= new Great(expr,right); break;
                case TokenType.GreaterEqual: expr= new AtMost(expr,right); break;
                case TokenType.Less: expr= new Less(expr,right); break;
                case TokenType.LessEqual: expr= new AtLeast(expr,right); break;
            }
        }
        return expr; 
    }
    public IExpression Term(){
       IExpression expr=Factor();
        List<TokenType> types=new List<TokenType>(){TokenType.Minus,TokenType.Plus};
        while(Match(types)){
            IExpression right=Factor();
            if(Previous().type==TokenType.Plus) expr = new Plus(expr,right);
            else expr = new Minus(expr,right);
        }
        return expr; 
    }
    public IExpression Factor(){
        IExpression expr=Power();
        List<TokenType> types=new List<TokenType>(){TokenType.Pow};
        while(Match(types)){
            IExpression right=Power();
            expr = new Power(expr,right);
        }
        return expr; 
    }
    public IExpression Power(){
        IExpression expr=Unary();
        List<TokenType> types=new List<TokenType>(){TokenType.BangEqual,TokenType.EqualEqual};
        while(Match(types)){
            IExpression right=Unary();
            if(Previous().type==TokenType.BangEqual) expr = new Differ(expr,right);
            else expr = new Equals(expr,right);
        }
        return expr; 
    }
    ////pendiente mannana pensar en forma de hacer el trycatch en el check del ast y terminar de hacer el parser de expresiones
    public IExpression Unary(){
       
    }
    public IExpression Primary(){
       
    }

    #endregion
     
}