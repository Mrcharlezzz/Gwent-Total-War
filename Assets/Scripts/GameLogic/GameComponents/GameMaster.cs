using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    public Player player1;
    public Player player2;
    public Player currentplayer;
    public Player notcurrentplayer;
    public int[] roundpoints=new int[2];
    public Player Winner;
    public bool dragging;
    public bool globalModified=false;
    private int round=1;
    private int turn=0;
    public GameObject turnUI;
    public void StartGame()
    {
        //Initializes the Game
        int randomPlayer=Random.Range(0, 2); //Generates a random number 0 or 1
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

        turnUI.GetComponent<TurnUI>().playerText.text=$"{currentplayer.gameObject.name}";
        GameObject.Find("Pass").GetComponent<Button>().enabled=false;
        turnUI.SetActive(true);


    }
    public void NextTurn()
    {
        //Conditions for switching turns
    }
    public void Pass()
    {
        //Logic for the round after pass
    }
    public void EndRound()
    {

    }
    public void StartRound()
    {

    }
    public void EndGame()
    {

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
        
    }   


}
