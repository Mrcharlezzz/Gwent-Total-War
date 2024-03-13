using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DragandDrop : MonoBehaviour
{
    public bool dragging =false;
    private bool isOverDropZone=false;
    public Vector2 startPosition;
    public GameObject dropZone;
    public bool alreadyplayed;
    private bool inPlayer1Turn=true;
    public GameObject player;
    GameObject gamemaster;
    void Awake()
    {
        player=GameObject.Find("player");
        gamemaster=GameObject.Find("gamemaster");
        alreadyplayed=false;
    }
    void Update()
    {
        if(dragging)
        {
            transform.position= new Vector2(Input.mousePosition.x,Input.mousePosition.y);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        isOverDropZone=true;
        dropZone=collision.gameObject;
        
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        isOverDropZone=false;
        dropZone=null;
    }
    public void StartDrag()
    {
        Debug.Log("StartDrag called. alreadyplayed: " + alreadyplayed);

        if(!alreadyplayed)
        {
            startPosition= transform.position;
            dragging=true;
            gamemaster.GetComponent<GameMaster>().dragging=true;
        }
        else{
            Debug.Log ("Cannot be dragged again");
        }

    }
    public void EndDrad()
    {
        
        
        
        if(!alreadyplayed)
        {
        dragging=false;
        gamemaster.GetComponent<GameMaster>().dragging=false;

        if(isOverDropZone)
        
        {
            player.GetComponent<Player>().PlayCard(gameObject,dropZone);
        }

        else
        {
            transform.position=startPosition;
        }
        isOverDropZone=false;
        }
    }
}

