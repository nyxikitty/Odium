using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using VRC.SDKBase;

// Haters will say this is fake
namespace Odium.Components
{
    /// <summary>
    /// Extension methods for Photon networking operations
    /// </summary>
    public static class PhotonExtensions
    {
        /// <summary>
        /// Raises a Photon event with IL2CPP object
        /// </summary>
        public static void RaiseEvent(byte eventCode, Il2CppSystem.Object eventData,
            RaiseEventOptions options, SendOptions sendOptions)
        {
            PhotonNetwork.Method_Public_Static_Boolean_Byte_Object_RaiseEventOptions_SendOptions_0(
                eventCode, eventData, options, sendOptions);
        }

        /// <summary>
        /// Raises a Photon event with managed object (automatically converted to IL2CPP)
        /// </summary>
        public static void RaiseEvent(byte eventCode, object eventData,
            RaiseEventOptions options, SendOptions sendOptions)
        {
            var il2CppObject = Serializer.FromManagedToIL2CPP<Il2CppSystem.Object>(eventData);
            RaiseEvent(eventCode, il2CppObject, options, sendOptions);
        }

        /// <summary>
        /// Gets all players in the current Photon room
        /// </summary>
        public static Il2CppSystem.Collections.Concurrent.ConcurrentDictionary<int, Player> GetAllPlayers()
        {
            return VRC.Player.prop_Player_0.prop_Player_1.prop_Room_0.prop_ConcurrentDictionary_2_Int32_Player_0;
        }

        /// <summary>
        /// Sends a low-level Photon event with custom delivery options
        /// </summary>
        public static void SendLowLevelEvent(byte eventCode, object payload,
            byte channel = 8, bool encrypt = true, bool reliable = false)
        {
            if (Networking.LocalPlayer == null) return;

            try
            {
                var parameters = new ParameterDictionary();
                parameters.Add(244, new StructWrapper<byte>(Pooling.Readonly)
                {
                    value = eventCode,
                });
                parameters.Add(245, Serializer.FromManagedToIL2CPP<Il2CppSystem.Object>(payload));

                var sendOptions = new SendOptions
                {
                    DeliveryMode = DeliveryMode.UnreliableUnsequenced,
                    Encrypt = encrypt,
                    Channel = channel,
                    Reliability = reliable
                };

                PhotonNetwork.field_Public_Static_LoadBalancingClient_0
                    .field_Private_LoadBalancingPeer_0
                    .SendOperation(253, parameters, sendOptions);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to send low-level event: {ex.Message}");
            }
        }

        /// <summary>
        /// Converts a Vector3 to a byte array for network transmission
        /// </summary>
        public static byte[] SerializeVector3(Vector3 vector)
        {
            var bytes = new byte[12];
            Buffer.BlockCopy(BitConverter.GetBytes(vector.x), 0, bytes, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(vector.y), 0, bytes, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(vector.z), 0, bytes, 8, 4);
            return bytes;
        }

        /// <summary>
        /// Converts a byte array back to a Vector3
        /// </summary>
        public static Vector3 DeserializeVector3(byte[] bytes)
        {
            if (bytes?.Length != 12)
                throw new ArgumentException("Byte array must be exactly 12 bytes for Vector3 deserialization");

            return new Vector3(
                BitConverter.ToSingle(bytes, 0),
                BitConverter.ToSingle(bytes, 4),
                BitConverter.ToSingle(bytes, 8)
            );
        }
    }
}