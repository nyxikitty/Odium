using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Elements;

using MainMenuContent = VRC.UI.Pages.QM.Dashboard;

namespace Odium.ButtonAPI.QM
{
    public class QMNestedMenu : QMMenuBase
    {
        protected bool IsMenuRoot;
        protected GameObject BackButton;
        protected QMSingleButton MainButton;
        protected Transform parent;

        public QMNestedMenu(QMMenuBase location, float posX, float posY, string btnText, string menuTitle, string tooltip, bool halfButton = false, Sprite sprite = null, Sprite bgImage = null)
        {
            btnQMLoc = location.GetMenuName();
            Initialize(false, btnText, posX, posY, menuTitle, tooltip, halfButton, sprite, bgImage);
        }

        public QMNestedMenu(DefaultVRCMenu location, float posX, float posY, string btnText, string menuTitle, string tooltip, bool halfButton = false, Sprite sprite = null, Sprite bgImage = null)
        {
            btnQMLoc = "Menu_" + location.ToString();
            Initialize(false, btnText, posX, posY, menuTitle, tooltip, halfButton, sprite, bgImage);
        }

        public QMNestedMenu(Transform target, float posX, float posY, string btnText, string menuTitle, string tooltip, bool halfButton = false, Sprite sprite = null, Sprite bgImage = null)
        {
            parent = target;
            Initialize(false, btnText, posX, posY, menuTitle, tooltip, halfButton, sprite, bgImage);
        }

        private void Initialize(bool isRoot, string btnText, float btnPosX, float btnPosY, string menuTitle, string tooltip, bool halfButton, Sprite sprite, Sprite bgImage)
        {
            MenuName = $"{ApiUtils.Identifier}-QMMenu-{ApiUtils.RandomNumbers()}";
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
            IsMenuRoot = isRoot;
            if (IsMenuRoot)
            {
                List<UIPage> list = ApiUtils.QuickMenu.prop_MenuStateController_0.field_Public_ArrayOf_UIPage_0.ToList();
                list.Add(MenuPage);
                ApiUtils.QuickMenu.prop_MenuStateController_0.field_Public_ArrayOf_UIPage_0 = list.ToArray();
            }
            var vlg = MenuObject.transform.Find("ScrollRect/Viewport/VerticalLayoutGroup");
            for (int i = 0; i < vlg.childCount; i++)
            {
                var child = vlg.GetChild(i);
                if (child == null)
                    continue;
                UnityEngine.Object.Destroy(child.gameObject);
            }
            MenuTitleText = MenuObject.GetComponentInChildren<TextMeshProUGUI>(true);
            SetMenuTitle(menuTitle);
            BackButton = MenuObject.transform.GetChild(0).Find("LeftItemContainer/Button_Back").gameObject;
            BackButton.SetActive(true);
            BackButton.GetComponentInChildren<Button>().onClick = new Button.ButtonClickedEvent();
            BackButton.GetComponentInChildren<Button>().onClick.AddListener(new Action(() =>
            {
                if (isRoot)
                {
                    if (btnQMLoc.StartsWith("Menu_"))
                    {
                        ApiUtils.QuickMenu.prop_MenuStateController_0.Method_Public_Void_String_Boolean_Boolean_PDM_0("QuickMenu" + btnQMLoc.Remove(0, 5));
                        return;
                    }
                    ApiUtils.QuickMenu.prop_MenuStateController_0.Method_Public_Void_String_Boolean_Boolean_PDM_0(btnQMLoc);
                    return;
                }
                MenuPage.Method_Protected_Virtual_New_Void_0();
            }));
            MenuObject.transform.GetChild(0).Find("RightItemContainer/Button_QM_Expand").gameObject.SetActive(false);
            MenuObject.transform.GetChild(0).Find("RightItemContainer/Button_QM_Report").gameObject.SetActive(false);

            // Modified MainButton creation to handle Transform parent
            if (parent != null)
            {
                MainButton = new QMSingleButton(parent, btnPosX, btnPosY, btnText, new Action(OpenMe), tooltip, halfButton, sprite, bgImage);
            }
            else
            {
                MainButton = new QMSingleButton(btnQMLoc, btnPosX, btnPosY, btnText, new Action(OpenMe), tooltip, halfButton, sprite, bgImage);
            }

            ClearChildren();
            MenuObject.transform.Find("ScrollRect").GetComponent<ScrollRect>().enabled = false;
        }

        // For some reason toggling the menuobject comps and active state before claling open menu msc method doesn't work lmao. - Blaze
        // It's fixed now tho.
        public void OpenMe()
        {
            ApiUtils.QuickMenu.prop_MenuStateController_0.Method_Public_Void_String_UIContext_Boolean_EnumNPublicSealedvaNoLeRiBoIn6vUnique_0(
                MenuPage.field_Public_String_0,
                null,
                false,
                UIPage.EnumNPublicSealedvaNoLeRiBoIn6vUnique.Left);
            MenuObject.SetActive(true);
            MenuObject.GetComponent<Canvas>().enabled = true;
            MenuObject.GetComponent<CanvasGroup>().enabled = true;
            MenuObject.GetComponent<GraphicRaycaster>().enabled = true;
        }

        public void CloseMe()
        {
            BackButton.GetComponent<Button>().onClick.Invoke();
        }

        public QMSingleButton GetMainButton()
        {
            return MainButton;
        }

        public GameObject GetBackButton()
        {
            return BackButton;
        }
    }
}