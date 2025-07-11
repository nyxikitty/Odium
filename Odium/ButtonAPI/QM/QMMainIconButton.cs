using Odium.Odium;
using System;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Elements;

namespace Odium.ButtonAPI.QM
{
    class QMMainIconButton
    {
        public static void CreateButton(Sprite sprite, Action onClick = null)
        {
            try
            {
                Transform itemContainer = AssignedVariables.userInterface.transform.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/Header_H1/RightItemContainer");
                Transform iconButton = itemContainer.Find("Button_QM_Report");

                if (itemContainer == null || iconButton == null)
                {
                    Debug.LogError("Could not find required QuickMenu elements!");
                    return;
                }

                GameObject newIconButton = GameObject.Instantiate(iconButton.gameObject, itemContainer);
                newIconButton.name = "Button_QMOdium" + Guid.NewGuid();
                newIconButton.SetActive(true);

                RectTransform rectTransform = newIconButton.GetComponent<RectTransform>();
                rectTransform.localPosition = Vector3.zero;
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localScale = Vector3.one;

                Transform iconTransform = newIconButton.transform.Find("Icon");
                if (iconTransform != null)
                {
                    Image iconImage = iconTransform.GetComponent<Image>();
                    if (iconImage != null)
                    {
                        iconImage.sprite = sprite;
                        iconImage.overrideSprite = sprite;
                    }
                }

                Button button = newIconButton.GetComponent<Button>();
                if (button != null && onClick != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(onClick);
                }

                newIconButton.transform.SetSiblingIndex(iconButton.GetSiblingIndex());
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating QM icon button: {ex}");
            }
        }

        public static void CreateImage(Sprite sprite, Vector3 position, Vector3 size, bool includeBackground = false)
        {
            try
            {
                Transform itemContainer = AssignedVariables.userInterface.transform.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/Header_H1/RightItemContainer");
                Transform iconButton = itemContainer.Find("Button_QM_Report");

                if (itemContainer == null || iconButton == null)
                {
                    Debug.LogError("Could not find required QuickMenu elements!");
                    return;
                }

                GameObject newIconButton = GameObject.Instantiate(iconButton.gameObject, itemContainer);
                newIconButton.name = "Button_QMOdium" + Guid.NewGuid();
                newIconButton.SetActive(true);

                RectTransform rectTransform = newIconButton.GetComponent<RectTransform>();
                rectTransform.localPosition = position;
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localScale = size;

                Transform iconTransform = newIconButton.transform.Find("Icon");
                iconTransform.localPosition = new Vector3(-208.8547f, -22.7455f, 0);

                if (iconTransform != null)
                {
                    Image iconImage = iconTransform.GetComponent<Image>();
                    if (iconImage != null)
                    {
                        iconImage.sprite = sprite;
                        iconImage.overrideSprite = sprite;
                    }
                }

                var background = newIconButton.transform.FindChild("Background");
                background.gameObject.SetActive(includeBackground);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating QM icon button: {ex}");
            }
        }

        public static void CreateToggle(Sprite onSprite, Sprite offSprite, Action onAction = null, Action offAction = null)
        {
            try
            {
                Transform itemContainer = AssignedVariables.userInterface.transform.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/Header_H1/RightItemContainer");
                Transform iconButton = itemContainer.Find("Button_QM_Report");

                if (itemContainer == null || iconButton == null)
                {
                    Debug.LogError("Could not find required QuickMenu elements!");
                    return;
                }

                GameObject newToggleButton = GameObject.Instantiate(iconButton.gameObject, itemContainer);
                newToggleButton.name = "Toggle_QMOdium" + Guid.NewGuid();
                newToggleButton.SetActive(true);

                RectTransform rectTransform = newToggleButton.GetComponent<RectTransform>();
                rectTransform.localPosition = Vector3.zero;
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localScale = Vector3.one;

                Transform iconTransform = newToggleButton.transform.Find("Icon");
                if (iconTransform == null)
                {
                    Debug.LogError("Could not find Icon transform!");
                    return;
                }

                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage == null)
                {
                    Debug.LogError("Could not find Image component on Icon!");
                    return;
                }

                bool isToggled = false;
                iconImage.sprite = offSprite;
                iconImage.overrideSprite = offSprite;

                Button button = newToggleButton.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(new Action(() =>
                    {
                        isToggled = !isToggled;

                        if (isToggled)
                        {
                            iconImage.sprite = onSprite;
                            iconImage.overrideSprite = onSprite;
                            onAction?.Invoke();
                        }
                        else
                        {
                            iconImage.sprite = offSprite;
                            iconImage.overrideSprite = offSprite;
                            offAction?.Invoke();
                        }
                    }));
                }

                newToggleButton.transform.SetSiblingIndex(iconButton.GetSiblingIndex());
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating QM toggle button: {ex}");
            }
        }
    }
}