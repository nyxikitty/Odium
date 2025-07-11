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
using Odium.ButtonAPI.MM;
using Odium.Wrappers;
using System.Windows.Forms;
using Odium.Threadding;
using Odium.IUserPage.MM;

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

                Sprite DeafenIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\Deafen.png");
                Sprite HeadphonesIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\Headphones.png");
                Sprite MuteIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\Mute.png");
                Sprite MicrophoneIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\Microphone.png");
                Sprite SkipIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\Skip.png");
                Sprite PauseIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\Pause.png");
                Sprite RewindIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\Rewind.png");
                Sprite PlayIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\Play.png");
                Sprite LogoIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\OdiumIcon.png");

                List<QMNestedMenu> nestedMenus = Entry.Initialize(buttonImage, halfButtonImage);

                World.InitializePage(nestedMenus[0], buttonImage);
                Movement.InitializePage(nestedMenus[1], buttonImage);
                Exploits.InitializePage(nestedMenus[2], buttonImage);
                Settings.InitializePage(nestedMenus[3], buttonImage);
                AppBot.InitializePage(nestedMenus[4], buttonImage, halfButtonImage);
                Visuals.InitializePage(nestedMenus[5], buttonImage);
                GameHacks.InitializePage(nestedMenus[6], buttonImage);

                Functions.Initialize();
                WorldFunctions.Initialize();

                QMMainIconButton.CreateButton(RewindIcon, () => {
                    MediaControls.SpotifyRewind();
                });


                QMMainIconButton.CreateToggle(PauseIcon, PlayIcon,
                    () => {
                        MediaControls.SpotifyPause();
                    },
                    () => {
                        MediaControls.SpotifyPause();
                    });

                QMMainIconButton.CreateButton(SkipIcon, () =>
                {
                    MediaControls.SpotifySkip();
                });

                QMMainIconButton.CreateImage(LogoIcon, new Vector3(-150, -50), new Vector3(2.5f, 2.5f), false);

                Transform header = AssignedVariables.userInterface.transform.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/Header_H1");
                header.transform.localPosition = new Vector3(125.6729f, 1024f, 0f);

                Transform itemContainer = AssignedVariables.userInterface.transform.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/Header_H1/RightItemContainer");
                Transform iconButton = itemContainer.Find("Button_QM_Report");

                iconButton.gameObject.SetActive(false);

                Transform itemContainer2 = AssignedVariables.userInterface.transform.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/Header_H1/RightItemContainer");
                Transform iconButton2 = itemContainer.Find("Button_QM_Expand");

                iconButton2.gameObject.SetActive(false);
                DebugUI.InitializeDebugMenu();
                PlayerDebugUI.InitializeDebugMenu();

                SidebarListItemCloner.CreateSidebarItem("Odium Users");
                OdiumPerformancePanel.ShowPerformancePanel();
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("Odium", $"Error creating menu: {ex.Message}", LogLevel.Error);
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
