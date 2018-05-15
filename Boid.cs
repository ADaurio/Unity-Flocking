using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flocking
{
    /// <summary>
    /// Applies flocking behaviour to a gameobject.
    /// </summary>
    public class Boid : MonoBehaviour
    {
        [Header("Neighbourhood")]
        public int maxNeighbours = 10;
        public float radius = 5;
        [Space(10)]
        [Header("Flocking Coefficients")]
        [Tooltip("Alignment - forces the boid to face the same angle as its neighbours.  Weighted against cohesion and separation")]
        public float alignment = 0.3f;
        [Tooltip("Cohesion - forces the boid to move towards the centroid of its neighbours.  Weighted against alignment and separation")]
        public float cohesion = 0.3f;
        [Tooltip("Separation - forces the boid to distance itself from its neighbours.  Weighted against alignment and cohesion")]
        public float separation = 0.3f;
        [Space(10)]
        [Header("Speed")]
        [Tooltip("Maximum turning speed in degrees per second")]
        public float steeringRate = 360;
        [Tooltip("Movement speed")]
        public float speed = 1;
        [Space(10)]
        [Header("Obstacle Avoidance")]
        [Tooltip("Time in seconds for boids to predict a collision")]
        public float localAvoidanceTime = 2;
        public LayerMask avoidanceLayers = 0;
        [Space(10)]
        [Tooltip("If not null, forces the boid to move towards this Transform")]
        public Transform goal;

        private void Update()
        {
            Vector3 pos = transform.position;
            Vector3 direction = transform.forward;

            if (goal && (goal.position - pos).sqrMagnitude > 0.1f)
                direction += (goal.position - pos).normalized;

            CalculateFlockDirection(ref direction);
            direction = direction.normalized;
            RaycastHit hit;

            if (avoidanceLayers.value != 0 && Physics.Linecast(pos, pos + transform.forward * localAvoidanceTime * speed, out hit, avoidanceLayers))
            {
                AvoidObstacle(ref direction, hit);
                direction = direction.normalized;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction, Quaternion.FromToRotation(transform.forward, direction) * transform.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, steeringRate * Time.deltaTime);
            transform.position += transform.forward * speed * Time.deltaTime;
        }

        /// <summary>
        /// Calculates typical flocking behaviour.  Deals with alignment, cohesion and separation.
        /// </summary>
        /// <param name="direction">Movement direction</param>
        private void CalculateFlockDirection(ref Vector3 direction)
        {
            Vector3 pos = transform.position;
            Collider[] nearby = Physics.OverlapSphere(pos, radius);
            Vector3 averageSeparationDirection = Vector3.zero;
            Vector3 averageTravelDirection = Vector3.zero;
            Vector3 centroid = Vector3.zero;
            int counter = 0;

            for (int i = 0; i < nearby.Length; i++)
            {
                Collider c = nearby[i];
                Vector3 sightDir = (c.transform.position - pos).normalized;

                if (Vector3.Angle(transform.forward, sightDir) > 135) continue;

                counter++;
                averageSeparationDirection -= sightDir;
                averageTravelDirection += c.transform.forward;
                centroid += c.transform.position;

                if (counter < maxNeighbours)
                    break;
            }

            centroid /= nearby.Length;
            averageTravelDirection /= nearby.Length;
            averageSeparationDirection /= nearby.Length;

            direction += averageSeparationDirection.normalized * separation;
            direction += averageTravelDirection.normalized * alignment;
            direction += (centroid - pos).normalized * cohesion;
        }

        /// <summary>
        /// Avoids the obstacle hit by a raycast.
        /// </summary>
        /// <param name="direction">Movement direction</param>
        /// <param name="hit">RaycastHit containing an obstacle hit</param>
        private void AvoidObstacle(ref Vector3 direction, RaycastHit hit)
        {
            Vector3 colliderPoint = hit.collider.transform.position;
            Vector3 closest = ClosestPointOnLine(colliderPoint, new Vector3(transform.position.x, colliderPoint.y, transform.position.z), transform.forward);
            closest.y = colliderPoint.y;
            Vector3 offset = closest - colliderPoint;
            Vector3 dir = offset.normalized;
            float magnitude = hit.collider.bounds.extents.x - offset.magnitude;
            direction = dir * 1 / (magnitude * magnitude);
        }

        /// <summary>
        /// Find the closest point on a line to a point.
        /// </summary>
        /// <param name="p">Desired point</param>
        /// <param name="lO">Origin of the line</param>
        /// <param name="lDir">Line direction (unit vector)</param>
        /// <returns></returns>
        private Vector3 ClosestPointOnLine(Vector3 p, Vector3 lO, Vector3 lDir)
        {
            lDir = lDir.normalized;
            Vector3 offset = p - lO;
            float dot = Vector3.Dot(lDir, offset);
            return lO + lDir * dot;
        }

        /// <summary>
        /// Displays the neighbourhood range when selected.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}