using UnityEngine;
using VRC.Localization;

namespace Odium.ButtonAPI.QM
{
    public class QMSliderBase
    {
        protected GameObject slider;
        protected string sliderQMLoc;
        protected Transform parent;
        protected int[] initShift = new int[2];

        public GameObject GetGameObject()
        {
            return slider;
        }

        public void SetActive(bool state)
        {
            slider.gameObject.SetActive(state);
        }

        public void SetLocation(float sliderXLoc, float sliderYLoc)
        {
            // slider.GetComponent<RectTransform>().anchoredPosition += Vector2.right * (220f * (sliderXLoc + initShift[0]));
            slider.GetComponent<RectTransform>().anchoredPosition += Vector2.down * (98f * (sliderYLoc + initShift[1]));
        }

        public void SetTooltip(string tooltip)
        {
            foreach (var c in slider.GetComponents<ToolTip>())
            {
                c._localizableString = LocalizableStringExtensions.Localize(tooltip);
                c._alternateLocalizableString = LocalizableStringExtensions.Localize(tooltip);
            }
        }

        public void DestroyMe()
        {
            try
            {
                UnityEngine.Object.Destroy(slider);
            }
            catch { }
        }
    }
}
