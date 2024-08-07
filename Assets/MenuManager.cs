using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MenuManager : MonoBehaviour
{

    //General functions for menu switching and scenes

    public void LoadMainMenu() 
    {
        SceneManager.LoadScene(0);
    }
}
