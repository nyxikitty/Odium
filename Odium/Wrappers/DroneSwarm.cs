using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Odium.Wrappers
{
    public static class DroneSwarmWrapper
    {
        public static bool isSwarmActive = false;
        private static GameObject targetObject = null;
        private static System.Random random = new System.Random();
        private static Dictionary<VRC.SDK3.Components.VRCPickup, Vector3> droneOffsets = new Dictionary<VRC.SDK3.Components.VRCPickup, Vector3>();
        private static float updateInterval = 0.1f;
        private static float lastUpdateTime = 0f;
        private static float swarmRadius = 1.5f;
        private static float maxSpeed = 0.5f;
        private static float minDistanceBetweenDrones = 0.5f;
        private static float verticalOffset = 0.5f;
        public static void StartDroneSwarm(GameObject target, float radius = 1.5f, float yOffset = 0.5f)
        {
            if (target == null)
            {
                MelonLogger.Msg("[DroneSwarm] Target object is null!");
                return;
            }

            targetObject = target;
            swarmRadius = radius;
            verticalOffset = yOffset;
            isSwarmActive = true;
            droneOffsets.Clear();

            var drones = DroneWrapper.GetDrones();
            foreach (var drone in drones)
            {
                Vector3 randomOffset = UnityEngine.Random.onUnitSphere * UnityEngine.Random.Range(0.5f, swarmRadius);
                droneOffsets[drone] = randomOffset;
            }

            MelonLogger.Msg($"[DroneSwarm] Started swarm with {drones.Count} drones around {target.name} with {yOffset} vertical offset");
        }

        public static void StopDroneSwarm()
        {
            isSwarmActive = false;
            targetObject = null;
            MelonLogger.Msg("[DroneSwarm] Stopped swarm");
        }

        public static void UpdateDroneSwarm()
        {
            if (!isSwarmActive || targetObject == null)
                return;

            if (Time.time - lastUpdateTime < updateInterval)
                return;

            lastUpdateTime = Time.time;

            var drones = DroneWrapper.GetDrones();
            if (drones.Count == 0)
                return;

            Vector3 targetPosition = targetObject.transform.position;
            targetPosition.y += verticalOffset;

            foreach (var drone in drones)
            {
                if (drone == null || drone.gameObject == null)
                    continue;

                if (!droneOffsets.ContainsKey(drone))
                {
                    droneOffsets[drone] = UnityEngine.Random.onUnitSphere * UnityEngine.Random.Range(0.5f, swarmRadius);
                }

                Vector3 desiredPosition = targetPosition + droneOffsets[drone];

                Vector3 currentPosition = drone.transform.position;

                Vector3 direction = desiredPosition - currentPosition;

                float distance = direction.magnitude;
                if (distance > 0.01f)
                {
                    float speed = Mathf.Min(maxSpeed, distance);
                    direction = direction.normalized * speed;
                }

                direction += new Vector3(
                    (float)(random.NextDouble() - 0.5) * 0.05f,
                    (float)(random.NextDouble() - 0.5) * 0.05f,
                    (float)(random.NextDouble() - 0.5) * 0.05f
                );

                Vector3 newPosition = currentPosition + direction;

                foreach (var otherDrone in drones)
                {
                    if (otherDrone == drone || otherDrone == null)
                        continue;

                    float distanceToDrone = Vector3.Distance(newPosition, otherDrone.transform.position);
                    if (distanceToDrone < minDistanceBetweenDrones)
                    {
                        Vector3 avoidDirection = (newPosition - otherDrone.transform.position).normalized;
                        newPosition += avoidDirection * (minDistanceBetweenDrones - distanceToDrone) * 0.5f;
                    }
                }

                DroneWrapper.SetDronePosition(drone, newPosition);

                Vector3 lookDirection = (targetPosition - newPosition).normalized;
                lookDirection += new Vector3(
                    (float)(random.NextDouble() - 0.5) * 0.1f,
                    (float)(random.NextDouble() - 0.5) * 0.1f,
                    (float)(random.NextDouble() - 0.5) * 0.1f
                );

                Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
                DroneWrapper.SetDroneRotation(drone, lookRotation);
            }
        }

        public static void ChangeSwarmTarget(GameObject newTarget)
        {
            if (newTarget == null)
            {
                MelonLogger.Msg("[DroneSwarm] New target object is null!");
                return;
            }

            targetObject = newTarget;
            MelonLogger.Msg($"[DroneSwarm] Changed swarm target to {newTarget.name}");
        }

        public static void AdjustSwarmParameters(float radius = 1.5f, float speed = 0.5f, float minDistance = 0.5f, float yOffset = 0.5f)
        {
            swarmRadius = radius;
            maxSpeed = speed;
            minDistanceBetweenDrones = minDistance;
            verticalOffset = yOffset;

            if (isSwarmActive)
            {
                var drones = DroneWrapper.GetDrones();
                foreach (var drone in drones)
                {
                    droneOffsets[drone] = UnityEngine.Random.onUnitSphere * UnityEngine.Random.Range(0.5f, swarmRadius);
                }
            }

            MelonLogger.Msg($"[DroneSwarm] Parameters adjusted - Radius: {radius}, Speed: {speed}, MinDistance: {minDistance}, VerticalOffset: {yOffset}");
        }

        public static void SetVerticalOffset(float yOffset)
        {
            verticalOffset = yOffset;
            MelonLogger.Msg($"[DroneSwarm] Vertical offset set to: {yOffset}");
        }
    }
}
