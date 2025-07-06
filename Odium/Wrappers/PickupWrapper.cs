using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRC.SDKBase;

namespace Odium.Wrappers
{
    class PickupWrapper
    {

        public static List<VRC.SDK3.Components.VRCPickup> GetVRCPickups()
        {
            List<VRC.SDK3.Components.VRCPickup> pickups = new List<VRC.SDK3.Components.VRCPickup>();
            GameObject.FindObjectsOfType<VRC.SDK3.Components.VRCPickup>().ToList().ForEach(pickup =>
            {
                if (pickup.gameObject != null)
                {
                    pickups.Add(pickup);
                }
            });

            return pickups;
        }

        public static void DropAllPickupsInRange(float range)
        {
            var playerPosition = PlayerWrapper.LocalPlayer.transform.position;
            foreach (var pickup in GetVRCPickups())
            {
                if (pickup != null && pickup.gameObject != null)
                {
                    var distance = Vector3.Distance(playerPosition, pickup.transform.position);
                    if (distance <= range)
                    {
                        Networking.SetOwner(Networking.LocalPlayer, pickup.gameObject);
                        pickup.Drop();
                    }
                }
            }
        }

        public static void DropDronePickups()
        {
            var playerPosition = PlayerWrapper.LocalPlayer.transform.position;
            foreach (var pickup in GetVRCPickups())
            {
                if (pickup != null && pickup.gameObject != null && pickup.gameObject.name.Contains("Drone"))
                {
                    Networking.SetOwner(Networking.LocalPlayer, pickup.gameObject);
                    pickup.Drop();
                }
            }
        }

        public static void DropAllPickups()
        {
            foreach (var pickup in GetVRCPickups())
            {
                if (pickup != null && pickup.gameObject != null)
                {
                    Networking.SetOwner(Networking.LocalPlayer, pickup.gameObject);
                    pickup.Drop();
                }
            }
        }

        public static void BringAllPickupsToPlayer(VRC.Player player)
        {
            var playerPosition = player.transform.position;
            foreach (var pickup in GetVRCPickups())
            {
                if (pickup != null && pickup.gameObject != null)
                {
                    Networking.SetOwner(Networking.LocalPlayer, pickup.gameObject);
                    pickup.transform.position = playerPosition + Vector3.up * 0.5f;
                }
            }
        }

        public static List<VRC.SDK3.Components.VRCPickup> cachedPickups = new List<VRC.SDK3.Components.VRCPickup>();

        public static void HideAllPickups()
        {
            foreach (var pickup in GetVRCPickups())
            {
                if (pickup != null && pickup.gameObject != null)
                {
                    cachedPickups.Add(pickup);
                    pickup.gameObject.SetActive(false);
                }
            }
        }

        public static void ShowAllPickups()
        {
            foreach (var pickup in GetVRCPickups())
            {
                if (pickup != null && cachedPickups != null)
                {
                    pickup.gameObject.SetActive(true);
                }
            }
        }

        public static void RespawnAllPickups()
        {
            foreach (var pickup in GetVRCPickups())
            {
                if (pickup != null && pickup.gameObject != null)
                {
                    Networking.SetOwner(Networking.LocalPlayer, pickup.gameObject);
                    pickup.transform.position = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                }
            }
        }
    }
}
