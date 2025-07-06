using Odium.Components;
using Odium.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UnityEngine;
using Odium.ButtonAPI.QM;

namespace Odium.IUserPage
{
    class OdiumPage
    {
        public static float defaultVoiceGain = 0f;
        public static QMNestedMenu Initialize(QMNestedMenu qMNestedMenu1, Sprite bgImage)
        {
            
            //Get Targeted user            
            
            Sprite TeleportIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\TeleportIcon.png");
            Sprite GoHomeIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\GoHomeIcon.png");
            Sprite JoinMeIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\JoinMeIcon.png");
            Sprite OrbitIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\OrbitIcon.png");
            Sprite CogWheelIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\CogWheelIcon.png");
            Sprite MimicIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\MovementIcon.png");
            Sprite TabImage = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\TabImage.png");
            Sprite InfoIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\InfoIcon.png");

            QMNestedMenu appBotsPage = new QMNestedMenu(qMNestedMenu1, 1f, 1.5f, "App Bots", "App Bots", "Opens Select User menu", false, null, bgImage);
            QMNestedMenu pickupsPage = new QMNestedMenu(qMNestedMenu1, 2f, 1.5f, "Pickups", "Pickups", "Opens Select User menu", false, null, bgImage);
            QMNestedMenu functionsPage = new QMNestedMenu(qMNestedMenu1, 3f, 1.5f, "Functions", "Functions", "Opens Select User menu", false, null, bgImage);
            QMNestedMenu stalkPage = new QMNestedMenu(qMNestedMenu1, 4f, 1.5f, "Spy Utils", "Spy Utils", "Opens Select User menu", false, null, bgImage);

            // Stalk Audio
            new QMToggleButton(stalkPage, 1.5f, 2f, "Spy USpeak", () =>
            {
                var targetPlayer = ApiUtils.GetIUser();
                PlayerExtraMethods.focusTargetAudio(targetPlayer, true);
            }, delegate
            {
                var targetPlayer = ApiUtils.GetIUser();
                PlayerExtraMethods.focusTargetAudio(targetPlayer, false);
            }, "Focus audio on a single user and mutes everyone else", false, bgImage);
            
            
            //SPY AUDIO FUNCTION
            new QMToggleButton(stalkPage, 2.5f, 2f, "Max Voice Range", () =>
            {
                var targetPlayer = ApiUtils.GetIUser();
                PlayerExtraMethods.setInfiniteVoiceRange(targetPlayer, true);
            }, delegate
            {
                var targetPlayer = ApiUtils.GetIUser();
                PlayerExtraMethods.setInfiniteVoiceRange(targetPlayer, false);
            }, "Hear people from whatever distance they are", false, bgImage);
            
            // Spy Camera
            new QMToggleButton(stalkPage, 3.5f, 2f, "Spy Camera", () =>
            {
                var targetPlayer = ApiUtils.GetIUser();
                SpyCamera.Toggle(targetPlayer.field_Private_VRCPlayerApi_0, true);
            }, delegate
            {
                var targetPlayer = ApiUtils.GetIUser();
                SpyCamera.Toggle(targetPlayer.field_Private_VRCPlayerApi_0, false);
            }, "Allows to see from the point of view of other users", false, bgImage);

            new QMSingleButton(functionsPage, 1.5f, 1f, "TP Behind", () =>
            {
                var targetPlayer = ApiUtils.GetIUser();
                PlayerExtraMethods.teleportBehind(targetPlayer);
            }, "Teleport behind selected player facing them", false, TeleportIcon, bgImage);

            
            // Portal spam
            new QMToggleButton(functionsPage, 2.5f, 1f, "Portal Spam", () =>
            {
                var targetPlayer = ApiUtils.GetIUser();
                ActionWrapper.portalSpam = true;
                ActionWrapper.portalSpamPlayer = targetPlayer;
            }, delegate
            {
                ActionWrapper.portalSpam = false;
                ActionWrapper.portalSpamPlayer = null;
            }, "Spams portals on the target, be careful your name is still shown", false, bgImage);

            new QMSingleButton(functionsPage, 3.5f, 1f, "TP To", () =>
            {
                var targetPlayer = ApiUtils.GetIUser();
                PlayerExtraMethods.teleportTo(targetPlayer);
            }, "Teleport behind selected player facing them", false, TeleportIcon, bgImage);

            new QMSingleButton(functionsPage, 1.5f, 2f, "Copy ID", () =>
            {
                var targetPlayer = ApiUtils.GetIUser();
                Clipboard.SetText(targetPlayer.field_Private_APIUser_0.id.ToString());
            }, "Copy the id of the avatar the selected user is wearing", false, InfoIcon, bgImage);

            new QMSingleButton(functionsPage, 2.5f, 2f, "Copy Avatar ID", () =>
            {
                var targetPlayer = ApiUtils.GetIUser();
                Clipboard.SetText(targetPlayer.prop_ApiAvatar_0.id.ToString());
            }, "Copies to clipboard the selected user name", false, InfoIcon, bgImage);

            new QMSingleButton(functionsPage, 3.5f, 2f, "Copy Display Name", () =>
            {
                var targetPlayer = ApiUtils.GetIUser();
                Clipboard.SetText(targetPlayer.prop_APIUser_0.displayName.ToString());
            }, "Teleport behind selected player facing them", false, InfoIcon, bgImage);

            //Bring Pickups
            new QMSingleButton(pickupsPage, 1.5f, 1.5f, "Bring Pickups", () =>
            {
                var targetPlayer = ApiUtils.GetIUser();
                PickupWrapper.BringAllPickupsToPlayer(targetPlayer);
            }, "Bring all pickups in world to your position", false, TeleportIcon, bgImage);
            
            new QMToggleButton(pickupsPage, 2.5f, 1.5f, "Pickup Swastika", () =>
            {
                var targetPlayer = ApiUtils.GetIUser();
                SwasticaOrbit.Activated(targetPlayer,true);
            }, delegate
            {
                var targetPlayer = ApiUtils.GetIUser();
                SwasticaOrbit.Activated(targetPlayer,false);
            }, "Creates a Swastika with pickups and places it on top of the selected user.", false, bgImage);

            new QMToggleButton(pickupsPage, 3.5f, 1.5f, "Drone Swarm", () =>
            {
                var targetPlayer = ApiUtils.GetIUser();
                DroneSwarmWrapper.isSwarmActive = true;
                DroneSwarmWrapper.ChangeSwarmTarget(targetPlayer.gameObject);
            }, delegate
            {
                DroneSwarmWrapper.isSwarmActive = false;
            }, "Swarms your player with every available drone in the instance", false, bgImage);

            return appBotsPage;
        }
    }
}
