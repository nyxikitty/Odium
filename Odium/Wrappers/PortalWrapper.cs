using ExitGames.Client.Photon;
using Odium.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Odium.Wrappers
{
    class PortalWrapper
    {
        public static void CreatePortal(string InstanceID, Vector3 Position, float Rotation)
        {
            if (InstanceID != null)
            {
                PhotonExtensions.RaiseEvent(70, new Dictionary<byte, object>
                {
                    {
                        0,
                        0
                    },
                    {
                        5,
                        InstanceID
                    },
                    {
                        6,
                        PhotonExtensions.SerializeVector3(Position)
                    },
                    {
                        7,
                        Rotation
                    }
                }, new RaiseEventOptions(), SendOptions.SendReliable);
            }
        }
    }
}
