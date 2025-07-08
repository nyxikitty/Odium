using MelonLoader;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

namespace Odium.ButtonAPI.MM
{
    public class SidebarListItemCloner
    {
        private const string SIDEBAR_ITEM_PATH = "UserInterface/Canvas_MainMenu(Clone)/Container/MMParent/HeaderOffset/Menu_Social/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Navigation/Viewport/VerticalLayoutGroup/Cell_MM_SidebarListItem (1)";
        private const string PARENT_CONTAINER_PATH = "UserInterface/Canvas_MainMenu(Clone)/Container/MMParent/HeaderOffset/Menu_Social/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Navigation/Viewport/VerticalLayoutGroup";
        public static string VIEWPORT = "UserInterface/Canvas_MainMenu(Clone)/Container/MMParent/HeaderOffset/Menu_Social/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Content/Viewport/VerticalLayoutGroup/Content";
        public static string USER_TEMPLATE = "UserInterface/Canvas_MainMenu(Clone)/Container/MMParent/HeaderOffset/Menu_Social/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Content/Viewport/VerticalLayoutGroup/Content/Online_CellGrid_MM_Content/Cell_MM_User";
       
        public static List<string> strings = new List<string>()
        {
            "Locations_Vertical_Content",
            "Online_CellGrid_MM_Content",
            "ActiveOnAnotherPlatform",
            "MM_Foldout_Offline",
            "Offline"
        };

        public static GameObject CreateUserCard(string username, Sprite userThumbnail)
        {
            GameObject userCard = GameObject.Find(USER_TEMPLATE);
            userCard.transform.SetParent(GameObject.Find(VIEWPORT).transform, false);
            userCard.name = "ODIUM_Cell_MM_User - " + username;
            userCard.SetActive(true);
            return userCard;
        }

        public static GameObject CreateSidebarItem(string title)
        {
            GameObject sidebarItem = GameObject.Find(SIDEBAR_ITEM_PATH);
            GameObject sidebarItemParent = GameObject.Find(PARENT_CONTAINER_PATH);

            GameObject clonedSidebar = UnityEngine.Object.Instantiate(sidebarItem);
            clonedSidebar.transform.SetParent(sidebarItemParent.transform, false);
            clonedSidebar.name = "ODIUM_Cell_MM_SidebarListItem - " + title;

            clonedSidebar.transform.Find("Mask/Text_Name").GetComponent<TextMeshProUGUIEx>().prop_String_0 = title;

            clonedSidebar.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
            clonedSidebar.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(new Action(() =>
            {
                strings.ForEach(str =>
                {
                    GameObject content = GameObject.Find(VIEWPORT + "/" + str);
                    if (content != null)
                    {
                        content.SetActive(false);
                    }
                });

                OdiumConsole.LogGradient("OWIJRFUWEHR", "Sidebar item clicked: " + title);
            }));

            return clonedSidebar;
        }
    }
}