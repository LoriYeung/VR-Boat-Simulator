using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjectiveManager : MonoBehaviour
{



    [Header("Settings")]
    public int scoreToWin = 0;
    public bool hasTimer = false;

    public float gameOverDelay = 0.5f;

    public GameObject objectiveFailedPanel;
    public GameObject objectiveCompletedPanel;

    [Header("Objective info")]
    public int currentScore;
    public float timer = 1;
    public bool objectiveOver = false;

    [Header("UI References")]
    public TMP_Text timer_UI;
    
    private bool beginTimer = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //counts down the timer
        if(beginTimer && timer > 0){
            timer -= Time.deltaTime;
            timer_UI.text = "" + (int)timer;
        }

        UpdateObjectiveStatus();
    }

    public void SetTimer(float time) 
    {
        timer = time;
    }

    public void BeginTimer()
    {
        hasTimer = true;
        beginTimer = true;
    }

    public void BeginTimer(float time)
    {
        hasTimer = true;
        timer = time;
        beginTimer = true;
    }

    public void SetObjectiveOver(bool isOver)
    {
        objectiveOver = isOver;
    }
    
    public void UpdateObjectiveStatus() 
    {
        if(!objectiveOver)
        {
            if(timer <= 0 && currentScore < scoreToWin)
            {
                //player loses
                Debug.Log("Objective Failed!");
                Invoke("DisplayObjectiveFailedPanel", gameOverDelay);
                objectiveOver = true;
            }
            if(timer > 0 && currentScore >= scoreToWin)
            {
                //player wins
                Debug.Log("Objective Complete!");
                Invoke("DisplayObjectiveCompletedPanel", gameOverDelay);
                objectiveOver = true;
            }
        }
    }

    public void ResetObjective()
    {
        scoreToWin = 0;
        currentScore = 0;
        beginTimer = false;
        timer = 1;
        objectiveOver = false;
    }

    public void IncreaseScore(int scoreIncrease)
    {
        currentScore += scoreIncrease;
    }

    void DisplayObjectiveFailedPanel()
    {
        objectiveFailedPanel.SetActive(true);
    }

    void DisplayObjectiveCompletedPanel()
    {
        objectiveCompletedPanel.SetActive(true);
    }
}
