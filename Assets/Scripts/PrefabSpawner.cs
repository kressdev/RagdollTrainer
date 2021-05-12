using UnityEngine;

namespace JKress.AITrainer
{
    public class PrefabSpawner : MonoBehaviour
    {
        [SerializeField] GameObject basePrefab;

        [SerializeField] int xCount = 1;
        [SerializeField] int zCount = 1;

        [SerializeField] float offsetX = 10f;
        [SerializeField] float offsetZ = 10f;

        GameObject scenePrefab;

        void Awake()
        {
            //If prefab is in the scene, remove it
            scenePrefab = GameObject.FindWithTag("agentPrefab");
            if (scenePrefab != null) Destroy(scenePrefab);

            //Spawn prefabs along x and z from basePrefab 
            for (int i = 0; i < xCount; i++)
            {
                for (int j = 0; j < zCount; j++)
                {
                    Instantiate(basePrefab, new Vector3(i * offsetX, 0, j * offsetZ),
                        Quaternion.identity);
                }
            }
        }
    }
}
