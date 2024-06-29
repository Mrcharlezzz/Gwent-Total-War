using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Effects
{
    public static Dictionary<string,EffectDefinition> effects;
}
public class EffectDefinition
{
    public Dictionary<string,string> parameters;
    public Action action;
    public void Execute()
    {
        action.Execute(action.context,action.targets);
    }
}