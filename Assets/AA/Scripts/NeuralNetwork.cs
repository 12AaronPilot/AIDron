using System;
using UnityEngine;

[System.Serializable]
public class NeuralNetwork
{
    public int[] layers; // número de neuronas por capa
    public float[][] neurons; // valores por neurona
    public float[][][] weights; // pesos entre capas

    public NeuralNetwork(int[] layers)
    {
        this.layers = (int[])layers.Clone();
        InitNeurons();
        InitWeights();
    }

    void InitNeurons()
    {
        neurons = new float[layers.Length][];
        for (int i = 0; i < layers.Length; i++)
            neurons[i] = new float[layers[i]];
    }

    void InitWeights()
    {
        weights = new float[layers.Length - 1][][];

        for (int i = 0; i < layers.Length - 1; i++)
        {
            weights[i] = new float[layers[i]][];
            for (int j = 0; j < layers[i]; j++)
            {
                weights[i][j] = new float[layers[i + 1]];
                for (int k = 0; k < layers[i + 1]; k++)
                {
                    weights[i][j][k] = UnityEngine.Random.Range(-1f, 1f);
                }
            }
        }
    }

    public float[] FeedForward(float[] inputs)
    {
        if (inputs.Length != layers[0])
        {
            Debug.LogError($"[NeuralNetwork] Input length mismatch. Esperado {layers[0]}, recibido {inputs.Length}.");
            return new float[layers[layers.Length - 1]];
        }


        if (inputs.Length != layers[0])
        {
            Debug.LogError($"[NeuralNetwork] Input length mismatch. Esperado {layers[0]}, recibido {inputs.Length}.");
            return new float[layers[layers.Length - 1]];
        }

        // Cargar las entradas
        for (int i = 0; i < layers[0]; i++)
            neurons[0][i] = inputs[i];

        // Forward Propagation
        for (int i = 1; i < layers.Length; i++)
        {
            for (int j = 0; j < layers[i]; j++)
            {
                float sum = 0f;
                for (int k = 0; k < layers[i - 1]; k++)
                    sum += neurons[i - 1][k] * weights[i - 1][k][j];

                neurons[i][j] = (float)Math.Tanh(sum);
            }
        }

        return neurons[layers.Length - 1];
    }

    public NeuralNetwork Clone()
    {
        NeuralNetwork clone = new NeuralNetwork(this.layers);

        for (int i = 0; i < weights.Length; i++)
            for (int j = 0; j < weights[i].Length; j++)
                for (int k = 0; k < weights[i][j].Length; k++)
                    clone.weights[i][j][k] = weights[i][j][k];

        return clone;
    }

    public void Mutate(float mutationChance = 0.25f)
    {
        for (int i = 0; i < weights.Length; i++)
            for (int j = 0; j < weights[i].Length; j++)
                for (int k = 0; k < weights[i][j].Length; k++)
                    if (UnityEngine.Random.value < mutationChance)
                        weights[i][j][k] += UnityEngine.Random.Range(-0.5f, 0.5f);
    }
}