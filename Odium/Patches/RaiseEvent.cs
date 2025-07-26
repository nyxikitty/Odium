using ExitGames.Client.Photon;
using HarmonyLib;
using Odium.API;
using Odium.ApplicationBot;
using Odium.Components;
using Odium.Odium;
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
using VRC.SDKBase;

namespace Odium.Patches
{
    public static class RaiseEventPatches
    {
        public static void ApplyPatches()
        {
            OdiumEntry.HarmonyInstance.Patch(
                typeof(LoadBalancingClient).GetMethod(
                    nameof(LoadBalancingClient.Method_Public_Virtual_New_Boolean_Byte_Object_RaiseEventOptions_SendOptions_0),
                    BindingFlags.Public | BindingFlags.Instance
                ),
                prefix: new HarmonyMethod(typeof(RaiseEventPatches).GetMethod(nameof(PrefixSendEvent), BindingFlags.Static | BindingFlags.Public))
            );

            OdiumConsole.Log("RaiseEventPatches", "Applied RaiseEvent patches successfully.");
        }

        public static bool PrefixSendEvent(byte param_1, Il2CppSystem.Object param_2, RaiseEventOptions param_3, SendOptions param_4)
        {
            if (AssignedVariables.conduit)
            {
                if (param_1 == 1 && AssignedVariables.clientTalk)
                {
                    byte[] e = Serializer.FromIL2CPPToManaged<byte[]>(param_2);
                    EventHandlers.SendAudio(e, Networking.LocalPlayer.playerId);
                    return false;
                }

                if (param_1 == 74 && AssignedVariables.proxyPortals)
                {
                    byte[] e = Serializer.FromIL2CPPToManaged<byte[]>(param_2);
                    EventHandlers.DropPortal(e, Networking.LocalPlayer.playerId);

                    return false;
                }

                if (param_1 == 12 && AssignedVariables.proxyMovement)
                {
                    byte[] e = Serializer.FromIL2CPPToManaged<byte[]>(param_2);
                    EventHandlers.SendMovement(e, Networking.LocalPlayer.playerId);

                    return false;
                }
            }

            if (param_1 == 12 && ActionWrapper.serialize || Bot.movementMimic)
            {
                return false;
            }

            return true;
        }
    }
}