using Odium.ButtonAPI.QM;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VRC.Localization;

namespace Odium.Api.MM
{
    internal class MMBigSlider : UiElement
    {
        protected MMContainer _container;
        protected Slider _sliderComp;
        protected Image _sliderBackground;
        protected Image _fillArea;
        protected Image _handle;
        protected TextMeshProUGUIEx _valueText;

        internal MMBigSlider(MMContainer container, string text, float defaultValue, float minValue, float maxValue, System.Action<float> onValueChanged, string tooltip = "")
            => Init(container, text, defaultValue, minValue, maxValue, onValueChanged, tooltip);

        private void Init(MMContainer container, string text, float defaultValue, float minValue, float maxValue, System.Action<float> onValueChanged, string tooltip)
        {
            _container = container;

            try
            {
                var template = GetSliderTemplate();
                if (template == null)
                {
                    OdiumConsole.Log("MMBigSlider", "Slider template not found!");
                    return;
                }

                _obj = Object.Instantiate(template, _container.GetPlacementTransform(), false);
                _obj.name = ApiUtils.Identifier + "-BigSlider-" + ApiUtils.RandomNumbers();

                var titleMainContainer = _obj.transform.Find("TitleMainContainer");
                if (titleMainContainer != null)
                {
                    var titlesContainer = titleMainContainer.Find("TitlesContainer");
                    if (titlesContainer != null)
                    {
                        var titleTransform = titlesContainer.Find("Title");
                        if (titleTransform != null)
                        {
                            _text = titleTransform.GetComponent<TextMeshProUGUIEx>();
                            OdiumConsole.Log("MMBigSlider", $"Found title text component in TitleMainContainer: {_text != null}");
                        }
                    }
                }

                if (_text == null)
                {
                    var topContainer = _obj.transform.Find("TopContainer");
                    if (topContainer != null)
                    {
                        var titleTransform = topContainer.Find("Title");
                        if (titleTransform != null)
                        {
                            _text = titleTransform.GetComponent<TextMeshProUGUIEx>();
                            OdiumConsole.Log("MMBigSlider", $"Found title text component in TopContainer: {_text != null}");
                        }
                    }
                }

                if (_text == null)
                {
                    OdiumConsole.Log("MMBigSlider", "Title not found in expected paths, searching all children...");
                    for (int i = 0; i < _obj.transform.childCount; i++)
                    {
                        var child = _obj.transform.GetChild(i);
                        OdiumConsole.Log("MMBigSlider", $"  Child {i}: {child.name}");

                        var titleInChild = child.Find("Title");
                        if (titleInChild != null)
                        {
                            _text = titleInChild.GetComponent<TextMeshProUGUIEx>();
                            if (_text != null)
                            {
                                OdiumConsole.Log("MMBigSlider", $"Found title in child: {child.name}");
                                break;
                            }
                        }

                        for (int j = 0; j < child.childCount; j++)
                        {
                            var nestedChild = child.GetChild(j);
                            var nestedTitle = nestedChild.Find("Title");
                            if (nestedTitle != null)
                            {
                                _text = nestedTitle.GetComponent<TextMeshProUGUIEx>();
                                if (_text != null)
                                {
                                    OdiumConsole.Log("MMBigSlider", $"Found title in nested path: {child.name}/{nestedChild.name}");
                                    break;
                                }
                            }
                        }

                        if (_text != null) break;
                    }
                }

                var bottomContainer = _obj.transform.Find("BottomContainer");
                if (bottomContainer != null)
                {
                    var sliderTransform = bottomContainer.Find("Slider");
                    if (sliderTransform != null)
                    {
                        _sliderComp = sliderTransform.GetComponent<Slider>();
                        _sliderBackground = sliderTransform.GetComponent<Image>();

                        var backgroundTransform = sliderTransform.Find("Background");
                        if (backgroundTransform != null)
                            _sliderBackground = backgroundTransform.GetComponent<Image>();

                        var fillAreaTransform = sliderTransform.Find("Fill Area");
                        if (fillAreaTransform != null)
                        {
                            var fillTransform = fillAreaTransform.Find("Fill");
                            if (fillTransform != null)
                                _fillArea = fillTransform.GetComponent<Image>();
                        }

                        var handleSlideAreaTransform = sliderTransform.Find("Handle Slide Area");
                        if (handleSlideAreaTransform != null)
                        {
                            var handleTransform = handleSlideAreaTransform.Find("Handle");
                            if (handleTransform != null)
                                _handle = handleTransform.GetComponent<Image>();
                        }

                        var valueTextTransform = sliderTransform.Find("Text_MM_H3");
                        if (valueTextTransform != null)
                            _valueText = valueTextTransform.GetComponent<TextMeshProUGUIEx>();
                    }
                }

                _toolTips = _obj.GetComponents<ToolTip>();

                SetText(text);
                SetSliderRange(minValue, maxValue);
                SetSliderValue(defaultValue);
                SetValueChangeAction(onValueChanged);
                SetToolTip(tooltip);
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMBigSlider", $"Init Error: {ex.Message}");
            }
        }

        private GameObject GetSliderTemplate()
        {
            try
            {
                var menuPath = "UserInterface/Canvas_MainMenu(Clone)/Container/MMParent/HeaderOffset/Menu_Settings/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Content/Viewport/VerticalLayoutGroup/AudioAndVoice/MicOutputVolume";
                var template = GameObject.Find(menuPath);

                if (template == null)
                {
                    var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                    foreach (var obj in allObjects)
                    {
                        if (obj.name == "MicOutputVolume" && obj.transform.Find("BottomContainer/Slider") != null)
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
                OdiumConsole.Log("MMBigSlider", $"GetSliderTemplate Error: {ex.Message}");
                return null;
            }
        }

        internal void SetText(string newText)
        {
            try
            {
                OdiumConsole.Log("MMBigSlider", $"SetText called with: '{newText}', _text is null: {_text == null}");

                if (_text != null && !string.IsNullOrEmpty(newText))
                {
                    _text.richText = true;
                    _text._localizableString = LocalizableStringExtensions.Localize(newText);
                    _text.text = newText;
                    _text.SetText(newText);
                    // Make sure the text component is active
                    _text.gameObject.SetActive(true);
                    OdiumConsole.Log("MMBigSlider", $"Slider title set to: '{newText}'");
                }
                else if (_text == null)
                {
                    OdiumConsole.Log("MMBigSlider", "ERROR: _text is null, cannot set title!");
                }
                else if (string.IsNullOrEmpty(newText))
                {
                    OdiumConsole.Log("MMBigSlider", "ERROR: newText is null or empty!");
                }
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMBigSlider", $"SetText Error: {ex.Message}");
            }
        }

        internal void SetSliderValue(float value)
        {
            try
            {
                if (_sliderComp != null)
                {
                    _sliderComp.value = value;
                    UpdateValueText(value);
                }
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMBigSlider", $"SetSliderValue Error: {ex.Message}");
            }
        }

        internal void SetSliderRange(float minValue, float maxValue)
        {
            try
            {
                if (_sliderComp != null)
                {
                    _sliderComp.minValue = minValue;
                    _sliderComp.maxValue = maxValue;
                }
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMBigSlider", $"SetSliderRange Error: {ex.Message}");
            }
        }

        internal void SetValueChangeAction(System.Action<float> onValueChanged)
        {
            try
            {
                if (_sliderComp != null)
                {
                    _sliderComp.onValueChanged = new Slider.SliderEvent();
                    if (onValueChanged != null)
                    {
                        _sliderComp.onValueChanged.AddListener(DelegateSupport.ConvertDelegate<UnityAction<float>>(onValueChanged));
                    }
                    // Also update the value text when slider changes
                    System.Action<float> updateTextAction = UpdateValueText;
                    _sliderComp.onValueChanged.AddListener(DelegateSupport.ConvertDelegate<UnityAction<float>>(updateTextAction));
                }
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMBigSlider", $"SetValueChangeAction Error: {ex.Message}");
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
                OdiumConsole.Log("MMBigSlider", $"SetToolTip Error: {ex.Message}");
            }
        }

        internal void SetInteractable(bool interactable)
        {
            try
            {
                if (_sliderComp != null)
                    _sliderComp.interactable = interactable;
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMBigSlider", $"SetInteractable Error: {ex.Message}");
            }
        }

        internal void SetSliderColors(Color backgroundColor, Color fillColor, Color handleColor)
        {
            try
            {
                if (_sliderBackground != null)
                    _sliderBackground.color = backgroundColor;

                if (_fillArea != null)
                    _fillArea.color = fillColor;

                if (_handle != null)
                    _handle.color = handleColor;
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMBigSlider", $"SetSliderColors Error: {ex.Message}");
            }
        }

        internal void SetFillColor(Color fillColor)
        {
            try
            {
                if (_fillArea != null)
                    _fillArea.color = fillColor;
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMBigSlider", $"SetFillColor Error: {ex.Message}");
            }
        }

        internal void SetHandleColor(Color handleColor)
        {
            try
            {
                if (_handle != null)
                    _handle.color = handleColor;
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMBigSlider", $"SetHandleColor Error: {ex.Message}");
            }
        }

        internal void SetWholeNumbers(bool wholeNumbers)
        {
            try
            {
                if (_sliderComp != null)
                    _sliderComp.wholeNumbers = wholeNumbers;
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMBigSlider", $"SetWholeNumbers Error: {ex.Message}");
            }
        }

        internal float GetValue()
        {
            try
            {
                return _sliderComp?.value ?? 0f;
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMBigSlider", $"GetValue Error: {ex.Message}");
                return 0f;
            }
        }

        private void UpdateValueText(float value)
        {
            try
            {
                if (_valueText != null)
                {
                    string valueString = _sliderComp != null && _sliderComp.wholeNumbers
                        ? ((int)value).ToString()
                        : value.ToString("F2");

                    _valueText.text = valueString;
                    _valueText.SetText(valueString);
                }
            }
            catch (System.Exception ex)
            {
                OdiumConsole.Log("MMBigSlider", $"UpdateValueText Error: {ex.Message}");
            }
        }
    }
}