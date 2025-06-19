using UnityEngine;
namespace ChaseMacMillan.CurveDesigner
{
    public class CurveFollower : MonoBehaviour
    {
        public Curve3D curve;
        public float distanceAlongCurve = 0;
        public float speed = 1;
        public void Update()
        {
            if (curve == null)
            {
                Debug.LogWarning($"{gameObject.name} no tiene la curva asignada.");
                return;
            }

            distanceAlongCurve += Time.deltaTime * speed;
            PointOnCurve point = curve.GetPointAtDistanceAlongCurve(distanceAlongCurve);
            transform.position = point.position;
            transform.rotation = Quaternion.LookRotation(point.tangent, point.reference);
        }
    }
}