using System.Linq;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using VRC.UI.Core.Styles;
using VRC.UI.Elements.Controls;
using VRC.UI.Elements;

using MainMenuContent = VRC.UI.Pages.QM.Dashboard;
using System.Collections.Generic;
using System;
using VRC.Localization;

namespace VampClient.Api
{
    public class QMTabMenu : QMMenuBase
    {
        protected GameObject MainButton;
        protected GameObject BadgeObject;
        protected TextMeshProUGUI BadgeText;
        protected MenuTab MenuTabComp;

        public QMTabMenu(string MenuTitle, string tooltip, Sprite ButtonImage = null)
        {
            Initialize(MenuTitle, tooltip, ButtonImage);
        }

        private void Initialize(string MenuTitle, string tooltip, Sprite ButtonImage)
        {
            MenuName = $"{ApiUtils.Identifier}-Tab-Menu-{ApiUtils.RandomNumbers()}";
            MenuObject = UnityEngine.Object.Instantiate(ApiUtils.GetQMMenuTemplate(), ApiUtils.GetQMMenuTemplate().transform.parent);
            MenuObject.name = MenuName;
            MenuObject.SetActive(false);
            MenuObject.transform.SetSiblingIndex(19);

            var wrappedMSC = MenuObject.GetComponent<MainMenuContent>().field_Protected_InterfacePublicAbstractObBoObVoStObInVoStBoUnique_0;
            UnityEngine.Object.DestroyImmediate(MenuObject.GetComponent<MainMenuContent>());

            MenuPage = MenuObject.AddComponent<UIPage>();
            MenuPage.field_Public_String_0 = MenuName;
            MenuPage.field_Protected_InterfacePublicAbstractObBoObVoStObInVoStBoUnique_0 = wrappedMSC;
            MenuPage.field_Private_List_1_UIPage_0 = new Il2CppSystem.Collections.Generic.List<UIPage>();
            MenuPage.field_Private_List_1_UIPage_0.Add(MenuPage);
            ApiUtils.QuickMenu.prop_MenuStateController_0.field_Private_Dictionary_2_String_UIPage_0.Add(MenuName, MenuPage);

            List<UIPage> list = ApiUtils.QuickMenu.prop_MenuStateController_0.field_Public_ArrayOf_UIPage_0.ToList();
            list.Add(MenuPage);
            ApiUtils.QuickMenu.prop_MenuStateController_0.field_Public_ArrayOf_UIPage_0 = list.ToArray();

            var vlg = MenuObject.transform.Find("ScrollRect/Viewport/VerticalLayoutGroup");
            for (int i = 0; i < vlg.childCount; i++)
            {
                var child = vlg.GetChild(i);
                if (child == null)
                    continue;
                UnityEngine.Object.Destroy(child.gameObject);
            }

            MenuTitleText = MenuObject.GetComponentInChildren<TextMeshProUGUI>(true);
            SetMenuTitle(MenuTitle);
            MenuObject.transform.GetChild(0).Find("RightItemContainer/Button_QM_Expand").gameObject.SetActive(false);
            MenuObject.transform.GetChild(0).Find("RightItemContainer/Button_QM_Report").gameObject.SetActive(false);
            ClearChildren();
            MenuObject.transform.Find("ScrollRect").GetComponent<ScrollRect>().enabled = false;
            MainButton = UnityEngine.Object.Instantiate(ApiUtils.GetQMTabButtonTemplate(), ApiUtils.GetQMTabButtonTemplate().transform.parent);
            MainButton.name = MenuName;
            MenuTabComp = MainButton.GetComponent<MenuTab>();
            MenuTabComp.field_Private_MenuStateController_0 = ApiUtils.QuickMenu.prop_MenuStateController_0;
            MenuTabComp._controlName = MenuName;
            MenuTabComp.GetComponent<StyleElement>().field_Private_Selectable_0 = MenuTabComp.GetComponent<Button>();
            BadgeObject = MainButton.transform.GetChild(0).gameObject;
            BadgeText = BadgeObject.GetComponentInChildren<TextMeshProUGUI>();
            MainButton.GetComponent<Button>().onClick.AddListener(new Action(() =>
            {
                MenuObject.SetActive(true);
                MenuObject.GetComponent<Canvas>().enabled = true;
                MenuObject.GetComponent<CanvasGroup>().enabled = true;
                MenuObject.GetComponent<GraphicRaycaster>().enabled = true;
                MenuTabComp.GetComponent<StyleElement>().field_Private_Selectable_0 = MenuTabComp.GetComponent<Button>();
            }));

            UnityEngine.Object.Destroy(MainButton.GetComponent<MonoBehaviour1PublicVoVo5>());
            SetTooltip(tooltip);
            if (ButtonImage != null)
                SetImage(ButtonImage);
        }

        public void SetImage(Sprite newImg)
        {
            MainButton.transform.Find("Icon").GetComponent<Image>().sprite = newImg;
            MainButton.transform.Find("Icon").GetComponent<Image>().overrideSprite = newImg;
            MainButton.transform.Find("Icon").GetComponent<Image>().color = Color.white;
            MainButton.transform.Find("Icon").GetComponent<Image>().m_Color = Color.white;
        }

        public void SetIndex(int newPosition)
        {
            MainButton.transform.SetSiblingIndex(newPosition);
        }

        public void SetActive(bool newState)
        {
            MainButton.SetActive(newState);
        }

        public void SetBadge(bool showing = true, string text = "")
        {
            if (BadgeObject != null && BadgeText != null)
            {
                BadgeObject.SetActive(showing);
                BadgeText.text = text;
            }
        }

        public void OpenMe()
        {
            MainButton.GetComponent<Button>().onClick.Invoke();
        }

        public void SetTooltip(string tooltip)
        {
            foreach (var c in MainButton.GetComponents<ToolTip>())
            {
                c._localizableString = LocalizableStringExtensions.Localize(tooltip);
                c._alternateLocalizableString = LocalizableStringExtensions.Localize(tooltip);
            }
        }

        public GameObject GetMainButton()
        {
            return MainButton;
        }
    }
}
