using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChaseMacMillan.CurveDesigner;

[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour
{
    public NeuralNetwork brain;
    public int[] networkLayers = new int[] { 10, 16, 3 };

    public float thrust = 20f;
    private Rigidbody rb;

    private float fitness = 0f;
    public float Fitness => fitness;

    private float maxProgress = 0f;

    private Curve3D curve;
    private CurveFollower follower;

    private bool hasCollided = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (brain == null || brain.layers.Length != networkLayers.Length || brain.layers[0] != networkLayers[0])
            brain = new NeuralNetwork(networkLayers);

        follower = GetComponent<CurveFollower>();
        if (follower != null)
        {
            curve = follower.curve;
        }
    }

    void FixedUpdate()
    {
        float[] inputs = GetSensorData();
        float[] output = brain.FeedForward(inputs);
        ApplyMovement(output);

        if (follower != null && follower.distanceAlongCurve > maxProgress)
            maxProgress = follower.distanceAlongCurve;

        float penalty = hasCollided ? 5f : 0f;
        fitness = maxProgress - penalty;
    }

    float[] GetSensorData()
    {
        float[] sensors = new float[10];

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

        for (int i = 0; i < directions.Length; i++)
        {
            if (Physics.Raycast(transform.position, directions[i], out RaycastHit hit, 20f))
            {
                sensors[i] = hit.distance / 20f;
                Debug.DrawRay(transform.position, directions[i] * hit.distance, Color.red);
            }
            else
            {
                sensors[i] = 1f;
                Debug.DrawRay(transform.position, directions[i] * 20f, Color.red);
            }
        }

        if (follower != null && curve != null)
        {
            Vector3 targetPoint = curve.GetPositionAtDistanceAlongCurve(follower.distanceAlongCurve + 2f);
            Vector3 directionToCurve = (targetPoint - transform.position).normalized;
            float alignment = Vector3.Dot(transform.forward, directionToCurve);
            sensors[9] = (alignment + 1f) / 2f;
        }
        else
        {
            sensors[9] = 0.5f;
        }

        return sensors;
    }

    void ApplyMovement(float[] output)
    {
        Vector3 torque = new Vector3(output[0], output[1], 0f) * 50f;
        rb.AddTorque(torque);

        Vector3 forward = transform.forward * (output[2] + 1f) * thrust;
        rb.AddForce(forward);
    }

    void OnCollisionEnter(Collision other)
    {
        fitness -= 1f;
        hasCollided = true;
        enabled = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        PopulationManager manager = FindObjectOfType<PopulationManager>();
        if (manager != null)
            manager.RegisterFitness(this);
    }
}
