using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using VRC.SDKBase;

namespace Odium.Wrappers
{
    class DroneWrapper
    {
        public static VRC.PlayerDrone.DroneManager DroneManager;
        public static int DroneViewId = PlayerWrapper.ActorId + 10001;

        public static VRC.PlayerDrone.DroneManager GetDroneManager()
        {
            return GameObject.Find("UIManager/DroneManager").GetComponent<VRC.PlayerDrone.DroneManager>();
        }

        public static string GetDroneID(VRC.SDK3.Components.VRCPickup Drone)
        {
            var droneController = Drone.gameObject.GetComponent<VRC.PlayerDrone.DroneController>();
            return droneController.field_Private_String_0;
        }

        public static void RemoveDrone(string id)
        {
            GetDroneManager().Method_Private_Void_String_PDM_0(id);
        }

        /*
        public static void SpawnDrone()
        {
            // VRC.PlayerDrone.DroneFlightController remove
            // VRC.PlayerDrone.DroneController disable

            GetDroneManager().Method_Public_Virtual_Final_New_Void_Action_1_InterfacePublicAbstractTrGaStInBoSiVeSiObBoUnique_Action_1_LocalizableString_0(null, null);
            
            
            // Method_Public_Virtual_Final_New_Void_Action_1_InterfacePublicAbstractTrGaStInBoSiVeSiObBoUnique_Action_1_LocalizableString_0
        }
        */

        public static void SpawnDrone(Vector3 position, Vector3 Rotation)
        {
            // Dictionary Field 128 = viewID
            var vrcPlayer = PlayerWrapper.LocalPlayer._vrcplayer;
            //int viewID = vrcPlayer.Method_Public_Int32_0();
            int viewID = vrcPlayer.Method_Public_Int32_0();
            vrcPlayer.Method_Public_Int32_0();

            GetDroneManager().Method_Private_Void_Player_Vector3_Vector3_String_Int32_Color_Color_Color_PDM_0(PlayerWrapper.LocalPlayer, position, Rotation, Guid.NewGuid().ToString(), viewID, Color.black, Color.black, Color.black);
            //DroneViewId += 2;
        }

        public static List<VRC.SDK3.Components.VRCPickup> GetDrones()
        {
            List<VRC.SDK3.Components.VRCPickup> drones = new List<VRC.SDK3.Components.VRCPickup>();
            GameObject.FindObjectsOfType<VRC.SDK3.Components.VRCPickup>().ToList().ForEach(pickup =>
            {
                if (pickup.gameObject.name.Contains("VRCDrone"))
                {
                    drones.Add(pickup);
                }
            });

            return drones;
        }

        public static void DroneCrash()
        {
            for (int i = 0; i < GetDrones().Count; i++)
            {
                Networking.SetOwner(Networking.LocalPlayer, GetDrones()[i].gameObject);
                GetDrones()[i].gameObject.transform.position = new Vector3(222222399999, 0);
            }
        }

        public static void SetDronePosition(VRC.SDK3.Components.VRCPickup drone, Vector3 vector3)
        {
            Networking.SetOwner(Networking.LocalPlayer, drone.gameObject);
            drone.gameObject.transform.position = vector3;
        }

        public static void SetDroneRotation(VRC.SDK3.Components.VRCPickup drone, Quaternion quaternion)
        {
            Networking.SetOwner(Networking.LocalPlayer, drone.gameObject);
            drone.gameObject.transform.rotation = quaternion;
        }
    }
}
