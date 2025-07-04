using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Odium.Components
{
    static class PhotonExtensions
    {
        public static void OpRaiseEvent(byte code, Il2CppSystem.Object customObject, RaiseEventOptions RaiseEventOptions, SendOptions sendOptions) => PhotonNetwork.Method_Public_Static_Boolean_Byte_Object_RaiseEventOptions_SendOptions_0(code, customObject, RaiseEventOptions, sendOptions);

        public static void OpRaiseEvent(byte code, object customObject, RaiseEventOptions RaiseEventOptions, SendOptions sendOptions)
        {
            Il2CppSystem.Object Object = Serializer.FromManagedToIL2CPP<Il2CppSystem.Object>(customObject);
            OpRaiseEvent(code, Object, RaiseEventOptions, sendOptions);
        }
        public static Il2CppSystem.Collections.Concurrent.ConcurrentDictionary<int, Player> GetAllPhotonPlayers()
        {
            return VRC.Player.prop_Player_0.prop_Player_1.prop_Room_0.prop_ConcurrentDictionary_2_Int32_Player_0;
        }

        public static byte[] Vector3ToBytes(Vector3 vector3)
        {
            byte[] array = new byte[12];
            Buffer.BlockCopy(BitConverter.GetBytes(vector3.x), 0, array, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(vector3.y), 0, array, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(vector3.z), 0, array, 8, 4);
            return array;
        }
    }
}
