using TMPro;
using UnityEngine;
using VRC.UI.Elements;

namespace VampClient.Api
{
    public class QMMenuBase
    {
        protected string btnQMLoc;
        protected GameObject MenuObject;
        internal TextMeshProUGUI MenuTitleText;
        protected UIPage MenuPage;
        protected string MenuName;

        public string GetMenuName()
        {
            return MenuName;
        }

        public UIPage GetMenuPage()
        {
            return MenuPage;
        }

        public GameObject GetMenuObject()
        {
            return MenuObject;
        }

        public void SetMenuTitle(string newTitle)
        {
            TextMeshProUGUI componentInChildren = MenuObject.GetComponentInChildren<TextMeshProUGUI>(true);
            componentInChildren.text = newTitle;
            componentInChildren.richText = true;
        }

        public void ClearChildren()
        {
            for (int i = 0; i < MenuObject.transform.childCount; i++)
                if (MenuObject.transform.GetChild(i).name != "Header_H1" && MenuObject.transform.GetChild(i).name != "ScrollRect")
                    UnityEngine.Object.Destroy(MenuObject.transform.GetChild(i).gameObject);
        }
    }
}
