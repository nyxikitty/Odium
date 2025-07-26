using Odium.ButtonAPI.QM;
using UnityEngine;
using VRC.Localization;

namespace Odium.Api.MM
{
    internal class MMContainer : UiElement
    {
        protected MMPage _page;

        internal MMContainer(MMPage page, string title)
            => Init(page, title);

        private void Init(MMPage page, string title)
        {
            _page = page;

            _obj = Object.Instantiate(ApiUtils.MM_Container(), _page.GetTransform(), false);
            _obj.name = ApiUtils.Identifier + "-Container-" + ApiUtils.RandomNumbers();
            _text = _obj.transform.Find("MM_Foldout/Label").GetComponent<TextMeshProUGUIEx>();
            _placementTransform = _obj.transform.Find("Settings_Panel_1/VerticalLayoutGroup");

            ApiUtils.DestroyChildren(_placementTransform, x => x.name == "Background_Info");

            SetTitle(title);
        }

        internal void SetTitle(string newTitle)
        {
            _text._localizableString = LocalizableStringExtensions.Localize(newTitle);
            _text.text = newTitle;
        }
    }
}
