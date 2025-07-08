using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Odium.Patches
{
    class Patching
    {
        public static int patchCount = 0;
        private static HarmonyMethod GetPatch(string name)
        {
            return new HarmonyMethod(typeof(Patching).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic));
        }

        public static void Initialize()
        {
            OdiumEntry.HarmonyInstance.Patch(typeof(VRCPlusStatus).GetProperty(nameof(VRCPlusStatus.prop_Object1PublicTBoTUnique_1_Boolean_0)).GetGetMethod(), null, Patching.GetPatch("VRCPlusOverride"), null, null, null);
            patchCount++;

            OdiumConsole.Log("[Patching]", $"Patches initialized successfully. Total patches: {Patching.patchCount}");
        }

        private static void VRCPlusOverride(ref Object1PublicTBoTUnique<bool> __result)
        {
            if (__result == null)
                return;
            __result.prop_T_0 = true;
            __result.field_Protected_T_0 = true;
        }
    }
}
