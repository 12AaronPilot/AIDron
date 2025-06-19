using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Para usar funciones como .Average() y .Sort()
using ChaseMacMillan.CurveDesigner;

public class PopulationManager : MonoBehaviour
{
    // UI para mostrar estadísticas
    public TMPro.TextMeshProUGUI generationText;
    public TMPro.TextMeshProUGUI fitnessText;
    public TMPro.TextMeshProUGUI averageText;
    public TMPro.TextMeshProUGUI attemptsText;

    private int generation = 0;       // Contador de generaciones
    private int totalAttempts = 0;    // Cuántos intentos se han hecho en total (drones entrenados)

    public UIStats uiStats;           // Referencia al script que actualiza UI de generación y fitness
    private int generationCounter = 1;

    // Parámetros para crear y manejar los drones
    public GameObject dronePrefab;    // Prefab del dron
    public int populationSize = 30;   // Cuántos drones por generación
    public float trialTime = 30f;     // Cuánto dura cada época (segundos)
    public Curve3D curve;             // Curva que los drones deben seguir
    public CameraFollow cameraFollow; // Cámara que sigue al primer dron

    [HideInInspector] public List<GameObject> drones = new List<GameObject>(); // Lista de drones activos
    private float elapsedTime = 0;    // Tiempo acumulado desde que comenzó la época

    public List<DroneData> allData = new List<DroneData>(); // Almacena datos de fitness y cerebros de drones

    void Start()
    {
        CreateInitialPopulation(); // Al inicio, crear la primera generación
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= trialTime)
        {
            RegisterAllFitness();   // Evaluar todos los drones
            BreedNewGeneration();  // Crear nueva generación
            elapsedTime = 0;       // Reiniciar el tiempo
        }
    }

    // Crear la primera población de drones
    void CreateInitialPopulation()
    {
        for (int i = 0; i < populationSize; i++)
        {
            GameObject d = Instantiate(dronePrefab); // Crear dron
            d.name = "Drone_" + i;

            var controller = d.GetComponent<DroneController>();
            if (controller != null)
            {
                controller.brain = new NeuralNetwork(controller.networkLayers); // Asignar red neuronal nueva
            }

            var follower = d.GetComponent<CurveFollower>();
            if (follower != null && curve != null)
            {
                follower.curve = curve;
                follower.speed = 10f;
                follower.distanceAlongCurve = i * 2f; // Espaciarlos un poco

                // Posicionar sobre la curva
                PointOnCurve point = curve.GetPointAtDistanceAlongCurve(follower.distanceAlongCurve);
                d.transform.position = point.position;
                d.transform.rotation = Quaternion.LookRotation(point.tangent, point.reference);
            }

            drones.Add(d);

            // Hacer que la cámara siga al primero
            if (i == 0 && cameraFollow != null)
            {
                cameraFollow.target = d.transform;
            }
        }
    }

    // Crear una nueva generación de drones basados en los mejores anteriores
    void BreedNewGeneration()
    {
        // Ordenar por fitness (de mayor a menor)
        allData.Sort((a, b) => b.fitness.CompareTo(a.fitness));

        // Eliminar todos los drones actuales
        foreach (GameObject d in drones)
            Destroy(d);
        drones.Clear();

        List<NeuralNetwork> nextBrains = new List<NeuralNetwork>();

        DroneController refController = dronePrefab.GetComponent<DroneController>();
        int[] expectedLayout = refController != null ? refController.networkLayers : new int[] { 9, 10, 3 };

        // ELITISMO: mantener los mejores tal cual (sin mutar)
        for (int i = 0; i < 2 && i < allData.Count; i++)
        {
            NeuralNetwork elite = allData[i].brain.Clone();
            if (elite.layers[0] != expectedLayout[0])
                elite = new NeuralNetwork(expectedLayout);

            nextBrains.Add(elite);
        }

        // Resto: generar nuevos con mutación
        while (nextBrains.Count < populationSize)
        {
            NeuralNetwork parent = allData[Random.Range(0, Mathf.Min(allData.Count, populationSize / 2))].brain.Clone();

            if (parent.layers[0] != expectedLayout[0])
                parent = new NeuralNetwork(expectedLayout);

            parent.Mutate(0.1f); // Mutación ligera
            nextBrains.Add(parent);
        }

        // Crear nuevos drones con los cerebros nuevos
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

        generation++; // Nueva generación

        // Calcular estadísticas
        float bestFitness = allData.Count > 0 ? allData[0].fitness : 0f;
        float averageFitness = allData.Count > 0 ? allData.Average(d => d.fitness) : 0f;
        totalAttempts += populationSize;

        // Mostrar en UI
        if (generationText != null) generationText.text = $"Generación: {generation}";
        if (fitnessText != null) fitnessText.text = $"Fitness máximo: {bestFitness:F2}";
        if (averageText != null) averageText.text = $"Fitness promedio: {averageFitness:F2}";
        if (attemptsText != null) attemptsText.text = $"Intentos totales: {totalAttempts}";

        if (uiStats != null && allData.Count > 0)
        {
            uiStats.UpdateStats(generationCounter, allData[0].fitness);
        }
        generationCounter++;

        allData.Clear(); // Limpiar resultados anteriores

        if (cameraFollow != null && drones.Count > 0)
            cameraFollow.target = drones[0].transform;

        Debug.Log("Drones instanciados: " + drones.Count);
    }

    // Estructura para guardar el fitness y red neuronal de un dron
    [System.Serializable]
    public class DroneData
    {
        public NeuralNetwork brain;
        public float fitness;
    }

    // Registra un dron cuando termina (por colisión)
    public void RegisterFitness(DroneController drone)
    {
        DroneData data = new DroneData();
        data.fitness = drone.Fitness;
        data.brain = drone.brain.Clone();
        allData.Add(data);
    }

    // Al terminar la época, se registra el fitness de todos
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