using Odium.ButtonAPI.QM;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VRC.Localization;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Odium.Api.MM
{
    internal class MMEnum : UiElement
    {
        protected MMContainer _container;
        protected Button _buttonLeft;
        protected Button _buttonRight;
        protected Image _buttonLeftImage;
        protected Image _buttonRightImage;
        protected TextMeshProUGUIEx _selectionText;
        protected List<string> _options;
        protected int _currentIndex;
        protected System.Action<string, int> _onValueChanged;

        internal MMEnum(MMContainer container, string title, List<string> options, int defaultIndex = 0, System.Action<string, int> onValueChanged = null, string tooltip = "")
            => Init(container, title, options, defaultIndex, onValueChanged, tooltip);

        internal MMEnum(MMContainer container, string title, string[] options, int defaultIndex = 0, System.Action<string, int> onValueChanged = null, string tooltip = "")
            => Init(container, title, options.ToList(), defaultIndex, onValueChanged, tooltip);

        private void Init(MMContainer container, string title, List<string> options, int defaultIndex, System.Action<string, int> onValueChanged, string tooltip)
        {
            _container = container;
            _options = options ?? new List<string> { "Option 1", "Option 2" };
            _currentIndex = Mathf.Clamp(defaultIndex, 0, _options.Count - 1);
            _onValueChanged = onValueChanged;

            try
            {
                var template = GetEnumTemplate();
                if (template == null)
                {
                    OdiumConsole.Log("MMEnum", "Enum template not found!");
                    return;
                }

                _obj = UnityEngine.Object.Instantiate(template, _container.GetPlacementTransform(), false);
                _obj.name = ApiUtils.Identifier + "-Enum-" + ApiUtils.RandomNumbers();

                // Find the title text - try the correct hierarchy path first
                var leftItemContainer = _obj.transform.Find("LeftItemContainer");
                if (leftItemContainer != null)
                {
                    var titleTransform = leftItemContainer.Find("Title");
                    if (titleTransform != null)
                    {
                        _text = titleTransform.GetComponent<TextMeshProUGUIEx>();
                        OdiumConsole.Log("MMEnum", $"Found title text component: {_text != null}");
                    }
                }

                // Find the right container with buttons and selection box
                var rightItemContainer = _obj.transform.Find("RightItemContainer");
                if (rightItemContainer != null)
                {
                    // Find left button
                    var buttonLeftTransform = rightItemContainer.Find("ButtonLeft");
                    if (buttonLeftTransform != null)
                    {
                        _buttonLeft = buttonLeftTransform.GetComponent<Button>();
                        _buttonLeftImage = buttonLeftTransform.GetComponent<Image>();
                        buttonLeftTransform.gameObject.SetActive(true);
                        OdiumConsole.Log("MMEnum", $"Found left button: {_buttonLeft != null}");
                    }

                    // Find right button
                    var buttonRightTransform = rightItemContainer.Find("ButtonRight");
                    if (buttonRightTransform != null)
                    {
                        _buttonRight = buttonRightTransform.GetComponent<Button>();
                        _buttonRightImage = buttonRightTransform.GetComponent<Image>();
                        buttonRightTransform.gameObject.SetActive(true);
                        OdiumConsole.Log("MMEnum", $"Found right button: {_buttonRight != null}");
                    }

                    // Find option selection box
                    var optionSelectionBox = rightItemContainer.Find("OptionSelectionBox");
                    if (optionSelectionBox != null)
                    {
                        var selectionTextTransform = optionSelectionBox.Find("Text_MM_H3");
                        if (selectionTextTransform != null)
                        {
                            _selectionText = selectionTextTransform.GetComponent<TextMeshProUGUIEx>();
                            OdiumConsole.Log("MMEnum", $"Found selection text: {_selectionText != null}");
                        }
                    }
                }

                _toolTips = _obj.GetComponents<ToolTip>();

                // Setup the enum
                SetText(title);
                SetupButtons();
                UpdateDisplay();
                SetToolTip(tooltip);
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMEnum", $"Init Error: {ex.Message}");
            }
        }

        private GameObject GetEnumTemplate()
        {
            try
            {
                // Try to find an enum template in the settings menu
                var menuPath = "UserInterface/Canvas_MainMenu(Clone)/Container/MMParent/HeaderOffset/Menu_Settings/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Content/Viewport/VerticalLayoutGroup/AudioAndVoice/MicrophoneBehavior";
                var template = GameObject.Find(menuPath);

                if (template == null)
                {
                    // Fallback: search for any object with enum structure
                    var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                    foreach (var obj in allObjects)
                    {
                        if (obj.name == "MicrophoneBehavior" &&
                            obj.transform.Find("RightItemContainer/ButtonLeft") != null &&
                            obj.transform.Find("RightItemContainer/ButtonRight") != null &&
                            obj.transform.Find("RightItemContainer/OptionSelectionBox") != null)
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
                OdiumConsole.Log("MMEnum", $"GetEnumTemplate Error: {ex.Message}");
                return null;
            }
        }

        private void SetupButtons()
        {
            try
            {
                // Setup left button (previous option)
                if (_buttonLeft != null)
                {
                    _buttonLeft.onClick = new Button.ButtonClickedEvent();
                    _buttonLeft.onClick.AddListener(new Action(() => {
                        PreviousOption();
                    }));
                }

                // Setup right button (next option)
                if (_buttonRight != null)
                {
                    _buttonRight.onClick = new Button.ButtonClickedEvent();
                    _buttonRight.onClick.AddListener(new Action(() => {
                        NextOption();
                    }));
                }

                OdiumConsole.Log("MMEnum", "Button actions setup complete");
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMEnum", $"SetupButtons Error: {ex.Message}");
            }
        }

        internal void SetText(string newText)
        {
            try
            {
                OdiumConsole.Log("MMEnum", $"SetText called with: '{newText}', _text is null: {_text == null}");

                if (_text != null && !string.IsNullOrEmpty(newText))
                {
                    _text.richText = true;
                    _text._localizableString = LocalizableStringExtensions.Localize(newText);
                    _text.text = newText;
                    _text.SetText(newText);
                    _text.gameObject.SetActive(true);
                    OdiumConsole.Log("MMEnum", $"Enum title set to: '{newText}'");
                }
                else if (_text == null)
                {
                    OdiumConsole.Log("MMEnum", "ERROR: _text is null, cannot set title!");
                }
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMEnum", $"SetText Error: {ex.Message}");
            }
        }

        internal void SetOptions(List<string> newOptions)
        {
            try
            {
                if (newOptions != null && newOptions.Count > 0)
                {
                    _options = newOptions;
                    _currentIndex = Mathf.Clamp(_currentIndex, 0, _options.Count - 1);
                    UpdateDisplay();
                    OdiumConsole.Log("MMEnum", $"Options updated: {_options.Count} options");
                }
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMEnum", $"SetOptions Error: {ex.Message}");
            }
        }

        internal void SetOptions(string[] newOptions)
        {
            SetOptions(newOptions?.ToList());
        }

        internal void SetSelectedIndex(int index)
        {
            try
            {
                if (index >= 0 && index < _options.Count)
                {
                    _currentIndex = index;
                    UpdateDisplay();
                    TriggerValueChanged();
                }
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMEnum", $"SetSelectedIndex Error: {ex.Message}");
            }
        }

        internal void SetSelectedOption(string option)
        {
            try
            {
                int index = _options.IndexOf(option);
                if (index >= 0)
                {
                    SetSelectedIndex(index);
                }
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMEnum", $"SetSelectedOption Error: {ex.Message}");
            }
        }

        internal string GetSelectedOption()
        {
            try
            {
                if (_currentIndex >= 0 && _currentIndex < _options.Count)
                {
                    return _options[_currentIndex];
                }
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMEnum", $"GetSelectedOption Error: {ex.Message}");
            }
            return "";
        }

        internal int GetSelectedIndex()
        {
            return _currentIndex;
        }

        internal void SetValueChangeAction(System.Action<string, int> onValueChanged)
        {
            _onValueChanged = onValueChanged;
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
                OdiumConsole.Log("MMEnum", $"SetToolTip Error: {ex.Message}");
            }
        }

        internal void SetInteractable(bool interactable)
        {
            try
            {
                if (_buttonLeft != null)
                    _buttonLeft.interactable = interactable;

                if (_buttonRight != null)
                    _buttonRight.interactable = interactable;
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMEnum", $"SetInteractable Error: {ex.Message}");
            }
        }

        internal void SetButtonColors(Color leftColor, Color rightColor)
        {
            try
            {
                if (_buttonLeftImage != null)
                    _buttonLeftImage.color = leftColor;

                if (_buttonRightImage != null)
                    _buttonRightImage.color = rightColor;
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMEnum", $"SetButtonColors Error: {ex.Message}");
            }
        }

        internal void SetSelectionTextColor(Color color)
        {
            try
            {
                if (_selectionText != null)
                    _selectionText.color = color;
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMEnum", $"SetSelectionTextColor Error: {ex.Message}");
            }
        }

        private void PreviousOption()
        {
            try
            {
                if (_options.Count == 0) return;

                _currentIndex--;
                if (_currentIndex < 0)
                    _currentIndex = _options.Count - 1; // Wrap around to last option

                UpdateDisplay();
                TriggerValueChanged();

                OdiumConsole.Log("MMEnum", $"Previous option selected: {GetSelectedOption()} (index: {_currentIndex})");
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMEnum", $"PreviousOption Error: {ex.Message}");
            }
        }

        private void NextOption()
        {
            try
            {
                if (_options.Count == 0) return;

                _currentIndex++;
                if (_currentIndex >= _options.Count)
                    _currentIndex = 0; // Wrap around to first option

                UpdateDisplay();
                TriggerValueChanged();

                OdiumConsole.Log("MMEnum", $"Next option selected: {GetSelectedOption()} (index: {_currentIndex})");
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMEnum", $"NextOption Error: {ex.Message}");
            }
        }

        private void UpdateDisplay()
        {
            try
            {
                if (_selectionText != null && _options.Count > 0 && _currentIndex >= 0 && _currentIndex < _options.Count)
                {
                    string displayText = _options[_currentIndex];
                    _selectionText.text = displayText;
                    _selectionText.SetText(displayText);

                    // Update button states
                    if (_buttonLeft != null)
                        _buttonLeft.interactable = _options.Count > 1;

                    if (_buttonRight != null)
                        _buttonRight.interactable = _options.Count > 1;
                }
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMEnum", $"UpdateDisplay Error: {ex.Message}");
            }
        }

        private void TriggerValueChanged()
        {
            try
            {
                if (_onValueChanged != null && _currentIndex >= 0 && _currentIndex < _options.Count)
                {
                    _onValueChanged.Invoke(_options[_currentIndex], _currentIndex);
                }
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMEnum", $"TriggerValueChanged Error: {ex.Message}");
            }
        }
    }
}