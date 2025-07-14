using Odium.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Odium.ButtonAPI.QM;
using Odium.Odium;

namespace Odium.QMPages
{
    public static class Entry
    {
        public static QMTabMenu tabMenu;

        public static List<QMNestedMenu> Initialize(Sprite buttonImage, Sprite halfButtonImage)
        {
            tabMenu = new QMTabMenu("<color=#8d142b>Odium</color>", "Welcome to <color=#8d142b>Odium</color>", SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\OdiumIcon.png"));
            tabMenu.MenuTitleText.alignment = TextAlignmentOptions.Center;

            QMNestedMenu worldUtils = new QMNestedMenu(tabMenu, 1, 0, "<color=#8d142b>World</color>", "<color=#8d142b>World</color>", "World Utility Functions", false, SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\WorldIcon.png"), buttonImage);
            QMNestedMenu movementButton = new QMNestedMenu(tabMenu, 2, 0, "<color=#8d142b>Movement</color>", "<color=#8d142b>Movement</color>", "Movement Utility Functions", false, SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\MovementIcon.png"), buttonImage);
            QMNestedMenu exploitsButton = new QMNestedMenu(tabMenu, 3, 0, "<color=#8d142b>Exploits</color>", "<color=#8d142b>Exploits</color>", "World Utility Functions", false, SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\ExploitIcon.png"), buttonImage);
            QMNestedMenu settingsButton = new QMNestedMenu(tabMenu, 4, 3.5f, "<color=#8d142b>Settings</color>", "<color=#8d142b>Settings</color>", "World Utility Functions", true, null, halfButtonImage);
            QMNestedMenu appBotsButton = new QMNestedMenu(tabMenu, 1, 3.5f, "<color=#8d142b>App Bots</color>", "<color=#8d142b>App Bots</color>", "App Bots Utility Functions", true, null, halfButtonImage);
            QMNestedMenu visualsButton = new QMNestedMenu(tabMenu, 4, 0, "<color=#8d142b>Visuals</color>", "<color=#8d142b>Visuals</color>", "Visuals Utility Functions", false, SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\VisualIcon.png"), buttonImage);
            QMNestedMenu gameHacks = new QMNestedMenu(tabMenu, 2.5f, 1, "<color=#8d142b>Game Hacks</color>", "<color=#8d142b>Game Hacks</color>", "World Utility Functions", false, SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\WorldCheats.png"), buttonImage);

            List<QMNestedMenu> menus = new List<QMNestedMenu>
            {
                worldUtils,
                movementButton,
                exploitsButton,
                settingsButton,
                appBotsButton,
                visualsButton,
                gameHacks
            };

            return menus;
        }
    }
}
