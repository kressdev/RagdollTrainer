using UnityEngine;

namespace JKress.AITrainer
{
    public class Stabilizer : MonoBehaviour
    {
        public float uprightTorque = 1000f;

        public AnimationCurve uprightTorqueFunction;

        Rigidbody rb;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            var uprightAngle = Vector3.Angle(transform.up, Vector3.up) / 180;
            var balancePercent = uprightTorqueFunction.Evaluate(uprightAngle);
            var uprightTorqueVal = balancePercent * uprightTorque;

            var rot = Quaternion.FromToRotation(transform.up, Vector3.up);
            rb.AddTorque(new Vector3(rot.x, rot.y, rot.z) * uprightTorqueVal);
        }
    }
}
