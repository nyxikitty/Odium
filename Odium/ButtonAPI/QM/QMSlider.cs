
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnhollowerRuntimeLib;
using System;

namespace Odium.ButtonAPI.QM
{
    public class QMSlider : QMSliderBase
    {
        public QMSlider(QMMenuBase sliderMenu, float sliderXLocation, float sliderYLocation, Action<float> onSlide, string text, float defaultValue, float minValue, float maxValue)
        {
            sliderQMLoc = sliderMenu.GetMenuName();
            Initialize(sliderXLocation, sliderYLocation, onSlide, text, defaultValue, minValue, maxValue);
        }

        private Slider SliderOptions;
        private TextMeshProUGUIEx SliderTextOptions;
        private TextMeshProUGUIEx SliderCurrentValueText;
        private Transform ResetButton;

        private void Initialize(float sliderXLocation, float sliderYLocation, Action<float> onSlide, string text, float defaultValue, float minValue, float maxValue)
        {
            if (parent == null)
                parent = ApiUtils.QuickMenu.transform.Find("CanvasGroup/Container/Window/QMParent/" + sliderQMLoc).transform;
            slider = UnityEngine.Object.Instantiate(ApiUtils.GetQMSliderTemplate(), parent, true);

            Transform rightObjects = slider.transform.Find("RightItemContainer");
            Transform leftObjects = slider.transform.Find("LeftItemContainer");

            SliderOptions = rightObjects.GetComponentInChildren<Slider>();
            SliderCurrentValueText = rightObjects.GetComponentInChildren<TextMeshProUGUIEx>();

            rightObjects.Find("Cell_MM_ToggleSwitch").gameObject.SetActive(false);
            ResetButton = rightObjects.Find("Button");

            ResetButton.gameObject.SetActive(true);
            SetResetAction( () => { SliderOptions.value = defaultValue; });

            slider.name = $"{ApiUtils.Identifier}-Slider-{ApiUtils.RandomNumbers()}";

            slider.GetComponent<RectTransform>().sizeDelta = new Vector2(950f, 50f);
            slider.GetComponent<RectTransform>().anchoredPosition = new Vector2(40f, -310f);

            initShift[0] = 0;
            initShift[1] = 0;
            SetLocation(sliderXLocation, sliderYLocation);
            SetDefaultValue(defaultValue);
            SetMinMax(minValue, maxValue);
            SetCallback(onSlide);
            SetActive(true);

            // placeholder text
            SliderTextOptions = leftObjects.GetComponentInChildren<TextMeshProUGUIEx>();

            if (!SliderTextOptions)
                OdiumConsole.Log("Slider", "Cant get slider text");

            SliderTextOptions.prop_String_0 = text;
        }

        public void SetResetAction(Action buttonAction)
        {
            ResetButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            if (buttonAction != null)
                ResetButton.GetComponent<Button>().onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(buttonAction));
        }

        public void SetMinMax(float min, float max)
        {
            SliderOptions.minValue = min;
            SliderOptions.maxValue = max;
        }

        public void SetDefaultValue(float value)
        {
            SliderOptions.value = value;
            SliderCurrentValueText.prop_String_0 = value.ToString();
        }

        public void SetCallback(Action<float> sliderAction)
        {
            SliderOptions.onValueChanged.AddListener(new Action<float>( value => { SliderCurrentValueText.prop_String_0 = Math.Round(value, 2).ToString(); })); // refresh text value
            SliderOptions.onValueChanged.AddListener(sliderAction);
        }

        public void SetInteractable(bool newState)
        {
            SliderOptions.interactable = newState;
            RefreshSlider();
        }

        private void RefreshSlider()
        {
            slider.SetActive(false);
            slider.SetActive(true);
        }
    }
}
