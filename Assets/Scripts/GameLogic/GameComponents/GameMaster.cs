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
    private int turn=1;
    private bool playerpassed=false;
    public GameObject turnUI;
    public GameObject WeatherSlots;
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

        
        GameObject.Find("Pass").GetComponent<Button>().enabled=false;
        turnUI.GetComponent<TurnUI>().playerText.text=$"{currentplayer.gameObject.name}";
        turnUI.SetActive(true);


    }
    public void NextTurn()
    {
        //Conditions for switching turns
        if(!playerpassed)
        {
            currentplayer.hand.gameObject.SetActive(false);
            GameObject.Find("Pass").GetComponent<Button>().enabled=false;

            var aux=currentplayer;
            currentplayer=notcurrentplayer;
            notcurrentplayer=aux;
            
            turnUI.GetComponent<TurnUI>().playerText.text=$"{currentplayer.gameObject.name}";
            turnUI.SetActive(true);
        }
        turn++;
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
            currentplayer=roundWinner;
        }

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
        //Pendiente implementar variante con UI message
    }
    public void DrawCards(Player player,int n)
    {
        for(int i=0;i<n;i++){
            player.DrawCard();
        }
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
