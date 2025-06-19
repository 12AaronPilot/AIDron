using System;
using UnityEngine;

// Definimos una clase serializable para que Unity pueda mostrarla en el Inspector si es necesario
[System.Serializable]
public class NeuralNetwork
{
    // Estructura de la red: cuántas neuronas por capa
    public int[] layers;

    // Matriz que almacena los valores actuales de las neuronas en cada capa
    public float[][] neurons;

    // Matriz tridimensional que almacena los pesos entre las neuronas de una capa y la siguiente
    public float[][][] weights;

    // Constructor de la red neuronal, recibe la estructura de capas (por ejemplo: {10, 16, 3})
    public NeuralNetwork(int[] layers)
    {
        // Copia la estructura para asegurar que no se modifique desde fuera
        this.layers = (int[])layers.Clone();

        InitNeurons(); // Crea la estructura interna de neuronas
        InitWeights(); // Inicializa los pesos aleatorios entre neuronas
    }

    // Inicializa las neuronas de cada capa con valores por defecto (0)
    void InitNeurons()
    {
        neurons = new float[layers.Length][];
        for (int i = 0; i < layers.Length; i++)
            neurons[i] = new float[layers[i]]; // Una fila por capa, con tantas columnas como neuronas
    }

    // Inicializa los pesos entre neuronas con valores aleatorios
    void InitWeights()
    {
        weights = new float[layers.Length - 1][][]; // No hay pesos después de la última capa

        for (int i = 0; i < layers.Length - 1; i++)
        {
            weights[i] = new float[layers[i]][]; // Desde cada neurona de la capa actual
            for (int j = 0; j < layers[i]; j++)
            {
                weights[i][j] = new float[layers[i + 1]]; // ... hasta cada neurona de la siguiente capa
                for (int k = 0; k < layers[i + 1]; k++)
                {
                    // Valor aleatorio entre -1 y 1 (se puede ajustar)
                    weights[i][j][k] = UnityEngine.Random.Range(-1f, 1f);
                }
            }
        }
    }

    // Realiza la propagación hacia adelante (feedforward) de la red neuronal
    public float[] FeedForward(float[] inputs)
    {
        // Verifica que la entrada tenga el tamaño correcto
        if (inputs.Length != layers[0])
        {
            Debug.LogError($"[NeuralNetwork] Input length mismatch. Esperado {layers[0]}, recibido {inputs.Length}.");
            return new float[layers[layers.Length - 1]];
        }

        // Copia las entradas en la primera capa de neuronas
        for (int i = 0; i < layers[0]; i++)
            neurons[0][i] = inputs[i];

        // Para cada capa (excepto la de entrada), calcular los valores activados
        for (int i = 1; i < layers.Length; i++)
        {
            for (int j = 0; j < layers[i]; j++)
            {
                float sum = 0f;

                // Suma ponderada de la capa anterior
                for (int k = 0; k < layers[i - 1]; k++)
                    sum += neurons[i - 1][k] * weights[i - 1][k][j];

                // Función de activación: tanh (va de -1 a 1, útil para salida suave)
                neurons[i][j] = (float)Math.Tanh(sum);
            }
        }

        // Devuelve la salida final (última capa)
        return neurons[layers.Length - 1];
    }

    // Clona esta red neuronal (se usa en la reproducción genética para copiar cerebros)
    public NeuralNetwork Clone()
    {
        NeuralNetwork clone = new NeuralNetwork(this.layers);

        for (int i = 0; i < weights.Length; i++)
            for (int j = 0; j < weights[i].Length; j++)
                for (int k = 0; k < weights[i][j].Length; k++)
                    clone.weights[i][j][k] = weights[i][j][k]; // Copiar cada peso

        return clone;
    }

    // Aplica mutaciones aleatorias a los pesos de la red
    public void Mutate(float mutationChance = 0.25f)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    if (UnityEngine.Random.value < mutationChance)
                    {
                        // Cambia el peso ligeramente
                        weights[i][j][k] += UnityEngine.Random.Range(-0.5f, 0.5f);
                    }
                }
            }
        }
    }
}