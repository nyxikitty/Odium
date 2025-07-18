using MelonLoader;
using Odium.Components;
using Odium.Odium;
using Odium.UX;
using Odium.Wrappers;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.Localization;

public static class ToggleStateManager
{
    public static Dictionary<string, bool> ToggleStates = new Dictionary<string, bool>();
}
public class SelectedUser
{
    public static bool ui_ready = false;
    public static GameObject PageGrid;

    public static string get_selected_player_name()
    {
        var textObject = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_SelectedUser_Local/ScrollRect/Viewport/VerticalLayoutGroup/UserProfile_Compact/PanelBG/Info/Text_Username_NonFriend");
        if (textObject == null) return "";

        var textComponent = textObject.GetComponent<TextMeshProUGUIEx>();
        if (textComponent == null) return "";

        return textComponent.text;
    }

    public static GameObject CreateButton(GameObject parentGrid, string title, Action<VRC.Player> OnClick, string tooltip = "", bool DisplayIcon = false, Sprite sprite = null)
    {
        var buttonObject = GameObject.Instantiate(MainMenu.ExampleButton, parentGrid.transform);
        buttonObject.name = $"Button_{Guid.NewGuid()}";

        var text = buttonObject.transform.FindChild("TextLayoutParent/Text_H4");
        if (text != null)
        {
            TextMeshProUGUIEx textMesh = text.gameObject.GetComponent<TextMeshProUGUIEx>();
            textMesh.richText = true;
            textMesh.prop_String_0 = title;
        }

        var badge = buttonObject.transform.FindChild("Badge_MMJump");
        if (badge != null)
        {
            badge.gameObject.SetActive(false);
        }

        var tooltips = buttonObject.GetComponents<ToolTip>();
        foreach (var ttcomp in tooltips)
        {
            var lstring = new LocalizableString();
            lstring._localizationKey = tooltip;
            ttcomp._localizableString = lstring;
        }

        var buttonComponent = buttonObject.GetComponent<VRCButtonHandle>();
        if (buttonComponent != null)
        {
            buttonComponent._sendAnalytics = false;

            buttonComponent.m_OnClick.RemoveAllListeners();
            buttonComponent.onClick.RemoveAllListeners();
            buttonComponent.onClick.AddListener(new Action(() => {
                var playerName = get_selected_player_name();
                if (!string.IsNullOrEmpty(playerName))
                {
                    try
                    {
                        VRC.Player player = null;
                        foreach (var plr in PlayerWrapper.Players)
                        {
                            if (plr == null) continue;

                            var apiUser = plr.field_Private_APIUser_0;
                            if (apiUser == null) continue;

                            if (apiUser.displayName == playerName)
                            {
                                player = plr;
                                break;
                            }
                        }

                        if (player != null)
                        {
                            OnClick?.Invoke(player);
                        }
                    }
                    catch { }
                }
            }));
        }

        var icons = buttonObject.transform.FindChild("Icons");
        if (icons != null)
        {
            icons.gameObject.SetActive(DisplayIcon);
            if (DisplayIcon && sprite != null)
            {
                var icon = icons.FindChild("Icon");
                if (icon != null)
                {
                    var iconObj = icon.gameObject;
                    var imageComponent = iconObj.GetComponent<Image>();
                    if (imageComponent != null)
                    {
                        imageComponent.overrideSprite = sprite;
                    }
                }
            }
        }


        buttonObject.SetActive(true);
        return buttonObject;
    }
    public static GameObject CreateToggle(GameObject parentGrid, string title, Action<VRC.Player> OnEnable, Action<VRC.Player> OnDisable, string tooltip = "", bool DisplayIcon = false, Sprite sprite = null)
    {
        var buttonObject = GameObject.Instantiate(MainMenu.ExampleButton, parentGrid.transform);
        buttonObject.name = $"Button_{Guid.NewGuid()}";

        var text = buttonObject.transform.FindChild("TextLayoutParent/Text_H4");
        if (text != null)
        {
            TextMeshProUGUIEx textMesh = text.gameObject.GetComponent<TextMeshProUGUIEx>();
            textMesh.richText = true;
            textMesh.prop_String_0 = title;
        }

        var tooltips = buttonObject.GetComponents<ToolTip>();
        foreach (var ttcomp in tooltips)
        {
            var lstring = new LocalizableString();
            lstring._localizationKey = tooltip;
            ttcomp._localizableString = lstring;
        }

        var badge = buttonObject.transform.FindChild("Badge_MMJump");
        var badgeRenderer = badge.GetComponent<CanvasRenderer>();

        var buttonComponent = buttonObject.GetComponent<VRCButtonHandle>();
        if (buttonComponent != null)
        {
            badgeRenderer.SetColor(new UnityEngine.Color(1, 0, 0, 1));
            buttonComponent._sendAnalytics = false;

            buttonComponent.m_OnClick.RemoveAllListeners();
            buttonComponent.onClick.RemoveAllListeners();
            buttonComponent.onClick.AddListener(new Action(() => {

                var playerName = get_selected_player_name();
                if (!string.IsNullOrEmpty(playerName))
                {
                    try
                    {
                        var player = PlayerWrapper.Players.Find(plr =>
                            plr?.field_Private_APIUser_0?.displayName == playerName);

                        if (player != null)
                        {
                            bool currentState = ToggleStateManager.ToggleStates.ContainsKey(playerName) && ToggleStateManager.ToggleStates[playerName];
                            bool newState = !currentState;

                            ToggleStateManager.ToggleStates[playerName] = newState;

                            if (newState)
                            {
                                badgeRenderer.SetColor(new UnityEngine.Color(0, 1, 0, 1));
                                OnEnable?.Invoke(player);
                            }
                            else
                            {
                                badgeRenderer.SetColor(new UnityEngine.Color(1, 0, 0, 1));
                                OnDisable?.Invoke(player);
                            }
                        }
                    }
                    catch { }
                }
            }));
        }

        var icons = buttonObject.transform.FindChild("Icons");
        if (icons != null)
        {
            icons.gameObject.SetActive(DisplayIcon);
            if (DisplayIcon && sprite != null)
            {
                var icon = icons.FindChild("Icon");
                if (icon != null)
                {
                    var iconObj = icon.gameObject;
                    var imageComponent = iconObj.GetComponent<Image>();
                    if (imageComponent != null)
                    {
                        imageComponent.overrideSprite = sprite;
                    }
                }
            }
        }


        buttonObject.SetActive(true);
        return buttonObject;
    }


    public static void Setup()
    {
        if (!ui_ready)
        {
            string menuLogoPath = Path.Combine(Directory.GetCurrentDirectory(), "Odium", "ButtonBackground.png");
            Sprite MenuImage = SpriteUtil.LoadFromDisk(menuLogoPath);

            if (MainMenu.ExampleButton == null)
            {
                return;
            }


            if (PageGrid == null)
            {
                PageGrid = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_SelectedUser_Local/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_UserActions");
                return;
            }

            void ReplaceImageExSpritesWithoutIcons(Transform parent, Sprite newSprite)
            {
                foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
                {
                    if (!child.name.ToLower().Contains("background") && !child.name.Contains("Cell_Wallet_Contents"))
                        continue;

                    var image = child.GetComponent<Image>();
                    if (image != null)
                    {
                        image.overrideSprite = newSprite;
                    }

                    var tmpPro = child.GetComponent<TextMeshPro>();
                    if (tmpPro != null)
                    {
                        tmpPro.color = new Color(0.3894f, 0, 1, 1);
                    }

                    var tmpPro2 = child.GetComponent<TextMeshProUGUI>();
                    if (tmpPro2 != null)
                    {
                        tmpPro2.color = new Color(0.3894f, 0, 1, 1);
                    }

                    var tmpPro3 = child.GetComponent<TextMeshProUGUIEx>();
                    if (tmpPro3 != null)
                    {
                        tmpPro3.color = new Color(0.3894f, 0, 1, 1);
                    }

                    var tmpText = child.GetComponent<TMP_Text>();
                    if (tmpText != null)
                    {
                        tmpText.color = new Color(0.3894f, 0, 1, 1);
                    }

                }
            }

            ui_ready = true;
        }
    }
}