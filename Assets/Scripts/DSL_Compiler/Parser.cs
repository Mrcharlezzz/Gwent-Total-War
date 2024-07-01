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

    #region Expression Parsing
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
                Token parameter=Consume (TokenType.Identifier, "Invalid predicate argument");
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

    #region Compounds Parsing
    public string EffectName(){
        Consume(TokenType.Colon,"Expected ':' after name declaration");
        Token name= Consume(TokenType.StringLiteral,"Expected string in name declaration");
        Consume(TokenType.Comma,"Expected ',' after name declaration");
        return (string)name.literal;
    }
    public Dictionary<string,string> ParameterTypes(){
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
        return parameters;
    }
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
                definition.parameters=ParameterTypes();
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

    public Dictionary<string,object> Parameters(){
        Dictionary<string,object> parameters=new Dictionary<string,object>();
        while(!Match(TokenType.RightBrace)||!Match(TokenType.Name)){
            Token name=Consume(TokenType.Identifier,"Invalid parameter name");
            if(parameters.ContainsKey(name.lexeme)) throw Error(Previous(),$"The effect already contains {name.lexeme} parameter");
            Consume(TokenType.Colon, "Expected ':' after parameter name");
            if(Match(new List<TokenType>(){TokenType.NumberLiteral,TokenType.True,TokenType.False,TokenType.StringLiteral})) parameters[name.lexeme]=Previous().literal;
            else throw Error(Peek(), "Invalid parameter type");
        }
        return parameters;
    }

    public Effect Effect(){
        Token reserved =Previous();
        Effect effect=new Effect();
        Consume(TokenType.Colon,"Expected ':' after Effect declaration");
        if(Match(TokenType.StringLiteral)){
            effect.definition=(string)Previous().literal;
            return effect;
        }
        Consume(TokenType.LeftBrace,"Effect must declare a body");
        while(!Match(TokenType.RightBrace)){
            if(Match(TokenType.Name)){
                if(effect.definition!=null) throw Error(Previous(),"Params was already declared in this effect");
                effect.definition=EffectName();
                continue;
            }
            if(Match(TokenType.Identifier)){
                if(effect.parameters!=null) throw Error(Previous(),"Params was already declared in this effect");
                effect.parameters=Parameters();
                continue;
            }
            throw Error(Peek(), "Expected effect field");
        }
        if(effect.definition==null) throw Error(reserved,"There are missing effect arguments in the ocnstruction");
        return effect;
    }
    public List Source(Player triggerplayer){
        Token reserved=Previous();
        Consume(TokenType.Colon,"Expected ';' after Source declaration");
        Token literal=Consume(TokenType.StringLiteral,"Expected string in Source declaration");
        Consume(TokenType.Comma,"Expected ',' after Source declaration");
        switch(literal.literal){
            case "board": return new BoardList();
            case "hand": return new HandList(new Literal(triggerplayer));
            case "otherHand": return new HandList(new Literal(triggerplayer.Other()));
            case "deck": return new DeckList(new Literal(triggerplayer));
            case "otherDeck": return new DeckList(new Literal(triggerplayer.Other()));
            case "graveyard": return new GraveyardList(new Literal(triggerplayer));
            case "otherGraveyard": return new GraveyardList(new Literal(triggerplayer.Other()));
            case "field": return new FieldList(new Literal(triggerplayer));
            case "otherField": return new FieldList(new Literal(triggerplayer.Other()));
            case "parent":
                return null;
            default: throw Error(literal,"Invalid source");
        }
    }
    public bool Single(){
        Consume(TokenType.Colon,"Expected ':' after Single declaration");
        if(!Match(new List<TokenType>(){TokenType.True,TokenType.False})) throw Error(Peek(),"Expected Bool in Single declaration");
        Token boolean=Previous();
        Consume(TokenType.Comma,"Expected ',' after Single declaration");
        return (bool)boolean.literal;    
    }

    public (IExpression,Token) Predicate(){
        Consume(TokenType.Colon,"Expected ':' after Predicate definition");
        Consume(TokenType.LeftParen,"Expected '('");
        Token argument=Consume(TokenType.Identifier,"Expected Predicate argument");
        Consume(TokenType.RightParen,"Expected ')'");
        Consume(TokenType.Arrow,"Invalid Predicate construction, expected '=>'");   
        IExpression predicate=Expression();
        Consume(TokenType.RightBrace,"Expected '}'");
        return (predicate,argument);

    }//IMPLEMENTAR PREDICTATE PARSING

    public Selector Selector( Player triggerplayer){
        Token reserved=Previous();
        Consume(TokenType.Colon, "Expected ':' after Selector declaration");
        Consume(TokenType.LeftBrace, "Selector must declare a body");
        Selector selector=new Selector();
        selector.filtre=new ListFind();
        while(!Match(TokenType.RightBrace)){
            if(Match(TokenType.Source)){
                if(selector.filtre.list!=null) throw Error(Previous(),"Source was already declared in this Selector");
                selector.filtre.list=Source( triggerplayer);
                continue; 
            }
            if(Match(TokenType.Single)){
                if(selector.single!=null) throw Error(Previous(),"Single was already declared in this Selector");
                selector.single=Single();
                continue;
            }
            if(Match(TokenType.Predicate)){
                if(selector.filtre.predicate!=null) throw Error(Previous(),"Predicate was already declared in this Selector");
                var aux=Predicate();
                selector.filtre.predicate=aux.Item1;
                selector.filtre.parameter=aux.Item2;
            }
            throw Error(Peek(), "Expected effect field");
        }
        if(selector.single==null) selector.single=false;
        if(selector.filtre.predicate==null||selector.filtre.parameter==null) throw Error(reserved,"There are missing fields in Selector construction");
        return selector;
    }

    public EffectActivation EffectActivation(Player triggerplayer){
        Consume(TokenType.LeftBrace, "Expected '{'");
        EffectActivation activation=new EffectActivation();
        while(!Match(TokenType.RightBrace)){
            if(Match(TokenType.Effect)){
                if(activation.effect!=null) throw Error(Previous(), "Effect was already declared in this EffectActivation");
                activation.effect=Effect();
                continue;
            }
            if(Match(TokenType.Selector)){
                if(activation.selector!=null) throw Error(Previous(), "Selector was already declared in this EffectActivation");
                activation.selector=Selector(triggerplayer);
                continue;
            }
            if(Match(TokenType.PostAction)){
                if(activation.postAction!=null) throw Error(Previous(), "PostAction was already declared in this EffectActivation");
                activation.postAction=EffectActivation(triggerplayer);
                continue; 
            }
        }
        if(activation.effect==null) throw Error(Previous(),"There are missing fields in EffectActivation");
        
    }

    #endregion


}
    



    
  /////////////////////////////////////////////////////////////////////
  
    //  PENDIENTE: CONTINUAR VIENDO LO DEL PARSEO DE POSTACTION ANIDADOS
    //             IMPLEMENTAR AZUCAR SINTACTICA DE LISTAS DEL CONTEXT
    //  PIN DE IDEA IMPORTANTE:
    //  AGRUPAR MEDIANTE INTERFACES A LAS CLASES DEL AST PARA SABER QUE TIPO DEBERIAN DEVOLVER, PARA EL CHECK