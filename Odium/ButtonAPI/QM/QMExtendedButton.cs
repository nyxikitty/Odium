using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Odium.ButtonAPI.QM
{
    public class QMExtendedBackground : QMButtonBase
    {
        public QMExtendedBackground(QMMenuBase btnMenu, float btnXLocation, float btnYLocation, Sprite bgImage = null)
        {
            btnQMLoc = btnMenu.GetMenuName();
            Initialize(btnXLocation, btnYLocation, bgImage);
        }

        public QMExtendedBackground(DefaultVRCMenu btnMenu, float btnXLocation, float btnYLocation, Sprite bgImage = null)
        {
            btnQMLoc = "Menu_" + btnMenu;
            Initialize(btnXLocation, btnYLocation, bgImage);
        }

        public QMExtendedBackground(string btnMenu, float btnXLocation, float btnYLocation, Sprite bgImage = null)
        {
            btnQMLoc = btnMenu;
            Initialize(btnXLocation, btnYLocation, bgImage);
        }

        public QMExtendedBackground(Transform target, float btnXLocation, float btnYLocation, Sprite bgImage = null)
        {
            parent = target;
            Initialize(btnXLocation, btnYLocation, bgImage);
        }

        private void Initialize(float btnXLocation, float btnYLocation, Sprite bgImage = null)
        {
            if (parent == null)
                parent = ApiUtils.QuickMenu.transform.Find("CanvasGroup/Container/Window/QMParent/" + btnQMLoc).transform;

            // Create main button container
            button = UnityEngine.Object.Instantiate(ApiUtils.GetQMButtonTemplate(), parent, true);
            button.transform.Find("Badge_MMJump").gameObject.SetActive(false);
            button.name = $"{ApiUtils.Identifier}-Extended-Background-{ApiUtils.RandomNumbers()}";

            // Set size to span 2 buttons width
            // Single button width: 200f
            // Spacing between buttons: approximately 20f
            // Total width for 2 buttons: (200f * 2) + 20f = 420f
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(420f, 176f);
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(-68f, -250f);

            // Disable all interactive components
            button.GetComponent<Button>().enabled = false;

            // Hide all UI elements
            button.transform.Find("Icons/Icon").GetComponentInChildren<Image>().gameObject.SetActive(false);
            button.GetComponentInChildren<TextMeshProUGUI>().gameObject.SetActive(false);

            initShift[0] = 0;
            initShift[1] = 0;
            SetLocation(btnXLocation, btnYLocation);
            SetActive(true);

            if (bgImage != null)
            {
                ToggleBackgroundImage(true);
                SetBackgroundImage(bgImage);
            }
            else
            {
                ToggleBackgroundImage(true);
            }
        }

        public void SetBackgroundImage(Sprite newImg)
        {
            button.transform.Find("Background").GetComponent<Image>().sprite = newImg;
            button.transform.Find("Background").GetComponent<Image>().overrideSprite = newImg;
            RefreshButton();
        }

        public void ToggleBackgroundImage(bool state)
        {
            button.transform.Find("Background").gameObject.SetActive(state);
        }

        public Image GetBackgroundImage()
        {
            return button.transform.Find("Background").GetComponent<Image>();
        }

        private void RefreshButton()
        {
            button.SetActive(false);
            button.SetActive(true);
        }
    }
}