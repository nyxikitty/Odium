using Odium.Odium;
using System;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI;

namespace Odium.ButtonAPI.MM
{
    public class MMUserActionRow
    {
        public GameObject RowObject { get; private set; }
        public string Title { get; private set; }

        public MMUserActionRow(string title)
        {
            Title = title;
            CreateRow();
        }

        private void CreateRow()
        {
            try
            {

                var menuContainer = AssignedVariables.userInterface.transform.Find("Canvas_MainMenu(Clone)/Container/MMParent/HeaderOffset/Menu_UserDetail/ScrollRect/Viewport/VerticalLayoutGroup");

                if (menuContainer == null)
                {
                    Debug.LogError("Could not find user detail menu container");
                    return;
                }

                var row3 = menuContainer.Find("Row3");
                if (row3 == null)
                {
                    Debug.LogError("Could not find Row3 to clone");
                    return;
                }

                OdiumConsole.Log("Odium", $"Creating new user action row with title: {Title}");

                RowObject = UnityEngine.Object.Instantiate(row3.gameObject);
                RowObject.transform.SetParent(menuContainer, false);
                RowObject.transform.localScale = Vector3.one;
                RowObject.transform.localPosition = Vector3.zero;
                RowObject.transform.localRotation = Quaternion.identity;

                ClearAllButtons();

                SetRowTitle();

                RowObject.name = $"Odium_CustomUserRow_{Title.Replace(" ", "")}";

                OdiumConsole.Log("Odium", $"Successfully created custom user action row: {Title}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating custom user action row: {ex.Message}");
            }
        }

        private void ClearAllButtons()
        {
            if (RowObject == null) return;

            var cellGrid = RowObject.transform.Find("CellGrid_MM_Content");
            if (cellGrid != null)
            {
                for (int i = cellGrid.childCount - 1; i >= 0; i--)
                {
                    UnityEngine.Object.DestroyImmediate(cellGrid.GetChild(i).gameObject);
                }
                OdiumConsole.Log("Odium", "Cleared existing buttons from cloned user row");
            }
        }

        private void SetRowTitle()
        {
            if (RowObject == null) return;

            try
            {
                var headerH2 = RowObject.transform.Find("Header_H2");
                if (headerH2 != null)
                {
                    var leftItemContainer = headerH2.Find("LeftItemContainer");
                    if (leftItemContainer != null)
                    {
                        var textTitle = leftItemContainer.Find("Text_Title");
                        if (textTitle != null)
                        {
                            var textComponent = textTitle.GetComponent<TextMeshProUGUIEx>();
                            if (textComponent != null)
                            {
                                textComponent.text = Title;
                                OdiumConsole.Log("Odium", $"Set user row title to: {Title}");
                            }
                            else
                            {
                                OdiumConsole.Log("Odium", "Warning: Could not find TextMeshProUGUIEx component on Text_Title");
                            }
                        }
                        else
                        {
                            OdiumConsole.Log("Odium", "Warning: Could not find Text_Title");
                        }
                    }
                    else
                    {
                        OdiumConsole.Log("Odium", "Warning: Could not find LeftItemContainer");
                    }
                }
                else
                {
                    OdiumConsole.Log("Odium", "Warning: Could not find Header_H2");
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("Odium", $"Error setting row title: {ex.Message}");
            }
        }

        public MMUserButton AddButton(string text, Action action = null, Sprite icon = null)
        {
            return new MMUserButton(this, text, action, icon);
        }

        public void Destroy()
        {
            if (RowObject != null)
            {
                UnityEngine.Object.DestroyImmediate(RowObject);
                RowObject = null;
                OdiumConsole.Log("Odium", $"Destroyed user action row: {Title}");
            }
        }
    }

    public class MMUserButton
    {
        public string Text { get; private set; }
        public Action Action { get; private set; }
        public Sprite Icon { get; private set; }
        public GameObject ButtonObject { get; private set; }

        public MMUserButton(MMUserActionRow actionRow, string text, Action action = null, Sprite icon = null)
        {
            Text = text;
            Action = action;
            Icon = icon;

            if (actionRow?.RowObject != null)
            {
                AttachToRow(actionRow.RowObject);
            }
            else
            {
                Debug.LogError("User action row is null or not properly initialized");
            }
        }

        private void AttachToRow(GameObject customRow)
        {
            try
            {
                if (customRow == null)
                {
                    Debug.LogError("Custom user row is null");
                    return;
                }

                var cellGrid = customRow.transform.Find("CellGrid_MM_Content");
                if (cellGrid == null)
                {
                    Debug.LogError("Could not find CellGrid_MM_Content in custom user row");
                    return;
                }

                var menuContainer = AssignedVariables.userInterface.transform.Find("Canvas_MainMenu(Clone)/Container/MMParent/HeaderOffset/Menu_UserDetail/ScrollRect/Viewport/VerticalLayoutGroup");
                var originalRow3 = menuContainer?.Find("Row3");
                var originalCellGrid = originalRow3?.Find("CellGrid_MM_Content");
                var templateButton = originalCellGrid?.GetComponentInChildren<Button>();

                if (templateButton == null)
                {
                    Debug.LogError("Could not find template button to clone");
                    return;
                }

                OdiumConsole.Log("Odium", $"Adding button '{Text}' to custom user row");

                ButtonObject = UnityEngine.Object.Instantiate(templateButton.gameObject);
                ButtonObject.transform.SetParent(cellGrid, false);
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
                    OdiumConsole.Log("Odium", $"Custom user button '{Text}' clicked");
                    Action?.Invoke();
                }));

                ButtonObject.name = $"Odium_CustomUserButton_{Text.Replace(" ", "")}";

                OdiumConsole.Log("Odium", $"Successfully added button '{Text}' to custom user row");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error adding button to custom user row: {ex.Message}");
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
                OdiumConsole.Log("Odium", $"Destroyed user button: {Text}");
            }
        }
    }

    public static class MMUserExtensions
    {
        public static MMUserButton AddButton(this MMUserActionRow row, string text, Action action = null, Sprite icon = null)
        {
            return new MMUserButton(row, text, action, icon);
        }
    }
}