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
        [SerializeField] LayerMask layerMask;
        [SerializeField] float rayDown = 10;

        public Vector2 spawnX = new Vector2(-10, 10); //The region in which a target can be spawned.
        public Vector2 spawnY = new Vector2(0.5f, 1.5f);
        public Vector2 spawnZ = new Vector2(-10, 10);

        public bool respawnIfTouched = true; //Should the target respawn to a different position when touched

        const string k_Agent = "agent";

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
            Vector3 newTargetPos;
            Collider[] hitColliders;

            do 
            {
                newTargetPos = new Vector3(Random.Range(spawnX.x, spawnX.y), rayDown, Random.Range(spawnZ.x, spawnZ.y));

                RaycastHit hit;
                if (Physics.Raycast(newTargetPos, Vector3.down, out hit, Mathf.Infinity, layerMask))
                {
                    newTargetPos.y = hit.point.y + Random.Range(spawnY.x, spawnY.y);
                }

                hitColliders = Physics.OverlapSphere(newTargetPos, transform.localScale.x / 2);

            } while (hitColliders.Length > 0);

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
