using HarmonyLib;
using Odium.UX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRC;
using VRC.PlayerDrone;

namespace Odium.Patches
{
    [HarmonyPatch(typeof(DroneManager))]
    class Drone
    {
        [HarmonyPrefix]
        [HarmonyPatch("Method_Private_Void_Player_Vector3_Vector3_String_Int32_Color_Color_Color_PDM_0")]
        static void OnDroneSpawnPrefix(DroneManager __instance,
                                     Player param_1,           // Player spawning the drone
                                     Vector3 param_2,          // Position
                                     Vector3 param_3,          // Rotation/Forward direction
                                     string param_4,           // Drone Id
                                     int param_5,              // View Id
                                     Color param_6,            // Primary color
                                     Color param_7,            // Secondary color
                                     Color param_8)            // Tertiary color
        {
            try
            {
                string playerName = param_1?.field_Private_APIUser_0?.displayName ?? "Unknown";

                InternalConsole.LogIntoConsole(
                    $"<color=#31BCF0>[DroneManager]:</color> <color=#00AAFF>Drone Spawn</color> by <color=#00FF74>{playerName}</color>, Position: <color=#FFAA00>({param_2.x}, {param_2.y}, {param_2.z})</color>, Direction: <color=#FFAA00>({param_3.x}, {param_3.y}, {param_3.z})</color>, UUID: <color=#FFAA00>{param_4}</color>, View ID: <color=#FFAA00>{param_5}</color>, Colors: <color=#{ColorUtility.ToHtmlStringRGB(param_6)}>■</color>, <color=#{ColorUtility.ToHtmlStringRGB(param_7)}>■</color>, <color=#{ColorUtility.ToHtmlStringRGB(param_8)}>■</color>"
                );
            }
            catch (Exception e)
            {
                InternalConsole.LogIntoConsole($" <color=#FF0000>[ERROR]:</color> <color=#FF0000>Error in drone patch: {e.Message}</color>");
            }
        }
    }
}
