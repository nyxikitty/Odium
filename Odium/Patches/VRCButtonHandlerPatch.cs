using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace Odium.Patches
{
    class VRCButtonHandlePatching
    {
        private static HarmonyMethod GetPatch(string name)
        {
            return new HarmonyMethod(typeof(VRCButtonHandlePatching).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic));
        }

        public static void Initialize()
        {
            try
            {
                var onPointerClickMethod = typeof(VRCButtonHandle).GetMethod("OnPointerClick", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (onPointerClickMethod != null)
                {
                    OdiumEntry.HarmonyInstance.Patch(onPointerClickMethod, GetPatch("OnPointerClickPrefix"), null, null, null, null);
                }

                var handleClickMethod = typeof(VRCButtonHandle).GetMethod("HandleClick", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (handleClickMethod != null)
                {
                    OdiumEntry.HarmonyInstance.Patch(handleClickMethod, GetPatch("HandleClickPrefix"), null, null, null, null);
                }

                var unityEventInvokeMethod = typeof(UnityEngine.Events.UnityEvent).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
                if (unityEventInvokeMethod != null)
                {
                    OdiumEntry.HarmonyInstance.Patch(unityEventInvokeMethod, GetPatch("UnityEventInvokePrefix"), null, null, null, null);
                }

                OdiumConsole.Log("[VRCButtonHandlePatching]", "VRCButtonHandle patches initialized successfully");

                Patching.patchCount++;
                OdiumConsole.Log("[Patching]", $"Patches initialized successfully. Total patches: {Patching.patchCount}");

            }
            catch (Exception ex)
            {
                OdiumConsole.Log("[VRCButtonHandlePatching]", $"Failed to initialize patches: {ex.Message}");
            }
        }

        private static void OnPointerClickPrefix(VRCButtonHandle __instance, UnityEngine.EventSystems.PointerEventData eventData)
        {
            OdiumConsole.Log("[VRCButtonHandle]", $"OnPointerClick called on button: {__instance.gameObject.name}");
            OdiumConsole.Log("[VRCButtonHandle]", $"Button path: {GetGameObjectPath(__instance.gameObject)}");
        }

        private static void HandleClickPrefix(VRCButtonHandle __instance)
        {
            OdiumConsole.Log("[VRCButtonHandle]", $"HandleClick called on button: {__instance.gameObject.name}");
            LogButtonDetails(__instance);
        }

        private static void UnityEventInvokePrefix(UnityEngine.Events.UnityEvent __instance)
        {
            // Only log if this appears to be from a VRCButtonHandle
            var stackTrace = Environment.StackTrace;
            if (stackTrace.Contains("VRCButtonHandle"))
            {
                OdiumConsole.Log("[UnityEvent]", $"Invoke called from VRCButtonHandle context");
                OdiumConsole.Log("[UnityEvent]", $"Event listener count: {__instance.GetPersistentEventCount()}");

                // Log all persistent listeners
                for (int i = 0; i < __instance.GetPersistentEventCount(); i++)
                {
                    var target = __instance.GetPersistentTarget(i);
                    var methodName = __instance.GetPersistentMethodName(i);
                    OdiumConsole.Log("[UnityEvent]", $"Listener {i}: {target?.GetType().Name}.{methodName}");
                }
            }
        }

        // Helper method to get full GameObject path
        private static string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform parent = obj.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }

        // Helper method to log button details
        private static void LogButtonDetails(VRCButtonHandle button)
        {
            OdiumConsole.Log("[VRCButtonHandle]", $"Button GameObject: {button.gameObject.name}");
            OdiumConsole.Log("[VRCButtonHandle]", $"Button Active: {button.gameObject.activeInHierarchy}");

            // Try to get any text component
            var textComponents = button.GetComponentsInChildren<UnityEngine.UI.Text>();
            foreach (var text in textComponents)
            {
                OdiumConsole.Log("[VRCButtonHandle]", $"Button Text: {text.text}");
            }

            // Log all methods being called via reflection
            Type buttonType = button.GetType();
            OdiumConsole.Log("[VRCButtonHandle]", $"Button Type: {buttonType.Name}");
        }
    }
}