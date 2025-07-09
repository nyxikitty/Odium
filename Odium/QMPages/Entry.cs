using Odium.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Odium.ButtonAPI.QM;

namespace Odium.QMPages
{
    public static class Entry
    {
        public static QMTabMenu tabMenu;

        public static List<QMNestedMenu> Initialize(Sprite buttonImage, Sprite halfButtonImage)
        {
            tabMenu = new QMTabMenu("<color=#9101FF>             Odium</color>", "Welcome to <color=#B701FF>Odium</color>", SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\TabImage.png"));
            tabMenu.MenuTitleText.alignment = TextAlignmentOptions.Center;

            QMNestedMenu worldUtils = new QMNestedMenu(tabMenu, 1, 0, "World", "<color=#9101FF>                  World</color>", "World Utility Functions", false, SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\WorldIcon.png"), buttonImage);
            QMNestedMenu movementButton = new QMNestedMenu(tabMenu, 2, 0, "Movement", "<color=#9101FF>               Movement</color>", "Movement Utility Functions", false, SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\MovementIcon.png"), buttonImage);
            QMNestedMenu exploitsButton = new QMNestedMenu(tabMenu, 3, 0, "Exploits", "<color=#9101FF>                  Exploits</color>", "World Utility Functions", false, SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\ExploitIcon.png"), buttonImage);
            QMNestedMenu settingsButton = new QMNestedMenu(tabMenu, 4, 3.5f, "Settings", "<color=#9101FF>               Settings</color>", "World Utility Functions", true, null, halfButtonImage);
            QMNestedMenu appBotsButton = new QMNestedMenu(tabMenu, 1, 3.5f, "App Bots", "<color=#9101FF>               App Bots</color>", "App Bots Utility Functions", true, null, halfButtonImage);
            QMNestedMenu visualsButton = new QMNestedMenu(tabMenu, 4, 0, "Visuals", "<color=#9101FF>               Visuals</color>", "Visuals Utility Functions", false, SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\VisualIcon.png"), buttonImage);
            QMNestedMenu gameHacks = new QMNestedMenu(tabMenu, 2.5f, 1, "Game Hacks", "<color=#9101FF>                  Game Hacks</color>", "World Utility Functions", false, SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\WorldCheats.png"), buttonImage);

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
