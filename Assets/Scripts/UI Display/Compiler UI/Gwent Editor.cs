using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GwentEditor : MonoBehaviour
{
    public TMP_InputField inputField;

    public TextMeshProUGUI columnText;

    private void Update()
    {
        if (inputField.text.Contains("\n"))
        {
            AddLineNumbers();
        }
    }

    private void AddLineNumbers()
    {
        int clines = inputField.text.Split('\n').Length;
        string[] lines = new string[clines];
        for (int i = 0; i < clines; i++)
            lines[i] = "";

        for (int i = 0; i < clines; i++)
        {
            lines[i] = $"{i+1}";
        }
        columnText.text = string.Join("\n", lines);
    }
}
