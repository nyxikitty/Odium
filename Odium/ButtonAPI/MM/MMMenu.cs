using Odium.ButtonAPI.QM;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VRC.Localization;
using VRC.UI.Elements;
using VRC.UI.Pages.MM;

namespace Odium.Api.MM
{
    internal class MMMenu : UiElement
    {
        protected string _menuName;
        protected UIPage _uiPage;
        protected TextMeshProUGUIEx _menuTitle;
        protected TextMeshProUGUIEx _pageTitle;

        internal MMMenu(string title)
            => Init(title);

        private void Init(string title)
        {
            _menuName = ApiUtils.Identifier + "-Menu-" + ApiUtils.RandomNumbers();
            _obj = Object.Instantiate(ApiUtils.MM_Menu(), ApiUtils.MM_Menu().transform.parent, false);
            _obj.name = _menuName;
            _obj.SetActive(false);
            _obj.transform.SetSiblingIndex(17);

            var wrappedMSC = _obj.GetComponent<SettingsPage>().field_Protected_InterfacePublicAbstractObBoObVoStObInVoStBoUnique_0;

            Object.Destroy(_obj.GetComponent<MonoBehaviourPublicRe_h_vReSi_b_u_e_lReUnique>());
            Object.Destroy(_obj.GetComponent<DataContext>());
            Object.Destroy(_obj.GetComponent<MonoBehaviourPublicVoBoVoBoVoBoVoBoVoBoUnique>());
            Object.Destroy(_obj.GetComponent<SettingsPage>());

            _uiPage = _obj.AddComponent<UIPage>();
            _uiPage.field_Public_String_0 = _menuName;
            _uiPage.field_Protected_InterfacePublicAbstractObBoObVoStObInVoStBoUnique_0 = wrappedMSC;
            _uiPage.field_Private_List_1_UIPage_0 = new Il2CppSystem.Collections.Generic.List<UIPage>();
            _uiPage.field_Private_List_1_UIPage_0.Add(_uiPage);

            ApiUtils.MainMenu.prop_MenuStateController_0.field_Private_Dictionary_2_String_UIPage_0.Add(_menuName, _uiPage);
            var list = ApiUtils.MainMenu.prop_MenuStateController_0.field_Public_ArrayOf_UIPage_0.ToList();
            list.Add(_uiPage);
            ApiUtils.MainMenu.prop_MenuStateController_0.field_Public_ArrayOf_UIPage_0 = list.ToArray();

            var container = _obj.transform.Find("Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container");

            ApiUtils.DestroyChildren(container.Find("DynamicSidePanel_Header"), x => x.name == "TitleContainer" || x.name == "Separator");
            ApiUtils.DestroyChildren(container.Find("ScrollRect_Navigation/Viewport/VerticalLayoutGroup"));
            ApiUtils.DestroyChildren(container.Find("ScrollRect_Content/Viewport/VerticalLayoutGroup"));

            Object.Destroy(container.Find("ScrollRect_Content/Header_MM_H2/RightItemContainer/Field_MM_TextSearchField").gameObject);

            _menuTitle = container.Find("DynamicSidePanel_Header/TitleContainer/Text_Name").GetComponent<TextMeshProUGUIEx>();
            _pageTitle = container.Find("ScrollRect_Content/Header_MM_H2/LeftItemContainer/Text_Title").GetComponent<TextMeshProUGUIEx>();

            var vlg = container.Find("ScrollRect_Navigation/Viewport/VerticalLayoutGroup");
            vlg.GetComponent<Canvas>().enabled = true;
            vlg.GetComponent<GraphicRaycaster>().enabled = true;

            container.Find("ScrollRect_Content").GetComponent<VRCScrollRect>().field_Public_Boolean_0 = true;
            container.Find("ScrollRect_Navigation").GetComponent<VRCScrollRect>().field_Public_Boolean_0 = true;

            SetTitle(title);
        }

        internal void SetTitle(string newTitle)
        {
            _menuTitle._localizableString = LocalizableStringExtensions.Localize(newTitle);
            _menuTitle.text = newTitle;
        }

        internal void SetPageTitle(string newPageTitle)
        {
            _pageTitle._localizableString = LocalizableStringExtensions.Localize(newPageTitle);
            _pageTitle.text = newPageTitle;
        }

        internal void SetDefaultPage(MMPage page)
        {
            _obj.transform.Find("Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Navigation/Viewport/VerticalLayoutGroup")
                .GetComponent<MonoBehaviourPublicObBo_r_cBo1StAcObObUnique>().field_Private_MonoBehaviourPublicSt_ILo_nObGa_n_tToGaUnique_0 = page.GetHandler();

            SetPageTitle(page.GetText());
        }

        internal void OpenMenu()
        {
            ApiUtils.MainMenu.prop_MenuStateController_0.Method_Public_Void_String_UIContext_Boolean_EnumNPublicSealedvaNoLeRiBoIn6vUnique_0(
                _uiPage.field_Public_String_0,
                null,
                false,
                UIPage.EnumNPublicSealedvaNoLeRiBoIn6vUnique.Left);
            _obj.SetActive(true);
            _obj.GetComponent<Canvas>().enabled = true;
            _obj.GetComponent<CanvasGroup>().enabled = true;
            _obj.GetComponent<GraphicRaycaster>().enabled = true;
        }

        internal string GetMenuName()
        {
            return _menuName;
        }

        internal Transform GetTitleTransform()
        {
            return _obj.transform.Find("Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/DynamicSidePanel_Header");
        }

        internal Transform GetPageButtonTransform()
        {
            return _obj.transform.Find("Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Navigation/Viewport/VerticalLayoutGroup");
        }

        internal Transform GetPageContainerTransform()
        {
            return _obj.transform.Find("Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Content/Viewport/VerticalLayoutGroup");
        }
    }
}
