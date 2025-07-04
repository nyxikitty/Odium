using System;
using System.Reflection;
using ExitGames.Client.Photon;
using HarmonyLib;
using MelonLoader;
using Photon.Realtime;
using VRC.Economy;
using VRC.SDKBase;
using Odium.Patches;

namespace Odium.Patches
{
    public class AwooochysPatchInitializer
    {
        public static string ModuleName = "HookManager";
        public static readonly HarmonyLib.Harmony instance = new HarmonyLib.Harmony("DeepCoreV2.ultrapatch");
        public static int pass = 0;
        public static int fail = 0;
        private static HarmonyMethod GetPreFix(string methodName)
        {
            return new HarmonyMethod(typeof(AwooochysPatchInitializer).GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic));
        }
        [Obsolete]
        public static void Start()
        {
            System.Console.WriteLine("Startup" + "Starting Hooks...");
            try
            {
                ClonePatch.Patch();
                pass++;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ModuleName + "allowAvatarCopying:" + ex.Message);
                fail++;
            }
            
            
            try
            {
                instance.Patch(typeof(VRC_EventDispatcherRFC).GetMethod("Method_Public_Boolean_Player_VrcEvent_VrcBroadcastType_0"), new HarmonyMethod(typeof(AwooochysPatchInitializer).GetMethod("RPCPatch", BindingFlags.Static | BindingFlags.NonPublic)), null, null, null, null);
                System.Console.WriteLine("Hook" + "RPC IS NOW VISIBLE");
                pass++;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ModuleName + "VRC_EventDispatcherRFC.RPC:" + ex.Message);
                fail++;
            }
            
            try
            {
                RoomManagerPatch.Patch();
                pass++;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ModuleName + "RoomManager:" + ex.Message);
                fail++;
            }
            
            try
            {
                EasyPatching.DeepCoreInstance.Patch(typeof(VRCPlusStatus).GetProperty("prop_Object1PublicTYBoTYUnique_1_Boolean_0").GetGetMethod(), null,GetLocalPatch("GetVRCPlusStatus"), null, null, null);
                pass++;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ModuleName + "VRCPlusStatus:" + ex.Message);
                fail++;
            }
            try
            {
                instance.Patch(typeof(Store).GetMethod("Method_Private_Boolean_VRCPlayerApi_IProduct_PDM_0"), AwooochysPatchInitializer.GetPreFix("RetrunPrefix"), null, null, null, null);
                instance.Patch(typeof(Store).GetMethod("Method_Private_Boolean_IProduct_PDM_0"), AwooochysPatchInitializer.GetPreFix("RetrunPrefix"), null, null, null, null);
                pass++;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ModuleName + "Store:" + ex.Message);
                fail++;
            }
            try
            {

                pass++;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ModuleName + "QuickMenu:" + ex.Message);
                fail++;
            }
            System.Console.WriteLine(ModuleName + $"Placed {pass} hook successfully, with {fail} failed.");
        }
        

        /* private static void GetVRCPlusStatus(ref Object1PublicTYBoTYUnique<bool> __result)
        {
            bool flag = __result == null;
            if (!flag)
            {
                __result.prop_TYPE_0 = true;
            }
        }*/
        
        #region StoreStuff
        private static bool MarketPatch(VRCPlayerApi __0, IProduct __1, ref bool __result)
        {
            __result = true;
            return false;
        }
        private static bool RetrunPrefix(ref bool __result)
        {
            __result = true;
            return false;
        }
        #endregion
        internal static bool Patch_OnEventSent(byte __0, object __1, RaiseEventOptions __2, SendOptions __3)
        {
            if (Patches.PhotonDebugger.IsOnEventSendDebug)
            {
                return Patches.PhotonDebugger.OnEventSent(__0, __1, __2, __3);
            }
            return true;
        }

        public static HarmonyMethod GetLocalPatch(string name)
        {
            HarmonyMethod result;
            try
            {
                result = (HarmonyMethod)typeof(AwooochysPatchInitializer).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic).ToNewHarmonyMethod();
            }
            catch (Exception arg)
            {
                System.Console.WriteLine(ModuleName + (string.Format("{0}: {1}", name, arg)));
                result = null;
            }
            return result;
        }
    }
}