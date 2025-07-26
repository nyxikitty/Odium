using Odium.ButtonAPI.QM;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VRC.Localization;

namespace Odium.Api.MM
{
    internal class MMToggle : UiElement
    {
        private static readonly Vector3 _onPos = new Vector3(93, 0, 0), _offPos = new Vector3(30, 0, 0);

        protected MMContainer _container;
        protected Toggle _toggleComp;
        protected MonoBehaviour2PublicSiObSi_tCaOb_t_cBoObUnique _toggleSetting;
        protected ToggleBinding _toggleBinding;
        protected RadioButton _radioButton;
        protected Transform _handle;

        internal MMToggle(MMContainer container, string text, System.Action<bool> action, string tooltip, bool defaultState = false)
            => Init(container, text, action, tooltip, defaultState);

        private void Init(MMContainer container, string text, System.Action<bool> action, string tooltip, bool defaultState)
        {
            _container = container;

            _obj = Object.Instantiate(ApiUtils.MM_Toggle(), _container.GetPlacementTransform(), false);
            _obj.name = ApiUtils.Identifier + "-Toggle-" + ApiUtils.RandomNumbers();

            _toggleComp = _obj.GetComponent<Toggle>();
            _toggleSetting = _obj.GetComponent<MonoBehaviour2PublicSiObSi_tCaOb_t_cBoObUnique>();
            _toggleBinding = _obj.GetComponent<ToggleBinding>();
            _text = _obj.transform.Find("LeftItemContainer/Title").GetComponent<TextMeshProUGUIEx>();
            _radioButton = _obj.GetComponentInChildren<RadioButton>();
            _handle = _obj.transform.Find("RightItemContainer/Cell_MM_OnOffSwitch/Handle");
            _toolTips = _obj.GetComponents<ToolTip>();

            _toggleComp.Set(defaultState, false);
            if (defaultState)
                ChangeState(true);

            var dataObject = new Object1PublicTBoTUnique<bool>()
            {
                prop_T_0 = defaultState
            };
            _toggleSetting.field_Private_Object1PublicTBoTUnique_1_Boolean_0= dataObject;
            _toggleBinding.field_Private_Object1PublicTBoTUnique_1_Boolean_0 = dataObject;

            SetText(text);
            SetAction(action);
            SetToolTip(tooltip);
        }

        internal void SetText(string newText)
        {
            _text.richText = true;
            _text._localizableString = LocalizableStringExtensions.Localize(newText);
            _text.text = newText;
        }

        internal void SetAction(System.Action<bool> action)
        {
            _toggleComp.onValueChanged = new Toggle.ToggleEvent();
            _toggleComp.onValueChanged.AddListener(DelegateSupport.ConvertDelegate<UnityAction<bool>>(new System.Action<bool>(state =>
            {
                ChangeState(state, false);
            })));
            _toggleComp.onValueChanged.AddListener(action);
        }

        internal void SetToolTip(string newToolTip)
        {
            var ls = LocalizableStringExtensions.Localize(newToolTip);
            foreach (var t in _toolTips)
            {
                t._localizableString = ls;
                t._alternateLocalizableString = ls;
            }
        }

        internal void ChangeState(bool state, bool shouldInvoke = false)
        {
            _radioButton.Method_Public_Void_Boolean_0(state);
            _handle.localPosition = state ? _onPos : _offPos;
            _toggleComp.Set(state, shouldInvoke);
        }

        public bool GetState()
        {
            return _toggleComp.isOn;
        }
    }
}
