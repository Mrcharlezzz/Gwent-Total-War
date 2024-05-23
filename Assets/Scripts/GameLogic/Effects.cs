using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Effects
{
    public static Dictionary<string,EffectDefinition> effects;
}
public class EffectDefinition
{
    public Dictionary<string,object> parameters;
    public Action action;
    public void Execute()
    {
        
    }
}

public class Action
{
    
    public Context context;
    public List<Card> target;
    public List<StatementNode> statements;
}

public class Context
{
    public  Dictionary <string, object> intcontext;
    public GlobalContext globalcontext;
}

public abstract class StatementNode
{
    public abstract void Execute();
}

public class DeclarationNode
{

}




