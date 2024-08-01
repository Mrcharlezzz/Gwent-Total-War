using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;

public static class Database 
{
    public static int Count{get => storage.Count + deck1.Count + deck2.Count;}
    public static List<Card> storage;
    public static List<Card> deck1;
    public static List<Card> deck2;


    public static void Initialize(){
            deck1.Add(new Unit(0, 0, "Caballeria Hetaroi", "Caballería Hetaroi", Card.Type.Silver, "Caballeria griega", "Grecia", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 3));
            deck1.Add(new Unit(1, 0, "Caballeria Hetaroi", "Caballería Hetaroi", Card.Type.Silver, "Caballeria griega", "Grecia", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 3));
            deck1.Add(new Unit(2, 0, "Caballeria Hetaroi", "Caballería Hetaroi", Card.Type.Silver, "Caballeria griega", "Grecia", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 3));
            deck1.Add(new Unit(3, 0, "Escorpion", "Escorpión", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
            deck1.Add(new Unit(4, 0, "Escorpion", "Escorpión", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
            deck1.Add(new Unit(5, 0, "Escorpion", "Escorpión", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
            deck1.Add(new Unit(9, 0, "Arqueros griegos de elite", "Arquero Griego de Élite", Card.Type.Silver, "Expertos tiradores distinguidos por su precision", "Grecia", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 2));
            deck1.Add(new Unit(10, 0, "Arqueros griegos de elite", "Arquero Griego de Élite", Card.Type.Silver, "Expertos tiradores distinguidos por su precision", "Grecia", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 2));
            deck1.Add(new Unit(11, 0, "Arqueros griegos de elite", "Arquero Griego de Élite", Card.Type.Silver, "Expertos tiradores distinguidos por su precision", "Grecia", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 2));
            deck1.Add(new Unit(15, 0, "Catapulta", "Catapulta", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
            deck1.Add(new Unit(16, 0, "Catapulta", "Catapulta", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
            deck1.Add(new Unit(17, 0, "Catapulta", "Catapulta", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
            deck1.Add(new Boost(18, 0, "Estandarte del Ejercito", "Estandarte SPQR", Card.Type.Boost, "Estandarte que aumenta la moral de las tropas", "Neutral", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>())));
            deck1.Add(new Boost(19, 0, "Flechas incendiarias", "Flechas Incendiarias", Card.Type.Boost, "Aceite para fueguito", "Neutral", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>())));
            deck1.Add(new Boost(20, 0, "Mecanismos Mejorados", "Mecanismos Mejorados", Card.Type.Boost, "Mecanismos Mejorados", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>())));
            deck1.Add(new Weather(24, 0, "Terreno enfangado", "Terreno enfangado", Card.Type.Weather, "Terreno enfangado", "Neutral", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>())));
            deck1.Add(new Weather(25, 0, "Lluvia", "Lluvia", Card.Type.Weather, "Lluvia", "Neutral", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>())));
            deck1.Add(new Weather(26, 0, "Neblina", "Neblina", Card.Type.Weather, "Neblina", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>())));
            deck1.Add(new Decoy(37, 0, "Señuelo", "Señuelo", Card.Type.Decoy, "Señuelo", "Neutral", new List<Card.Position>(), new Onactivation(new List<EffectActivation>()), 0));
            deck1.Add(new Clear(39, 0, "Día Soleado", "Día Soleado", Card.Type.Clear, "Día Soleado", "Neutral", new List<Card.Position>(), new Onactivation(new List<EffectActivation>())));

            deck2.Add(new Unit(6, 0, "Velite", "Vélite", Card.Type.Silver, "Versatil guerrero de alcance medio diestro en el lanzamiento de astas y combate cuerpo a cuerpo", "Roma", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 4));
            deck2.Add(new Unit(7, 0, "Velite", "Vélite", Card.Type.Silver, "Versatil guerrero de alcance medio diestro en el lanzamiento de astas y combate cuerpo a cuerpo", "Roma", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 4));
            deck2.Add(new Unit(8, 0, "Velite", "Vélite", Card.Type.Silver, "Versatil guerrero de alcance medio diestro en el lanzamiento de astas y combate cuerpo a cuerpo", "Roma", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 4));
            deck2.Add(new Unit(12, 0, "Legionario", "Legionario", Card.Type.Silver, "Legionarios del emperador", "Roma", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>()), 2));
            deck2.Add(new Unit(13, 0, "Legionario", "Legionario", Card.Type.Silver, "Legionarios del emperador", "Roma", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>()), 2));
            deck2.Add(new Unit(14, 0, "Legionario", "Legionario", Card.Type.Silver, "Legionarios del emperador", "Roma", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>()), 2));
            deck2.Add(new Unit(21, 0, "Escorpion", "Escorpión", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
            deck2.Add(new Unit(22, 0, "Escorpion", "Escorpión", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
            deck2.Add(new Unit(23, 0, "Escorpion", "Escorpión", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
            deck2.Add(new Unit(27, 0, "Catapulta", "Catapulta", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
            deck2.Add(new Unit(28, 0, "Catapulta", "Catapulta", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
            deck2.Add(new Unit(29, 0, "Catapulta", "Catapulta", Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
            deck2.Add(new Boost(30, 0, "Estandarte del Ejercito", "Estandarte SPQR", Card.Type.Boost, "Estandarte que aumenta la moral de las tropas", "Neutral", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>())));
            deck2.Add(new Boost(31, 0, "Flechas incendiarias", "Flechas Incendiarias", Card.Type.Boost, "Aceite para fueguito", "Neutral", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>())));
            deck2.Add(new Boost(32, 0, "Mecanismos Mejorados", "Mecanismos Mejorados", Card.Type.Boost, "Mecanismos Mejorados", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>())));
            deck2.Add(new Weather(33, 0, "Terreno enfangado", "Terreno enfangado", Card.Type.Weather, "Terreno enfangado", "Neutral", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>())));
            deck2.Add(new Weather(34, 0, "Lluvia", "Lluvia", Card.Type.Weather, "Lluvia", "Neutral", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>())));
            deck2.Add(new Weather(35, 0, "Neblina", "Neblina", Card.Type.Weather, "Neblina", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>())));
            deck2.Add(new Decoy(36, 0, "Señuelo", "Señuelo", Card.Type.Decoy, "Señuelo", "Neutral", new List<Card.Position>(), new Onactivation(new List<EffectActivation>()), 0));
            deck2.Add(new Clear(38, 0, "Día Soleado", "Día Soleado", Card.Type.Clear, "Día Soleado", "Neutral", new List<Card.Position>(), new Onactivation(new List<EffectActivation>())));
    }

    public static Card Search(int id)
    {
        var a =storage;
        var b =deck1;
        var c =deck2;
        return storage.Concat(deck1).Concat(deck2).FirstOrDefault(t => t.id==id);
    } 
}



