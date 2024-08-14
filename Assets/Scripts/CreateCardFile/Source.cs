public static class CompilationSource{
    public static string Source = @"
    effect{
        Name:""Draw"",   // Draws a card from the Deck
        
        Action:(targets,context) =>{
            topCard=context.Deck.Pop();
            context.Hand.Push(topCard);
            context.Hand.Shuffle();    
        }
    }

    effect{
        Name: ""Damage"", //Weakens a Faction units
        Params:{
            amount: Number
        }
        Action: (targets, context) =>{
            for (target in targets){
                i=0;
                while(i++ < amount){
                    target.Power -= 1;
                }
            }
        }
    }

    effect{
        Name: ""ReturnToDeck"",
        Action(targets, context)=>{
            for(target in targets){
                owner = target.Owner;
                deck= context.DeckOfPlayer(owner);
                deck.Push(target);
                deck.Shuffle();
                context.Board.Remove(target);
            }
        }
    }

    card{  
        Name: ""TestCard1"", 
        Faction:""Newbies"",
        Range: [""Melee"", ""Siege""],
        Type: ""Oro"",
        Power: 5,
        OnActivation: [
            {
                Effect: ""Draw"",
            }
        ]
    }

    card{
        Name: ""TestCard2"",
        Faction:""Newbies"",
        Range: [ ""Ranged""],
        Type: ""Señuelo"",
        Power: 0,
        OnActivation: [
            {
                Effect: {
                    Name: ""Damage"",
                    amount: 2,
                }
                Selector: {
                    Source: ""board"", 
                    Single: false,
                    Predicate: (unit) => unit.Faction == ""Ro"" @ ""ma""
                }
            }
        ]
    }
    ";
}