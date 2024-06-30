using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;
#nullable enable


#pragma warning disable CS8602 // Dereference of a possibly null reference.

public static class Effects{
    public static Dictionary<string,EffectDefinition>? effects;
}
public class EffectDefinition: IASTNode{
    public string? name;
    public DefParameters? parameters;
    public Action? action;
    public void Execute(){
        action.Execute(action.context,action.targets);
    }
}

public class DefParameters: IASTNode{
    public DefParameters(Dictionary<string,string> parameterTypes){
        this.parameterTypes = parameterTypes;
    }
    public Dictionary<string,string>? parameterTypes;
}







#pragma warning restore CS8602 // Dereference of a possibly null reference.