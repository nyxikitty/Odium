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
            new QMToggleButton(stalkPage, 2f, 2f, "Stalk USpeak", () =>
            {
                var targetPlayer = ApiUtils.GetIUser();
                defaultVoiceGain = targetPlayer.field_Private_VRCPlayerApi_0.GetVoiceGain();
                targetPlayer.field_Private_VRCPlayerApi_0.SetVoiceDistanceFar(float.PositiveInfinity);
                PlayerWrapper.Players.ForEach(player =>
                {
                    player.field_Private_VRCPlayerApi_0.SetVoiceGain(0);
                });
            }, delegate
            {
                var targetPlayer = ApiUtils.GetIUser();
                targetPlayer.field_Private_VRCPlayerApi_0.SetVoiceDistanceFar(25f);
                PlayerWrapper.Players.ForEach(player =>
                {
                    player.field_Private_VRCPlayerApi_0.SetVoiceGain(defaultVoiceGain);
                });
            }, "Balls", false, bgImage);

            new QMToggleButton(stalkPage, 3f, 2f, "Stalk Camera", () =>
            {
                var targetPlayer = ApiUtils.GetIUser();
                SpyCamera.Toggle(targetPlayer.field_Private_VRCPlayerApi_0, true);
            }, delegate
            {
                var targetPlayer = ApiUtils.GetIUser();
                SpyCamera.Toggle(targetPlayer.field_Private_VRCPlayerApi_0, false);
            }, "Balls", false, bgImage);

            new QMSingleButton(functionsPage, 1.5f, 2f, "TP Behind", () =>
            {
                var targetPlayer = ApiUtils.GetIUser();
                Vector3 targetPosition = targetPlayer.transform.position;
                Vector3 targetForward = targetPlayer.transform.forward;

                float distanceBehind = 2f;
                Vector3 behindPosition = targetPosition - (targetForward * distanceBehind);

                PlayerWrapper.LocalPlayer.transform.position = behindPosition;

                Vector3 directionToTarget = (targetPosition - behindPosition).normalized;
                PlayerWrapper.LocalPlayer.transform.rotation = Quaternion.LookRotation(directionToTarget);
            }, "Teleport behind selected player facing them", false, TeleportIcon, bgImage);

            new QMToggleButton(functionsPage, 2.5f, 2f, "Portal Spam", () =>
            {
                var targetPlayer = ApiUtils.GetIUser();
                ActionWrapper.portalSpam = true;
                ActionWrapper.portalSpamPlayer = targetPlayer;
            }, delegate
            {
                ActionWrapper.portalSpam = false;
                ActionWrapper.portalSpamPlayer = null;

            }, "Balls", false, bgImage);

            new QMSingleButton(functionsPage, 3.5f, 2f, "TP To", () =>
            {
                var targetPlayer = ApiUtils.GetIUser();
                PlayerWrapper.LocalPlayer.transform.position = targetPlayer.transform.position;
            }, "Teleport behind selected player facing them", false, TeleportIcon, bgImage);

            new QMSingleButton(pickupsPage, 2.5f, 1.5f, "Bring Pickups", () =>
            {
                var targetPlayer = ApiUtils.GetIUser();
                PickupWrapper.BringAllPickupsToPlayer(targetPlayer);
            }, "Teleport behind selected player facing them", false, TeleportIcon, bgImage);

            return appBotsPage;
        }
    }
}
