using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;

public class Database: MonoBehaviour 
{
    public static int Count{get => storage.Count + deck1.Count + deck2.Count;}
    public static List<Card> storage;
    public static List<Card> deck1;
    public static List<Card> deck2;
    

    public static void SaveData(){
        string storageJson = JsonUtility.ToJson(storage);
        string deck1Json = JsonUtility.ToJson(deck1);
        string deck2Json = JsonUtility.ToJson(deck2);

        //Guardar en archivos
        File.WriteAllText(Application.persistentDataPath + "/storage.json", storageJson);
        File.WriteAllText(Application.persistentDataPath + "/deck1.json", deck1Json);
        File.WriteAllText(Application.persistentDataPath + "/deck2.json", deck2Json);
    }

    public static void LoadData()
    {
        storage = JsonUtility.FromJson<List<Card>>(File.ReadAllText(Application.persistentDataPath + "/storage.json"));
        deck1 = JsonUtility.FromJson<List<Card>>(File.ReadAllText(Application.persistentDataPath + "/deck1.json"));
        deck2 = JsonUtility.FromJson<List<Card>>(File.ReadAllText(Application.persistentDataPath + "/deck2.json"));
    }

    public static void Initialize(){
        LoadData();

        string path = Application.persistentDataPath;
        Debug.Log("Persistent Data Path: " + path);

        if (deck1 is null || deck1.Count==0)
        {
            storage = new List<Card>();
            deck1 = new List<Card>();
            deck2 = new List<Card>();
            deck1.Add(new Unit(0, null, "Caballeria Hetaroi", Resources.Load<Sprite>("CardImages/Caballería Hetaroi"), Card.Type.Silver, "Caballeria griega", "Grecia", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 3));
            deck1.Add(new Unit(1, null, "Caballeria Hetaroi", Resources.Load<Sprite>("CardImages/Caballería Hetaroi"), Card.Type.Silver, "Caballeria griega", "Grecia", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 3));
            deck1.Add(new Unit(2, null, "Caballeria Hetaroi", Resources.Load<Sprite>("CardImages/Caballería Hetaroi"), Card.Type.Silver, "Caballeria griega", "Grecia", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 3));
            deck1.Add(new Unit(3, null, "Escorpion", Resources.Load<Sprite>("CardImages/Escorpión"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
            deck1.Add(new Unit(4, null, "Escorpion", Resources.Load<Sprite>("CardImages/Escorpión"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
            deck1.Add(new Unit(5, null, "Escorpion", Resources.Load<Sprite>("CardImages/Escorpión"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
            deck1.Add(new Unit(9, null, "Arqueros griegos de elite", Resources.Load<Sprite>("CardImages/Arquero Griego de Élite"), Card.Type.Silver, "Expertos tiradores distinguidos por su precision", "Grecia", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 2));
            deck1.Add(new Unit(10, null, "Arqueros griegos de elite", Resources.Load<Sprite>("CardImages/Arquero Griego de Élite"), Card.Type.Silver, "Expertos tiradores distinguidos por su precision", "Grecia", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 2));
            deck1.Add(new Unit(11, null, "Arqueros griegos de elite", Resources.Load<Sprite>("CardImages/Arquero Griego de Élite"), Card.Type.Silver, "Expertos tiradores distinguidos por su precision", "Grecia", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 2));
            deck1.Add(new Unit(15, null, "Catapulta", Resources.Load<Sprite>("CardImages/Catapulta"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
            deck1.Add(new Unit(16, null, "Catapulta", Resources.Load<Sprite>("CardImages/Catapulta"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
            deck1.Add(new Unit(17, null, "Catapulta", Resources.Load<Sprite>("CardImages/Catapulta"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
            deck1.Add(new Boost(18, null, "Estandarte del Ejercito", Resources.Load<Sprite>("CardImages/Estandarte SPQR"), Card.Type.Boost, "Estandarte que aumenta la moral de las tropas", "Neutral", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>())));
            deck1.Add(new Boost(19, null, "Flechas incendiarias", Resources.Load<Sprite>("CardImages/Flechas Incendiarias"), Card.Type.Boost, "Aceite para fueguito", "Neutral", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>())));
            deck1.Add(new Boost(20, null, "Mecanismos Mejorados", Resources.Load<Sprite>("CardImages/Mecanismos Mejorados"), Card.Type.Boost, "Mecanismos Mejorados", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>())));
            deck1.Add(new Weather(24, null, "Terreno enfangado", Resources.Load<Sprite>("CardImages/Terreno enfangado"), Card.Type.Weather, "Terreno enfangado", "Neutral", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>())));
            deck1.Add(new Weather(25, null, "Lluvia", Resources.Load<Sprite>("CardImages/Lluvia"), Card.Type.Weather, "Lluvia", "Neutral", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>())));
            deck1.Add(new Weather(26, null, "Neblina", Resources.Load<Sprite>("CardImages/Neblina"), Card.Type.Weather, "Neblina", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>())));
            deck1.Add(new Decoy(37, null, "Señuelo", Resources.Load<Sprite>("CardImages/Señuelo"), Card.Type.Decoy, "Señuelo", "Neutral", new List<Card.Position>(), new Onactivation(new List<EffectActivation>()), 0));
            deck1.Add(new Clear(39, null, "Día Soleado", Resources.Load<Sprite>("CardImages/Día Soleado"), Card.Type.Clear, "Día Soleado", "Neutral", new List<Card.Position>(), new Onactivation(new List<EffectActivation>())));

            deck2.Add(new Unit(6, null, "Velite", Resources.Load<Sprite>("CardImages/Vélite"), Card.Type.Silver, "Versatil guerrero de alcance medio diestro en el lanzamiento de astas y combate cuerpo a cuerpo", "Roma", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 4));
            deck2.Add(new Unit(7, null, "Velite", Resources.Load<Sprite>("CardImages/Vélite"), Card.Type.Silver, "Versatil guerrero de alcance medio diestro en el lanzamiento de astas y combate cuerpo a cuerpo", "Roma", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 4));
            deck2.Add(new Unit(8, null, "Velite", Resources.Load<Sprite>("CardImages/Vélite"), Card.Type.Silver, "Versatil guerrero de alcance medio diestro en el lanzamiento de astas y combate cuerpo a cuerpo", "Roma", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 4));
            deck2.Add(new Unit(12, null, "Legionario", Resources.Load<Sprite>("CardImages/Legionario"), Card.Type.Silver, "Legionarios del emperador", "Roma", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>()), 2));
            deck2.Add(new Unit(13, null, "Legionario", Resources.Load<Sprite>("CardImages/Legionario"), Card.Type.Silver, "Legionarios del emperador", "Roma", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>()), 2));
            deck2.Add(new Unit(14, null, "Legionario", Resources.Load<Sprite>("CardImages/Legionario"), Card.Type.Silver, "Legionarios del emperador", "Roma", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>()), 2));
            deck2.Add(new Unit(21, null, "Escorpion", Resources.Load<Sprite>("CardImages/Escorpión"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
            deck2.Add(new Unit(22, null, "Escorpion", Resources.Load<Sprite>("CardImages/Escorpión"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
            deck2.Add(new Unit(23, null, "Escorpion", Resources.Load<Sprite>("CardImages/Escorpión"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
            deck2.Add(new Unit(27, null, "Catapulta", Resources.Load<Sprite>("CardImages/Catapulta"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
            deck2.Add(new Unit(28, null, "Catapulta", Resources.Load<Sprite>("CardImages/Catapulta"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
            deck2.Add(new Unit(29, null, "Catapulta", Resources.Load<Sprite>("CardImages/Catapulta"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
            deck2.Add(new Boost(30, null, "Estandarte del Ejercito", Resources.Load<Sprite>("CardImages/Estandarte SPQR"), Card.Type.Boost, "Estandarte que aumenta la moral de las tropas", "Neutral", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>())));
            deck2.Add(new Boost(31, null, "Flechas incendiarias", Resources.Load<Sprite>("CardImages/Flechas Incendiarias"), Card.Type.Boost, "Aceite para fueguito", "Neutral", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>())));
            deck2.Add(new Boost(32, null, "Mecanismos Mejorados", Resources.Load<Sprite>("CardImages/Mecanismos Mejorados"), Card.Type.Boost, "Mecanismos Mejorados", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>())));
            deck2.Add(new Weather(33, null, "Terreno enfangado", Resources.Load<Sprite>("CardImages/Terreno enfangado"), Card.Type.Weather, "Terreno enfangado", "Neutral", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>())));
            deck2.Add(new Weather(34, null, "Lluvia", Resources.Load<Sprite>("CardImages/Lluvia"), Card.Type.Weather, "Lluvia", "Neutral", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>())));
            deck2.Add(new Weather(35, null, "Neblina", Resources.Load<Sprite>("CardImages/Neblina"), Card.Type.Weather, "Neblina", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>())));
            deck2.Add(new Decoy(36, null, "Señuelo", Resources.Load<Sprite>("CardImages/Señuelo"), Card.Type.Decoy, "Señuelo", "Neutral", new List<Card.Position>(), new Onactivation(new List<EffectActivation>()), 0));
            deck2.Add(new Clear(38, null, "Día Soleado", Resources.Load<Sprite>("CardImages/Día Soleado"), Card.Type.Clear, "Día Soleado", "Neutral", new List<Card.Position>(), new Onactivation(new List<EffectActivation>())));
            SaveData();
        }
        
    }

    public static Card Search(int id)
    {
        return storage.Concat(deck1).Concat(deck2).FirstOrDefault(t => t.id==id);
    } 
}


