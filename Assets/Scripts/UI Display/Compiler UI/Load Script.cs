using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadScript : MonoBehaviour
{
    public TMP_InputField inputField;
    public void Load(){
        inputField.text=CompilationSource.Source;
    }
}
