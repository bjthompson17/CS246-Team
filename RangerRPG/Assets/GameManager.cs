using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static bool GamePaused = false;
    public static bool GameOver = false;

    public GameObject PausePanel;
    public GameObject GameOverPanel;
    
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            if(GamePaused) {
                Resume();
            } else {
                Pause();
            }
        }

        if(GameOver) {
            gameObject.SetActive(false);
            GameOverPanel.SetActive(true);
        }
    }

    public void Resume() {
        if(PausePanel)
            PausePanel.SetActive(false);
        GamePaused = false;
        Time.timeScale = 1;
    }

    public void Pause() {
        if(PausePanel)
            PausePanel.SetActive(true);
        GamePaused = true;
        Time.timeScale = 0;
    }
}
