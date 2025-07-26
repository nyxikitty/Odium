using Odium.ButtonAPI.QM;
using UnityEngine;
using UnityEngine.UI;
using VRC.Localization;
using VRC.UI.Client.Marketplace;
using VRC.UI.Core.Styles;
using VRC.UI.Elements.Controls;

namespace Odium.Api.MM
{
    internal class MMTabButton : UiElement
    {
        protected MMMenu _menu;
        protected MenuTab _menuTab;
        protected GameObject _badgeObj;
        protected TextMeshProUGUIEx _badgeText;

        internal MMTabButton(MMMenu menu, string btnText, string toolTip, Sprite icon = null)
            => Init(menu, btnText, toolTip, icon);

        private void Init(MMMenu menu, string btnText, string toolTip, Sprite icon)
        {
            var collider = ApiUtils.MainMenu.transform.Find("Container/PageButtons").GetComponent<BoxCollider>();
            if (collider.extents.x != 1200)
                collider.extents = new Vector3(1200, 50, 0.5f);

            _obj = Object.Instantiate(ApiUtils.MM_TabButton(), ApiUtils.MM_TabButton().transform.parent, false);
            _obj.name = ApiUtils.Identifier + "-Tab-Button-" + ApiUtils.RandomNumbers();

            Object.Destroy(_obj.GetComponent<SubscriptionNotifierComponent>());

            _menu = menu;
            _menuTab = _obj.GetComponent<MenuTab>();
            _menuTab.field_Private_MenuStateController_0 = ApiUtils.MainMenu.prop_MenuStateController_0;
            _menuTab._controlName = _menu.GetMenuName();
            _menuTab.GetComponent<StyleElement>().field_Private_Selectable_0 = _obj.GetComponent<Button>();

            _text = _obj.transform.Find("Text_H4").GetComponent<TextMeshProUGUIEx>();

            _badgeObj = _obj.transform.Find("Tab_Badge").gameObject;
            _badgeText = _badgeObj.GetComponentInChildren<TextMeshProUGUIEx>();

            _obj.GetComponent<Button>().onClick.AddListener(new System.Action(() =>
            {
                _menu.GetGameObject().SetActive(true);
                _menu.GetGameObject().GetComponent<Canvas>().enabled = true;
                _menu.GetGameObject().GetComponent<CanvasGroup>().enabled = true;
                _menu.GetGameObject().GetComponent<GraphicRaycaster>().enabled = true;
                _menuTab.GetComponent<StyleElement>().field_Private_Selectable_0 = _obj.GetComponent<Button>();
            }));

            _toolTips = _obj.GetComponents<ToolTip>();

            SetBtnText(btnText);
            SetToolTip(toolTip);
            if (icon != null)
                SetIcon(icon);
        }

        internal void SetBtnText(string newText)
        {
            _text._localizableString = LocalizableStringExtensions.Localize(newText);
            _text.text = newText;
        }

        internal void SetToolTip(string newToolTip)
        {
            var lstt = LocalizableStringExtensions.Localize(newToolTip);
            foreach (var t in _toolTips)
            {
                t._localizableString = lstt;
                t._alternateLocalizableString = lstt;
            }
        }

        internal void SetIcon(Sprite icon)
        {
            _obj.transform.Find("Icon").GetComponent<Image>().sprite = icon;
            _obj.transform.Find("Icon").GetComponent<Image>().overrideSprite = icon;
            _obj.transform.Find("Icon").GetComponent<Image>().color = Color.white;
            _obj.transform.Find("Icon").GetComponent<Image>().m_Color = Color.white;
        }

        internal void SetBadge(bool state, string msg = "")
        {
            if (_badgeObj != null && _badgeText != null)
            {
                _badgeObj.SetActive(state);
                _badgeText.text = msg;
            }
        }
    }
}
