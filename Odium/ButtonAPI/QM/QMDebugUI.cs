using Odium.AwooochysResourceManagement;
using Odium.ButtonAPI.QM;
using Odium.Odium;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Core.Styles;
using Odium.QMPages;
using VRC.UI;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Http;
using System.Collections;
using System.Threading.Tasks;
using MelonLoader;
using Odium.Threadding;
using Newtonsoft.Json;

namespace Odium.ButtonAPI.QM
{
    public class DebugUI
    {
        public static GameObject label;
        public static GameObject background;
        public static TextMeshProUGUI text;

        // Cached values to prevent rapid updates
        private static string cachedPing = "0";
        private static string cachedFPS = "0";
        private static string cachedBuild = "Unknown";
        public static int cachedPlayerTags = 0;
        public static int cachedOdiumUsers = 0;
        public static int activeRoomCount = 0;
        public static long msResponse = 0;
        public static bool isConnectedToServer = false;

        // Flag to prevent external updates
        private static bool isUpdating = false;

        // Visibility state
        private static bool isVisible = true;

        public static async void InitializeDebugMenu()
        {
            try
            {
                var userInterface = AssignedVariables.userInterface;
                if (userInterface == null)
                {
                    OdiumConsole.Log("DebugUI", "User interface not found");
                    return;
                }

                var dashboardHeader = userInterface.transform.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Header_QuickLinks");
                dashboardHeader.gameObject.SetActive(true);
                dashboardHeader.transform.Find("HeaderBackground").gameObject.SetActive(true);

                if (dashboardHeader == null)
                {
                    OdiumConsole.Log("DebugUI", "Dashboard header template not found");
                    return;
                }

                label = UnityEngine.Object.Instantiate(dashboardHeader.gameObject);
                label.SetActive(true);

                label.transform.SetParent(userInterface.transform.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/Wing_Right"));

                label.transform.localPosition = new Vector3(400f, -525f, 0f);
                label.transform.localRotation = Quaternion.identity;
                label.transform.localScale = new Vector3(1f, 1f, 1f);

                var invisibleGraphic = label.GetComponent<UIInvisibleGraphic>();
                if (invisibleGraphic != null)
                    invisibleGraphic.enabled = false;

                text = label.GetComponentInChildren<TextMeshProUGUIEx>();
                if (text == null)
                {
                    OdiumConsole.Log("DebugUI", "Text component not found");
                    return;
                }

                text.alignment = TextAlignmentOptions.TopLeft;
                text.outlineWidth = 0.2f;
                text.fontSize = 20f;
                text.fontSizeMax = 25f;
                text.fontSizeMin = 18f;
                text.richText = true;

                Transform LeftItemContainer = label.transform.Find("LeftItemContainer");
                if (LeftItemContainer != null)
                {
                    var layoutGroup = LeftItemContainer.GetComponent<LayoutGroup>();
                    if (layoutGroup != null)
                    {
                        layoutGroup.enabled = false;
                        OdiumConsole.Log("DebugUI", "Disabled LayoutGroup on LeftItemContainer");
                    }

                    var contentSizeFitter = LeftItemContainer.GetComponent<ContentSizeFitter>();
                    if (contentSizeFitter != null)
                    {
                        contentSizeFitter.enabled = false;
                        OdiumConsole.Log("DebugUI", "Disabled ContentSizeFitter on LeftItemContainer");
                    }
                }

                var labelLayoutGroup = label.GetComponent<LayoutGroup>();
                if (labelLayoutGroup != null)
                {
                    labelLayoutGroup.enabled = false;
                    OdiumConsole.Log("DebugUI", "Disabled LayoutGroup on label");
                }

                if (text != null)
                {
                    RectTransform rectTransform = text.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.anchoredPosition = new Vector2(295, 220f);
                        OdiumConsole.Log("DebugUI", $"Set anchored position to: {rectTransform.anchoredPosition}");
                    }
                }

                var mask = label.AddComponent<Mask>();
                mask.showMaskGraphic = false;

                background = label.transform.Find("HeaderBackground")?.gameObject;
                if (background == null)
                {
                    OdiumConsole.Log("DebugUI", "Background object not found");
                    return;
                }

                background.transform.localPosition = new Vector3(0f, 0f, 0f);
                background.transform.localScale = new Vector3(0.4f, 7.5f, 1f);
                background.transform.localRotation = Quaternion.identity;

                background.SetActive(true);

                var bgImage = background.GetComponent<VRC.UI.ImageEx>();
                if (bgImage == null)
                {
                    OdiumConsole.Log("DebugUI", "Background image component not found");
                    return;
                }

                string bgPath = System.IO.Path.Combine(Environment.CurrentDirectory, "Odium", "QMDebugUIBackground.png");
                var sprite = bgPath.LoadSpriteFromDisk();
                if (sprite == null)
                {
                    OdiumConsole.Log("DebugUI", $"Failed to load background sprite from path: {bgPath}");
                    bgImage.m_Color = new Color(0.443f, 0.133f, 1.0f, 1.0f);
                }
                else
                {
                    bgImage.overrideSprite = sprite;
                }

                var styleElement = background.GetComponent<StyleElement>();
                if (styleElement != null)
                {
                    UnityEngine.Object.Destroy(styleElement);
                }

                label.SetActive(true);
                background.SetActive(true);

                MelonCoroutines.Start(UpdateLoop());

                OdiumConsole.Log("DebugUI", "Debug menu positioned correctly!");
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("DebugUI", $"Failed to initialize debug menu: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggles the visibility of the entire Debug UI
        /// </summary>
        public static void ToggleVisibility()
        {
            try
            {
                isVisible = !isVisible;

                if (label != null)
                {
                    label.SetActive(isVisible);
                    OdiumConsole.Log("DebugUI", $"Debug UI visibility toggled to: {isVisible}");
                }
                else
                {
                    OdiumConsole.Log("DebugUI", "Cannot toggle visibility - Debug UI not initialized");
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("DebugUI", $"Failed to toggle visibility: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets the visibility of the Debug UI to a specific state
        /// </summary>
        /// <param name="visible">True to show, false to hide</param>
        public static void SetVisibility(bool visible)
        {
            try
            {
                isVisible = visible;

                if (label != null)
                {
                    label.SetActive(isVisible);
                    OdiumConsole.Log("DebugUI", $"Debug UI visibility set to: {isVisible}");
                }
                else
                {
                    OdiumConsole.Log("DebugUI", "Cannot set visibility - Debug UI not initialized");
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("DebugUI", $"Failed to set visibility: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the current visibility state of the Debug UI
        /// </summary>
        /// <returns>True if visible, false if hidden</returns>
        public static bool IsVisible()
        {
            return isVisible && label != null && label.activeInHierarchy;
        }

        public static IEnumerator UpdateLoop()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(5f);
                yield return MelonCoroutines.Start(GetUserCountCoroutine());

                isUpdating = true;

                cachedPing = ApiUtils.GetPing();
                cachedFPS = ApiUtils.GetFPS();
                cachedBuild = ApiUtils.GetBuild();
                cachedPlayerTags = AssignedVariables.playerTagsCount;

                UpdateDisplay();

                isUpdating = false;
            }
        }

        private static float lastUpdateTime = 0f;
        private static readonly float UPDATE_INTERVAL = 1f;

        private static void UpdateDisplay()
        {
            if (Time.time - lastUpdateTime < UPDATE_INTERVAL)
                return;

            lastUpdateTime = Time.time;

            if (text != null)
            {
                text.text = $@"
License Type: Private

License Duration: Lifetime

Player Tags: {cachedPlayerTags}

Active Users: {cachedOdiumUsers}

Active Rooms: {activeRoomCount}

Build: Beta-1.6-{cachedBuild}

Server: {msResponse}ms

Client: Connected

Ping: {cachedPing}

FPS: {cachedFPS}
        ";
            }
        }

        private static IEnumerator GetUserCountCoroutine()
        {
            var www = UnityEngine.Networking.UnityWebRequest.Get("https://snoofz.net/api/odium/users/list");

            yield return www.SendWebRequest();

            if (www.isDone)
            {
                if (string.IsNullOrEmpty(www.error))
                {
                    try
                    {
                        var jsonContent = www.downloadHandler.text;
                        var users = JsonConvert.DeserializeObject<List<object>>(jsonContent);
                        var userCount = users != null ? users.Count : 0;
                        cachedOdiumUsers = userCount;
                        AssignedVariables.odiumUsersCount = userCount;
                    }
                    catch (Exception)
                    {
                        cachedOdiumUsers = 0;
                    }
                }
                else
                {
                    cachedOdiumUsers = 0;
                }
            }

            www.Dispose();
        }

        public static void AdjustPosition(float x, float y, float z)
        {
            if (label != null)
            {
                label.transform.localPosition = new Vector3(x, y, z);
                OdiumConsole.Log("DebugUI", $"Position adjusted to: {x}, {y}, {z}");
            }
        }

        public static void AdjustBackgroundScale(float x, float y, float z)
        {
            if (background != null)
            {
                background.transform.localScale = new Vector3(x, y, z);
                OdiumConsole.Log("DebugUI", $"Background scale adjusted to: {x}, {y}, {z}");
            }
        }

        public static void FixBackgroundWidth()
        {
            if (background != null)
            {
                background.transform.localScale = new Vector3(1f, 8f, 1f);
                OdiumConsole.Log("DebugUI", "Background width adjusted");
            }
        }
    }
}