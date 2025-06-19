using UnityEngine;

// Este namespace es necesario porque el componente Curve3D y PointOnCurve están definidos en esa librería externa
namespace ChaseMacMillan.CurveDesigner
{
    // Este script se encarga de mover el objeto (por ejemplo, un dron) a lo largo de una curva 3D definida en la escena
    public class CurveFollower : MonoBehaviour
    {
        // Referencia a la curva que se debe seguir. Esta curva es de tipo Curve3D (una clase del plugin CurveDesigner)
        public Curve3D curve;

        // Posición actual a lo largo de la curva, expresada como una distancia
        public float distanceAlongCurve = 0;

        // Velocidad a la que se avanza por la curva. Este valor puede ser modificado en tiempo real (por ejemplo, desde un slider)
        public float speed = 1;

        // Este método se llama automáticamente cada frame
        public void Update()
        {
            // Si no se ha asignado ninguna curva, no hacer nada y mostrar advertencia
            if (curve == null)
            {
                Debug.LogWarning($"{gameObject.name} no tiene la curva asignada.");
                return;
            }

            // Avanzar la distancia a lo largo de la curva, considerando el tiempo y la velocidad actual
            distanceAlongCurve += Time.deltaTime * speed;

            // Obtener el punto exacto en la curva correspondiente a la distancia recorrida
            PointOnCurve point = curve.GetPointAtDistanceAlongCurve(distanceAlongCurve);

            // Mover el objeto a la posición del punto en la curva
            transform.position = point.position;

            // Ajustar la rotación del objeto para que mire en la dirección de la tangente de la curva
            // Esto hace que el dron se oriente correctamente mientras avanza
            transform.rotation = Quaternion.LookRotation(point.tangent, point.reference);
        }
    }
}