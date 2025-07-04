using Odium.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using VampClient.Api;

namespace Odium.QMPages
{
    class Entry
    {
        public static List<QMNestedMenu> Initialize(Sprite buttonImage, Sprite halfButtonImage)
        {
            QMTabMenu qMTabMenu = new QMTabMenu("<color=#9101FF>             Odium</color>", "Welcome to <color=#B701FF>Odium</color>", SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\TabImage.png"));
            qMTabMenu.MenuTitleText.alignment = TextAlignmentOptions.Center;

            QMNestedMenu worldUtils = new QMNestedMenu(qMTabMenu, 1, 0, "World", "<color=#9101FF>                  World</color>", "World Utility Functions", false, SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\WorldIcon.png"), buttonImage);
            QMNestedMenu movementButton = new QMNestedMenu(qMTabMenu, 2, 0, "Movement", "<color=#9101FF>               Movement</color>", "Movement Utility Functions", false, SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\MovementIcon.png"), buttonImage);
            QMNestedMenu exploitsButton = new QMNestedMenu(qMTabMenu, 3, 0, "Exploits", "<color=#9101FF>                  Exploits</color>", "World Utility Functions", false, SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\ExploitIcon.png"), buttonImage);
            QMNestedMenu settingsButton = new QMNestedMenu(qMTabMenu, 4, 3.5f, "Settings", "<color=#9101FF>               Settings</color>", "World Utility Functions", true, null, halfButtonImage);
            QMNestedMenu appBotsButton = new QMNestedMenu(qMTabMenu, 1, 3.5f, "App Bots", "<color=#9101FF>               App Bots</color>", "App Bots Utility Functions", true, null, halfButtonImage);
            QMNestedMenu visualsButton = new QMNestedMenu(qMTabMenu, 4, 0, "Visuals", "<color=#9101FF>               Visuals</color>", "Visuals Utility Functions", false, SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\VisualIcon.png"), buttonImage);

            List<QMNestedMenu> menus = new List<QMNestedMenu>
            {
                worldUtils,
                movementButton,
                exploitsButton,
                settingsButton,
                appBotsButton,
                visualsButton
            };

            return menus;
        }
    }
}
