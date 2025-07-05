using UnityEngine;
using UnityEngine.Windows;
using VRC.Localization;

namespace Odium.ButtonAPI.QM
{
    public class QMButtonBase
    {
        protected GameObject button;
        protected string btnQMLoc;
        protected Transform parent;
        protected int[] initShift = new int[2];

        public GameObject GetGameObject()
        {
            return button;
        }

        public void SetActive(bool state)
        {
            button.gameObject.SetActive(state);
        }

        public void SetLocation(float buttonXLoc, float buttonYLoc)
        {
            button.GetComponent<RectTransform>().anchoredPosition += Vector2.right * (232f * (buttonXLoc + initShift[0]));
            button.GetComponent<RectTransform>().anchoredPosition += Vector2.down * (210f * (buttonYLoc + initShift[1]));
        }

        public void SetTooltip(string tooltip)
        {
            foreach (var c in button.GetComponents<ToolTip>())
            {
                c._localizableString = LocalizableStringExtensions.Localize(tooltip);
                c._alternateLocalizableString = LocalizableStringExtensions.Localize(tooltip);
            }
        }

        public void DestroyMe()
        {
            try
            {
                Object.Destroy(button);
            }
            catch { }
        }
    }
}
