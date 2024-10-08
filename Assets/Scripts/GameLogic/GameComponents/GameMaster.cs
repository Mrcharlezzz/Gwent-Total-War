using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    public Player player1;
    public Player player2;
    public Player currentplayer;
    public Player notcurrentplayer;
    public Player Winner;
    public Board board;
    public bool dragging;
    public bool globalModified=false;
    private int round=1;
    public int turn=0;
    public int selectionCount=0;
    private bool playerpassed=false;
    public GameObject turnUI;
    public GameObject message;
    public GameObject invalidEffectSign;
    public GameObject WeatherSlots;
    public GameObject cardPrefab;
    public Button pass;
    public void StartGame()
    {
        //Initializes the Game
        GlobalContext.gameMaster = this;
        int randomPlayer=UnityEngine.Random.Range(0, 2); //Generates a random number 0 or 1
        if (randomPlayer==0)
        {
            currentplayer=player1;
            notcurrentplayer=player2;
        }
        else
        {
            currentplayer=player2;
            notcurrentplayer=player1;
        }
        
        currentplayer.hand.gameObject.SetActive(false);
        notcurrentplayer.hand.gameObject.SetActive(false);

        player1.deck.Shuffle();
        player2.deck.Shuffle();

        DrawCards(currentplayer,10);
        DrawCards(notcurrentplayer,10);
        
        pass.enabled=false;
        turnUI.GetComponent<TurnUI>().playerText.text=SpanishTranslate($"{currentplayer.gameObject.name}");
        turnUI.GetComponent<TurnUI>().selectiontext.enabled=true;
        turnUI.SetActive(true);

    }
    public void NextTurn()
    {
        //Conditions for switching turns
        turn++;
        if(!playerpassed)
        {
            currentplayer.hand.gameObject.SetActive(false);
            PlayerSwitch();

            pass.enabled=false;
            turnUI.GetComponent<TurnUI>().playerText.text=SpanishTranslate($"{currentplayer.gameObject.name}");
            turnUI.SetActive(true);
        }
        
    }
    public void Pass()
    {
        //Logic for the round after pass
        if(!playerpassed)
        {    
            
            NextTurn();
            Debug.Log("Passed");
            playerpassed=true;
        }
        else{
            EndRound();
            playerpassed=false;
        } 
    }
    public void EndRound()
    {
        Player roundWinner=null;
        if(player1.totalpower>player2.totalpower)
        {
            roundWinner=player1;
            player1.roundpoints++;
        }
        else if(player1.totalpower<player2.totalpower)
        {
            roundWinner=player2;
            player2.roundpoints++;
        }
        else
        {
            player1.roundpoints++;
            player2.roundpoints++;
        }


        if(roundWinner!=null)
        {
            if(currentplayer!=roundWinner)
            {
                PlayerSwitch();
            }
            message.GetComponent<Message>().text.text=SpanishTranslate($"{roundWinner.gameObject.name}");
            
        }
        else{
            message.GetComponent<Message>().text.text=$"Empate";
            message.GetComponent<Message>().winnertext.SetActive(false);
        }
        
        pass.enabled=false;
        message.SetActive(true);
        
        player1.leaderCard.alreadyused=false;
        player1.roundupdate=true;
        player2.roundupdate=true;

        player1.PlayerClear();
        player2.PlayerClear();
        foreach (Transform child in WeatherSlots.transform)
        {
            child.gameObject.GetComponent<DropZone>().ZoneClear();
        }

        if(math.abs(player1.roundpoints-player2.roundpoints)==2)
        {
            EndGame();
        }
        else if(round==3)
        {            
            EndGame();
        }
        round++;
        StartRound();
    }
    public void StartRound()
    {
        DrawCards(player1,2);
        DrawCards(player2,2);
    }
    public void EndGame()
    {
        //In case of Draw result in the last round winnertext never turns on
        message.GetComponent<Message>().winnertext.SetActive(true); 
        message.GetComponent<Message>().winnertext.GetComponent<TextMeshProUGUI>().fontSize=50;
        message.GetComponent<Message>().text.fontSize=50; 
        message.GetComponent<Message>().winnertext.GetComponent<TextMeshProUGUI>().text="LA GUERRA LA HA GANADO:";
        if(player1.roundpoints>player2.roundpoints)
        {
            message.GetComponent<Message>().text.text="JUGADOR 1!!!";   
        }
        else if(player1.roundpoints<player2.roundpoints)
        {
            message.GetComponent<Message>().text.text="JUGADOR 2!!!";
        }
        else{
            message.GetComponent<Message>().winnertext.SetActive(false);
            message.GetComponent<Message>().text.text=$"... solo muertes innecesarias\n Empate";
        }
        pass.enabled=false;
        message.GetComponent<Message>().gameended=true;
        message.SetActive(true);
    }


    public void StartSelection()
    {
        //Card selection at the beggining of the game

        pass.enabled=false;
        foreach(Transform card in currentplayer.hand.gameObject.transform)
        {
            card.gameObject.GetComponent<DragandDrop>().enabled=false;
            card.gameObject.GetComponent<Button>().enabled=true;
        }   
    }
    public void EndSelection()
    {
        Debug.Log("End");
        Debug.Log($"turn {turn}");
        pass.enabled=true;
        foreach(Transform card in currentplayer.hand.gameObject.transform)
        {
            card.gameObject.GetComponent<DragandDrop>().enabled=true;
            card.gameObject.GetComponent<Button>().enabled=false;
        }
        currentplayer.deck.Shuffle(); 
        DrawCards(currentplayer,2);

        if(turn==0)
        {
            turn++;
            currentplayer.hand.gameObject.SetActive(false);
            PlayerSwitch();

            Debug.Log($"Turn {turn}");

            pass.enabled=false;
            turnUI.GetComponent<TurnUI>().playerText.text=SpanishTranslate($"{currentplayer.gameObject.name}");
            turnUI.GetComponent<TurnUI>().selectiontext.enabled=true;
            turnUI.SetActive(true);
        }
        else{
            NextTurn();
        }
    }
    public void DrawCards(Player player,int n)
    {
        for(int i=0;i<n;i++){
            player.DrawCard();
        }
    }
    
    public void PlayerSwitch()
    {
        var aux=currentplayer;
        currentplayer=notcurrentplayer;
        notcurrentplayer=aux;
    }
    
    public void ModifyZones(){
        foreach (Transform zone in player1.field.unitRows.transform){
            zone.gameObject.GetComponent<FieldDropZone>().Modify();
        }
        foreach (Transform zone in player2.field.unitRows.transform){
            zone.gameObject.GetComponent<FieldDropZone>().Modify();
        }
    }
    
    public void SetDecks(){
        List<string> debug=Database.deck1.Concat(Database.deck2).Select(t => t.name).ToList();
        
        foreach(Card card in Database.deck1){
            if(card is Leader leader) player1.leaderCard.leader=leader;
            else player1.deck.Push(card);
            card.owner=1;
        }
        if(player1.leaderCard.leader.id==-1){
            player1.leaderCard.leader=new Leader(Database.Count, 1, "Sin líder", "DefaultImage", Card.Type.Leader, "Facción sin líder", "Neutral", new List<Card.Position>(){}, new Onactivation(new List<EffectActivation>()));
            Database.deck1.Add(player1.leaderCard.leader);
        }
        player1.leaderCard.ShowLeader();

        foreach (Card card in Database.deck2){
            if(card is Leader leader) player2.leaderCard.leader=leader;
            else player2.deck.Push(card);
            card.owner=2;
        }
        if (player2.leaderCard.leader.id==-1){
            player2.leaderCard.leader=new Leader(Database.Count, 2, "Sin líder", "DefaultImage", Card.Type.Leader, "Facción sin líder", "Neutral", new List<Card.Position>(){}, new Onactivation(new List<EffectActivation>()));
            Database.deck2.Add(player2.leaderCard.leader);
        }
        player2.leaderCard.ShowLeader();
    }

    string SpanishTranslate(string a)
    {
        switch (a)
        {
            case "player1": return "Jugador 1";
            case "player2": return "Jugador 2";
            default: return a;
        }
    }
    
    public void BeforeStart()
    {
        GlobalContext.gameMaster = this;
        GameTools.cardPrefab=cardPrefab;

        Database.Initialize();
        SetDecks();
        StartGame();
    } 
    void Update()
    {
        if(globalModified)
        {
            player1.fieldModified=true;
            player2.fieldModified=true;
            globalModified=false;
        }
    }   


}
