using UnityEngine;
using UnityEngine.UI;
using ChaseMacMillan.CurveDesigner;

public class SpeedSliderController : MonoBehaviour
{
    public Slider speedSlider;    // Referencia al componente Slider de UI
    public float minSpeed = 1f;   // Velocidad m�nima para los drones
    public float maxSpeed = 50f; // Velocidad m�xima para los drones

    void Start()
    {
        // Al iniciar, ajustamos la velocidad seg�n el valor actual del slider
        if (speedSlider != null)
            UpdateSpeed(speedSlider.value);
    }

    // Este m�todo es llamado cada vez que se mueve el slider
    public void UpdateSpeed(float value)
    {
        // Interpolamos entre la velocidad m�nima y m�xima seg�n el valor del slider (0-1)
        float currentSpeed = Mathf.Lerp(minSpeed, maxSpeed, value);

        // Aplicamos la nueva velocidad a todos los objetos que siguen la curva
        foreach (var follower in FindObjectsOfType<CurveFollower>())
        {
            follower.speed = currentSpeed;
        }
    }
}
