using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStats : MonoBehaviour
{
    public Text generationText;
    public Text fitnessText;

    private int generationCount = 0;

    public void UpdateStats(int generation, float bestFitness)
    {
        generationCount = generation;
        generationText.text = "Generación: " + generationCount;
        fitnessText.text = "Mejor Fitness: " + bestFitness.ToString("F2");
    }
}