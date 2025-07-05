using MelonLoader;
using Odium.Modules;
using Odium.Odium;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Odium.ButtonAPI.QM;
using VRC.Localization;
using VRC.UI.Elements;
using TMPro;
using System.IO;
using Odium.QMPages;

namespace Odium.Components
{
    class QM
    {
        public static List<string> ObjectsToFind = new List<string>
        {
            "UserInterface",
            "Canvas_QuickMenu(Clone)"
        };

        public static List<string> Menus = new List<string>
        {
            "Menus",
            "Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/BackgroundLayer01"
        };

        public static List<string> Buttons = new List<string>
        {
            "Buttons",
            "Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickLinks/Button_Worlds",
            "Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickLinks/Button_Avatars",
            "Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickLinks/Button_Social",
            "Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickLinks/Button_ViewGroups",
            "Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickActions/Button_Respawn",
            "Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickActions/Button_GoHome",
            "Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickActions/Button_SelectUser",
            "Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickActions/Button_Safety"
        };

        public static int currentObjectIndex = 0;

        public static void SetupMenu()
            => MelonCoroutines.Start(WaitForQM());

        private static IEnumerator WaitForQM()
        {
            while (UnityEngine.Object.FindObjectOfType<VRC.UI.Elements.QuickMenu>() == null)
                yield return null;

            AdBlock.OnQMInit();
            CreateMenu();

            OdiumConsole.LogGradient("Odium", $"{ObjectsToFind[currentObjectIndex]} found!");

            currentObjectIndex += 1;

            OdiumConsole.Log("Odium", $"Waiting for {ObjectsToFind[currentObjectIndex]} [{currentObjectIndex}]...", LogLevel.Info);

            AssignedVariables.quickMenu = AssignedVariables.userInterface.transform.Find(ObjectsToFind[currentObjectIndex])?.gameObject;
            if (AssignedVariables.quickMenu != null)
            {
                OdiumConsole.LogGradient("Odium", $"{ObjectsToFind[currentObjectIndex]} found!", LogLevel.Info);
                OdiumConsole.Log("Odium", $"Setting up {ObjectsToFind[currentObjectIndex]}...", LogLevel.Info);
                OdiumConsole.Log("Odium", $"Applying theme to {Menus[0]}...", LogLevel.Info);

                SpriteUtil.ApplySpriteToMenu(Menus[1], "QMBackground.png");

                OdiumConsole.Log("Odium", $"Applying theme to {Buttons[0]}...", LogLevel.Info);

                for (int i = 1; i < Buttons.Count; i++)
                {
                    SpriteUtil.ApplySpriteToButton(Buttons[i], "QMHalfButton.png");
                }
                yield break;
            }
        }

        public static void CreateMenu()
        {

            try
            {
                Sprite buttonImage = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\ButtonBackground.png");
                Sprite halfButtonImage = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\QMHalfButton.png");

                List<QMNestedMenu> nestedMenus = Entry.Initialize(buttonImage, halfButtonImage);

                World.InitializePage(nestedMenus[0], buttonImage);
                Movement.InitializePage(nestedMenus[1], buttonImage);
                Exploits.InitializePage(nestedMenus[2], buttonImage);
                Settings.InitializePage(nestedMenus[3], buttonImage);
                AppBot.InitializePage(nestedMenus[4], buttonImage, halfButtonImage);
                Visuals.InitializePage(nestedMenus[5], buttonImage);

            }
            catch (Exception ex)
            {
            }
        }

        public static IEnumerator WaitForUI()
        {
            OdiumConsole.Log("Odium", $"Waiting for {ObjectsToFind[currentObjectIndex]} [{currentObjectIndex}]...", LogLevel.Info);

            while ((AssignedVariables.userInterface = GameObject.Find("UserInterface")) == null)
                yield return null;

            OdiumConsole.LogGradient("Odium", $"{ObjectsToFind[currentObjectIndex]} found!");

            currentObjectIndex += 1;

            OdiumConsole.Log("Odium", $"Waiting for {ObjectsToFind[currentObjectIndex]} [{currentObjectIndex}]...", LogLevel.Info);

            while (true)
            {

                AssignedVariables.quickMenu = AssignedVariables.userInterface.transform.Find(ObjectsToFind[currentObjectIndex])?.gameObject;
                if (AssignedVariables.quickMenu != null)
                {
                    OdiumConsole.LogGradient("Odium", $"{ObjectsToFind[currentObjectIndex]} found!", LogLevel.Info);
                    OdiumConsole.Log("Odium", $"Setting up {ObjectsToFind[currentObjectIndex]}...", LogLevel.Info);
                    OdiumConsole.Log("Odium", $"Applying theme to {Menus[0]}...", LogLevel.Info);

                    SpriteUtil.ApplySpriteToMenu(Menus[1], "QMBackground.png");

                    OdiumConsole.Log("Odium", $"Applying theme to {Buttons[0]}...", LogLevel.Info);

                    for (int i = 1; i < Buttons.Count; i++)
                    {
                        SpriteUtil.ApplySpriteToButton(Buttons[i], "QMHalfButton.png");
                    }

                    break;
                }

                yield return null;
            }

        }
    }
}
