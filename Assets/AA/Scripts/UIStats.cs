using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStats : MonoBehaviour
{
    public Text generationText;  // Texto UI para mostrar generación actual
    public Text fitnessText;     // Texto UI para mostrar el mejor fitness

    private int generationCount = 0;

    // Método público para actualizar la UI con los datos actuales
    public void UpdateStats(int generation, float bestFitness)
    {
        generationCount = generation;
        generationText.text = "Generación: " + generationCount;
        fitnessText.text = "Mejor Fitness: " + bestFitness.ToString("F2");
    }
}
