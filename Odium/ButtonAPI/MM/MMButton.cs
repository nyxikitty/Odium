using Odium.ButtonAPI.QM;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VRC.Localization;

namespace Odium.Api.MM
{
    internal class MMButton : UiElement
    {
        protected MMContainer _container;
        protected Button _buttonComp;
        protected Image _buttonImage;
        protected Transform _iconTransform;
        protected Image _iconImage;
        protected TextMeshProUGUIEx _buttonText;

        internal MMButton(MMContainer container, string text, System.Action action, string tooltip = "")
            => Init(container, text, "", action, tooltip, null);

        internal MMButton(MMContainer container, string titleText, string buttonText, System.Action action, string tooltip = "", Sprite icon = null)
            => Init(container, titleText, buttonText, action, tooltip, icon);

        private void Init(MMContainer container, string text, string buttonText, System.Action action, string tooltip, Sprite icon = null)
        {
            _container = container;

            try
            {
                var template = GetButtonTemplate();
                if (template == null)
                {
                    OdiumConsole.Log("MMButton", "Template not found!");
                    return;
                }

                _obj = Object.Instantiate(template, _container.GetPlacementTransform(), false);
                _obj.name = ApiUtils.Identifier + "-Button-" + ApiUtils.RandomNumbers();

                var rightContainer = _obj.transform.Find("RightItemContainer");
                if (rightContainer != null)
                {
                    var buttonTransform = rightContainer.Find("Button (1)");
                    if (buttonTransform != null)
                    {
                        _buttonComp = buttonTransform.GetComponent<Button>();
                        _buttonImage = buttonTransform.GetComponent<Image>();
                        buttonTransform.gameObject.SetActive(true);

                        var buttonTextTransform = buttonTransform.Find("Text_MM_H3");
                        if (buttonTextTransform != null)
                        {
                            _buttonText = buttonTextTransform.GetComponent<TextMeshProUGUIEx>();
                            OdiumConsole.Log("MMButton", $"Found button text component: {_buttonText != null}");
                        }
                        else
                        {
                            OdiumConsole.Log("MMButton", "Button text transform 'Text_MM_H3' not found!");
                            // Try alternative paths
                            buttonTextTransform = buttonTransform.Find("Text");
                            if (buttonTextTransform != null)
                                _buttonText = buttonTextTransform.GetComponent<TextMeshProUGUIEx>();
                        }

                        _iconTransform = buttonTransform.Find("Icon");
                        if (_iconTransform != null)
                        {
                            _iconImage = _iconTransform.GetComponent<Image>();
                            // Enable the icon GameObject by default
                            _iconTransform.gameObject.SetActive(true);
                            if (_iconImage != null)
                                _iconImage.enabled = true;
                        }
                    }
                }

                // Try to find the title in the correct location
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
                            _text = titleTransform.GetComponent<TextMeshProUGUIEx>();
                    }
                }

                _toolTips = _obj.GetComponents<ToolTip>();

                SetText(text);
                SetButtonText(buttonText);
                SetAction(action);
                SetToolTip(tooltip);
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMButton", $"Init Error: {ex.Message}");
            }
        }

        private GameObject GetButtonTemplate()
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
                OdiumConsole.Log("MMButton", $"GetButtonTemplate Error: {ex.Message}");
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
                    // Force text update
                    _text.SetText(newText);
                }
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMButton", $"SetText Error: {ex.Message}");
            }
        }

        internal void SetButtonText(string newText)
        {
            try
            {
                OdiumConsole.Log("MMButton", $"SetButtonText called with: '{newText}', _buttonText is null: {_buttonText == null}");

                if (_buttonText != null)
                {
                    _buttonText.richText = true;
                    if (!string.IsNullOrEmpty(newText))
                    {
                        _buttonText._localizableString = LocalizableStringExtensions.Localize(newText);
                        _buttonText.text = newText;
                        _buttonText.SetText(newText);
                        _buttonText.gameObject.SetActive(true);
                        OdiumConsole.Log("MMButton", $"Button text set to: '{newText}'");
                    }
                    else
                    {
                        _buttonText.text = "";
                        _buttonText.SetText(newText);
                        OdiumConsole.Log("MMButton", "Button text cleared");
                    }
                }
                else
                {
                    OdiumConsole.Log("MMButton", "ERROR: _buttonText is null, cannot set text!");
                }
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMButton", $"SetButtonText Error: {ex.Message}");
            }
        }

        internal void SetAction(System.Action action)
        {
            try
            {
                if (_buttonComp != null)
                {
                    _buttonComp.onClick = new Button.ButtonClickedEvent();
                    if (action != null)
                        _buttonComp.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(action));
                }
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMButton", $"SetAction Error: {ex.Message}");
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
                OdiumConsole.Log("MMButton", $"SetToolTip Error: {ex.Message}");
            }
        }

        internal void SetIcon(Sprite icon)
        {
            try
            {
                if (_iconTransform != null && _iconImage != null)
                {
                    _iconImage.sprite = icon;
                    _iconImage.enabled = icon != null;
                    _iconTransform.gameObject.SetActive(icon != null);
                }
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMButton", $"SetIcon Error: {ex.Message}");
            }
        }

        internal void SetInteractable(bool interactable)
        {
            try
            {
                if (_buttonComp != null)
                    _buttonComp.interactable = interactable;
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMButton", $"SetInteractable Error: {ex.Message}");
            }
        }

        internal void SetButtonColor(Color color)
        {
            try
            {
                if (_buttonImage != null)
                    _buttonImage.color = color;
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMButton", $"SetButtonColor Error: {ex.Message}");
            }
        }
    }
}