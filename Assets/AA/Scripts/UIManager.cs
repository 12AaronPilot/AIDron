using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public PopulationManager populationManager;
    public Button startButton, pauseButton, resetButton, exitButton;

    private bool isPaused = false;

    void Start()
    {
        startButton.onClick.AddListener(StartSimulation);
        pauseButton.onClick.AddListener(TogglePause);
        resetButton.onClick.AddListener(ResetSimulation);
        exitButton.onClick.AddListener(ExitSimulation);
    }

    void StartSimulation()
    {
        Time.timeScale = 1f;
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }

    void ResetSimulation()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ExitSimulation()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
