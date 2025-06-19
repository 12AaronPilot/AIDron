using UnityEngine;
using UnityEngine.UI;
using ChaseMacMillan.CurveDesigner;

public class SpeedSliderController : MonoBehaviour
{
    public Slider speedSlider;
    public float minSpeed = 1f;
    public float maxSpeed = 50f;

    void Start()
    {
        // Asegurar que el slider empiece con un valor medio si está vacío
        if (speedSlider != null)
            UpdateSpeed(speedSlider.value);
    }

    public void UpdateSpeed(float value)
    {
        float currentSpeed = Mathf.Lerp(minSpeed, maxSpeed, value);

        foreach (var follower in FindObjectsOfType<CurveFollower>())
        {
            follower.speed = currentSpeed;
        }
    }
}