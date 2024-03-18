using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    public GameObject unitRows;
    public GameObject boostSlots;
    
    public void Clear()
    {
        foreach(Transform child in unitRows.transform)
        {
            child.gameObject.GetComponent<DropZone>().ZoneClear();
        }
        foreach(Transform child in boostSlots.transform)
        {
            child.gameObject.GetComponent<DropZone>().ZoneClear();
        }
    }
}

