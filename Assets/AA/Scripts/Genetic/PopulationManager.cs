using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ChaseMacMillan.CurveDesigner;

public class PopulationManager : MonoBehaviour
{
    public TMPro.TextMeshProUGUI generationText;
    public TMPro.TextMeshProUGUI fitnessText;
    public TMPro.TextMeshProUGUI averageText;
    public TMPro.TextMeshProUGUI attemptsText;

    private int generation = 0;
    private int totalAttempts = 0;

    public UIStats uiStats;
    private int generationCounter = 1;

    public GameObject dronePrefab;
    public int populationSize = 30;
    public float trialTime = 30f;
    public Curve3D curve;
    public CameraFollow cameraFollow;

    [HideInInspector] public List<GameObject> drones = new List<GameObject>();
    private float elapsedTime = 0;

    public List<DroneData> allData = new List<DroneData>();

    void Start()
    {
        CreateInitialPopulation();
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= trialTime)
        {
            RegisterAllFitness();
            BreedNewGeneration();
            elapsedTime = 0;
        }
    }

    void CreateInitialPopulation()
    {
        for (int i = 0; i < populationSize; i++)
        {
            GameObject d = Instantiate(dronePrefab);
            d.name = "Drone_" + i;

            var controller = d.GetComponent<DroneController>();
            if (controller != null)
            {
                controller.brain = new NeuralNetwork(controller.networkLayers);
            }

            var follower = d.GetComponent<CurveFollower>();
            if (follower != null && curve != null)
            {
                follower.curve = curve;
                follower.speed = 10f;
                follower.distanceAlongCurve = i * 2f;

                PointOnCurve point = curve.GetPointAtDistanceAlongCurve(follower.distanceAlongCurve);
                d.transform.position = point.position;
                d.transform.rotation = Quaternion.LookRotation(point.tangent, point.reference);
            }

            drones.Add(d);

            if (i == 0 && cameraFollow != null)
            {
                cameraFollow.target = d.transform;
            }
        }
    }

    void BreedNewGeneration()
    {
        allData.Sort((a, b) => b.fitness.CompareTo(a.fitness));

        foreach (GameObject d in drones)
            Destroy(d);
        drones.Clear();

        List<NeuralNetwork> nextBrains = new List<NeuralNetwork>();

        DroneController refController = dronePrefab.GetComponent<DroneController>();
        int[] expectedLayout = refController != null ? refController.networkLayers : new int[] { 9, 10, 3 };

        // Elitismo
        for (int i = 0; i < 2 && i < allData.Count; i++)
        {
            NeuralNetwork elite = allData[i].brain.Clone();

            if (elite.layers[0] != expectedLayout[0])
                elite = new NeuralNetwork(expectedLayout);

            nextBrains.Add(elite);
        }

        // Resto con mutaciones
        while (nextBrains.Count < populationSize)
        {
            NeuralNetwork parent = allData[Random.Range(0, Mathf.Min(allData.Count, populationSize / 2))].brain.Clone();

            if (parent.layers[0] != expectedLayout[0])
                parent = new NeuralNetwork(expectedLayout);

            parent.Mutate(0.1f);
            nextBrains.Add(parent);
        }

        for (int i = 0; i < populationSize; i++)
        {
            Vector3 spawnPosition = transform.position + new Vector3(0, 0, i * 2f);
            GameObject d = Instantiate(dronePrefab, spawnPosition, Quaternion.identity);
            d.name = "Drone_" + i;

            var controller = d.GetComponent<DroneController>();
            if (controller != null)
            {
                var parentBrain = nextBrains[i];
                if (parentBrain.layers[0] != expectedLayout[0])
                {
                    Debug.LogWarning($"[NeuralNetwork] Input length mismatch. Esperado {expectedLayout[0]}, recibido {parentBrain.layers[0]}. Se regeneró.");
                    controller.brain = new NeuralNetwork(expectedLayout);
                }
                else
                {
                    controller.brain = parentBrain.Clone();
                }
            }

            var follower = d.GetComponent<CurveFollower>();
            if (follower != null && curve != null)
            {
                follower.curve = curve;
                follower.speed = 10f; 
                follower.distanceAlongCurve = i * 2f;

                var point = curve.GetPointAtDistanceAlongCurve(follower.distanceAlongCurve);
                d.transform.position = point.position;
                d.transform.rotation = Quaternion.LookRotation(point.tangent, point.reference);
            }

            drones.Add(d);

            if (i == 0 && cameraFollow != null)
                cameraFollow.target = d.transform;
        }

        generation++;

        float bestFitness = allData.Count > 0 ? allData[0].fitness : 0f;
        float averageFitness = allData.Count > 0 ? allData.Average(d => d.fitness) : 0f;
        totalAttempts += populationSize;

        if (generationText != null) generationText.text = $"Generación: {generation}";
        if (fitnessText != null) fitnessText.text = $"Fitness máximo: {bestFitness:F2}";
        if (averageText != null) averageText.text = $"Fitness promedio: {averageFitness:F2}";
        if (attemptsText != null) attemptsText.text = $"Intentos totales: {totalAttempts}";

        if (uiStats != null && allData.Count > 0)
        {
            uiStats.UpdateStats(generationCounter, allData[0].fitness);
        }
        generationCounter++;

        allData.Clear();

        if (cameraFollow != null && drones.Count > 0)
            cameraFollow.target = drones[0].transform;

        Debug.Log("Drones instanciados: " + drones.Count);
    }

    [System.Serializable]
    public class DroneData
    {
        public NeuralNetwork brain;
        public float fitness;
    }

    public void RegisterFitness(DroneController drone)
    {
        DroneData data = new DroneData();
        data.fitness = drone.Fitness;
        data.brain = drone.brain.Clone();
        allData.Add(data);
    }

    void RegisterAllFitness()
    {
        allData.Clear();

        foreach (GameObject d in drones)
        {
            var c = d.GetComponent<DroneController>();
            if (c != null)
                RegisterFitness(c);
        }
    }
}