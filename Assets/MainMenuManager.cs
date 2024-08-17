using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadLevel(int sceneId)
    {
        SceneManager.LoadScene(sceneId);
    }

    public void QuitGame()
    {
        Debug.Log("Game is closed");
        Application.Quit(0);
    }
}
