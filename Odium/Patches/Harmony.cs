using HarmonyLib;
using MelonLoader;
using Odium.UX;
using System;
using System.Reflection;
using VRC.SDK3.StringLoading;
using VRC.Udon;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;
using VRC.Core;
using System.Net.Http;
using UnityEngine.Networking;

namespace Odium.Patches
{
    class Patching
    {
        public static int patchCount = 0;

        public static void Initialize()
        {
            // VRC+ Subscription Bypass
            OdiumEntry.HarmonyInstance.Patch(
                typeof(VRCPlusStatus).GetProperty(nameof(VRCPlusStatus.prop_Object1PublicTBoTUnique_1_Boolean_0)).GetGetMethod(),
                postfix: new HarmonyMethod(typeof(Patching).GetMethod(nameof(VRCPlusOverride), BindingFlags.Static | BindingFlags.NonPublic))
            );
            patchCount++;

            // Udon Network Event Patch (Blocks ListPatrons exploit)
            OdiumEntry.HarmonyInstance.Patch(
                typeof(UdonBehaviour).GetMethod(
                    nameof(UdonBehaviour.SendCustomNetworkEvent),
                    new[] { typeof(NetworkEventTarget), typeof(string) }
                ),
                prefix: new HarmonyMethod(typeof(Patching).GetMethod(nameof(OnUdonNetworkEvent), BindingFlags.Static | BindingFlags.NonPublic))
            );
            patchCount++;

            // Block malicious string downloads (Jar's worlds)
            OdiumEntry.HarmonyInstance.Patch(
                typeof(VRCStringDownloader).GetMethod(nameof(VRCStringDownloader.LoadUrl)),
                prefix: new HarmonyMethod(typeof(Patching).GetMethod(nameof(OnStringDownload), BindingFlags.Static | BindingFlags.NonPublic))
            );
            patchCount++;

            OdiumEntry.HarmonyInstance.Patch(
                typeof(HttpClient).GetMethod(nameof(UnityWebRequest.Get)),
                prefix: new HarmonyMethod(typeof(Patching).GetMethod(nameof(OnGet), BindingFlags.Static | BindingFlags.NonPublic))
            );
            patchCount++;

            MelonLogger.Msg($"Patches initialized successfully. Total patches: {patchCount}");
        }

        private static bool OnGet(string url)
        {
            try
            {
                var currentWorld = RoomManager.field_Internal_Static_ApiWorld_0;
                if (currentWorld == null) return true;

                string authorId = currentWorld.authorId;
                if (authorId == "LyCh6jlK6X")
                {
                    OdiumConsole.LogGradient("BLOCKED", $"Prevented string download in Jar's world (URL: {url})");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error in OnStringDownload: {ex}");
            }

            return true;
        }

        private static void VRCPlusOverride(ref Object1PublicTBoTUnique<bool> __result)
        {
            if (__result == null) return;
            __result.prop_T_0 = true;
            __result.field_Protected_T_0 = true;
        }

        private static bool OnUdonNetworkEvent(UdonBehaviour __instance, NetworkEventTarget target, string eventName)
        {
            // Stupid niggers using this shit to crash me, not anymore
            if (eventName != "ListPatrons") return true;

            var sender = __instance.GetComponentInParent<VRCPlayer>();

            if (sender == null || sender == VRCPlayer.field_Internal_Static_VRCPlayer_0)
                return true;

            InternalConsole.LogIntoConsole($"[BLOCKED] Crash attempt from {sender.field_Private_VRCPlayerApi_0.displayName}!", "[Udon]");
            return false;
        }

        private static bool OnUdonRunProgram(UdonBehaviour __instance, string programName)
        {
            if (programName != "ListPatrons") return true;

            var sender = __instance.GetComponentInParent<VRCPlayer>();

            if (sender == null || sender == VRCPlayer.field_Internal_Static_VRCPlayer_0)
                return true;

            InternalConsole.LogIntoConsole($"[BLOCKED] Crash attempt from {sender.field_Private_VRCPlayerApi_0.displayName}!", "[Udon]");
            return false;
        }

        private static bool OnStringDownload(string url)
        {
            try
            {
                var currentWorld = RoomManager.field_Internal_Static_ApiWorld_0;
                if (currentWorld == null) return true;

                string authorId = currentWorld.authorId;
                if (authorId == "LyCh6jlK6X")
                {
                    OdiumConsole.LogGradient("BLOCKED", $"Prevented string download in Jar's world (URL: {url})");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error in OnStringDownload: {ex}");
            }

            return true;
        }
    }
}