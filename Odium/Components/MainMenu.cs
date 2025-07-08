using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using VRC.Localization;
using VRC.UI.Controls;
using System.IO;
using UnityEngine.UI;
using Odium.Modules;
using Odium.Components;
using Odium.Odium;
using Odium.Patches;
using Odium.Wrappers;
using System.Linq.Expressions;
using System.Collections;
using MelonLoader;

namespace Odium.UX
{
    internal class MainMenu
    {
        public static GameObject ExampleButton;
        public static GameObject AdBanner, QuickLinksHeader, QuickActionsHeader;
        public static GameObject LinksButtons, ActionsButtons, buttonsQuickLinksGrid, buttonsQuickActionsGrid, LaunchPadText;
        public static GameObject ConsoleParent, ConsoleTemplate;
        public static GameObject ConsoleObject;
        public static GameObject SafteyButton;
        public static GameObject UserInterface;

        public static bool ui_ready = false;

        private static List<string> processed_buttons = new List<string>();
        private static DateTime lastGradientChange, lastTimeCheck = DateTime.Now;
        private static float gradientShift = 0f;
        public static GameObject MenuInstance;
        public static MenuStateController _menuStateController;

        public static MenuStateController MenuStateControllerInstance
        {
            get
            {
                if (_menuStateController == null)
                    _menuStateController = MenuInstance.GetComponent<MenuStateController>();
                return _menuStateController;
            }
        }


        public static void Setup()
        {
            var currentTime = DateTime.Now;
            if (LaunchPadText == null)
            {
                LaunchPadText = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/Header_H1/LeftItemContainer/Text_Title");
                return;
            }

            if (ConsoleTemplate == null)
            {
                ConsoleTemplate = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickLinks/Button_Worlds");
                return;
            }

            if (ConsoleParent == null)
            {
                ConsoleParent = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Carousel_Banners");
                return;
            }

            if (AdBanner == null)
            {
                AdBanner = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Carousel_Banners/Image_MASK/Image");
                return;
            }

            if (QuickActionsHeader == null)
            {
                QuickActionsHeader = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Header_QuickActions");
                return;
            }

            if (QuickLinksHeader == null)
            {
                QuickLinksHeader = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Header_QuickLinks");
                return;
            }

            if (LinksButtons == null)
            {
                LinksButtons = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickLinks");
                return;
            }

            if (ActionsButtons == null)
            {
                ActionsButtons = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickActions");
                return;
            }


            if (buttonsQuickLinksGrid == null)
            {
                buttonsQuickLinksGrid = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickLinks");
                return;
            }

            if (buttonsQuickActionsGrid == null)
            {
                buttonsQuickActionsGrid = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickActions");
                return;
            }

            if (SafteyButton == null)
            {
                SafteyButton = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickActions/Button_Safety");
            }


            if (!ui_ready)
            {
                QuickActionsHeader.SetActive(false);
                // QuickLinksHeader.SetActive(false);
                AdBanner.SetActive(false);

                InternalConsole.LogIntoConsole("MainMenu Ready");

                ConsoleObject = GameObject.Instantiate(ConsoleTemplate, ConsoleParent.transform);
                InitConsole(ConsoleObject);

                SetupButton(SafteyButton, "Button_Safety");

                ExampleButton = GameObject.Instantiate(ConsoleTemplate, ConsoleParent.transform);
                ExampleButton.name = "ExampleButton";
                ExampleButton.SetActive(false);
                // InitPage();

                GameObject originalButton = GameObject.Find("UserInterface/Canvas_MainMenu(Clone)/Container/PageButtons/HorizontalLayoutGroup/Marketplace_Button_Tab");

                GameObject newButton = GameObject.Instantiate(originalButton, originalButton.transform.parent);

                newButton.name = "Heart_Button_Tab";

                Transform iconTransform = newButton.transform.Find("Icon");
                if (iconTransform != null)
                {
                    GameObject iconGameObject = iconTransform.gameObject;
                    Image imageEx = iconGameObject.GetComponent<Image>();
                    if (imageEx != null)
                    {
                        string LogoPath = Path.Combine(Directory.GetCurrentDirectory(), "Odium", "TabImage.png");
                        Sprite Logo = SpriteUtil.LoadFromDisk(LogoPath);
                        imageEx.sprite = Logo;
                    }
                }

                Transform textTransform = newButton.transform.Find("Text_H4");
                if (textTransform != null)
                {
                    GameObject textGameObject = textTransform.gameObject;
                    TextMeshProUGUIEx textEx = textGameObject.GetComponent<TextMeshProUGUIEx>();
                    if (textEx != null)
                    {
                        textEx.text = "Odium Client";
                    }
                }

                var uselessComponent = newButton.GetComponent<VRC.UI.Client.Marketplace.SubscriptionNotifierComponent>();
                if (uselessComponent != null)
                {
                    uselessComponent.enabled = false;
                }

                MenuInstance = GameObject.Find("UserInterface/Canvas_MainMenu(Clone)");

                var originalPage = GameObject.Find("UserInterface/Canvas_MainMenu(Clone)/Container/MMParent/HeaderOffset/Menu_MM_Marketplace");
                var CustomPage = GameObject.Instantiate(originalPage, originalPage.transform.parent);
                var CustomPageComponent = CustomPage.GetComponent<MonoBehaviour1PublicOb_sGa_ppaObwapuBuObUnique>();

                var MenuControllerObject = GameObject.Find("UserInterface/Canvas_MainMenu(Clone)");
                var MenuController = MenuControllerObject.GetComponent<VRC.UI.Controls.MenuStateController>();
                MenuController.field_Private_Dictionary_2_String_UIPage_0.Add("XD", CustomPageComponent);
                MenuController.field_Private_HashSet_1_UIPage_0.Add(CustomPageComponent);

                var menuTab = newButton.GetComponent<VRC.UI.Elements.Controls.MenuTab>();
                menuTab._sendAnalytics = false;
                menuTab.prop_String_0 = "Odium";

                UnityEngine.UI.Button button = newButton.GetComponent<UnityEngine.UI.Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();

                    button.onClick.AddListener(new Action(() => {
                        CustomPage.SetActive(true);
                    }));
                }
            }

            if (ConsoleObject != null)
            {
                ConsoleObject.transform.localPosition = new UnityEngine.Vector3(0, -272, 0);
            }

            if (LaunchPadText != null)
            {
                if ((currentTime - lastGradientChange).TotalMilliseconds >= 250)
                {
                    var TextComponent = LaunchPadText.GetComponent<TextMeshProUGUIEx>();
                    if (TextComponent != null)
                    {
                        TextComponent.enableVertexGradient = true;
                        gradientShift += Time.deltaTime * 0.5f;
                        gradientShift += Time.deltaTime * 0.5f;

                        float tl = Mathf.Sin(gradientShift + 0f) * 0.5f + 1.5f;
                        float tr = Mathf.Sin(gradientShift + 1f) * 0.5f + 1.5f;
                        float bl = Mathf.Sin(gradientShift + 2f) * 0.5f + 1.5f;
                        float br = Mathf.Sin(gradientShift + 3f) * 0.5f + 1.5f;

                        VertexGradient gradient = new VertexGradient(
                            new Color(tl, tl, tl),
                            new Color(tr, tr, tr),
                            new Color(bl, bl, bl),
                            new Color(br, br, br)
                        );


                        TextComponent.colorGradient = gradient;
                    }
                    lastGradientChange = currentTime;
                }

                var launchPadText = LaunchPadText.GetComponent<TextMeshProUGUIEx>();
                if (launchPadText != null)
                {
                    SetText(launchPadText);
                }
            }

            var Links_childCount = buttonsQuickLinksGrid.transform.GetChildCount();
            for (int i = 0; i < Links_childCount; i++)
            {
                var child = buttonsQuickLinksGrid.transform.GetChild(i);
                if (child == null) continue;

                string btnName = child.gameObject.name;
                if (btnName.Contains("Button_Worlds")) child.localPosition = new UnityEngine.Vector3(-348, 70, 0);
                else if (btnName.Contains("Button_Avatars")) child.localPosition = new UnityEngine.Vector3(-116, 70, 0);
                else if (btnName.Contains("Button_Social")) child.localPosition = new UnityEngine.Vector3(116, 70, 0);
                else if (btnName.Contains("Button_ViewGroups")) child.localPosition = new UnityEngine.Vector3(348, 70, 0);

                if (!ui_ready)
                {
                    SetupButton(child.gameObject);
                    processed_buttons.Add(btnName);
                }
                else if (!processed_buttons.Contains(btnName))
                {
                    SetupButton(child.gameObject);
                }
            }

            var Actions_childCount = buttonsQuickActionsGrid.transform.GetChildCount();
            for (int i = 0; i < Links_childCount; i++)
            {
                var child = buttonsQuickActionsGrid.transform.GetChild(i);
                if (child == null) continue;

                string btnName = child.gameObject.name;
                if (btnName.Contains("Button_GoHome")) child.localPosition = new UnityEngine.Vector3(-225, -15, 0);
                else if (btnName.Contains("Button_Respawn")) child.localPosition = new UnityEngine.Vector3(0, -15, 0);
                else if (btnName.Contains("Button_SelectUser")) child.localPosition = new UnityEngine.Vector3(225, -15, 0);

                if (!ui_ready)
                {
                    SetupButton(child.gameObject);
                    processed_buttons.Add(btnName);
                }
                else if (!processed_buttons.Contains(btnName))
                {
                    SetupButton(child.gameObject);
                }
                else if (btnName.Contains("Button_Safety"))
                {
                    SetupButton(child.gameObject);
                }
            }

            LinksButtons.transform.localPosition = new UnityEngine.Vector3(0, -100, 0);
            ActionsButtons.transform.localPosition = new UnityEngine.Vector3(0, -780, 0);
            GameObject safetyButton = AssignedVariables.userInterface.transform.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickActions/Button_Safety").gameObject;
            safetyButton.gameObject.SetActive(false);
            ui_ready = true;
        }

        public static void SetText(TextMeshProUGUIEx textComponent)
        {
            textComponent.text = "Odium";
        }

        private static void SetupButton(GameObject button, string name = "")
        {
            var background = button.transform.FindChild("Background");
            if (background != null)
            {
                var rectTransform = background.gameObject.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = new UnityEngine.Vector2(0, -80);
                }
            }

            var icons = button.transform.FindChild("Icons");
            if (icons != null)
            {
                icons.gameObject.SetActive(false);
            }

            var badge = button.transform.FindChild("Badge_MMJump");
            if (badge != null)
            {
                badge.gameObject.SetActive(false);
            }
        }

        private static void InitConsole(GameObject consoleClone)
        {
            var background = consoleClone.transform.FindChild("Background");
            if (background != null)
            {
                var rectTransform = background.gameObject.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = new UnityEngine.Vector2(725, 480);
                }

                var renderComponent = background.gameObject.GetComponent<Image>();
                if (renderComponent != null)
                {

                    string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "Odium", "QMConsole.png");
                    var sprite = SpriteLoader.LoadFromDisk(logoPath);

                    renderComponent.overrideSprite = sprite;
                }

            }

            var icons = consoleClone.transform.FindChild("Icons");
            if (icons != null)
            {
                icons.gameObject.SetActive(false);
            }


            var badge = consoleClone.transform.FindChild("Badge_MMJump");
            if (badge != null)
            {
                badge.gameObject.SetActive(false);
            }

            var text = consoleClone.transform.FindChild("TextLayoutParent/Text_H4");
            if (text != null)
            {
                var rectTransform = text.gameObject.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.localPosition = new UnityEngine.Vector3(0, 250, 0);
                    rectTransform.sizeDelta = new UnityEngine.Vector2(650, 100);
                }

                TextMeshProUGUIEx textMesh = text.gameObject.GetComponent<TextMeshProUGUIEx>();
                textMesh.alignment = TextAlignmentOptions.TopLeft;
                textMesh.richText = true;
                textMesh.prop_String_0 = "";
            }

            var tooltips = consoleClone.GetComponents<ToolTip>();
            foreach (var tooltip in tooltips)
            {
                var lstring = new LocalizableString();
                lstring._localizationKey = "[ CONSOLE ]";
                tooltip._localizableString = lstring;
            }

            InternalConsole.LogIntoConsole("Console Ready");
        }

        private static void ResizeButton(Transform button)
        {
            Transform bg = button.Find("Background");
            if (bg != null)
            {
                RectTransform rect = bg.GetComponent<RectTransform>();
                if (rect != null)
                    rect.sizeDelta = new UnityEngine.Vector2(0, -90f);
            }

            Transform icon = button.Find("Icons/Icon");
            if (icon != null)
            {
                icon.localPosition -= new Vector3(0, 50f, 0);
                icon.localScale = UnityEngine.Vector3.zero;
            }

            Transform text = button.Find("TextLayoutParent/Text_H4");
            if (text != null)
            {
                text.localPosition += new Vector3(0, 50f, 0);
                text.localScale = UnityEngine.Vector3.one;
            }

            Transform badge = button.Find("Badge_MMJump");
            if (badge != null)
            {
                badge.localScale = UnityEngine.Vector3.zero;
            }
        }
    }

    internal class InternalConsole
    {
        private static List<string> ConsoleLogCache = new List<string>();

        public static void LogIntoConsole(string txt, string type = "<color=#A500FE>[Log]</color>", string color = "A500FE")
        {
            var time = DateTime.Now.ToString("HH:mm");
            var sb = new StringBuilder();
            sb.Append($" <size=19><color=#{color}>[{time}]</color> ");
            sb.Append(type);
            sb.Append(" ");
            sb.Append(txt);
            sb.Append("</color>");

            ConsoleLogCache.Add(sb.ToString());
        }

        public static void ProcessLogCache()
        {
            try {
                if (ConsoleLogCache.Count > 25)
                {
                    ConsoleLogCache.RemoveRange(0, ConsoleLogCache.Count - 21);
                }

                int paddingLines = 20 - ConsoleLogCache.Count;
                var logs = new StringBuilder("\n\n");
                for (int i = 0; i < paddingLines; i++) logs.AppendLine();

                foreach (var entry in ConsoleLogCache)
                {
                    logs.AppendLine(entry + "</color>");
                }

                MainMenu.ConsoleObject.SetActive(false);
                var text = MainMenu.ConsoleObject.transform.FindChild("TextLayoutParent/Text_H4");
                if (text != null)
                {
                    var rectTransform = text.gameObject.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.localPosition = new UnityEngine.Vector3(0, 250, 0);
                        rectTransform.sizeDelta = new UnityEngine.Vector2(650, 100);
                    }

                    TextMeshProUGUIEx textMesh = text.gameObject.GetComponent<TextMeshProUGUIEx>();
                    textMesh.alignment = TextAlignmentOptions.TopLeft;
                    textMesh.richText = true;
                    textMesh.enableWordWrapping = true;
                    textMesh.prop_String_0 = logs.ToString();
                }
                MainMenu.ConsoleObject.SetActive(true);
            } catch (Exception ex)
            {

            }
        }
    }
}
