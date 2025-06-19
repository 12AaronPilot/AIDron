using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChaseMacMillan.CurveDesigner;

// Este script controla el comportamiento de cada dron, incluyendo movimiento,
// recolección de datos sensoriales, cálculo de fitness y comunicación con el gestor de población.

[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour
{
    public NeuralNetwork brain; // Red neuronal que toma decisiones
    public int[] networkLayers = new int[] { 10, 16, 3 }; // Estructura de capas de la red (entrada, oculta, salida)

    public float thrust = 20f; // Fuerza de impulso hacia adelante
    private Rigidbody rb; // Referencia al componente Rigidbody para física

    private float fitness = 0f; // Medida de desempeño del dron
    public float Fitness => fitness; // Propiedad para acceder al fitness externamente

    private float maxProgress = 0f; // Máxima distancia recorrida en la curva

    private Curve3D curve; // Referencia a la curva que debe seguir
    private CurveFollower follower; // Referencia al script que mueve el dron en la curva

    private bool hasCollided = false; // Marca si el dron ya chocó

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Si la red no existe o no coincide con el tamaño esperado, se crea una nueva
        if (brain == null || brain.layers.Length != networkLayers.Length || brain.layers[0] != networkLayers[0])
            brain = new NeuralNetwork(networkLayers);

        // Se obtiene la curva asociada al CurveFollower
        follower = GetComponent<CurveFollower>();
        if (follower != null)
        {
            curve = follower.curve;
        }
    }

    void FixedUpdate()
    {
        // Obtiene los datos de los sensores virtuales del dron
        float[] inputs = GetSensorData();

        // Calcula la salida de la red neuronal (decisión del dron)
        float[] output = brain.FeedForward(inputs);

        // Aplica movimiento en base a esa salida
        ApplyMovement(output);

        // Si avanzó más sobre la curva, actualiza progreso
        if (follower != null && follower.distanceAlongCurve > maxProgress)
            maxProgress = follower.distanceAlongCurve;

        // Penaliza si chocó
        float penalty = hasCollided ? 5f : 0f;

        // El fitness se calcula como distancia recorrida menos penalización
        fitness = maxProgress - penalty;
    }

    // Simula sensores con raycasts
    float[] GetSensorData()
    {
        float[] sensors = new float[10];

        // Direcciones de sensores (frontal, laterales, arriba, abajo, etc.)
        Vector3[] directions = new Vector3[]
        {
            transform.forward,
            Quaternion.Euler(0, -45, 0) * transform.forward,
            Quaternion.Euler(0, 45, 0) * transform.forward,
            -transform.right,
            transform.right,
            transform.up,
            -transform.up,
            Quaternion.Euler(-30, 0, 0) * transform.forward,
            Quaternion.Euler(30, 0, 0) * transform.forward
        };

        // Lanza raycasts en esas direcciones para detectar obstáculos
        for (int i = 0; i < directions.Length; i++)
        {
            if (Physics.Raycast(transform.position, directions[i], out RaycastHit hit, 20f))
            {
                sensors[i] = hit.distance / 20f; // Normaliza la distancia
                Debug.DrawRay(transform.position, directions[i] * hit.distance, Color.red);
            }
            else
            {
                sensors[i] = 1f; // Nada detectado
                Debug.DrawRay(transform.position, directions[i] * 20f, Color.red);
            }
        }

        // El último sensor mide qué tan alineado está el dron con la curva
        if (follower != null && curve != null)
        {
            Vector3 targetPoint = curve.GetPositionAtDistanceAlongCurve(follower.distanceAlongCurve + 2f);
            Vector3 directionToCurve = (targetPoint - transform.position).normalized;
            float alignment = Vector3.Dot(transform.forward, directionToCurve);
            sensors[9] = (alignment + 1f) / 2f; // Convierte a rango [0,1]
        }
        else
        {
            sensors[9] = 0.5f; // Valor neutral si no hay curva
        }

        return sensors;
    }

    // Aplica fuerzas físicas para mover el dron en base a la red neuronal
    void ApplyMovement(float[] output)
    {
        Vector3 torque = new Vector3(output[0], output[1], 0f) * 50f;
        rb.AddTorque(torque); // Gira el dron

        Vector3 forward = transform.forward * (output[2] + 1f) * thrust;
        rb.AddForce(forward); // Lo impulsa hacia adelante
    }

    // Si el dron choca, se detiene y reporta su fitness al manager
    void OnCollisionEnter(Collision other)
    {
        fitness -= 1f; // Penalización adicional
        hasCollided = true;
        enabled = false; // Detiene el comportamiento del dron

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Informa al gestor de población sobre su resultado
        PopulationManager manager = FindObjectOfType<PopulationManager>();
        if (manager != null)
            manager.RegisterFitness(this);
    }
}

