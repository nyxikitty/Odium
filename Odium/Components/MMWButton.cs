using Odium.Odium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRC.Localization;

namespace Odium.Components
{
    class MMWButton
    {
        public static GameObject CreateCustomWorldButton(string buttonText, string iconPath, UnityEngine.Events.UnityAction clickAction)
        {
            try
            {
                // Find the template and parent
                GameObject worldButtonTemplate = AssignedVariables.userInterface.transform
                    .Find("Canvas_MainMenu(Clone)/Container/MMParent/HeaderOffset/Menu_MM_WorldInformation/Panel_World_Information/Content/Viewport/BodyContainer_World_Details/ScrollRect/Viewport/VerticalLayoutGroup/Actions/AddToFavorites")
                    .gameObject;

                Transform actionsPage = AssignedVariables.userInterface.transform
                    .Find("Canvas_MainMenu(Clone)/Container/MMParent/HeaderOffset/Menu_MM_WorldInformation/Panel_World_Information/Content/Viewport/BodyContainer_World_Details/ScrollRect/Viewport/VerticalLayoutGroup/Actions")
                    .transform;

                if (worldButtonTemplate == null || actionsPage == null)
                {
                    Debug.LogError("Failed to find template or parent transform");
                    return null;
                }

                // Create the new button
                GameObject newButton = GameObject.Instantiate(worldButtonTemplate, actionsPage);
                newButton.name = "MMWButton_" + Guid.NewGuid();

                // Set up localization
                var lstring = new LocalizableString();
                lstring._localizationKey = buttonText;

                var textComponent = newButton.transform.Find("Text_ButtonName")?.GetComponent<TextMeshProUGUIEx>();
                if (textComponent != null)
                {
                    textComponent._localizableString = lstring;
                }

                // Set up icon if path provided
                if (!string.IsNullOrEmpty(iconPath))
                {
                    Sprite icon = SpriteUtil.LoadFromDisk(iconPath);
                    var iconComponent = newButton.transform.Find("Text_ButtonName/Icon_Add")?.GetComponent<VRC.UI.ImageEx>();
                    if (iconComponent != null && icon != null)
                    {
                        iconComponent.sprite = icon;
                    }
                }

                // Get the button component and clear existing listeners
                UnityEngine.UI.Button buttonComponent = newButton.GetComponent<UnityEngine.UI.Button>();
                if (buttonComponent != null)
                {
                    // Remove all existing listeners
                    buttonComponent.onClick.RemoveAllListeners();

                    // Add the custom action if provided
                    if (clickAction != null)
                    {
                        buttonComponent.onClick.AddListener(clickAction);
                    }
                }

                // Make sure the button is active
                newButton.SetActive(true);

                return newButton;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create custom world button: {ex.Message}");
                return null;
            }
        }
    }
}
