
/// <summary>
/// <c>Source code for card editor</c>
/// </summary>
public static class CompilationSource{
    public static string Source = @"effect{
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
        //Returns selected cards from the board to their respective owners deck
        Name: ""ReturnToDeck"", 
        Action: (targets, context)=>{
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
        Range: [],
        Name: ""TestCard2"",
        Faction:""Newbies"",
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

    card{
        Faction: ""Newbies"",
        Name: ""TestCard3"",
        Type: ""Clima"",
        Range: [""Melee"" , ""Ranged""],
        OnActivation: [
            {
                Effect: ""ReturnToDeck"",
                Selector: {
                    Source: ""board"",
                    Single: false,
                    Predicate: (unit) => unit.Faction == ""Gre"" @ ""cia""
                }
            },
        ]
    }
    card{
        Type: ""Plata"",
        Faction: ""Newbies"",
        Power: 10,
        Name: ""TestCard4"",
        Range: [""Melee"" , ""Ranged""],
        OnActivation: [
            {
                Effect: {
                    Name:""Damage"",
                    amount: 3,
                }
                Selector: {
                    Source: ""board"",
                    Single: false,
                    Predicate: (unit) => unit.Faction == ""Ro"" @ ""ma""
                }
                PostAction: {
                    Effect: ""ReturnToDeck"",
                    Selector:{
                        Source: ""parent"",
                        Single: false,
                        Predicate: (unit) => unit.Power < 3
                    }
                }
            },
            {
                Effect: ""Draw"",
            }
        ]
    }

    card{  
        Name: ""TestCard5"", 
        Faction:""Newbies"",
        Range: [],
        Type: ""Líder"",
        OnActivation: [
            {
                Effect: ""Draw"",
            }
        ]
    }

    // The following effect will run into an infinite loop and trigger
    //the invalid effect response
    effect{
        Name:""Infinito"",
        Action: (targets, context)=>{
            board = context.Board;
            for(target in board){
                while(target.Power > -1){
                    target.Power++;
                }
            }
        }
    }
    card{
        Name:""Satoru Gojo"",
        Faction:""Jujutsu Kaisen"",
        Type:""Oro"",
        Power: 100,
        Range: [""Melee"",""Ranged"",""Siege""],
        OnActivation: [
            {
                Effect:""Infinito"",
            }
        ]

    }


    //***************************************************************************

    //All the code below this line is invalid code for error reporting testing
    //Erase it in case you want to test the correct code and its runtime execution
    
    //***************************************************************************
    effect 
    {
        Name: ""Damage"",
        Params: 
        {
            Amount: Number
            Tur: Tur lll
            SHJ: String   
        }
        Name: """";,
        Action: (targets, context) =>
        {
            for (target in targets) 
            {
                while(i-- < Amount)
                {
                    target.Power -= 1+32*4-(35/7 - 3)/""2""@45 + 23-""asas"";
                };
            }
        }
    }
    card
    {
        Type: ""Oro"",
        Name: ""Beluga"",
        Faction: ""Northern Realms""
        Power: 10,
        Range: [],
        OnActivation:
        [
            {
                Effect: 
                {
                    Name: ""Damage"",
                    Amount: 5
                }
                Selector:
                {
                    Source: ""board"",
                    Single: false,
                    Predicate: (unit) => unit.Faction.Hand[2].Power[3] == ""Northern"" @@ ""Realms""^4
                }
                PostAction:
                {
                    Effect: ""Return to Deck"",
                    Post;;;
                };;;
            }
            {,,
            }
            {
                l;ssl;a.l;
            },
        ]
    }
    effect
    {
        Name:""Draw"",
        Action: (caballo,linche) => 
        {
            topCard = context.Deck.Pop(bottom);
            context.Hand.Push(topCard);
            context.Hand.Shuffle();
            i++;
            i+=false;
        }
    }
    

    // It's finally over :)
    
    ";
}