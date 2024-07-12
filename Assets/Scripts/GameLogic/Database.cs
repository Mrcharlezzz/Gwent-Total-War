using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public static class Database 
{
    public static List<Card> storage = new List<Card>();
    public static List<Card> deck1 = new List<Card>();
    public static List<Card> deck2 = new List<Card>();

    public static void Poblate(){
        Deck deck = GlobalContext.gameMaster.player1.deck;
        deck.Push(new Unit(0, null, "Caballeria Hetaroi", Resources.Load<Sprite>("CardImages/Caballería Hetaroi"), Card.Type.Silver, "Caballeria griega", "Grecia", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 3));
        deck.Push(new Unit(1, null, "Caballeria Hetaroi", Resources.Load<Sprite>("CardImages/Caballería Hetaroi"), Card.Type.Silver, "Caballeria griega", "Grecia", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 3));
        deck.Push(new Unit(2, null, "Caballeria Hetaroi", Resources.Load<Sprite>("CardImages/Caballería Hetaroi"), Card.Type.Silver, "Caballeria griega", "Grecia", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 3));
        deck.Push(new Unit(3, null, "Escorpion", Resources.Load<Sprite>("CardImages/Escorpión"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
        deck.Push(new Unit(4, null, "Escorpion", Resources.Load<Sprite>("CardImages/Escorpión"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
        deck.Push(new Unit(5, null, "Escorpion", Resources.Load<Sprite>("CardImages/Escorpión"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
        deck.Push(new Unit(9, null, "Arqueros griegos de elite", Resources.Load<Sprite>("CardImages/Arquero Griego de Élite"), Card.Type.Silver, "Expertos tiradores distinguidos por su precision", "Grecia", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 2));
        deck.Push(new Unit(10, null, "Arqueros griegos de elite", Resources.Load<Sprite>("CardImages/Arquero Griego de Élite"), Card.Type.Silver, "Expertos tiradores distinguidos por su precision", "Grecia", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 2));
        deck.Push(new Unit(11, null, "Arqueros griegos de elite", Resources.Load<Sprite>("CardImages/Arquero Griego de Élite"), Card.Type.Silver, "Expertos tiradores distinguidos por su precision", "Grecia", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 2));
        deck.Push(new Unit(15, null, "Catapulta", Resources.Load<Sprite>("CardImages/Catapulta"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
        deck.Push(new Unit(16, null, "Catapulta", Resources.Load<Sprite>("CardImages/Catapulta"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
        deck.Push(new Unit(17, null, "Catapulta", Resources.Load<Sprite>("CardImages/Catapulta"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
        deck.Push(new Boost(18, null, "Estandarte del Ejercito", Resources.Load<Sprite>("CardImages/Estandarte SPQR"), Card.Type.Boost, "Estandarte que aumenta la moral de las tropas", "Neutral", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>())));
        deck.Push(new Boost(19, null, "Flechas incendiarias", Resources.Load<Sprite>("CardImages/Flechas Incendiarias"), Card.Type.Boost, "Aceite para fueguito", "Neutral", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>())));
        deck.Push(new Boost(20, null, "Mecanismos Mejorados", Resources.Load<Sprite>("CardImages/Mecanismos Mejorados"), Card.Type.Boost, "Mecanismos Mejorados", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>())));
        deck.Push(new Weather(24, null, "Terreno enfangado", Resources.Load<Sprite>("CardImages/Terreno enfangado"), Card.Type.Weather, "Terreno enfangado", "Neutral", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>())));
        deck.Push(new Weather(25, null, "Lluvia", Resources.Load<Sprite>("CardImages/Lluvia"), Card.Type.Weather, "Lluvia", "Neutral", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>())));
        deck.Push(new Weather(26, null, "Neblina", Resources.Load<Sprite>("CardImages/Neblina"), Card.Type.Weather, "Neblina", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>())));
        deck.Push(new Decoy(37, null, "Señuelo", Resources.Load<Sprite>("CardImages/Señuelo"), Card.Type.Decoy, "Señuelo", "Neutral", new List<Card.Position>(), new Onactivation(new List<EffectActivation>()), 0));
        deck.Push(new Clear(39, null, "Día Soleado", Resources.Load<Sprite>("CardImages/Día Soleado"), Card.Type.Clear, "Día Soleado", "Neutral", new List<Card.Position>(), new Onactivation(new List<EffectActivation>())));

        foreach(Card card in deck1){
            deck.Push(card);
        }

        deck = GlobalContext.gameMaster.player2.deck;
        deck.Push(new Unit(6, null, "Velite", Resources.Load<Sprite>("CardImages/Vélite"), Card.Type.Silver, "Versatil guerrero de alcance medio diestro en el lanzamiento de astas y combate cuerpo a cuerpo", "Roma", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 4));
        deck.Push(new Unit(7, null, "Velite", Resources.Load<Sprite>("CardImages/Vélite"), Card.Type.Silver, "Versatil guerrero de alcance medio diestro en el lanzamiento de astas y combate cuerpo a cuerpo", "Roma", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 4));
        deck.Push(new Unit(8, null, "Velite", Resources.Load<Sprite>("CardImages/Vélite"), Card.Type.Silver, "Versatil guerrero de alcance medio diestro en el lanzamiento de astas y combate cuerpo a cuerpo", "Roma", new List<Card.Position>(){Card.Position.Melee, Card.Position.Ranged}, new Onactivation(new List<EffectActivation>()), 4));
        deck.Push(new Unit(12, null, "Legionario", Resources.Load<Sprite>("CardImages/Legionario"), Card.Type.Silver, "Legionarios del emperador", "Roma", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>()), 2));
        deck.Push(new Unit(13, null, "Legionario", Resources.Load<Sprite>("CardImages/Legionario"), Card.Type.Silver, "Legionarios del emperador", "Roma", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>()), 2));
        deck.Push(new Unit(14, null, "Legionario", Resources.Load<Sprite>("CardImages/Legionario"), Card.Type.Silver, "Legionarios del emperador", "Roma", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>()), 2));
        deck.Push(new Unit(21, null, "Escorpion", Resources.Load<Sprite>("CardImages/Escorpión"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
        deck.Push(new Unit(22, null, "Escorpion", Resources.Load<Sprite>("CardImages/Escorpión"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
        deck.Push(new Unit(23, null, "Escorpion", Resources.Load<Sprite>("CardImages/Escorpión"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Ranged, Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 3));
        deck.Push(new Unit(27, null, "Catapulta", Resources.Load<Sprite>("CardImages/Catapulta"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
        deck.Push(new Unit(28, null, "Catapulta", Resources.Load<Sprite>("CardImages/Catapulta"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
        deck.Push(new Unit(29, null, "Catapulta", Resources.Load<Sprite>("CardImages/Catapulta"), Card.Type.Silver, "Poderosa arma de asedio", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>()), 5));
        deck.Push(new Boost(30, null, "Estandarte del Ejercito", Resources.Load<Sprite>("CardImages/Estandarte SPQR"), Card.Type.Boost, "Estandarte que aumenta la moral de las tropas", "Neutral", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>())));
        deck.Push(new Boost(31, null, "Flechas incendiarias", Resources.Load<Sprite>("CardImages/Flechas Incendiarias"), Card.Type.Boost, "Aceite para fueguito", "Neutral", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>())));
        deck.Push(new Boost(32, null, "Mecanismos Mejorados", Resources.Load<Sprite>("CardImages/Mecanismos Mejorados"), Card.Type.Boost, "Mecanismos Mejorados", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>())));
        deck.Push(new Weather(33, null, "Terreno enfangado", Resources.Load<Sprite>("CardImages/Terreno enfangado"), Card.Type.Weather, "Terreno enfangado", "Neutral", new List<Card.Position>(){Card.Position.Melee}, new Onactivation(new List<EffectActivation>())));
        deck.Push(new Weather(34, null, "Lluvia", Resources.Load<Sprite>("CardImages/Lluvia"), Card.Type.Weather, "Lluvia", "Neutral", new List<Card.Position>(){Card.Position.Ranged}, new Onactivation(new List<EffectActivation>())));
        deck.Push(new Weather(35, null, "Neblina", Resources.Load<Sprite>("CardImages/Neblina"), Card.Type.Weather, "Neblina", "Neutral", new List<Card.Position>(){Card.Position.Siege}, new Onactivation(new List<EffectActivation>())));
        deck.Push(new Decoy(36, null, "Señuelo", Resources.Load<Sprite>("CardImages/Señuelo"), Card.Type.Decoy, "Señuelo", "Neutral", new List<Card.Position>(), new Onactivation(new List<EffectActivation>()), 0));
        deck.Push(new Clear(38, null, "Día Soleado", Resources.Load<Sprite>("CardImages/Día Soleado"), Card.Type.Clear, "Día Soleado", "Neutral", new List<Card.Position>(), new Onactivation(new List<EffectActivation>())));

        foreach(Card card in deck2){
            deck.Push(card);
        }
    }
    public static int Count{get => storage.Count + deck1.Count + deck2.Count;}

    public static Card Search(int id)
    {
        return storage.Concat(deck1).Concat(deck2).FirstOrDefault(t => t.id==id);
    } 
}


