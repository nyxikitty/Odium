using UnityEngine;
using UnityEngine.Windows;
using VRC.Localization;

namespace Odium.ButtonAPI.QM
{
    public class QMEnumBase
    {
        protected GameObject enumHolder;
        protected string enumQMLoc;
        protected Transform parent;
        protected int[] initShift = new int[2];

        public GameObject GetGameObject()
        {
            return enumHolder;
        }

        public void SetActive(bool state)
        {
            enumHolder.gameObject.SetActive(state);
        }

        public void SetLocation(float enumXLoc, float enumYLoc)
        {
            enumHolder.GetComponent<RectTransform>().anchoredPosition += Vector2.right * (960f * (enumXLoc + initShift[0]));
            enumHolder.GetComponent<RectTransform>().anchoredPosition += Vector2.down * (110f * (enumYLoc + initShift[1]));
        }

        public void SetTooltip(string tooltip)
        {
            foreach (var c in enumHolder.GetComponents<ToolTip>())
            {
                c._localizableString = LocalizableStringExtensions.Localize(tooltip);
                c._alternateLocalizableString = LocalizableStringExtensions.Localize(tooltip);
            }
        }

        public void DestroyMe()
        {
            try
            {
                UnityEngine.Object.Destroy(enumHolder);
            }
            catch { }
        }
    }
}
