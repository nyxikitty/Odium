using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Odium.Wrappers;

namespace Odium.Modules
{
    internal class portalTrap
    {
        public static DateTime LastPortalSpawn = DateTime.Now;
        public static void OnUpdate()
        {
            if (ActionWrapper.portalTrapPlayer != null && ActionWrapper.portalTrap)
            {
                var currentTime = DateTime.Now;
                if ((currentTime - LastPortalSpawn).TotalMilliseconds >= 500)
                {
                    LastPortalSpawn = currentTime;
                    var playerVelocity = PlayerWrapper.GetVelocity(ActionWrapper.portalTrapPlayer);
                    float speed = playerVelocity.magnitude;

                    if (speed > 2.5f)
                    {
                        LastPortalSpawn = currentTime;

                        Vector3 velocityDir = playerVelocity.normalized;
                        Vector3 currentPosition = PlayerWrapper.GetPosition(ActionWrapper.portalTrapPlayer);
                        Vector3 spawnPosition = currentPosition + velocityDir * 3f;

                        Portal.SpawnPortal(spawnPosition, "aywxh5ah");
                    }
                }
            }
        }
    }
}
