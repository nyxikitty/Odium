using MelonLoader;
using Odium.Odium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Odium.ButtonAPI.QM
{
    public class QMLabel
    {
        private static Transform quickActionsTransform;

        /// <summary>
        /// Get the QuickActions transform (call this before using other functions)
        /// </summary>
        /// <returns>True if found, false otherwise</returns>
        public static bool InitializeQuickActions()
        {
            try
            {
                quickActionsTransform = AssignedVariables.userInterface.transform.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickActions");

                if (quickActionsTransform == null)
                {
                    MelonLogger.Error("QuickActions transform not found!");
                    return false;
                }

                MelonLogger.Msg("QuickActions transform found successfully!");
                return true;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error finding QuickActions: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Insert text into an existing Text component in QuickActions
        /// </summary>
        /// <param name="text">Text to insert</param>
        /// <param name="append">Whether to append or replace existing text</param>
        public static void InsertTextIntoExistingComponent(string text, bool append = false)
        {
            if (quickActionsTransform == null && !InitializeQuickActions())
                return;

            try
            {
                // Try to find Text component (Legacy UI)
                Text textComponent = quickActionsTransform.GetComponentInChildren<Text>();
                if (textComponent != null)
                {
                    if (append)
                        textComponent.text += text;
                    else
                        textComponent.text = text;

                    MelonLogger.Msg($"Text updated: {textComponent.text}");
                    return;
                }

                // Try to find TextMeshPro component
                TextMeshProUGUI tmpComponent = quickActionsTransform.GetComponentInChildren<TextMeshProUGUI>();
                if (tmpComponent != null)
                {
                    if (append)
                        tmpComponent.text += text;
                    else
                        tmpComponent.text = text;

                    MelonLogger.Msg($"TextMeshPro updated: {tmpComponent.text}");
                    return;
                }

                MelonLogger.Warning("No Text or TextMeshPro component found in QuickActions!");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error inserting text: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new text element and add it to QuickActions
        /// </summary>
        /// <param name="text">Text to display</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="color">Text color</param>
        /// <returns>The created text GameObject</returns>
        public static GameObject CreateNewTextElement(string text, int fontSize = 14, Color? color = null)
        {
            if (quickActionsTransform == null && !InitializeQuickActions())
                return null;

            try
            {
                // Create new GameObject for text
                GameObject textObject = new GameObject("QuickAction_Text_" + DateTime.Now.Ticks);
                textObject.transform.SetParent(quickActionsTransform, false);

                // Add RectTransform
                RectTransform rectTransform = textObject.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;

                // Try to add TextMeshPro first (preferred)
                try
                {
                    TextMeshProUGUI tmpText = textObject.AddComponent<TextMeshProUGUI>();
                    tmpText.text = text;
                    tmpText.fontSize = fontSize;
                    tmpText.color = color ?? Color.white;
                    tmpText.alignment = TextAlignmentOptions.Center;
                    tmpText.raycastTarget = false; // Prevent UI blocking
                }
                catch
                {
                    // Fallback to legacy Text component
                    Text legacyText = textObject.AddComponent<Text>();
                    legacyText.text = text;
                    legacyText.fontSize = fontSize;
                    legacyText.color = color ?? Color.white;
                    legacyText.alignment = TextAnchor.MiddleCenter;
                    legacyText.raycastTarget = false;

                    // Try to find a font
                    legacyText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }

                MelonLogger.Msg($"Created new text element: {text}");
                return textObject;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error creating text element: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Insert text into a specific button's text component within QuickActions
        /// </summary>
        /// <param name="buttonIndex">Index of the button (0-based)</param>
        /// <param name="text">Text to set</param>
        public static void InsertTextIntoButton(int buttonIndex, string text)
        {
            if (quickActionsTransform == null && !InitializeQuickActions())
                return;

            try
            {
                // Get all button children
                Button[] buttons = quickActionsTransform.GetComponentsInChildren<Button>();

                if (buttonIndex >= 0 && buttonIndex < buttons.Length)
                {
                    Button targetButton = buttons[buttonIndex];

                    // Try to find text component in button
                    Text buttonText = targetButton.GetComponentInChildren<Text>();
                    if (buttonText != null)
                    {
                        buttonText.text = text;
                        MelonLogger.Msg($"Button {buttonIndex} text updated: {text}");
                        return;
                    }

                    TextMeshProUGUI buttonTMP = targetButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonTMP != null)
                    {
                        buttonTMP.text = text;
                        MelonLogger.Msg($"Button {buttonIndex} TextMeshPro updated: {text}");
                        return;
                    }

                    MelonLogger.Warning($"No text component found in button {buttonIndex}");
                }
                else
                {
                    MelonLogger.Error($"Button index {buttonIndex} is out of range. Found {buttons.Length} buttons.");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error updating button text: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all current button texts (useful for debugging)
        /// </summary>
        /// <returns>Array of button text strings</returns>
        public static string[] GetAllButtonTexts()
        {
            if (quickActionsTransform == null && !InitializeQuickActions())
                return new string[0];

            try
            {
                Button[] buttons = quickActionsTransform.GetComponentsInChildren<Button>();
                string[] buttonTexts = new string[buttons.Length];

                for (int i = 0; i < buttons.Length; i++)
                {
                    Text text = buttons[i].GetComponentInChildren<Text>();
                    TextMeshProUGUI tmp = buttons[i].GetComponentInChildren<TextMeshProUGUI>();

                    buttonTexts[i] = text?.text ?? tmp?.text ?? $"Button_{i}";
                }

                MelonLogger.Msg($"Found {buttons.Length} buttons in QuickActions");
                return buttonTexts;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error getting button texts: {ex.Message}");
                return new string[0];
            }
        }

        /// <summary>
        /// Clear all text from QuickActions
        /// </summary>
        public static void ClearAllText()
        {
            if (quickActionsTransform == null && !InitializeQuickActions())
                return;

            try
            {
                int clearedCount = 0;

                // Clear all Text components
                Text[] textComponents = quickActionsTransform.GetComponentsInChildren<Text>();
                foreach (Text text in textComponents)
                {
                    text.text = "";
                    clearedCount++;
                }

                // Clear all TextMeshPro components
                TextMeshProUGUI[] tmpComponents = quickActionsTransform.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (TextMeshProUGUI tmp in tmpComponents)
                {
                    tmp.text = "";
                    clearedCount++;
                }

                MelonLogger.Msg($"Cleared {clearedCount} text components");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error clearing text: {ex.Message}");
            }
        }

        /// <summary>
        /// Find QuickActions and log its structure (useful for debugging)
        /// </summary>
        public static void DebugQuickActionsStructure()
        {
            if (quickActionsTransform == null && !InitializeQuickActions())
                return;

            try
            {
                MelonLogger.Msg("=== QuickActions Structure ===");
                MelonLogger.Msg($"Transform name: {quickActionsTransform.name}");
                MelonLogger.Msg($"Child count: {quickActionsTransform.childCount}");

                for (int i = 0; i < quickActionsTransform.childCount; i++)
                {
                    Transform child = quickActionsTransform.GetChild(i);
                    MelonLogger.Msg($"Child {i}: {child.name} (Active: {child.gameObject.activeInHierarchy})");

                    // Check for text components
                    Text text = child.GetComponentInChildren<Text>();
                    TextMeshProUGUI tmp = child.GetComponentInChildren<TextMeshProUGUI>();
                    Button button = child.GetComponent<Button>();

                    if (text != null) MelonLogger.Msg($"  - Has Text: '{text.text}'");
                    if (tmp != null) MelonLogger.Msg($"  - Has TextMeshPro: '{tmp.text}'");
                    if (button != null) MelonLogger.Msg($"  - Has Button component");
                }
                MelonLogger.Msg("=== End Structure ===");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error debugging structure: {ex.Message}");
            }
        }
    }
}
