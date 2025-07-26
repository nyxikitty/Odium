using Odium.ButtonAPI.QM;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VRC.Localization;
using TMPro;

namespace Odium.Api.MM
{
    internal class MMLabel : UiElement
    {
        protected MMContainer _container;
        protected Button _buttonComp;
        protected Transform _iconTransform;

        internal MMLabel(MMContainer container, string text, string tooltip = "")
            => Init(container, text, tooltip);

        private void Init(MMContainer container, string text, string tooltip)
        {
            _container = container;

            try
            {
                var template = GetLabelTemplate();
                if (template == null)
                {
                    OdiumConsole.Log("MMLabel", "Template not found!");
                    return;
                }

                _obj = Object.Instantiate(template, _container.GetPlacementTransform(), false);
                _obj.name = ApiUtils.Identifier + "-Label-" + ApiUtils.RandomNumbers();

                // Disable the button component so it's not clickable
                var rightContainer = _obj.transform.Find("RightItemContainer");
                if (rightContainer != null)
                {
                    var buttonTransform = rightContainer.Find("Button (1)");
                    if (buttonTransform != null)
                    {
                        _buttonComp = buttonTransform.GetComponent<Button>();
                        if (_buttonComp != null)
                        {
                            _buttonComp.enabled = false;
                            _buttonComp.interactable = false;
                        }

                        // Hide the button completely
                        buttonTransform.gameObject.SetActive(false);

                        // Also hide the icon if it exists
                        _iconTransform = buttonTransform.Find("Icon");
                        if (_iconTransform != null)
                        {
                            _iconTransform.gameObject.SetActive(false);
                        }
                    }
                }

                // Find and setup the title text
                var titleMainContainer = _obj.transform.Find("TitleMainContainer");
                if (titleMainContainer != null)
                {
                    var titlesContainer = titleMainContainer.Find("TitlesContainer");
                    if (titlesContainer != null)
                    {
                        var titleTransform = titlesContainer.Find("Title");
                        if (titleTransform != null)
                            _text = titleTransform.GetComponent<TextMeshProUGUIEx>();
                    }
                }

                // Fallback: Try the old path in case structure varies
                if (_text == null)
                {
                    var leftContainer = _obj.transform.Find("LeftItemContainer");
                    if (leftContainer != null)
                    {
                        var titleTransform = leftContainer.Find("Title");
                        if (titleTransform != null)
                        {
                            _text = titleTransform.GetComponent<TextMeshProUGUIEx>();

                            // Move the text out of LeftItemContainer to the main object for full-width centering
                            if (_text != null)
                            {
                                // Disable the left container since we don't want left-aligned layout
                                leftContainer.gameObject.SetActive(false);

                                // Move text to the main object and make it span full width
                                _text.transform.SetParent(_obj.transform, false);

                                // Setup the text to fill the entire width
                                var textRect = _text.GetComponent<RectTransform>();
                                if (textRect != null)
                                {
                                    textRect.anchorMin = new Vector2(0f, 0f);
                                    textRect.anchorMax = new Vector2(1f, 1f);
                                    textRect.offsetMin = Vector2.zero;
                                    textRect.offsetMax = Vector2.zero;
                                    textRect.anchoredPosition = Vector2.zero;
                                }
                            }
                        }
                    }
                }

                // Center align the text
                if (_text != null)
                {
                    _text.alignment = TextAlignmentOptions.Center;
                    _text.horizontalAlignment = HorizontalAlignmentOptions.Center;
                    _text.verticalAlignment = VerticalAlignmentOptions.Middle;
                }

                _toolTips = _obj.GetComponents<ToolTip>();

                SetText(text);
                SetToolTip(tooltip);
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMLabel", $"Init Error: {ex.Message}");
            }
        }

        private GameObject GetLabelTemplate()
        {
            try
            {
                var menuPath = "UserInterface/Canvas_MainMenu(Clone)/Container/MMParent/HeaderOffset/Menu_Settings/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Content/Viewport/VerticalLayoutGroup/AudioAndVoice/AudioVolume/Settings_Panel_1/VerticalLayoutGroup/OtherUsersVolume";
                var template = GameObject.Find(menuPath);

                if (template == null)
                {
                    var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                    foreach (var obj in allObjects)
                    {
                        if (obj.name == "OtherUsersVolume" && obj.transform.Find("RightItemContainer/Button (1)") != null)
                        {
                            template = obj;
                            break;
                        }
                    }
                }

                return template;
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMLabel", $"GetLabelTemplate Error: {ex.Message}");
                return null;
            }
        }

        internal void SetText(string newText)
        {
            try
            {
                if (_text != null && !string.IsNullOrEmpty(newText))
                {
                    _text.richText = true;
                    _text._localizableString = LocalizableStringExtensions.Localize(newText);
                    _text.text = newText;
                    _text.SetText(newText);

                    // Ensure center alignment is maintained
                    _text.alignment = TextAlignmentOptions.Center;
                    _text.horizontalAlignment = HorizontalAlignmentOptions.Center;
                    _text.verticalAlignment = VerticalAlignmentOptions.Middle;
                }
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMLabel", $"SetText Error: {ex.Message}");
            }
        }

        internal void SetToolTip(string newToolTip)
        {
            try
            {
                if (!string.IsNullOrEmpty(newToolTip) && _toolTips != null)
                {
                    var ls = LocalizableStringExtensions.Localize(newToolTip);
                    foreach (var t in _toolTips)
                    {
                        if (t != null)
                        {
                            t._localizableString = ls;
                            t._alternateLocalizableString = ls;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMLabel", $"SetToolTip Error: {ex.Message}");
            }
        }

        internal void SetTextColor(Color color)
        {
            try
            {
                if (_text != null)
                    _text.color = color;
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMLabel", $"SetTextColor Error: {ex.Message}");
            }
        }

        internal void SetFontSize(float fontSize)
        {
            try
            {
                if (_text != null)
                    _text.fontSize = fontSize;
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMLabel", $"SetFontSize Error: {ex.Message}");
            }
        }

        internal void SetTextAlignment(TextAlignmentOptions alignment)
        {
            try
            {
                if (_text != null)
                    _text.alignment = alignment;
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMLabel", $"SetTextAlignment Error: {ex.Message}");
            }
        }
    }
}