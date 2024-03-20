using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    public Player player1;
    public Player player2;
    public Player currentplayer;
    public Player notcurrentplayer;
    public Player Winner;
    public bool dragging;
    public bool globalModified=false;
    private int round=1;
    public int turn=0;
    public int selectionCount=0;
    private bool playerpassed=false;
    public GameObject turnUI;
    public GameObject message;
    public GameObject WeatherSlots;
    public Button pass;
    public void StartGame()
    {
        //Initializes the Game
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

        DrawCards(currentplayer,10);
        DrawCards(notcurrentplayer,10);
        
        pass.enabled=false;
        turnUI.GetComponent<TurnUI>().playerText.text=$"{currentplayer.gameObject.name} Selection";
        turnUI.SetActive(true);

        StartSelection();
        
        

        

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
            turnUI.GetComponent<TurnUI>().playerText.text=$"{currentplayer.gameObject.name}";
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
            message.GetComponent<Message>().text.text=$"Round Winner:\n{roundWinner.gameObject.name}";
            
        }
        else{
            message.GetComponent<Message>().text.text=$"Tied Round";
        }
        
        pass.enabled=false;
        message.SetActive(true);

        player1.roundupdate=true;
        player2.roundupdate=true;

        player1.PlayerClear();
        player2.PlayerClear();
        foreach (Transform child in WeatherSlots.transform)
        {
            child.gameObject.GetComponent<DropZone>().ZoneClear();
        }

        if((math.abs(player1.roundpoints-player2.roundpoints)==2))
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
        if(player1.roundpoints>player2.roundpoints)
        {
            message.GetComponent<Message>().text.text=$"PLAYER 1\nIS THE\nWINNER!!!";   
        }
        else if(player1.roundpoints<player2.roundpoints)
        {
            message.GetComponent<Message>().text.text=$"PLAYER 2\nIS THE\nWINNER!!!";
        }
        else{
            message.GetComponent<Message>().text.text=$"TIED UP";
        }
        pass.enabled=false;
        message.GetComponent<Message>().gameended=true;
        message.SetActive(true);
    }

    public void DrawCards(Player player,int n)
    {
        for(int i=0;i<n;i++){
            player.DrawCard();
        }
    }

    public void StartSelection()
    {
        //Card selection at the beggining of the game
        Debug.Log("startselect");
        Debug.Log($"current {currentplayer}");
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
        currentplayer.playerdeck.Shuffle();
        DrawCards(currentplayer,2);

        if(turn==0)
        {
            turn++;
            currentplayer.hand.gameObject.SetActive(false);
            PlayerSwitch();

            Debug.Log($"Turn {turn}");

            pass.enabled=false;
            turnUI.GetComponent<TurnUI>().playerText.text=$"{currentplayer.gameObject.name} Selection";
            turnUI.SetActive(true);
        }
        else{
            NextTurn();
        }
    }
    
    public void PlayerSwitch()
    {
        var aux=currentplayer;
        currentplayer=notcurrentplayer;
        notcurrentplayer=aux;
    }

    
    
    void Start()
    {
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
