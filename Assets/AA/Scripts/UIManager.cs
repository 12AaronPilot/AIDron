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

    // Variable booleana para llevar el control de si el juego está en pausa o no
    private bool isPaused = false;

    void Start()
    {
        // En cuanto arranca la escena, se asignan los métodos que se ejecutarán
        // cuando el jugador haga clic en cada botón:

        // Este botón reanuda la simulación
        startButton.onClick.AddListener(StartSimulation);

        // Este botón pausa o reanuda la simulación, alternando entre ambos estados
        pauseButton.onClick.AddListener(TogglePause);

        // Este botón reinicia completamente la escena actual
        resetButton.onClick.AddListener(ResetSimulation);

        // Este botón cierra la simulación o la detiene si estás dentro del editor
        exitButton.onClick.AddListener(ExitSimulation);
    }

    void StartSimulation()
    {
        // Asegura que el juego esté corriendo (no pausado)
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

        // Reinicia la escena actual (usando su índice en el build settings)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ExitSimulation()
    {
        // Esta sección cierra el juego 

        // #if UNITY_EDITOR
        // Esta condición especial se usa para saber si estamos en el Editor de Unity
        // Es una "directiva de preprocesador", lo que significa que este bloque se compila
        // solo si Unity está ejecutando el código en modo Editor (no en el build final).
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Detiene el modo Play en el editor

        // #else
        // Si NO estamos en el editor, se ejecutará el bloque de abajo:
#else
        Application.Quit(); // Cierra completamente la aplicación si es un juego compilado (EXE, WebGL, etc.)

        // #endif
        // Fin de la condición. Solo uno de los dos bloques anteriores se ejecutará.
#endif
    }
}
