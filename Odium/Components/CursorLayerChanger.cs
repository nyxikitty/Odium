using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;


namespace CursorLayerMod
{
    public class CursorLayerMod : MelonMod
    {
        public const int TARGET_LAYER = 9999;
        public const string CURSOR_PATH = "CursorManager/MouseArrow/VRCUICursorIcon";
        public static bool hasSetLayer = false;
        public static void OnUpdate()
        {
            if (!hasSetLayer)
            {
                SetCursorLayer();
            }
        }

        public static void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            hasSetLayer = false;
            MelonLogger.Msg($"Scene loaded: {sceneName}, resetting cursor layer flag");
        }

        public static void SetCursorLayer()
        {
            GameObject cursorIcon = FindCursorIcon();

            if (cursorIcon != null)
            {
                // Try multiple methods to set the layer
                bool success = false;

                // Method 1: Canvas sorting order
                Canvas canvas = cursorIcon.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.sortingOrder = TARGET_LAYER;
                    canvas.overrideSorting = true;
                    success = true;
                    MelonLogger.Msg($"Set Canvas sorting order to {TARGET_LAYER}");
                }

                // Method 2: Renderer sorting order
                Renderer renderer = cursorIcon.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sortingOrder = TARGET_LAYER;
                    success = true;
                    MelonLogger.Msg($"Set Renderer sorting order to {TARGET_LAYER}");
                }

                // Method 3: Image/Graphic sorting
                Graphic graphic = cursorIcon.GetComponent<Graphic>();
                if (graphic != null && graphic.canvas != null)
                {
                    graphic.canvas.sortingOrder = TARGET_LAYER;
                    graphic.canvas.overrideSorting = true;
                    success = true;
                    MelonLogger.Msg($"Set Graphic Canvas sorting order to {TARGET_LAYER}");
                }

                // Method 4: Transform hierarchy manipulation
                Transform parent = cursorIcon.transform.parent;
                if (parent != null)
                {
                    // Move to be last child (renders on top)
                    cursorIcon.transform.SetAsLastSibling();
                    success = true;
                    MelonLogger.Msg("Set cursor as last sibling in hierarchy");
                }

                if (success)
                {
                    hasSetLayer = true;
                    MelonLogger.Msg("Successfully set cursor to highest layer!");
                }
            }
        }

        public static GameObject FindCursorIcon()
        {
            // Method 1: Direct path search
            GameObject cursor = GameObject.Find(CURSOR_PATH);
            if (cursor != null) return cursor;

            // Method 2: Search through CursorManager
            GameObject cursorManager = GameObject.Find("CursorManager");
            if (cursorManager != null)
            {
                Transform found = cursorManager.transform.Find("MouseArrow/VRCUICursorIcon");
                if (found != null) return found.gameObject;

                // Alternative path
                found = cursorManager.transform.Find("VRCUICursorIcon");
                if (found != null) return found.gameObject;
            }

            // Method 3: Search by name in all objects
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("VRCUICursorIcon") || obj.name.Contains("CursorIcon"))
                {
                    return obj;
                }
            }

            return null;
        }
    }
}