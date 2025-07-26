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
        public static QMMenuPage tabMenu;

        public static List<QMNestedMenu> Initialize(Sprite buttonImage, Sprite halfButtonImage)
        {
            tabMenu = new QMMenuPage("Odium", "Welcome to Odium", SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\OdiumIcon.png"));
            tabMenu.MenuTitleText.alignment = TextAlignmentOptions.Center;

            GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)/CanvasGroup/Container/ThankYouCharacter/Character/VRCat_Front").SetActive(false);

            QMNestedMenu worldUtils = new QMNestedMenu(tabMenu, 1, 0, "World", "World", "World Utility Functions", false, SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\WorldIcon.png"), buttonImage);
            QMNestedMenu movementButton = new QMNestedMenu(tabMenu, 2, 0, "Movement", "Movement", "Movement Utility Functions", false, SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\MovementIcon.png"), buttonImage);
            QMNestedMenu exploitsButton = new QMNestedMenu(tabMenu, 3, 0, "Exploits", "Exploits", "World Utility Functions", false, SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\ExploitIcon.png"), buttonImage);
            QMNestedMenu settingsButton = new QMNestedMenu(tabMenu, 4, 3.5f, "Settings", "Settings", "World Utility Functions", true, null, halfButtonImage);
            QMNestedMenu appBotsButton = new QMNestedMenu(tabMenu, 1, 3.5f, "App Bots", "App Bots", "App Bots Utility Functions", true, null, halfButtonImage);
            QMNestedMenu visualsButton = new QMNestedMenu(tabMenu, 4, 0, "Visuals", "Visuals", "Visuals Utility Functions", false, SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\VisualIcon.png"), buttonImage);
            QMNestedMenu gameHacks = new QMNestedMenu(tabMenu, 1.5f, 1, "Game Hacks", "Game Hacks", "World Utility Functions", false, SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\WorldCheats.png"), buttonImage);
            QMNestedMenu protectionsButton = new QMNestedMenu(tabMenu, 2.5f, 1, "Protections", "Protections", "World Utility Functions", false, SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\ShieldIcon.png"), buttonImage);
            QMNestedMenu conduitButton = new QMNestedMenu(tabMenu, 3.5f, 1, "Conduit", "Conduit", "World Utility Functions", false, SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\WorldIcon.png"), buttonImage);

            List<QMNestedMenu> menus = new List<QMNestedMenu>
            {
                worldUtils,
                movementButton,
                exploitsButton,
                settingsButton,
                appBotsButton,
                visualsButton,
                gameHacks,
                protectionsButton,
                conduitButton
            };

            return menus;
        }
    }
}
