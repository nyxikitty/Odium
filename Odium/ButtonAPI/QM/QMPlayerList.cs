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
using VRC.Ui;
using static System.Net.Mime.MediaTypeNames;
using System.Collections;
using Odium.UI;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.X509;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using MelonLoader;

namespace Odium.ButtonAPI.QM
{
    internal class PlayerDebugUI
    {
        // Constants
        private const int MAX_LINES = 33;
        private const int MAX_CHARACTERS_PER_LINE = 68;
        private const int MAX_DISPLAYED_USERS = 38; // New constant for user limit

        public static GameObject label;
        public static GameObject background;
        public static TextMeshProUGUI text;
        public static List<string> messageList = new List<string>();
        private static string displayText = ""; // Added missing variable
        private static object playerListCoroutine; // To manage the coroutine

        private static readonly Dictionary<string, Color> keywordColors = new Dictionary<string, Color>
        {
            {"Join", Color.green},
            {"Leave", Color.red},
            {"+", Color.green},
            {"-", Color.red},
            {"Debug", Color.yellow},
            {"Log", Color.magenta},
            {"Photon", Color.magenta},
            {"Warn", Color.cyan},
            {"Error", Color.red},
            {"RPC", Color.white}
        };

        public static void InitializeDebugMenu()
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

                label.transform.SetParent(userInterface.transform.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/Wing_Left"));

                label.transform.localPosition = new Vector3(-450f, -500f, 0f);
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
                        rectTransform.anchoredPosition = new Vector2(180, 390f);
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
                background.transform.localScale = new Vector3(0.6f, 10f, 1f);
                background.transform.localRotation = Quaternion.identity;

                background.SetActive(true);

                var bgImage = background.GetComponent<ImageEx>();
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

                StartPlayerListLoop();

                OdiumConsole.Log("DebugUI", "Debug menu positioned correctly!");
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("DebugUI", $"Failed to initialize debug menu: {ex.Message}");
            }
        }

        public static void StartPlayerListLoop()
        {
            if (playerListCoroutine != null)
            {
                StopPlayerListLoop();
            }
            playerListCoroutine = MelonLoader.MelonCoroutines.Start(PlayerListLoop());
        }

        public static void StopPlayerListLoop()
        {
            if (playerListCoroutine != null)
            {
                MelonCoroutines.Stop(playerListCoroutine);
                playerListCoroutine = null;
            }
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

        public static string ColorToHex(Color color, bool includeHash = false)
        {
            try
            {
                string r = Mathf.RoundToInt(color.r * 255).ToString("X2");
                string g = Mathf.RoundToInt(color.g * 255).ToString("X2");
                string b = Mathf.RoundToInt(color.b * 255).ToString("X2");

                return includeHash ? $"#{r}{g}{b}" : $"{r}{g}{b}";
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("DebugUI", $"Failed to convert color to hex: {ex.Message}");
                return includeHash ? "#FFFFFF" : "FFFFFF";
            }
        }

        public static string FormatMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                OdiumConsole.Log("DebugUI", "Message is null or empty");
                return string.Empty;
            }

            try
            {
                string formattedMessage = message;
                if (message.Length > MAX_CHARACTERS_PER_LINE)
                {
                    int lastSpace = message.LastIndexOf(' ', MAX_CHARACTERS_PER_LINE);
                    if (lastSpace == -1) lastSpace = MAX_CHARACTERS_PER_LINE;

                    formattedMessage = message.Substring(0, lastSpace) + "\n" +
                                     FormatMessage(message.Substring(lastSpace + 1));
                }

                foreach (var kvp in keywordColors)
                {
                    if (formattedMessage.Contains(kvp.Key))
                    {
                        string colorTag = $"<color={ColorToHex(kvp.Value)}>";
                        formattedMessage = formattedMessage.Replace(kvp.Key, $"{colorTag}{kvp.Key}</color>");
                    }
                }

                return formattedMessage + "\n";
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("DebugUI", $"Failed to format message: {ex.Message}");
                return message + "\n";
            }
        }
        public static string GetPlatformIcon(string platform)
        {
            switch (platform?.ToLower())
            {
                case "standalonewindows":
                    return "[<color=#00BFFF>PC</color>]";
                case "android":
                    return "[<color=#32CD32>QUEST</color>]";
                case "ios":
                    return "[<color=#FF69B4>iOS</color>]";
                default:
                    return "[<color=#FFFFFF>UNK</color>]";
            }
        }

        public static IEnumerator PlayerListLoop()
        {
            while (true)
            {
                try
                {
                    displayText = "";

                    if (PlayerManager.prop_PlayerManager_0?.field_Private_List_1_Player_0 != null &&
                        PlayerManager.prop_PlayerManager_0.field_Private_List_1_Player_0.Count > 0)
                    {
                        int totalPlayerCount = PlayerManager.prop_PlayerManager_0.field_Private_List_1_Player_0.Count;
                        int displayedCount = Math.Min(totalPlayerCount, MAX_DISPLAYED_USERS);

                        // Show total count and if there are more players than displayed
                        if (totalPlayerCount > MAX_DISPLAYED_USERS)
                        {
                            displayText += $"<color=#00FF00>Players Online: {totalPlayerCount}</color> <color=#FFFF00>(Showing {displayedCount}/{totalPlayerCount})</color>\n\n";
                        }
                        else
                        {
                            displayText += $"<color=#00FF00>Players Online: {totalPlayerCount}</color>\n\n";
                        }

                        // Display only the first MAX_DISPLAYED_USERS players
                        for (int i = 0; i < displayedCount; i++)
                        {
                            var player = PlayerManager.prop_PlayerManager_0.field_Private_List_1_Player_0[i];
                            if (player?.field_Private_APIUser_0 != null)
                            {
                                string platform = PlayerRankTextDisplay.GetPlayerPlatform(player);
                                string platformText = GetPlatformIcon(platform);
                                bool friend = PlayerRankTextDisplay.IsFriend(player);
                                bool adult = PlayerRankTextDisplay.IsAdult(player);

                                string friendText = friend ? "<color=#FFD700>[FRIEND]</color>" : "";
                                string adultText = adult ? "<color=#90EE90>[18+]</color>" : "";

                                string rankDisplay = PlayerRankTextDisplay.GetRankDisplayName(PlayerRankTextDisplay.GetPlayerRank(player.field_Private_APIUser_0));

                                Color rankColor = PlayerRankTextDisplay.GetRankColor(PlayerRankTextDisplay.GetPlayerRank(player.field_Private_APIUser_0));
                                string hexColor = PlayerRankTextDisplay.ColorToHex(rankColor);
                                string rankName = PlayerRankTextDisplay.GetRankDisplayName(PlayerRankTextDisplay.GetPlayerRank(player.field_Private_APIUser_0));
                                displayText += $"<size=16><color={hexColor}>{player.field_Private_APIUser_0.displayName}</color></size> [{rankDisplay}] <size=16>{platformText}</size> <size=16>{friendText}</color> <size=16>{adultText}</size>\n";
                            }
                        }
                    }
                    else
                    {
                        displayText = "<color=#FF0000>No players found</color>\n";
                    }

                    messageList = new List<string>()
                    {
                        displayText
                    };

                    UpdateDisplay();
                }
                catch (Exception ex)
                {
                    OdiumConsole.Log("DebugUI", $"Error in PlayerListLoop: {ex.Message}");
                    displayText = $"<color=#FF0000>Error: {ex.Message}</color>\n";
                    UpdateDisplay();
                }

                yield return new WaitForSeconds(1f);
            }
        }

        private static void UpdateDisplay()
        {
            try
            {
                if (text == null)
                {
                    OdiumConsole.Log("DebugUI", "Text component is null, cannot update display");
                    return;
                }

                text.text = string.Join("", messageList);
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("DebugUI", $"Failed to update display: {ex.Message}");
            }
        }

        public static void Cleanup()
        {
            StopPlayerListLoop();
            if (label != null)
            {
                UnityEngine.Object.Destroy(label);
                label = null;
            }
            background = null;
            text = null;
            messageList.Clear();
            displayText = "";
        }
    }

    public class CoroutineStarter : MonoBehaviour
    {
        private static CoroutineStarter _instance;
        public static CoroutineStarter Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("CoroutineStarter");
                    _instance = go.AddComponent<CoroutineStarter>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
    }
}