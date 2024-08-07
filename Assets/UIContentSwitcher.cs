using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIContentSwitcher : MonoBehaviour
{

    [Header("Settings")]
    public bool pauseGameOnEnable = false;
    public GameObject[] contentObjects;

    int contentIndex = 0;

    public void OnEnable()
    {
        FreezeGame();
    }

    public void SwitchContents() 
    {
        contentObjects[contentIndex].SetActive(false);
        contentObjects[++contentIndex].SetActive(true);
    }

    public void SwitchBackwards()
    {
        contentObjects[contentIndex].SetActive(false);
        contentObjects[--contentIndex].SetActive(true);
    }

    public void FreezeGame() 
    {
        Time.timeScale = 0f;
    }

    public void UnFreezeGame() 
    {
        Time.timeScale = 1f;
    }
}
