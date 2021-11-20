using UnityEngine;
using Random = UnityEngine.Random;
using Unity.MLAgents;
using UnityEngine.Events;

namespace Unity.MLAgentsExamples
{
    /// <summary>
    /// Utility class to allow target placement and collision detection with an agent
    /// Add this script to the target you want the agent to touch.
    /// Callbacks will be triggered any time the target is touched with a collider tagged as 'tagToDetect'
    /// </summary>
    public class TargetController : MonoBehaviour
    {
        public float spawnRadius; //The radius in which a target can be randomly spawned.

        public bool respawnIfTouched; //Should the target respawn to a different position when touched

        const string k_Agent = "agent";

        // Start is called before the first frame update
        void Start() //OnEnable
        {
            //m_startingPos = transform.localPosition; //Use local position
            
            if (respawnIfTouched)
            {
                MoveTargetToRandomPosition();
            }
        }

        void FixedUpdate()
        {
            if (transform.localPosition.y < -5)
            {
                Debug.Log($"{transform.name} Fell Off Platform");
                MoveTargetToRandomPosition();
            }
        }

        /// <summary>
        /// Moves target to a random position within specified radius.
        /// </summary>
        public void MoveTargetToRandomPosition()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, transform.localScale.x / 2);
            Vector3 newTargetPos = Vector3.zero;

            while (hitColliders.Length > 0)
            {
                newTargetPos = Random.insideUnitSphere * spawnRadius;
                newTargetPos.y = Random.Range(0.6f, 1.4f);

                hitColliders = Physics.OverlapSphere(newTargetPos, transform.localScale.x / 2);
            }

            transform.localPosition = newTargetPos; //Use local position
        }

        private void OnCollisionEnter(Collision col)
        {
            if (col.transform.CompareTag(k_Agent))
            {
                if (respawnIfTouched)
                {
                    MoveTargetToRandomPosition();
                }
            }
        }
    }
}
