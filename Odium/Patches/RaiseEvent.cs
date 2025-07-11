using ExitGames.Client.Photon;
using HarmonyLib;
using Odium.ApplicationBot;
using Odium.UX;
using Odium.Wrappers;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;

namespace Odium.Patches
{
    [HarmonyPatch(typeof(LoadBalancingClient))]
    public class PhotonNetworkPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("Method_Public_Virtual_New_Boolean_Byte_Object_RaiseEventOptions_SendOptions_0")]
        static bool PrefixSendEvent(byte __0, Il2CppSystem.Object __1, RaiseEventOptions __2, SendOptions __3)
        {
            if (__0 == 12 && ActionWrapper.serialize || Bot.movementMimic)
            {
                return false;
            }

            if (__0 != 43) return true;
            try
            {
                if (__1 != null)
                {
                    if (__1.TryCast<Il2CppSystem.Array>() != null)
                    {
                        var array = __1.TryCast<Il2CppSystem.Array>();
                        if (array.Length > 1)
                        {
                            for (global::System.Int32 i = 0; i < array.Length; i++)
                            {
                                var element = array.GetValue(i);
                                OdiumConsole.LogGradient("PhotonEvent", element?.ToString() ?? "null");
                            }
                        }
                        else
                        {
                            OdiumConsole.LogGradient("PhotonEvent", "Array too short");
                        }
                    }
                    else
                    {
                        OdiumConsole.LogGradient("PhotonEvent", __1.ToString());
                    }
                }
                else
                {
                    OdiumConsole.LogGradient("PhotonEvent", "Event data is null");
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("PhotonEvent", $"Error logging Photon event 43: {ex.Message}", LogLevel.Error);
            }

            return true;
        }
    }
}