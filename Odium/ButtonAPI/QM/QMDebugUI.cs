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

namespace Odium.ButtonAPI.QM
{
    internal class DebugUI
    {
        // Constants
        private const int MAX_LINES = 20;
        private const int MAX_CHARACTERS_PER_LINE = 68;

        public static GameObject label;
        public static GameObject background;
        public static TextMeshProUGUI text;
        public static List<string> messageList = new List<string>();

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
                label.transform.SetParent(userInterface.transform.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/Wing_Right"));
                label.transform.localPosition = new Vector3(0f, -235.8705f, 0f);

                text = label.GetComponentInChildren<TextMeshProUGUIEx>();
                if (text == null)
                {
                    OdiumConsole.Log("DebugUI", "Text component not found");
                    return;
                }

                text.alignment = TextAlignmentOptions.TopLeft;
                text.faceColor = Color.magenta;
                text.outlineColor = Color.magenta;
                text.outlineWidth = 0.2f;
                text.fontSize = 20f;
                text.fontSizeMax = 25f;
                text.fontSizeMin = 18f;

                var mask = label.AddComponent<Mask>();
                mask.showMaskGraphic = false;

                background = label.transform.Find("HeaderBackground")?.gameObject;
                if (background == null)
                {
                    OdiumConsole.Log("DebugUI", "Background object not found");
                    return;
                }

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
                    return;
                }

                bgImage.overrideSprite = sprite;

                var styleElement = background.GetComponent<StyleElement>();
                if (styleElement != null)
                {
                    UnityEngine.Object.Destroy(styleElement);
                }

                background.transform.localPosition = new Vector3(0f, 299.7452f, 0f);
                background.transform.localScale = new Vector3(1f, 7.0909f, 1f);

                label.SetActive(true);
                background.SetActive(true);
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("DebugUI", $"Failed to initialize debug menu: {ex.Message}");
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

                // Apply color coding
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

        public static void LogMessage(string message)
        {
            try
            {
                if (messageList.Count >= MAX_LINES)
                {
                    messageList.RemoveAt(0);
                }

                string formattedMessage = FormatMessage(message);
                messageList.Add(formattedMessage);
                UpdateDisplay();
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("DebugUI", $"Failed to log message: {ex.Message}");
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
    }
}