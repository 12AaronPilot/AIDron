using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Necesario para usar componentes de UI como Button
using UnityEngine.SceneManagement; // Para poder recargar (reiniciar) escenas

public class UIManager : MonoBehaviour
{
    // Referencia al script PopulationManager, que controla a los drones y las generaciones
    public PopulationManager populationManager;

    // Botones de la interfaz que se configuran desde el Canvas en el editor
    public Button startButton, pauseButton, resetButton, exitButton;

    // Variable booleana para llevar el control de si el juego est� en pausa o no
    private bool isPaused = false;

    void Start()
    {
        // En cuanto arranca la escena, se asignan los m�todos que se ejecutar�n
        // cuando el jugador haga clic en cada bot�n:

        // Este bot�n reanuda la simulaci�n
        startButton.onClick.AddListener(StartSimulation);

        // Este bot�n pausa o reanuda la simulaci�n, alternando entre ambos estados
        pauseButton.onClick.AddListener(TogglePause);

        // Este bot�n reinicia completamente la escena actual
        resetButton.onClick.AddListener(ResetSimulation);

        // Este bot�n cierra la simulaci�n o la detiene si est�s dentro del editor
        exitButton.onClick.AddListener(ExitSimulation);
    }

    void StartSimulation()
    {
        // Asegura que el juego est� corriendo (no pausado)
        Time.timeScale = 1f;
    }

    void TogglePause()
    {
        // Cambia el valor de pausa. Si estaba en pausa, lo quita; si no, lo activa.
        isPaused = !isPaused;

        // Detiene o reanuda el tiempo del juego cambiando la escala del tiempo
        Time.timeScale = isPaused ? 0f : 1f;
    }

    void ResetSimulation()
    {
        // Por si estaba pausado, vuelve a activar el tiempo
        Time.timeScale = 1f;

        // Reinicia la escena actual (usando su �ndice en el build settings)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ExitSimulation()
    {
        // Esta secci�n cierra el juego 

        // #if UNITY_EDITOR
        // Esta condici�n especial se usa para saber si estamos en el Editor de Unity
        // Es una "directiva de preprocesador", lo que significa que este bloque se compila
        // solo si Unity est� ejecutando el c�digo en modo Editor (no en el build final).
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Detiene el modo Play en el editor

        // #else
        // Si NO estamos en el editor, se ejecutar� el bloque de abajo:
#else
        Application.Quit(); // Cierra completamente la aplicaci�n si es un juego compilado (EXE, WebGL, etc.)

        // #endif
        // Fin de la condici�n. Solo uno de los dos bloques anteriores se ejecutar�.
#endif
    }
}
