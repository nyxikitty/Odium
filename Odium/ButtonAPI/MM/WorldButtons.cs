using Odium.Odium;
using System;
using UnityEngine;
using UnityEngine.UI;
using VRC.Ui;

namespace Odium.ButtonAPI.MM
{
    public class MMWorldActionRow
    {
        public GameObject HeaderObject { get; private set; }
        public GameObject ActionsObject { get; private set; }
        public string Title { get; private set; }

        public MMWorldActionRow(string title)
        {
            Title = title;
            CreateRow();
        }

        private void CreateRow()
        {
            try
            {

                var menuContainer = AssignedVariables.userInterface.transform.Find("Canvas_MainMenu(Clone)/Container/MMParent/HeaderOffset/Menu_MM_WorldInformation/Panel_World_Information/Content/Viewport/BodyContainer_World_Details/ScrollRect/Viewport/VerticalLayoutGroup");

                if (menuContainer == null)
                {
                    Debug.LogError("Could not find menu container");
                    return;
                }

                var originalHeader = menuContainer.Find("Header_Actions");
                var originalActions = menuContainer.Find("Actions");

                if (originalHeader == null)
                {
                    Debug.LogError("Could not find Header_Actions to clone");
                    return;
                }

                if (originalActions == null)
                {
                    Debug.LogError("Could not find Actions to clone");
                    return;
                }

                OdiumConsole.Log("Odium", $"Creating new action row with title: {Title}");

                HeaderObject = UnityEngine.Object.Instantiate(originalHeader.gameObject);
                HeaderObject.transform.SetParent(menuContainer, false);
                HeaderObject.transform.localScale = Vector3.one;
                HeaderObject.name = $"Odium_Header_{Title.Replace(" ", "")}";

                ActionsObject = UnityEngine.Object.Instantiate(originalActions.gameObject);
                ActionsObject.transform.SetParent(menuContainer, false);
                ActionsObject.transform.localScale = Vector3.one;
                ActionsObject.name = $"Odium_Actions_{Title.Replace(" ", "")}";

                int originalActionsIndex = originalActions.GetSiblingIndex();
                HeaderObject.transform.SetSiblingIndex(originalActionsIndex + 1);
                ActionsObject.transform.SetSiblingIndex(originalActionsIndex + 2);

                UpdateHeaderTitle();

                ClearAllButtons();

                OdiumConsole.Log("Odium", $"Successfully created custom action row: {Title}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating custom action row: {ex.Message}");
            }
        }

        private void UpdateHeaderTitle()
        {
            if (HeaderObject == null) return;

            try
            {

                var textComponent = HeaderObject.GetComponentInChildren<TextMeshProUGUIEx>();
                if (textComponent == null)
                {

                    var leftContainer = HeaderObject.transform.Find("LeftItemContainer");
                    if (leftContainer != null)
                    {
                        var textTitle = leftContainer.Find("Text_Title");
                        if (textTitle != null)
                        {
                            textComponent = textTitle.GetComponent<TextMeshProUGUIEx>();
                        }
                    }
                }

                if (textComponent != null)
                {
                    textComponent.text = Title;
                    OdiumConsole.Log("Odium", $"Set header title to: {Title}");
                }
                else
                {
                    OdiumConsole.Log("Odium", "Warning: Could not find text component in header");
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("Odium", $"Error updating header title: {ex.Message}");
            }
        }

        private void ClearAllButtons()
        {
            if (ActionsObject == null) return;

            for (int i = ActionsObject.transform.childCount - 1; i >= 0; i--)
            {
                var child = ActionsObject.transform.GetChild(i);
                if (child.GetComponent<Button>() != null)
                {
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
            }
            OdiumConsole.Log("Odium", "Cleared existing buttons from cloned actions container");
        }

        public MMWorldButton AddButton(string text, Action action = null, Sprite icon = null)
        {
            return new MMWorldButton(this, text, action, icon);
        }

        public void Destroy()
        {
            if (HeaderObject != null)
            {
                UnityEngine.Object.DestroyImmediate(HeaderObject);
                HeaderObject = null;
            }

            if (ActionsObject != null)
            {
                UnityEngine.Object.DestroyImmediate(ActionsObject);
                ActionsObject = null;
            }

            OdiumConsole.Log("Odium", $"Destroyed action row: {Title}");
        }
    }

    public class MMWorldButton
    {
        public string Text { get; private set; }
        public Action Action { get; private set; }
        public Sprite Icon { get; private set; }
        public GameObject ButtonObject { get; private set; }

        public MMWorldButton(MMWorldActionRow actionRow, string text, Action action = null, Sprite icon = null)
        {
            Text = text;
            Action = action;
            Icon = icon;

            if (actionRow?.ActionsObject != null)
            {
                AttachToRow(actionRow.ActionsObject);
            }
            else
            {
                Debug.LogError("Action row is null or not properly initialized");
            }
        }

        private void AttachToRow(GameObject actionsContainer)
        {
            try
            {
                if (actionsContainer == null)
                {
                    Debug.LogError("Actions container is null");
                    return;
                }

                var menuContainer = AssignedVariables.userInterface.transform.Find("Canvas_MainMenu(Clone)/Container/MMParent/HeaderOffset/Menu_MM_WorldInformation/Panel_World_Information/Content/Viewport/BodyContainer_World_Details/ScrollRect/Viewport/VerticalLayoutGroup");
                var originalActions = menuContainer?.Find("Actions");
                GameObject templateButton = null;

                if (originalActions != null)
                {

                    var makeHomeButton = originalActions.Find("MakeHome");
                    if (makeHomeButton != null)
                    {
                        templateButton = makeHomeButton.gameObject;
                    }
                    else
                    {

                        for (int i = 0; i < originalActions.childCount; i++)
                        {
                            var child = originalActions.GetChild(i);
                            if (child.GetComponent<Button>() != null)
                            {
                                templateButton = child.gameObject;
                                break;
                            }
                        }
                    }
                }

                if (templateButton == null)
                {
                    Debug.LogError("Could not find template button");
                    return;
                }

                OdiumConsole.Log("Odium", $"Adding button '{Text}' to actions container");

                ButtonObject = UnityEngine.Object.Instantiate(templateButton);
                ButtonObject.transform.SetParent(actionsContainer.transform, false);
                ButtonObject.transform.localScale = Vector3.one;
                ButtonObject.transform.localPosition = Vector3.zero;
                ButtonObject.transform.localRotation = Quaternion.identity;

                var buttonComponent = ButtonObject.GetComponent<Button>();
                if (buttonComponent == null)
                {
                    Debug.LogError("Cloned object doesn't have Button component");
                    UnityEngine.Object.DestroyImmediate(ButtonObject);
                    return;
                }

                UpdateButtonText();

                UpdateButtonIcon();

                buttonComponent.onClick.RemoveAllListeners();
                buttonComponent.onClick.AddListener(new Action(() =>
                {
                    OdiumConsole.Log("Odium", $"Custom button '{Text}' clicked");
                    Action?.Invoke();
                }));

                ButtonObject.name = $"Odium_CustomButton_{Text.Replace(" ", "")}";

                OdiumConsole.Log("Odium", $"Successfully added button '{Text}' to actions container");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error adding button to actions container: {ex.Message}");
            }
        }

        private void UpdateButtonText()
        {
            if (ButtonObject == null) return;

            var buttonTextComponent = ButtonObject.GetComponentInChildren<TextMeshProUGUIEx>();
            if (buttonTextComponent == null)
            {

                var textTransform = ButtonObject.transform.Find("Text_ButtonName");
                if (textTransform != null)
                    buttonTextComponent = textTransform.GetComponent<TextMeshProUGUIEx>();
            }

            if (buttonTextComponent != null)
            {
                buttonTextComponent.text = Text;
                OdiumConsole.Log("Odium", $"Set button text to: {Text}");
            }
            else
            {
                OdiumConsole.Log("Odium", "Warning: Could not find text component");
            }
        }

        private void UpdateButtonIcon()
        {
            if (ButtonObject == null) return;

            ImageEx iconImage = null;
            var textButtonNameTransform = ButtonObject.transform.Find("Text_ButtonName");
            if (textButtonNameTransform != null)
            {
                var iconTransform = textButtonNameTransform.Find("Icon");
                if (iconTransform != null)
                {
                    iconImage = iconTransform.GetComponent<ImageEx>();
                }
            }

            if (Icon != null && iconImage != null)
            {
                iconImage.sprite = Icon;
                iconImage.enabled = true;
                OdiumConsole.Log("Odium", "Icon set successfully");
            }
            else if (iconImage != null)
            {
                iconImage.enabled = false;
                OdiumConsole.Log("Odium", "Icon hidden (no sprite provided)");
            }
        }

        public void Destroy()
        {
            if (ButtonObject != null)
            {
                UnityEngine.Object.DestroyImmediate(ButtonObject);
                ButtonObject = null;
                OdiumConsole.Log("Odium", $"Destroyed button: {Text}");
            }
        }
    }

    public static class MMWorldExtensions
    {
        public static MMWorldButton AddButton(this MMWorldActionRow row, string text, Action action = null, Sprite icon = null)
        {
            return new MMWorldButton(row, text, action, icon);
        }
    }
}