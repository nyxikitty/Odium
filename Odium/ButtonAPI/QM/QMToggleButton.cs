using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Odium.ButtonAPI.QM
{
    public class QMToggleButton : QMButtonBase
    {
        protected TextMeshProUGUI btnTextComp;
        protected Button btnComp;
        protected Image btnImageComp;
        protected bool currentState;
        protected Action OnAction;
        protected Action OffAction;

        public QMToggleButton(QMMenuBase location, float btnXPos, float btnYPos, string btnText, Action onAction, Action offAction, string tooltip, bool defaultState = false, Sprite bgImage = null)
        {
            btnQMLoc = location.GetMenuName();
            Initialize(btnXPos, btnYPos, btnText, onAction, offAction, tooltip, defaultState, bgImage);
        }

        public QMToggleButton(DefaultVRCMenu location, float btnXPos, float btnYPos, string btnText, Action onAction, Action offAction, string tooltip, bool defaultState = false, Sprite bgImage = null)
        {
            btnQMLoc = "Menu_" + location;
            Initialize(btnXPos, btnYPos, btnText, onAction, offAction, tooltip, defaultState, bgImage);
        }

        public QMToggleButton(Transform target, float btnXPos, float btnYPos, string btnText, Action onAction, Action offAction, string tooltip, bool defaultState = false, Sprite bgImage = null)
        {
            parent = target;
            Initialize(btnXPos, btnYPos, btnText, onAction, offAction, tooltip, defaultState, bgImage);
        }

        private void Initialize(float btnXLocation, float btnYLocation, string btnText, Action onAction, Action offAction, string tooltip, bool defaultState, Sprite bgImage = null)
        {
            if (parent == null)
                parent = ApiUtils.QuickMenu.transform.Find("CanvasGroup/Container/Window/QMParent/" + btnQMLoc).transform;
            button = UnityEngine.Object.Instantiate(ApiUtils.GetQMButtonTemplate(), parent, true);
            button.name = $"{ApiUtils.Identifier}-Toggle-Button-{ApiUtils.RandomNumbers()}";
            button.transform.Find("Badge_MMJump").gameObject.SetActive(false);
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 176);
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(-68, -250);
            btnTextComp = button.GetComponentInChildren<TextMeshProUGUI>(true);
            btnTextComp.color = Color.white;
            btnComp = button.GetComponentInChildren<Button>(true);
            btnComp.onClick = new Button.ButtonClickedEvent();
            btnComp.onClick.AddListener(new Action(HandleClick));
            btnImageComp = button.transform.Find("Icons/Icon").GetComponentInChildren<Image>(true);
            btnImageComp.gameObject.SetActive(true);

            initShift[0] = 0;
            initShift[1] = 0;
            SetLocation(btnXLocation, btnYLocation);
            SetButtonText(btnText);
            SetButtonActions(onAction, offAction);
            SetTooltip(tooltip);
            SetActive(true);

            currentState = defaultState;
            var tmpIcon = currentState ? ApiUtils.OnIconSprite() : ApiUtils.OffIconSprite();
            btnImageComp.sprite = tmpIcon;
            btnImageComp.overrideSprite = tmpIcon;

            if (bgImage != null)
            {
                ToggleBackgroundImage(true);
                // SetBackgroundImage(bgImage);
            }
            else
            {
                ToggleBackgroundImage(false);
            }
        }

        private void HandleClick()
        {
            currentState = !currentState;
            var stateIcon = currentState ? ApiUtils.OnIconSprite() : ApiUtils.OffIconSprite();
            btnImageComp.sprite = stateIcon;
            btnImageComp.overrideSprite = stateIcon;
            //Config.Save();
            if (currentState)
                OnAction.Invoke();
            else
                OffAction.Invoke();
        }

        public void SetButtonText(string buttonText)
        {
            var tmp = button.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            tmp.richText = true;
            tmp.text = buttonText;
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

        public void SetButtonActions(Action onAction, Action offAction)
        {
            OnAction = onAction;
            OffAction = offAction;
        }

        public void SetToggleState(bool newState, bool shouldInvoke = false)
        {
            try
            {
                var newIcon = newState ? ApiUtils.OnIconSprite() : ApiUtils.OffIconSprite();
                btnImageComp.sprite = newIcon;
                btnImageComp.overrideSprite = newIcon;
                currentState = newState;

                if (shouldInvoke)
                {
                    if (newState)
                        OnAction.Invoke();
                    else
                        OffAction.Invoke();
                }
            }
            catch { }
        }

        public void SetInteractable(bool newState)
        {
            button.GetComponent<Button>().interactable = newState;
            RefreshButton();
        }

        public void ClickMe()
        {
            HandleClick();
        }

        public bool GetCurrentState()
        {
            return currentState;
        }

        private void RefreshButton()
        {
            button.SetActive(false);
            button.SetActive(true);
        }
    }
}
