using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;

public static class Database 
{
    public static int Count{get => storage.Count + deck1.Count + deck2.Count;}
    public static List<Card> storage=new List<Card>();
    public static List<Card> deck1=new List<Card>();
    public static List<Card> deck2=new List<Card>();


    public static void Initialize(){
            deck1.Add(new Unit(Count, 0, "Caballeria Hetaroi", "Caballería Hetaroi", Card.Type.Silver, "Caballeria griega", "Grecia", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 3));
            deck1.Add(new Unit(Count, 0, "Caballeria Hetaroi", "Caballería Hetaroi", Card.Type.Silver, "Caballeria griega", "Grecia", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 3));
            deck1.Add(new Unit(Count, 0, "Caballeria Hetaroi", "Caballería Hetaroi", Card.Type.Silver, "Caballeria griega", "Grecia", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 3));
            deck1.Add(new Unit(Count, 0, "Escorpion", "Escorpión", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
            deck1.Add(new Unit(Count, 0, "Escorpion", "Escorpión", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
            deck1.Add(new Unit(Count, 0, "Escorpion", "Escorpión", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
            deck1.Add(new Unit(Count, 0, "Arqueros griegos de elite", "Arquero Griego de Élite", Card.Type.Silver, "Expertos tiradores distinguidos por su precision", "Grecia", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 2));
            deck1.Add(new Unit(Count, 0, "Arqueros griegos de elite", "Arquero Griego de Élite", Card.Type.Silver, "Expertos tiradores distinguidos por su precision", "Grecia", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 2));
            deck1.Add(new Unit(Count, 0, "Arqueros griegos de elite", "Arquero Griego de Élite", Card.Type.Silver, "Expertos tiradores distinguidos por su precision", "Grecia", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 2));
            deck1.Add(new Unit(Count, 0, "Catapulta", "Catapulta", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
            deck1.Add(new Unit(Count, 0, "Catapulta", "Catapulta", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
            deck1.Add(new Unit(Count, 0, "Catapulta", "Catapulta", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
            deck1.Add(new Boost(Count, 0, "Estandarte del Ejercito", "Estandarte SPQR", Card.Type.Boost, "Estandarte que aumenta la moral de las tropas", "Neutral", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>())));
            deck1.Add(new Boost(Count, 0, "Flechas incendiarias", "Flechas Incendiarias", Card.Type.Boost, "Aceite para fueguito", "Neutral", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>())));
            deck1.Add(new Boost(Count, 0, "Mecanismos Mejorados", "Mecanismos Mejorados", Card.Type.Boost, "Mecanismos Mejorados", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>())));
            deck1.Add(new Weather(Count, 0, "Terreno enfangado", "Terreno enfangado", Card.Type.Weather, "Terreno enfangado", "Neutral", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>())));
            deck1.Add(new Weather(Count, 0, "Lluvia", "Lluvia", Card.Type.Weather, "Lluvia", "Neutral", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>())));
            deck1.Add(new Weather(Count, 0, "Neblina", "Neblina", Card.Type.Weather, "Neblina", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>())));
            deck1.Add(new Decoy(Count, 0, "Señuelo", "Señuelo", Card.Type.Decoy, "Señuelo", "Neutral", new List<Card.Position>(), new Onactivation(new List<EffectActivation>()), 0));
            deck1.Add(new Clear(Count, 0, "Día Soleado", "Día Soleado", Card.Type.Clear, "Día Soleado", "Neutral", new List<Card.Position>(), new Onactivation(new List<EffectActivation>())));

            deck2.Add(new Unit(Count, 0, "Velite", "Vélite", Card.Type.Silver, "Versatil guerrero de alcance medio diestro en el lanzamiento de astas y combate cuerpo a cuerpo", "Roma", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 4));
            deck2.Add(new Unit(Count, 0, "Velite", "Vélite", Card.Type.Silver, "Versatil guerrero de alcance medio diestro en el lanzamiento de astas y combate cuerpo a cuerpo", "Roma", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 4));
            deck2.Add(new Unit(Count, 0, "Velite", "Vélite", Card.Type.Silver, "Versatil guerrero de alcance medio diestro en el lanzamiento de astas y combate cuerpo a cuerpo", "Roma", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 4));
            deck2.Add(new Unit(Count, 0, "Legionario", "Legionario", Card.Type.Silver, "Legionarios del emperador", "Roma", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>()), 2));
            deck2.Add(new Unit(Count, 0, "Legionario", "Legionario", Card.Type.Silver, "Legionarios del emperador", "Roma", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>()), 2));
            deck2.Add(new Unit(Count, 0, "Legionario", "Legionario", Card.Type.Silver, "Legionarios del emperador", "Roma", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>()), 2));
            deck2.Add(new Unit(Count, 0, "Escorpion", "Escorpión", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
            deck2.Add(new Unit(Count, 0, "Escorpion", "Escorpión", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
            deck2.Add(new Unit(Count, 0, "Escorpion", "Escorpión", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
            deck2.Add(new Unit(Count, 0, "Catapulta", "Catapulta", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
            deck2.Add(new Unit(Count, 0, "Catapulta", "Catapulta", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
            deck2.Add(new Unit(Count, 0, "Catapulta", "Catapulta", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
            deck2.Add(new Boost(Count, 0, "Estandarte del Ejercito", "Estandarte SPQR", Card.Type.Boost, "Estandarte que aumenta la moral de las tropas", "Neutral", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>())));
            deck2.Add(new Boost(Count, 0, "Flechas incendiarias", "Flechas Incendiarias", Card.Type.Boost, "Aceite para fueguito", "Neutral", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>())));
            deck2.Add(new Boost(Count, 0, "Mecanismos Mejorados", "Mecanismos Mejorados", Card.Type.Boost, "Mecanismos Mejorados", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>())));
            deck2.Add(new Weather(Count, 0, "Terreno enfangado", "Terreno enfangado", Card.Type.Weather, "Terreno enfangado", "Neutral", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>())));
            deck2.Add(new Weather(Count, 0, "Lluvia", "Lluvia", Card.Type.Weather, "Lluvia", "Neutral", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>())));
            deck2.Add(new Weather(Count, 0, "Neblina", "Neblina", Card.Type.Weather, "Neblina", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>())));
            deck2.Add(new Decoy(Count, 0, "Señuelo", "Señuelo", Card.Type.Decoy, "Señuelo", "Neutral", new List<Card.Position>(), new Onactivation(new List<EffectActivation>()), 0));
            deck2.Add(new Clear(Count, 0, "Día Soleado", "Día Soleado", Card.Type.Clear, "Día Soleado", "Neutral", new List<Card.Position>(), new Onactivation(new List<EffectActivation>())));
    }

    public static Card Search(int id)
        {
        var a =storage;
        var b =deck1;
        var c =deck2;
        return storage.Concat(deck1).Concat(deck2).FirstOrDefault(t => t.id==id);
    } 
}



