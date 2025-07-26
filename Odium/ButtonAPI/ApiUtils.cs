using Odium.Components;
using Odium.Odium;
using Odium.Wrappers;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Elements;

namespace Odium.ButtonAPI.QM
{
    public static class ApiUtils
    {
        public const string Identifier = "Odium";
        public static readonly System.Random random = new System.Random();

        #region Component Instances
        private static QuickMenu _quickMenu;
        private static MainMenu _socialMenu;
        private static GameObject _selectedUserPageGrid;

        public static QuickMenu QuickMenu
        {
            get
            {
                if (_quickMenu == null)
                    _quickMenu = Resources.FindObjectsOfTypeAll<QuickMenu>()[0];
                return _quickMenu;
            }
        }

        // Use your own GetAllPlayers right here cause I put mine on here
        public static VRC.Player GetPlayerByDisplayName(string name)
        {
            VRC.Player vrcPlayer = PlayerWrapper.GetAllPlayers().Find(plr => plr.field_Private_APIUser_0.displayName == name);
            return vrcPlayer;
        }

        public static VRC.Player GetIUser()
        {
            var textObject = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_SelectedUser_Local/ScrollRect/Viewport/VerticalLayoutGroup/UserProfile_Compact/PanelBG/Info/Text_Username_Friend");
            textObject.SetActive(true);
            if (textObject == null) return null;

            var textComponent = textObject.GetComponent<TextMeshProUGUIEx>();
            if (textComponent == null) return null;

            VRC.Player plr = GetPlayerByDisplayName(textComponent.text);
            textObject.SetActive(false);
            return plr;
        }

        internal static GameObject MMButtonTemplate()
        {
            var menuPath = "UserInterface/Canvas_MainMenu(Clone)/Container/MMParent/HeaderOffset/Menu_Settings/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Content/Viewport/VerticalLayoutGroup/AudioAndVoice/AudioVolume/Settings_Panel_1/VerticalLayoutGroup/OtherUsersVolume";

            var template = GameObject.Find(menuPath);
            if (template == null)
            {
                template = GameObject.Find("OtherUsersVolume");
            }

            return template;
        }

        public static string GetMMIUser()
        {
            var textObject = AssignedVariables.userInterface.transform.Find("Canvas_MainMenu(Clone)/Container/MMParent/HeaderOffset/Menu_UserDetail/Header_MM_UserName/LeftItemContainer/Text_Title");
            if (textObject == null)
            {
                OdiumConsole.Log("Odium", "Text object not found in GetMMIUser method", LogLevel.Error);
                return null;
            }

            var textComponent = textObject.GetComponent<TextMeshProUGUIEx>();
            if (textComponent == null)
            {
                OdiumConsole.Log("Odium", "Text component not found in GetMMIUser method", LogLevel.Error);
                return null;
            }

            return textComponent.prop_String_0;
        }

        public class BasePaths
        {
            public const string QuickMenuWindow = "CanvasGroup/Container/Window/";
            public const string QuickMenuParent = QuickMenuWindow + "QMParent/";
        }

        public class MenuPaths
        {
            public const string QuickMenu_Dashboard_Menu = BasePaths.QuickMenuParent + "Menu_Dashboard/";
        }

        public class EnumPaths
        {
            public const string QuickMenu_NameplateVisibility_Enum = BasePaths.QuickMenuParent + "Menu_QM_GeneralSettings/Panel_QM_ScrollRect/Viewport/VerticalLayoutGroup/UIElements/QM_Settings_Panel/VerticalLayoutGroup/NameplateVisibility/";
        }

        public class SliderPaths
        {
            public const string QuickMenu_AvatarCullingBeyond_Slider = BasePaths.QuickMenuParent + "Menu_QM_GeneralSettings/Panel_QM_ScrollRect/Viewport/VerticalLayoutGroup/AvatarCulling/QM_Settings_Panel/VerticalLayoutGroup/HideBeyond/";
        }

        public class ButtonPaths
        {
            public const string QuickMenu_Settings_Tab = BasePaths.QuickMenuWindow + "Page_Buttons_QM/HorizontalLayoutGroup/Page_Settings/";
            public const string QuickMenu_RejoinWorld_Button = BasePaths.QuickMenuParent + "Menu_Here/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_WorldActions/Button_RejoinWorld/";
            public const string QuickMenu_ReportUser_Button = BasePaths.QuickMenuParent + "Menu_Dashboard/Header_H1/RightItemContainer/Button_QM_Report/";
            public const string QuickMenu_UserActions_Button = BasePaths.QuickMenuParent + "Menu_SelectedUser_Local/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_UserActions/";
        }

        private static GameObject _qmSliderTemplate;
        private static GameObject _qmLabelTemplate;
        private static GameObject _qmEnumTemplate;

        public static GameObject GetQMLabelTemplate() => (_qmLabelTemplate) ? _qmLabelTemplate : _qmLabelTemplate = QuickMenu.transform.Find(LabelPaths.QuickMenu_MainHeader_Label).gameObject;
        public static GameObject GetQMEnumTemplate() => (_qmEnumTemplate) ? _qmEnumTemplate : _qmEnumTemplate = QuickMenu.transform.Find(EnumPaths.QuickMenu_NameplateVisibility_Enum).gameObject;
        public static GameObject GetQMSliderTemplate() => (_qmSliderTemplate) ? _qmSliderTemplate : _qmSliderTemplate = QuickMenu.transform.Find(SliderPaths.QuickMenu_AvatarCullingBeyond_Slider).gameObject;

        public class LabelPaths
        {
            public const string QuickMenu_NonFriendList_Label = BasePaths.QuickMenuParent + "Menu_SelectedUser_Local/ScrollRect/Viewport/VerticalLayoutGroup/UserProfile_Compact/PanelBG/Info/Text_Username_NonFriend/"; // Name
            public const string QuickMenu_MainHeader_Label = MenuPaths.QuickMenu_Dashboard_Menu + "Header_H1/LeftItemContainer/Text_Title/";
        }

        public static string GetMMWorldName()
        {
            var textObject = AssignedVariables.userInterface.transform.Find("Canvas_MainMenu(Clone)/Container/MMParent/HeaderOffset/Menu_MM_WorldInformation/Header_MM_H1/Text_WorldName");
            if (textObject == null)
            {
                OdiumConsole.Log("Odium", "Text object not found in GetMMWorldName method", LogLevel.Error);
                return null;
            }

            var textComponent = textObject.GetComponent<TextMeshProUGUIEx>();
            if (textComponent == null)
            {
                OdiumConsole.Log("Odium", "Text component not found in GetMMWorldName method", LogLevel.Error);
                return null;
            }

            return textComponent.prop_String_0;
        }

        public static string GetFPS()
        {
            var textObject = AssignedVariables.userInterface.transform.Find("Canvas_MainMenu(Clone)/Container/MMParent/HeaderOffset/Menu_Settings/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Content/Viewport/VerticalLayoutGroup/Debug/Debug/Settings_Panel_1/VerticalLayoutGroup/DebugStats/LeftItemContainer/Cell_MM_SettingStat (1)/Text_Detail_Original");
            if (textObject == null)
            {
                return null;
            }

            var textComponent = textObject.GetComponent<TextMeshProUGUIEx>();
            if (textComponent == null)
            {
                return null;
            }

            return textComponent.prop_String_0;
        }

        public static string GetPing()
        {
            var textObject = AssignedVariables.userInterface.transform.Find("Canvas_MainMenu(Clone)/Container/MMParent/HeaderOffset/Menu_Settings/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Content/Viewport/VerticalLayoutGroup/Debug/Debug/Settings_Panel_1/VerticalLayoutGroup/DebugStats/LeftItemContainer/Cell_MM_SettingStat (2)/Text_Detail_Original");
            if (textObject == null)
            {
                return null;
            }

            var textComponent = textObject.GetComponent<TextMeshProUGUIEx>();
            if (textComponent == null)
            {
                return null;
            }

            return textComponent.prop_String_0;
        }

        public static string GetBuild()
        {
            var textObject = AssignedVariables.userInterface.transform.Find("Canvas_MainMenu(Clone)/Container/MMParent/HeaderOffset/Menu_Settings/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Content/Viewport/VerticalLayoutGroup/Debug/Debug/Settings_Panel_1/VerticalLayoutGroup/DebugStats/LeftItemContainer/Cell_MM_SettingStat (3)/Text_Detail_Original");
            if (textObject == null)
            {
                return null;
            }

            var textComponent = textObject.GetComponent<TextMeshProUGUIEx>();
            if (textComponent == null)
            {
                return null;
            }

            return textComponent.prop_String_0;
        }

        public static GameObject GetSelectedUserPageGrid()
        {
            if (_selectedUserPageGrid == null)
            {
                _selectedUserPageGrid = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_SelectedUser_Local/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_UserActions");

                return _selectedUserPageGrid;
            }

            return _selectedUserPageGrid;
        }

        public static MainMenu MainMenu
        {
            get
            {
                if (_socialMenu == null)
                    _socialMenu = Resources.FindObjectsOfTypeAll<MainMenu>()[0];
                return _socialMenu;
            }
        }
        #endregion

        #region Main Menu
        private static GameObject _mmMenu;
        private static GameObject _mmTabButton;
        private static GameObject _mmTitleButton;
        private static GameObject _mmPageContainer;
        private static GameObject _mmPageButton;
        private static GameObject _mmContainer;
        private static GameObject _mmToggle;
        private static GameObject _mmSlider;
        private static GameObject _mmAvatarList;
        private static GameObject _mmKeybind;
        private static GameObject _mmSelector;
        private static GameObject _mmButton;

        public static GameObject MM_Menu()
        {
            if (_mmMenu == null)
                _mmMenu = MainMenu.transform.Find("Container/MMParent/HeaderOffset/Menu_Settings").gameObject;
            return _mmMenu;
        }

        public static GameObject MM_TabButton()
        {
            if (_mmTabButton == null)
                _mmTabButton = MainMenu.transform.Find("Container/PageButtons/HorizontalLayoutGroup/Marketplace_Button_Tab").gameObject;
            return _mmTabButton;
        }

        public static GameObject MM_TitleButton()
        {
            if (_mmTitleButton == null)
                _mmTitleButton = MainMenu.transform.Find("Container/MMParent/HeaderOffset/Menu_Settings/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/DynamicSidePanel_Header/Button_Logout").gameObject;
            return _mmTitleButton;
        }

        public static GameObject MM_PageContainer()
        {
            if (_mmPageContainer == null)
                _mmPageContainer = MainMenu.transform.Find("Container/MMParent/HeaderOffset/Menu_Settings/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Content/Viewport/VerticalLayoutGroup/AudioAndVoice").gameObject;
            return _mmPageContainer;
        }

        public static GameObject MM_PageButton()
        {
            if (_mmPageButton == null)
                _mmPageButton = MainMenu.transform.Find("Container/MMParent/HeaderOffset/Menu_Settings/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Navigation/Viewport/VerticalLayoutGroup/Cell_MM_Audio & Voice").gameObject;
            return _mmPageButton;
        }

        public static GameObject MM_Container()
        {
            if (_mmContainer == null)
                _mmContainer = MainMenu.transform.Find("Container/MMParent/HeaderOffset/Menu_Settings/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Content/Viewport/VerticalLayoutGroup/AudioAndVoice/AudioVolume").gameObject;
            return _mmContainer;
        }

        public static GameObject MM_Toggle()
        {
            if (_mmToggle == null)
                _mmToggle = MainMenu.transform.Find("Container/MMParent/HeaderOffset/Menu_Settings/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Content/Viewport/VerticalLayoutGroup/ComfortAndSafety/Comfort/Settings_Panel_1/VerticalLayoutGroup/PersonalSpace").gameObject;
            return _mmToggle;
        }

        public static GameObject MM_Slider()
        {
            if (_mmSlider == null)
                _mmSlider = MainMenu.transform.Find("Container/MMParent/HeaderOffset/Menu_Settings/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Content/Viewport/VerticalLayoutGroup/AudioAndVoice/AudioVolume/Settings_Panel_1/VerticalLayoutGroup/MasterVolume").gameObject;
            return _mmSlider;
        }

        public static GameObject MM_AvatarList()
        {
            if (_mmAvatarList == null)
                _mmAvatarList = MainMenu.transform.Find("Container/MMParent/HeaderOffset/Menu_Avatars/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Navigation/Viewport/VerticalLayoutGroup/VerticalLayoutGroup User/Cell_MM_SidebarListItem (Favorites)").gameObject;
            return _mmAvatarList;
        }

        public static GameObject MM_Keybind()
        {
            if (_mmKeybind == null)
                _mmKeybind = MainMenu.transform.Find("Container/MMParent/HeaderOffset/Menu_Settings/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Content/Viewport/VerticalLayoutGroup/AudioAndVoice/Microphone/Settings_Panel_1/VerticalLayoutGroup/ChangeDevice").gameObject;
            return _mmKeybind;
        }

        public static GameObject MM_Selector()
        {
            if (_mmSelector == null)
                _mmSelector = MainMenu.transform.Find("Container/MMParent/HeaderOffset/Menu_Settings/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Content/Viewport/VerticalLayoutGroup/AudioAndVoice/Microphone/Settings_Panel_1/VerticalLayoutGroup/MicrophoneBehavior").gameObject;
            return _mmSelector;
        }

        public static GameObject MM_Button()
        {
            if (_mmButton == null)
                _mmButton = MainMenu.transform.Find("Container/MMParent/HeaderOffset/Menu_Settings/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Content/Viewport/VerticalLayoutGroup/Mirrors/PersonalMirror/Settings_Panel_1/VerticalLayoutGroup/ResetPersonalMirror").gameObject;
            return _mmButton;
        }
        #endregion

        #region Templates
        private static GameObject _qmMenuTemplate;
        private static GameObject _qmTabTemplate;
        private static GameObject _qmButtonTemplate;

        public static GameObject GetQMMenuTemplate()
        {
            if (_qmMenuTemplate == null)
                _qmMenuTemplate = QuickMenu.transform.Find("CanvasGroup/Container/Window/QMParent/Menu_Dashboard").gameObject;
            return _qmMenuTemplate;
        }

        public static GameObject GetQMTabButtonTemplate()
        {
            if (_qmTabTemplate == null)
                _qmTabTemplate = QuickMenu.transform.Find("CanvasGroup/Container/Window/Page_Buttons_QM/HorizontalLayoutGroup/Page_Settings").gameObject;
            return _qmTabTemplate;
        }

        public static GameObject GetQMButtonTemplate()
        {
            if (_qmButtonTemplate == null)
                _qmButtonTemplate = QuickMenu.transform.Find("CanvasGroup/Container/Window/QMParent/Menu_Here/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_WorldActions/Button_RejoinWorld").gameObject;
            return _qmButtonTemplate;
        }

        public static GameObject GetQMSmalltTemplate()
        {
            if (_qmButtonTemplate == null)
                _qmButtonTemplate = QuickMenu.transform.Find("UserInterface/Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/Header_H1/RightItemContainer/Button_QM_Report/").gameObject;
            return _qmButtonTemplate;
        }
        #endregion

        #region Helpers
        private static Sprite _onSprite;
        private static Sprite _offSprite;

        public static Sprite OnIconSprite()
        {
            return SpriteUtil.LoadFromDisk(Path.Combine(Environment.CurrentDirectory, "Odium", "ToggleSwitchOn.png"));
        }

        public static Sprite OffIconSprite()
        {
            return SpriteUtil.LoadFromDisk(Path.Combine(Environment.CurrentDirectory, "Odium", "ToggleSwitchOff.png"));
        }

        public static int RandomNumbers()
        {
            return random.Next(100000, 999999);
        }

        public static string GetSelectedPageName()
        {
            return QuickMenu.prop_MenuStateController_0.field_Private_UIPage_0.field_Public_String_0;
        }

        public static void DestroyChildren(Transform transform)
            => DestroyChildren(transform, null);

        public static void DestroyChildren(Transform transform, Func<Transform, bool> exclude)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);

                if (exclude != null && exclude(child))
                    continue; // Skip destroying if excluded

                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }
        #endregion
    }
}