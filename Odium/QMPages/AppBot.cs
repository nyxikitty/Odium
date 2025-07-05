using MelonLoader;
using Odium.ApplicationBot;
using Odium.Components;
using Odium.IUserPage;
using Odium.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Odium.ButtonAPI.QM;
using VRC.Core;

namespace Odium.QMPages
{
    class AppBot
    {
        public static string Current_World_id { get { return RoomManager.prop_ApiWorldInstance_0.id; } }
        public static float xCount = 1;
        public static float yCount = 0;
        public static int botIndex = 0;
        public static List<QMNestedMenu> activeBots = new List<QMNestedMenu>();

        public static string get_selected_player_name()
        {
            var textObject = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_SelectedUser_Local/ScrollRect/Viewport/VerticalLayoutGroup/UserProfile_Compact/PanelBG/Info/Text_Username_NonFriend");
            if (textObject == null) return "";

            var textComponent = textObject.GetComponent<TextMeshProUGUIEx>();
            if (textComponent == null) return "";

            return textComponent.text;
        }
        

        public static void InitializePage(QMNestedMenu appBotsButton, Sprite bgImage, Sprite halfButtonImage)
        {
            
            Sprite TeleportIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\TeleportIcon.png");
            Sprite GoHomeIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\GoHomeIcon.png");
            Sprite JoinMeIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\JoinMeIcon.png");
            Sprite OrbitIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\OrbitIcon.png");
            Sprite CogWheelIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\CogWheelIcon.png");
            Sprite MimicIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\MovementIcon.png");
            Sprite TabImage = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\TabImage.png");

            QMNestedMenu qMNestedMenu1 = new QMNestedMenu(ApiUtils.GetSelectedUserPageGrid().transform, 0, 0, "Odium", "Odium", "Opens Select User menu", true, TabImage, bgImage);
            QMNestedMenu appBotsPage = OdiumPage.Initialize(qMNestedMenu1, bgImage);

            QMSingleButton joinMeButton = new QMSingleButton(appBotsButton, 1, 0, "Join Me", () =>
            {
                ApplicationBot.Entry.ActiveBotIds.ForEach(botId =>
                {
                    SocketConnection.SendCommandToClients($"JoinWorld {Current_World_id} {botId}");
                });
            }, "Make all bots join your instance", false, JoinMeIcon, bgImage);

            QMSingleButton goHomeButton = new QMSingleButton(appBotsButton, 2, 0, "Go Home", () =>
            {
                ApplicationBot.Entry.ActiveBotIds.ForEach(botId =>
                {
                    SocketConnection.SendCommandToClients($"JoinWorld wrld_aeef8228-4e86-4774-9cbb-02027cf73730:91363~region(us) {botId}");
                });
            }, "Send all bots back to their home", false, GoHomeIcon, bgImage);
            
            QMSingleButton tpToMeButton = new QMSingleButton(appBotsButton, 1, 2, "TP To Me", () =>
            {
                ApplicationBot.Entry.ActiveBotIds.ForEach(botId =>
                {
                    SocketConnection.SendCommandToClients($"TeleportToPlayer {PlayerWrapper.LocalPlayer.field_Private_APIUser_0.id} {botId}");
                });
            }, "Teleport all bots to your location", false, TeleportIcon, bgImage);

            QMToggleButton orbitToggle = new QMToggleButton(appBotsButton, 2, 3, "Orbit Me", () =>
            {
                ApplicationBot.Entry.ActiveBotIds.ForEach(botId =>
                {
                    SocketConnection.SendCommandToClients($"OrbitSelected {PlayerWrapper.LocalPlayer.field_Private_APIUser_0.id} {botId}");
                });
            }, () =>
            {
                ApplicationBot.Entry.ActiveBotIds.ForEach(botId =>
                {
                    SocketConnection.SendCommandToClients($"OrbitSelected 0 {botId}");
                });
            }, "Toggle bots orbiting around you", false, bgImage);

            QMToggleButton lagger = new QMToggleButton(appBotsButton, 1, 3, "Chatbox Lagger", () =>
            {
                ApplicationBot.Entry.ActiveBotIds.ForEach(botId =>
                {
                    SocketConnection.SendCommandToClients($"ChatBoxLagger true {botId}");
                });
            }, () =>
            {
                ApplicationBot.Entry.ActiveBotIds.ForEach(botId =>
                {
                    SocketConnection.SendCommandToClients($"OrbitSelected false {botId}");
                });
            }, "Toggle bots orbiting around you", false, bgImage);

            QMNestedMenu qMNestedMenu = new QMNestedMenu(appBotsButton, 4, 3.5f, "Profiles", "Profiles", "Manage bot profiles", true, null, halfButtonImage);
            QMNestedMenu loginMenu = new QMNestedMenu(qMNestedMenu, 4, 3f, "Setup", "Setup", "Manage bot profiles", true, null, halfButtonImage);
            QMNestedMenu launchMenu = new QMNestedMenu(qMNestedMenu, 4, 3.5f, "Launch", "Launch", "Manage bot profiles", true, null, halfButtonImage);

            new QMSingleButton(loginMenu, 1, 0, "Bot 1", () =>
            {
                ApplicationBot.Entry.LaunchBotLogin(20);
            }, "Send all bots back to their home", false, CogWheelIcon, bgImage);

            new QMSingleButton(loginMenu, 2, 0, "Bot 2", () =>
            {
                ApplicationBot.Entry.LaunchBotLogin(21);
            }, "Send all bots back to their home", false, CogWheelIcon, bgImage);

            new QMSingleButton(loginMenu, 3, 0, "Bot 3", () =>
            {
                ApplicationBot.Entry.LaunchBotLogin(22);
            }, "Send all bots back to their home", false, CogWheelIcon, bgImage);

            new QMSingleButton(loginMenu, 4, 0, "Bot 4", () =>
            {
                ApplicationBot.Entry.LaunchBotLogin(23);
            }, "Send all bots back to their home", false, CogWheelIcon, bgImage);

            new QMSingleButton(loginMenu, 1, 1, "Bot 5", () =>
            {
                ApplicationBot.Entry.LaunchBotLogin(24);
            }, "Send all bots back to their home", false, CogWheelIcon, bgImage);

            new QMSingleButton(loginMenu, 2, 1, "Bot 6", () =>
            {
                ApplicationBot.Entry.LaunchBotLogin(25);
            }, "Send all bots back to their home", false, CogWheelIcon, bgImage);

            new QMSingleButton(loginMenu, 3, 1, "Bot 7", () =>
            {
                ApplicationBot.Entry.LaunchBotLogin(26);
            }, "Send all bots back to their home", false, CogWheelIcon, bgImage);

            new QMSingleButton(loginMenu, 4, 1, "Bot 8", () =>
            {
                ApplicationBot.Entry.LaunchBotLogin(27);
            }, "Send all bots back to their home", false, CogWheelIcon, bgImage);

            new QMSingleButton(qMNestedMenu, 1, 3, "Clear Bots", delegate
            {
                activeBots.ForEach(botMenu =>
                {
                    if (botMenu != null)
                    {
                        UnityEngine.Object.Destroy(botMenu.GetMenuObject());
                    }
                });
            }, "Manage bot profiles", false, null, bgImage);

            new QMSingleButton(launchMenu, 1, 0, "Bot 1", () =>
            {
                ApplicationBot.Entry.LaunchBotHeadless(20);
                string bot = ApplicationBot.Entry.ActiveBotIds[0];
                string botId = bot.Split('-')[0];

                QMNestedMenu bot1 = new QMNestedMenu(qMNestedMenu, xCount, yCount, botId, botId, "Manage bot profiles", false, null, bgImage);

                new QMSingleButton(bot1, 2, 1.5f, "TP To Me", () =>
                {
                    SocketConnection.SendCommandToClients($"TeleportToPlayer {PlayerWrapper.LocalPlayer.field_Private_APIUser_0.id} {bot}");
                }, "Teleport all bots to your location", false, TeleportIcon, bgImage);

                new QMToggleButton(bot1, 3, 1.5f, "Chatbox Lagger", () =>
                {
                    SocketConnection.SendCommandToClients($"ChatBoxLagger true {bot}");
                }, () =>
                {
                    SocketConnection.SendCommandToClients($"OrbitSelected false {bot}");
                }, "Toggle bots orbiting around you", false, bgImage);

                QMNestedMenu appBotsPage1 = new QMNestedMenu(appBotsPage, xCount, yCount, botId, botId, "Manage bot profiles", false, null, bgImage);

                new QMSingleButton(appBotsPage1, 1, 0f, "TP To", () =>
                {
                    SocketConnection.SendCommandToClients($"TeleportToPlayer {PlayerWrapper.GetPlayerByDisplayName(get_selected_player_name()).field_Private_APIUser_0.id} {bot}");
                }, "Teleport all bots to your location", false, TeleportIcon, bgImage);

                new QMToggleButton(appBotsPage1, 2, 0, "Orbit",
                () => {
                    SocketConnection.SendCommandToClients($"OrbitSelected {PlayerWrapper.GetPlayerByDisplayName(get_selected_player_name()).field_Private_APIUser_0.id} {bot}");
                },
                () => {
                    SocketConnection.SendCommandToClients($"OrbitSelected 0");
                },
                "Make bots orbit the selected player", false, bgImage);

                new QMToggleButton(appBotsPage1, 3, 0, "Portal Spam",
                    () => {
                        SocketConnection.SendCommandToClients($"PortalSpam {PlayerWrapper.GetPlayerByDisplayName(get_selected_player_name()).field_Private_APIUser_0.id} {bot}");
                    },
                    () => {
                        SocketConnection.SendCommandToClients($"PortalSpam 0");
                    },
                    "Make bots mimic selected player's movement", false, bgImage);

                new QMToggleButton(appBotsPage1, 4, 0, "IK Mimic",
                    () => {
                        SocketConnection.SendCommandToClients($"MovementMimic {PlayerWrapper.GetPlayerByDisplayName(get_selected_player_name()).field_Private_APIUser_0.id} {bot}");
                    },
                    () => {
                        SocketConnection.SendCommandToClients($"MovementMimic 0");
                    },
                    "Make bots mimic selected player's movement", false, bgImage);
                activeBots.Add(bot1);
                xCount += 1;
            }, "Send all bots back to their home", false, CogWheelIcon, bgImage);

            new QMSingleButton(launchMenu, 2, 0, "Bot 2", () =>
            {
                ApplicationBot.Entry.LaunchBotHeadless(21);
                string bot = ApplicationBot.Entry.ActiveBotIds[1];
                string botId = bot.Split('-')[0];

                QMNestedMenu bot1 = new QMNestedMenu(qMNestedMenu, xCount, yCount, botId, botId, "Manage bot profiles", false, null, bgImage);
                new QMSingleButton(bot1, 2, 1.5f, "TP To Me", () =>
                {
                    SocketConnection.SendCommandToClients($"TeleportToPlayer {PlayerWrapper.GetPlayerByDisplayName(get_selected_player_name()).field_Private_APIUser_0.id} {bot}");
                }, "Teleport all bots to your location", false, TeleportIcon, bgImage);

                new QMToggleButton(bot1, 3, 1.5f, "Chatbox Lagger", () =>
                {
                    SocketConnection.SendCommandToClients($"ChatBoxLagger true {bot}");
                }, () =>
                {
                    SocketConnection.SendCommandToClients($"OrbitSelected false {bot}");
                }, "Toggle bots orbiting around you", false, bgImage);

                QMNestedMenu appBotsPage1 = new QMNestedMenu(appBotsPage, xCount, yCount, botId, botId, "Manage bot profiles", false, null, bgImage);

                new QMSingleButton(appBotsPage1, 1, 0f, "TP To", () =>
                {
                    SocketConnection.SendCommandToClients($"TeleportToPlayer {PlayerWrapper.GetPlayerByDisplayName(get_selected_player_name()).field_Private_APIUser_0.id} {bot}");
                }, "Teleport all bots to your location", false, TeleportIcon, bgImage);

                new QMToggleButton(appBotsPage1, 2, 0, "Orbit",
                () => {
                    SocketConnection.SendCommandToClients($"OrbitSelected {PlayerWrapper.GetPlayerByDisplayName(get_selected_player_name()).field_Private_APIUser_0.id} {bot}");
                },
                () => {
                    SocketConnection.SendCommandToClients($"OrbitSelected 0");
                },
                "Make bots orbit the selected player", false, bgImage);

                new QMToggleButton(appBotsPage1, 3, 0, "Portal Spam",
                    () => {
                        SocketConnection.SendCommandToClients($"PortalSpam {PlayerWrapper.GetPlayerByDisplayName(get_selected_player_name()).field_Private_APIUser_0.id} {bot}");
                    },
                    () => {
                        SocketConnection.SendCommandToClients($"PortalSpam 0");
                    },
                    "Make bots mimic selected player's movement", false, bgImage);

                new QMToggleButton(appBotsPage1, 4, 0, "IK Mimic",
                    () => {
                        SocketConnection.SendCommandToClients($"MovementMimic {PlayerWrapper.GetPlayerByDisplayName(get_selected_player_name()).field_Private_APIUser_0.id} {bot}");
                    },
                    () => {
                        SocketConnection.SendCommandToClients($"MovementMimic 0");
                    },
                    "Make bots mimic selected player's movement", false, bgImage);
                activeBots.Add(bot1);
                xCount += 1;
            }, "Send all bots back to their home", false, CogWheelIcon, bgImage);


            new QMSingleButton(launchMenu, 3, 0, "Bot 3", () =>
            {
                ApplicationBot.Entry.LaunchBotHeadless(22);
                string bot = ApplicationBot.Entry.ActiveBotIds[2];
                string botId = bot.Split('-')[0];

                QMNestedMenu bot1 = new QMNestedMenu(qMNestedMenu, xCount, yCount, botId, botId, "Manage bot profiles", false, null, bgImage);

                new QMSingleButton(bot1, 2, 1.5f, "TP To Me", () =>
                {
                    SocketConnection.SendCommandToClients($"TeleportToPlayer {PlayerWrapper.LocalPlayer.field_Private_APIUser_0.id} {bot}");
                }, "Teleport all bots to your location", false, TeleportIcon, bgImage);

                new QMToggleButton(bot1, 3, 1.5f, "Chatbox Lagger", () =>
                {
                    SocketConnection.SendCommandToClients($"ChatBoxLagger true {bot}");
                }, () =>
                {
                    SocketConnection.SendCommandToClients($"OrbitSelected false {bot}");
                }, "Toggle bots orbiting around you", false, bgImage);

                QMNestedMenu appBotsPage1 = new QMNestedMenu(appBotsPage, xCount, yCount, botId, botId, "Manage bot profiles", false, null, bgImage);

                new QMSingleButton(appBotsPage1, 1, 0f, "TP To", () =>
                {
                    SocketConnection.SendCommandToClients($"TeleportToPlayer {PlayerWrapper.GetPlayerByDisplayName(get_selected_player_name()).field_Private_APIUser_0.id} {bot}");
                }, "Teleport all bots to your location", false, TeleportIcon, bgImage);

                new QMToggleButton(appBotsPage1, 2, 0, "Orbit",
                () => {
                    SocketConnection.SendCommandToClients($"OrbitSelected {PlayerWrapper.GetPlayerByDisplayName(get_selected_player_name()).field_Private_APIUser_0.id} {bot}");
                },
                () => {
                    SocketConnection.SendCommandToClients($"OrbitSelected 0");
                },
                "Make bots orbit the selected player", false, bgImage);

                new QMToggleButton(appBotsPage1, 3, 0, "Portal Spam",
                    () => {
                        SocketConnection.SendCommandToClients($"PortalSpam {PlayerWrapper.GetPlayerByDisplayName(get_selected_player_name()).field_Private_APIUser_0.id} {bot}");
                    },
                    () => {
                        SocketConnection.SendCommandToClients($"PortalSpam 0");
                    },
                    "Make bots mimic selected player's movement", false, bgImage);

                new QMToggleButton(appBotsPage1, 4, 0, "IK Mimic",
                    () => {
                        SocketConnection.SendCommandToClients($"MovementMimic {PlayerWrapper.GetPlayerByDisplayName(get_selected_player_name()).field_Private_APIUser_0.id} {bot}");
                    },
                    () => {
                        SocketConnection.SendCommandToClients($"MovementMimic 0");
                    },
                    "Make bots mimic selected player's movement", false, bgImage);
                activeBots.Add(bot1);
                xCount += 1;
            }, "Send all bots back to their home", false, CogWheelIcon, bgImage);


            new QMSingleButton(launchMenu, 4, 0, "Bot 4", () =>
            {
                ApplicationBot.Entry.LaunchBotHeadless(23);
                string bot = ApplicationBot.Entry.ActiveBotIds[3];
                string botId = bot.Split('-')[0];

                QMNestedMenu bot1 = new QMNestedMenu(qMNestedMenu, xCount, yCount, botId, botId, "Manage bot profiles", false, null, bgImage);

                new QMSingleButton(bot1, 2, 1.5f, "TP To Me", () =>
                {
                    SocketConnection.SendCommandToClients($"TeleportToPlayer {PlayerWrapper.LocalPlayer.field_Private_APIUser_0.id} {bot}");
                }, "Teleport all bots to your location", false, TeleportIcon, bgImage);

                new QMToggleButton(bot1, 3, 1.5f, "Chatbox Lagger", () =>
                {
                    SocketConnection.SendCommandToClients($"ChatBoxLagger true {bot}");
                }, () =>
                {
                    SocketConnection.SendCommandToClients($"OrbitSelected false {bot}");
                }, "Toggle bots orbiting around you", false, bgImage);

                QMNestedMenu appBotsPage1 = new QMNestedMenu(appBotsPage, xCount, yCount, botId, botId, "Manage bot profiles", false, null, bgImage);

                new QMSingleButton(appBotsPage1, 1, 0f, "TP To", () =>
                {
                    SocketConnection.SendCommandToClients($"TeleportToPlayer {PlayerWrapper.GetPlayerByDisplayName(get_selected_player_name()).field_Private_APIUser_0.id} {bot}");
                }, "Teleport all bots to your location", false, TeleportIcon, bgImage);

                new QMToggleButton(appBotsPage1, 2, 0, "Orbit",
                () => {
                    SocketConnection.SendCommandToClients($"OrbitSelected {PlayerWrapper.GetPlayerByDisplayName(get_selected_player_name()).field_Private_APIUser_0.id} {bot}");
                },
                () => {
                    SocketConnection.SendCommandToClients($"OrbitSelected 0");
                },
                "Make bots orbit the selected player", false, bgImage);

                new QMToggleButton(appBotsPage1, 3, 0, "Portal Spam",
                    () => {
                        SocketConnection.SendCommandToClients($"PortalSpam {PlayerWrapper.GetPlayerByDisplayName(get_selected_player_name()).field_Private_APIUser_0.id} {bot}");
                    },
                    () => {
                        SocketConnection.SendCommandToClients($"PortalSpam 0");
                    },
                    "Make bots mimic selected player's movement", false, bgImage);

                new QMToggleButton(appBotsPage1, 4, 0, "IK Mimic",
                    () => {
                        SocketConnection.SendCommandToClients($"MovementMimic {PlayerWrapper.GetPlayerByDisplayName(get_selected_player_name()).field_Private_APIUser_0.id} {bot}");
                    },
                    () => {
                        SocketConnection.SendCommandToClients($"MovementMimic 0");
                    },
                    "Make bots mimic selected player's movement", false, bgImage);
                activeBots.Add(bot1);
                xCount += 1;
            }, "Send all bots back to their home", false, CogWheelIcon, bgImage);

            new QMSingleButton(launchMenu, 1, 1, "Bot 5", () =>
            {
                ApplicationBot.Entry.LaunchBotHeadless(24);
            }, "Send all bots back to their home", false, CogWheelIcon, bgImage);

            new QMSingleButton(launchMenu, 2, 1, "Bot 6", () =>
            {
                ApplicationBot.Entry.LaunchBotHeadless(25);
            }, "Send all bots back to their home", false, CogWheelIcon, bgImage);

            new QMSingleButton(launchMenu, 3, 1, "Bot 7", () =>
            {
                ApplicationBot.Entry.LaunchBotHeadless(26);
            }, "Send all bots back to their home", false, CogWheelIcon, bgImage);

            new QMSingleButton(launchMenu, 4, 1, "Bot 8", () =>
            {
                ApplicationBot.Entry.LaunchBotHeadless(27);
            }, "Send all bots back to their home", false, CogWheelIcon, bgImage);
        }
    }
}
