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
using Odium.Api.MM;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Odium.API;
using VRC.Core;
using VRC;

namespace Odium.Components
{
    public class QM
    {
        internal static MMButton pingLabel;
        internal static MMButton activeRoomsLabel;
        internal static MMButton activeUsersLabel;
        internal static MMButton subscriptionStatusLabel;

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
            OdiumNotificationLoader.Initialize();
            OdiumBottomNotification.Initialize();
            OdiumInputDialog.Initialize();
            OdiumMusicPlayer.Initialize();
            Modules.WindowsMediaController.Initialize();

            OdiumMusicPlayer.SetLocalPosition(new Vector3(-1276.393f, -259.0728f, -15.7656f));
            OdiumMusicPlayer.Show();

            OdiumConsole.LogGradient("MusicPlayerSetup", "Music player fully initialized and configured!", LogLevel.Info);

            string track = Modules.WindowsMediaController.GetTrackName();
            string artist = Modules.WindowsMediaController.GetArtistName();
            bool isPlaying = Modules.WindowsMediaController.IsPlaying();
            Sprite albumArt = Modules.WindowsMediaController.GetAlbumArtSprite();

            OdiumMusicPlayer.SetTrackName("Always");
            OdiumMusicPlayer.SetArtistName("Daniel Caesar");
            OdiumMusicPlayer.SetCurrentTrackTime("1");

            // Subscribe to events
            Modules.WindowsMediaController.OnMediaInfoChanged += (mediaInfo) => {
                OdiumMusicPlayer.SetTrackName(mediaInfo.trackName);
                OdiumMusicPlayer.SetArtistName(mediaInfo.artistName);
            };

            Modules.WindowsMediaController.OnAlbumArtChanged += (texture) => {
                if (texture != null)
                {
                    OdiumMusicPlayer.SetAlbumArtFromTexture2D(texture);
                }
            };

            // Debug info
            Modules.WindowsMediaController.DebugCurrentSession();

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
                Protections.InitializePage(nestedMenus[7], buttonImage);
                Conduit.InitializePage(nestedMenus[8], buttonImage);

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

                // Center Image
                QMMainIconButton.CreateImage(LogoIcon, new Vector3(-150, -50), new Vector3(2.5f, 2.5f), false);

                Transform header = AssignedVariables.userInterface.transform.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/Header_H1");
                header.transform.localPosition = new Vector3(125.6729f, 1024f, 0f);

                // Right container
                Transform itemContainer = AssignedVariables.userInterface.transform.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/Header_H1/RightItemContainer");
                Transform iconButton = itemContainer.Find("Button_QM_Report");

                iconButton.gameObject.SetActive(false);

                // Left container
                Transform itemContainer2 = AssignedVariables.userInterface.transform.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_Dashboard/Header_H1/RightItemContainer");
                Transform iconButton2 = itemContainer.Find("Button_QM_Expand");

                iconButton2.gameObject.SetActive(false);

                DebugUI.InitializeDebugMenu();
                PlayerDebugUI.InitializeDebugMenu();

                SidebarListItemCloner.CreateSidebarItem("Odium Users");

                EventHandlers.PingServer();
                EventHandlers.RequestActiveCount();

                // Create main menu with proper sizing
                MMMenu mainMenu = new MMMenu("Odium");
                mainMenu.GetGameObject().transform.Find("Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/DynamicSidePanel_Header")
                    .GetComponent<RectTransform>().sizeDelta = new Vector2(-32, 100);
                var rt = mainMenu.GetGameObject().transform.Find("Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Navigation").GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0, -100);
                rt.sizeDelta = Vector2.zero;

                // Load common icons
                Sprite odiumIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\OdiumIcon.png");
                Sprite worldIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\WorldIcon.png");
                Sprite settingsIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\Reset.png");
                Sprite securityIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\ShieldIcon.png");
                Sprite playersIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\People.png");
                Sprite uiIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\UI.png");

                // Main tab button
                MMTabButton mmTabMenu = new MMTabButton(mainMenu, "Odium Client", "Main features and controls for Odium", odiumIcon);

                // ===============================
                // MAIN PAGES
                // ===============================

                MMPage dashboardPage = new MMPage(mainMenu, "Dashboard", "Odium Dashboard", "Quick access to main features", odiumIcon);
                MMPage worldPage = new MMPage(mainMenu, "World", "World Features", "World manipulation and tools", worldIcon);
                MMPage playerPage = new MMPage(mainMenu, "Players", "Player Features", "Player interaction and utilities", playersIcon);
                MMPage uiPage = new MMPage(mainMenu, "Interface", "UI & Visual", "User interface and visual modifications", uiIcon);
                MMPage securityPage = new MMPage(mainMenu, "Security", "Security & Protection", "Safety and protection features", securityIcon);
                MMPage settingsPage = new MMPage(mainMenu, "Settings", "Configuration", "Application settings and preferences", settingsIcon);

                // ===============================
                // DASHBOARD PAGE - Quick access to common features
                // ===============================

                MMContainer dashboardGeneral = new MMContainer(dashboardPage, "Quick Actions");
                MMContainer dashboardStatus = new MMContainer(dashboardPage, "System Status");


                pingLabel = new MMButton(
                    dashboardStatus,
                    "0 ms",
                    "Ping",
                    () =>
                    {
                        EventHandlers.PingServer();
                        OdiumConsole.Log("Ping", "Ping refresh requested");
                    },
                    "Refresh Ping",
                    settingsIcon
                );

                activeRoomsLabel = new MMButton(
                    dashboardStatus,
                    "0",
                    "Rooms",
                    () =>
                    {
                        EventHandlers.RequestActiveCount();
                        OdiumConsole.Log("Rooms", "Active rooms status checked");
                    },
                    "Check Active Rooms",
                    settingsIcon
                );

                activeUsersLabel = new MMButton(
                    dashboardStatus,
                    "0",
                    "Users",
                    () =>
                    {
                        EventHandlers.RequestActiveCount();
                        OdiumConsole.Log("Users", "Active users status checked");
                    },
                    "Check Active Users",
                    settingsIcon
                );

                subscriptionStatusLabel = new MMButton(
                    dashboardStatus,
                    "Active",
                    "Status",
                    () =>
                    {
                        EventHandlers.RequestActiveCount();
                        OdiumConsole.Log("Subscription", "Subscription status checked");
                    },
                    "Check Subscription Status",
                    settingsIcon
                );

                // Quick toggle buttons for main features
                new MMToggle(dashboardGeneral, "Master Protection", new Action<bool>((v) => {
                    // Toggle all protection features
                    AssignedVariables.chatBoxAntis = v;
                    AssignedVariables.instanceLock = v;
                    OdiumConsole.Log("Dashboard", $"Master protection: {(v ? "Enabled" : "Disabled")}");
                }), "Enable/disable all protection features", true);

                new MMToggle(dashboardGeneral, "Debug Mode", new Action<bool>((v) => {
                    DebugUI.SetVisibility(v);
                    PlayerDebugUI.SetVisibility(v);
                    OdiumConsole.Log("Dashboard", $"Debug mode: {(v ? "Enabled" : "Disabled")}");
                }), "Toggle all debug interfaces", false);

                new MMDivider(dashboardGeneral);

                var refreshAllButton = new MMButton(
                    dashboardGeneral,
                    "Refresh Everything",
                    "Refresh All",
                    () =>
                    {
                        NameplateModifier.RefreshEverything();
                        OdiumConsole.Log("Dashboard", "Refreshing all systems...");
                    },
                    "Refresh all Odium modifications",
                    settingsIcon
                );

                // ===============================
                // WORLD PAGE - World manipulation
                // ===============================

                MMContainer worldGeneral = new MMContainer(worldPage, "World Control");
                MMContainer worldGames = new MMContainer(worldPage, "Game Modes");
                MMContainer worldUdon = new MMContainer(worldPage, "UDON Security");

                // World general features
                new MMToggle(worldGeneral, "Instance Protection", new Action<bool>((v) => {
                    AssignedVariables.instanceLock = v;
                }), "Prevent unwanted instance modifications", false);

                // Game-specific features
                new MMToggle(worldGames, "Murder 4 Instance Lock", new Action<bool>((v) => {
                    AssignedVariables.instanceLock = v;
                }), "Prevent Murder 4 game manipulation", false);

                new MMDivider(worldGames);

                // UDON security features
                new MMToggle(worldUdon, "UDON Parameter Check", new Action<bool>((v) => {
                    AssignedVariables.udonParamCheck = v;
                }), "Log invalid event parameters and missing keys", AssignedVariables.udonParamCheck);

                new MMToggle(worldUdon, "UDON Dictionary Check", new Action<bool>((v) => {
                    AssignedVariables.udonDictCheck = v;
                }), "Log missing required dictionary keys in UDON events", AssignedVariables.udonDictCheck);

                new MMToggle(worldUdon, "Data Type Validation", new Action<bool>((v) => {
                    AssignedVariables.dataTypeCheck = v;
                }), "Log invalid data types during unboxing operations", AssignedVariables.dataTypeCheck);

                new MMToggle(worldUdon, "View ID Filtering", new Action<bool>((v) => {
                    AssignedVariables.filterViewIds = v;
                }), "Log suspicious viewIds and invalid PhotonViews", AssignedVariables.filterViewIds);

                new MMToggle(worldUdon, "UDON Data Validation", new Action<bool>((v) => {
                    AssignedVariables.udonDataCheck = v;
                }), "Log invalid player data in UDON events", AssignedVariables.udonDataCheck);

                // ===============================
                // PLAYER PAGE - Player interaction
                // ===============================

                MMContainer playerNameplates = new MMContainer(playerPage, "Nameplates");
                MMContainer playerInteraction = new MMContainer(playerPage, "Player Tools");
                MMContainer playerSafety = new MMContainer(playerPage, "Safety Features");

                new MMButton(
                    playerNameplates,
                    "Odium Avatar",
                    "Switch",
                    () =>
                    {
                        SimpleAvatarPedestal pedestal = new SimpleAvatarPedestal();
                        pedestal.Method_Private_Void_ApiAvatar_0(new ApiAvatar
                        {
                            id = "avtr_63f6f843-c894-4936-b613-650961b4cae0",
                            name = "OdiumBotAvatar",
                            authorName = "OdiumBot",
                            imageUrl = "https://api.vrchat.cloud/api/1/image/file_c13cebe8-4c02-4a6c-8eec-dead81595570/1/512"
                        });
                        pedestal.Method_Public_Void_0();
                    },
                    "Refresh all nameplate modifications",
                    settingsIcon
                );

                // Nameplate features
                new MMToggle(playerNameplates, "Custom Nameplates", new Action<bool>((v) => {
                    NameplateModifier.SetCustomBackground(v);
                    NameplateModifier.SetJoinDateVisibility(v);
                    NameplateModifier.SetCrashDetection(v);
                }), "Enable custom nameplate modifications", true);

                new MMDivider(playerNameplates);

                new MMToggle(playerNameplates, "Show Join Dates", new Action<bool>((v) => {
                    NameplateModifier.SetJoinDateVisibility(v);
                }), "Display player account creation dates", true);

                new MMToggle(playerNameplates, "Nameplate Statistics", new Action<bool>((v) => {
                    NameplateModifier.SetPlateStats(v);
                }), "Show detailed player statistics", true);

                new MMToggle(playerNameplates, "Crash Detection", new Action<bool>((v) => {
                    NameplateModifier.SetCrashDetection(v);
                }), "Detect and mark crashed players", true);

                new MMDivider(playerNameplates);

                var nameplateRefreshButton = new MMButton(
                    playerNameplates,
                    "Refresh Nameplates",
                    "Refresh",
                    () =>
                    {
                        NameplateModifier.RefreshEverything();
                    },
                    "Refresh all nameplate modifications",
                    settingsIcon
                );

                new MMDivider(playerNameplates);

                // Nameplate positioning controls
                var yOffsetSlider = new MMBigSlider(
                    playerNameplates,
                    "Stats Y Position",
                    180f,
                    0f,
                    500f,
                    (value) =>
                    {
                        NameplateModifier.SetStatsYOffset(value);
                    },
                    "Adjust vertical position of nameplate stats"
                );

                var spacingSlider = new MMBigSlider(
                    playerNameplates,
                    "Plate Spacing",
                    30f,
                    10f,
                    100f,
                    (value) =>
                    {
                        NameplateModifier.SetPlateSpacing(value);
                    },
                    "Adjust spacing between nameplate plates"
                );

                // ===============================
                // UI PAGE - Interface modifications
                // ===============================

                MMContainer uiQuickMenu = new MMContainer(uiPage, "Quick Menu");
                MMContainer uiChatbox = new MMContainer(uiPage, "Chatbox");
                MMContainer uiVisual = new MMContainer(uiPage, "Visual Effects");

                // Quick Menu UI
                new MMToggle(uiQuickMenu, "Player List UI", new Action<bool>((v) => {
                    PlayerDebugUI.SetVisibility(v);
                }), "Toggle Quick Menu Player List interface", true);

                new MMToggle(uiQuickMenu, "Debug Interface", new Action<bool>((v) => {
                    DebugUI.SetVisibility(v);
                }), "Toggle debug information overlay", false);

                // Chatbox modifications
                new MMToggle(uiChatbox, "Content Filtering", new Action<bool>((v) => {
                    AssignedVariables.chatBoxAntis = v;
                }), "Filter spam and unwanted content from chatboxes", true);

                // Visual quality settings
                var qualityEnum = new MMEnum(
                    uiVisual,
                    "Rendering Quality",
                    new string[] { "Performance", "Balanced", "Quality", "Ultra" },
                    1,
                    (option, index) =>
                    {
                        OdiumConsole.Log("UI", $"Quality preset changed to: {option} (Level {index})");
                        // Apply quality settings here
                    },
                    "Adjust rendering quality preset"
                );

                // ===============================
                // SECURITY PAGE - Protection features
                // ===============================

                MMContainer securityGeneral = new MMContainer(securityPage, "General Protection");
                MMContainer securityAnti = new MMContainer(securityPage, "Anti-Exploitation");
                MMContainer securityLogging = new MMContainer(securityPage, "Security Logging");

                // General protection
                new MMToggle(securityGeneral, "Master Security", new Action<bool>((v) => {
                    // Enable all security features
                    AssignedVariables.udonParamCheck = v;
                    AssignedVariables.udonDictCheck = v;
                    AssignedVariables.dataTypeCheck = v;
                    AssignedVariables.filterViewIds = v;
                    AssignedVariables.udonDataCheck = v;
                }), "Enable comprehensive security protection", true);

                new MMDivider(securityGeneral);

                // Anti-exploitation
                new MMToggle(securityAnti, "UDON Exploit Protection", new Action<bool>((v) => {
                    AssignedVariables.udonParamCheck = v;
                    AssignedVariables.udonDictCheck = v;
                }), "Protect against UDON-based exploits", true);

                new MMToggle(securityAnti, "Photon Security", new Action<bool>((v) => {
                    AssignedVariables.filterViewIds = v;
                }), "Enhanced Photon networking security", true);

                // ===============================
                // SETTINGS PAGE - Configuration
                // ===============================

                MMContainer settingsApp = new MMContainer(settingsPage, "Application");
                MMContainer settingsPerformance = new MMContainer(settingsPage, "Performance");
                MMContainer settingsAdvanced = new MMContainer(settingsPage, "Advanced");

                // Application settings
                var loggingLevelEnum = new MMEnum(
                    settingsApp,
                    "Logging Level",
                    new string[] { "Error", "Warning", "Info", "Debug", "Verbose" },
                    2,
                    (option, index) =>
                    {
                        OdiumConsole.Log("Settings", $"Logging level set to: {option}");
                        // Set logging level here
                    },
                    "Set console logging verbosity level"
                );

                // Performance settings
                var refreshRateSlider = new MMBigSlider(
                    settingsPerformance,
                    "Refresh Rate (seconds)",
                    10f,
                    1f,
                    60f,
                    (value) =>
                    {
                        // Set refresh interval
                        OdiumConsole.Log("Settings", $"Refresh rate set to: {value} seconds");
                    },
                    "How often to refresh nameplate data"
                );

                new MMToggle(settingsPerformance, "Auto Refresh", new Action<bool>((v) => {
                    if (v)
                        NameplateModifier.EnableAutoRefresh();
                    else
                        NameplateModifier.DisableAutoRefresh();
                }), "Automatically refresh nameplate data", true);

                // Advanced settings
                new MMDivider(settingsAdvanced);

                var resetButton = new MMButton(
                    settingsAdvanced,
                    "Reset All Settings",
                    "Reset",
                    () =>
                    {
                        // Reset all settings to default
                        NameplateModifier.ResetStatsPositioning();
                        OdiumConsole.Log("Settings", "All settings reset to defaults");
                    },
                    "Reset all Odium settings to default values",
                    settingsIcon
                );

                var exportButton = new MMButton(
                    settingsAdvanced,
                    "Export Configuration",
                    "Export",
                    () =>
                    {
                        OdiumConsole.Log("Settings", "Configuration exported");
                        // Export settings logic here
                    },
                    "Export current settings to file",
                    odiumIcon
                );
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
