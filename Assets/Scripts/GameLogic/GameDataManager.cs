using UnityEngine;
using System.Collections;
using System;

public class GameDataManager : MonoBehaviour
{
    void OnApplicationQuit()
    {
        Database.SaveData(); // Asume que SaveData es un método estático en tu clase Database que guarda tus datos
    }
}