using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;                   // El objetivo que la c�mara debe seguir
    public Vector3 offset = new Vector3(0, 2, -5); // Desfase de posici�n para no estar encima del dron
    public CameraFollow cameraFollow;

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.LookAt(target);          // La c�mara siempre mira al dron
        }
    }
}
