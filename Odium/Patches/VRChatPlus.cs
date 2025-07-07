using HarmonyLib;
using System;

namespace Odium.Patches
{
    [HarmonyPatch(typeof(VRCPlusStatus))]
    public class VRCPlusStatusPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("get_prop_Object1PublicTBoTUnique_1_Boolean_0")]
        static void VRCPlusOverride(ref Object1PublicTBoTUnique<bool> __result)
        {
            if (__result == null) return;
            __result.prop_T_0 = true;
            __result.field_Protected_T_0 = true;
        }
    }
}