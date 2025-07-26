using Odium.ButtonAPI.QM;
using UnityEngine;
using UnityEngine.UI;
using VRC.Localization;

namespace Odium.Api.MM
{
    internal class MMPage : UiElement
    {
        protected MMMenu _menu;
        protected GameObject _btnObj;
        protected MonoBehaviourPublicSt_ILo_nObGa_n_tToGaUnique _handler;
        protected string _title;

        internal MMPage(MMMenu menu, string btnText, string title, string btnToolTip, Sprite icon = null)
            => Init(menu, btnText, title, btnToolTip, icon);

        private void Init(MMMenu menu, string btnText, string title, string btnToolTip, Sprite icon)
        {
            _menu = menu;

            _obj = Object.Instantiate(ApiUtils.MM_PageContainer(), _menu.GetPageContainerTransform(), false);
            _obj.name = ApiUtils.Identifier + "-Page-Container-" + ApiUtils.RandomNumbers();

            ApiUtils.DestroyChildren(_obj.transform);

            var numbers = ApiUtils.RandomNumbers();

            _btnObj = Object.Instantiate(ApiUtils.MM_PageButton(), _menu.GetPageButtonTransform(), false);
            _btnObj.name = ApiUtils.Identifier + "-Page-Button-" + numbers;
            _btnObj.GetComponent<Button>().onClick.AddListener(new System.Action(() =>
            {
                _menu.SetPageTitle(_title);
            }));

            _btnObj.transform.Find("Icon").GetComponent<RectTransform>().sizeDelta = new Vector2(42, 42);

            var pageName = ApiUtils.Identifier + "-Page-Container-" + numbers;
            _handler = _btnObj.GetComponent<MonoBehaviourPublicSt_ILo_nObGa_n_tToGaUnique>();
            _handler.prop_String_0 = pageName;
            _handler.prop_LocalizableString_0 = LocalizableStringExtensions.Localize(pageName);
            _handler.field_Private_Button_0 = _btnObj.GetComponent<VRCButtonHandle>();
            _handler.field_Private_MonoBehaviourPublicObBo_r_cBo1StAcObObUnique_0 = _btnObj.GetComponentInParent<MonoBehaviourPublicObBo_r_cBo1StAcObObUnique>();
            _handler._targetObjectToEnable = _obj;

            _text = _btnObj.transform.Find("Mask/Text_Name").GetComponent<TextMeshProUGUIEx>();
            _toolTips = _btnObj.GetComponents<ToolTip>();

            SetTitle(title);
            SetButtonText(btnText);
            SetToolTip(btnToolTip);
            if (icon != null)
                SetIcon(icon);
        }

        internal void SetTitle(string newTitle)
        {
            _title = newTitle;
        }

        internal void SetButtonText(string newText)
        {
            _text._localizableString = LocalizableStringExtensions.Localize(newText);
            _text.text = newText;
        }

        internal void SetToolTip(string newToolTip)
        {
            var ttsl = LocalizableStringExtensions.Localize(newToolTip);
            foreach (var c in _toolTips)
            {
                c._localizableString = ttsl;
                c._alternateLocalizableString = ttsl;
            }
        }

        internal void SetIcon(Sprite icon)
        {
            var iconComp = _btnObj.transform.Find("Icon").GetComponent<Image>();
            iconComp.sprite = icon;
            iconComp.overrideSprite = icon;
            iconComp.color = Color.white;
            iconComp.m_Color = Color.white;
        }

        public MonoBehaviourPublicSt_ILo_nObGa_n_tToGaUnique GetHandler()
        {
            return _handler;
        }

        public string GetText()
        {
            return _title;
        }

        public Transform GetTransform()
        {
            return _obj.transform;
        }
    }
}
