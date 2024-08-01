using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitEditor : MonoBehaviour
{
    public GameObject editorCanvas;
    public GameObject gameCanvas;

    public void ExitAndPlay()
    {
        editorCanvas.SetActive(false);
        gameCanvas.SetActive(true);
        GlobalContext.gameMaster.BeforeStart();
    }
}
