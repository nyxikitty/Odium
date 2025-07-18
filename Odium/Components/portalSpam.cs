using Odium.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odium.Modules
{
    public class portalSpam
    {
        public static DateTime LastPortalSpawn = DateTime.Now;
        public static void OnUpdate()
        {
            if (ActionWrapper.portalSpamPlayer != null && ActionWrapper.portalSpam)
            {
                var currentTime = DateTime.Now;
                if ((currentTime - LastPortalSpawn).TotalMilliseconds >= 1)
                {
                    LastPortalSpawn = currentTime;
                    var transform = PlayerWrapper.GetBonePosition(ActionWrapper.portalSpamPlayer, UnityEngine.HumanBodyBones.Head);
                    transform.y -= 2f;
                    Portal.SpawnPortal(transform, "gghzak9f");
                }
            }
        }
    }
}
