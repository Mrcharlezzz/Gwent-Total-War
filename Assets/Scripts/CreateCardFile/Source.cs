public static class CompilationSource{
    public static string Source = @"
    effect{
        Name:""Draw"",
        Action:(context,targets) =>{
            topCard=context.Deck.Pop();
            context.Hand.Add(topCard);
            context.Hand.Shuffle();    
        }
    }
    ";
}