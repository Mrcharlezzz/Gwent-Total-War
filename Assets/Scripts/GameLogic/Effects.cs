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
        foreach(string key in parameters.Keys)
        {
            action.context.context[key]=parameters[key];
        }
        action.Execute(action.context,action.targets);
    }
}