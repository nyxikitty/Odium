using Odium.Components;
using Odium.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VampClient.Api;

namespace Odium.IUserPage
{
    class OdiumPage
    {
        public static float defaultVoiceGain = 0f;
        public static QMNestedMenu Initialize(QMNestedMenu qMNestedMenu1, Sprite bgImage)
        {
            
            //Get Targeted user
            var targetPlayer = ApiUtils.GetIUser();
            
            
            Sprite TeleportIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\TeleportIcon.png");
            Sprite GoHomeIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\GoHomeIcon.png");
            Sprite JoinMeIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\JoinMeIcon.png");
            Sprite OrbitIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\OrbitIcon.png");
            Sprite CogWheelIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\CogWheelIcon.png");
            Sprite MimicIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\MovementIcon.png");
            Sprite TabImage = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\TabImage.png");


            QMNestedMenu appBotsPage = new QMNestedMenu(qMNestedMenu1, 1f, 1.5f, "App Bots", "App Bots", "Opens Select User menu", false, null, bgImage);
            QMNestedMenu pickupsPage = new QMNestedMenu(qMNestedMenu1, 2f, 1.5f, "Pickups", "Pickups", "Opens Select User menu", false, null, bgImage);
            QMNestedMenu functionsPage = new QMNestedMenu(qMNestedMenu1, 3f, 1.5f, "Functions", "Functions", "Opens Select User menu", false, null, bgImage);
            QMNestedMenu stalkPage = new QMNestedMenu(qMNestedMenu1, 4f, 1.5f, "Spy Utils", "Spy Utils", "Opens Select User menu", false, null, bgImage);

            // Stalk Audio
            new QMToggleButton(stalkPage, 1f, 2f, "Focus Voice", () =>
            {
                PlayerExtraMethods.focusTargetAudio(targetPlayer,true);
            }, delegate
            {
                PlayerExtraMethods.focusTargetAudio(targetPlayer,false);
            }, "Focus audio on a single user and mutes everyone else", false, bgImage);
            
            
            //SPY AUDIO FUNCTION
            new QMToggleButton(stalkPage, 2f, 2f, "Listen", () =>
            {
                PlayerExtraMethods.setInfiniteVoiceRange(targetPlayer,true);
            }, delegate
            {
                PlayerExtraMethods.setInfiniteVoiceRange(targetPlayer,false);
            }, "Hear people from whatever distance they are", false, bgImage);
            
            // Spy Camera
            new QMToggleButton(stalkPage, 3f, 2f, "POV Camera", () =>
            {
                SpyCamera.Toggle(targetPlayer, true);
            }, delegate
            {
                SpyCamera.Toggle(targetPlayer, false);
            }, "Allows to see from the point of view of other users", false, bgImage);
            
            
            // Teleport behind
            new QMSingleButton(functionsPage, 1.5f, 2f, "TP Behind", () =>
            {
                PlayerExtraMethods.teleportBehind(targetPlayer);
            }, "Teleport behind selected player facing them", false, TeleportIcon, bgImage);

            
            // Portal spam
            new QMToggleButton(functionsPage, 2.5f, 2f, "Portal Spam", () =>
            {
                ActionWrapper.portalSpam = true;
                ActionWrapper.portalSpamPlayer = targetPlayer;
            }, delegate
            {
                ActionWrapper.portalSpam = false;
                ActionWrapper.portalSpamPlayer = null;
            }, "Spams portals on the target, be careful your name is still shown", false, bgImage);

            
            // Teleport to
            new QMSingleButton(functionsPage, 3.5f, 2f, "TP To", () =>
            {
                PlayerExtraMethods.teleportTo(targetPlayer);
            }, "Teleport behind selected player facing them", false, TeleportIcon, bgImage);

            //Bring Pickups
            new QMSingleButton(pickupsPage, 2.5f, 1.5f, "Bring Pickups", () =>
            {
                PickupWrapper.BringAllPickupsToPlayer(targetPlayer);
            }, "Bring all pickups in world to your position", false, TeleportIcon, bgImage);
            
            new QMToggleButton(pickupsPage, 3.5f, 1.5f, "Funny by Awooochy", () =>
            {
                SwasticaOrbit.Activated(targetPlayer,true);
            }, delegate
            {
                SwasticaOrbit.Activated(targetPlayer,false);
            }, "Does some warcrimes", false, bgImage);

            return appBotsPage;
        }
    }
}
