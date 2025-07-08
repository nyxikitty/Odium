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
                // Get the right container and template button
                Transform itemContainer = AssignedVariables.userInterface.transform.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/Header_H1/RightItemContainer");
                Transform iconButton = itemContainer.Find("Button_QM_Report");

                if (itemContainer == null || iconButton == null)
                {
                    Debug.LogError("Could not find required QuickMenu elements!");
                    return;
                }

                // Create new button
                GameObject newIconButton = GameObject.Instantiate(iconButton.gameObject, itemContainer);
                newIconButton.name = "Button_QMOdium" + Guid.NewGuid();
                newIconButton.SetActive(true);

                // Reset transform
                RectTransform rectTransform = newIconButton.GetComponent<RectTransform>();
                rectTransform.localPosition = Vector3.zero;
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localScale = Vector3.one;

                // Set icon
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

                // Setup button handler
                Button button = newIconButton.GetComponent<Button>();
                if (button != null && onClick != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(onClick);
                }

                // Reorder to be before the report button
                newIconButton.transform.SetSiblingIndex(iconButton.GetSiblingIndex());
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
                // Get the right container and template button
                Transform itemContainer = AssignedVariables.userInterface.transform.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/Header_H1/RightItemContainer");
                Transform iconButton = itemContainer.Find("Button_QM_Report");

                if (itemContainer == null || iconButton == null)
                {
                    Debug.LogError("Could not find required QuickMenu elements!");
                    return;
                }

                // Create new button
                GameObject newToggleButton = GameObject.Instantiate(iconButton.gameObject, itemContainer);
                newToggleButton.name = "Toggle_QMOdium" + Guid.NewGuid();
                newToggleButton.SetActive(true);

                // Reset transform
                RectTransform rectTransform = newToggleButton.GetComponent<RectTransform>();
                rectTransform.localPosition = Vector3.zero;
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localScale = Vector3.one;

                // Get icon transform
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