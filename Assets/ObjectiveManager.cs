using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    
    public void UpdateObjectiveStatus() 
    {
        if(timer <= 0 && currentScore < scoreToWin)
        {
            //player loses
            Debug.Log("Objective Failed!");
            Invoke("DisplayObjectiveFailedPanel", gameOverDelay);
        }
        if(timer > 0 && currentScore >= scoreToWin)
        {
            //player wins
            Debug.Log("Objective Complete!");
            Invoke("DisplayObjectiveCompletedPanel", gameOverDelay);
        }
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
