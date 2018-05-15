using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flocking
{
    /// <summary>
    /// Spawns boids for the demo scene.  Unrelated to flocking.
    /// </summary>
    public class Spawner : MonoBehaviour
    {
        public float radius = 2;

        [Tooltip("Number of boids to spawn")]
        public int boidCount = 100;

        [Tooltip("Time between boid spawns")]
        public float spawnIncrement = 0.1f;

        public GameObject boidPrefab;

        [Tooltip("If not null, will force spawned boids to move towards the transform")]
        public Transform goal;

        private void Start()
        {
            StartCoroutine(SpawnBoids(spawnIncrement));
        }

        /// <summary>
        /// Spawn boids over time.
        /// </summary>
        /// <param name="delay">Amount of time between spawns.</param>
        private IEnumerator SpawnBoids(float delay)
        {
            for (int i = 0; i < boidCount; i++)
            {
                Vector3 spawnPoint = Random.insideUnitSphere;
                spawnPoint.z = Mathf.Abs(spawnPoint.z);
                GameObject g = Instantiate(boidPrefab, transform.TransformPoint(spawnPoint), Quaternion.LookRotation(spawnPoint, transform.up));

                if (goal)
                {
                    Boid b = g.GetComponent<Boid>();

                    if (b)
                        b.goal = goal;
                }

                yield return new WaitForSeconds(delay);
            }
        }

        /// <summary>
        /// Displays the spawn radius.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}