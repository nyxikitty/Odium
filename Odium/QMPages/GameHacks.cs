using MelonLoader;
using Odium.ButtonAPI.QM;
using Odium.Components;
using Odium.GameCheats;
using Odium.Odium;
using Odium.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRC.SDKBase;

namespace Odium.QMPages
{
    class GameHacks
    {
        public static void InitializePage(QMNestedMenu gameHacks, Sprite buttonImage)
        {
            Sprite M4Icon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\Murder.png");
            Sprite KillAllIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\Kill.png");
            Sprite WinIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\Win.png");
            Sprite PeopleIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\People.png");
            Sprite GunIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\Gun.png");
            Sprite WorldIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\WorldIcon.png");
            Sprite FTACIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\FTAC.png");

            QMNestedMenu murder4NestedMenu = new QMNestedMenu(gameHacks, 1, 0, "<color=#8d142b>Murder 4</color>", "<color=#8d142b>Murder 4</color>", "Opens Select User menu", false, M4Icon, buttonImage);
            QMNestedMenu winTriggers = new QMNestedMenu(murder4NestedMenu, 1, 0, "<color=#8d142b>Win Triggers</color>", "<color=#8d142b>Win Triggers</color>", "Opens Select User menu", false, WinIcon, buttonImage);
            QMNestedMenu playerActions = new QMNestedMenu(murder4NestedMenu, 2, 0, "<color=#8d142b>Player Actions</color>", "<color=#8d142b>Player Actions</color>", "Opens Select User menu", false, PeopleIcon, buttonImage);
            QMNestedMenu worldActions = new QMNestedMenu(murder4NestedMenu, 3, 0, "<color=#8d142b>World Actions</color>", "<color=#8d142b>World Actions</color>", "Opens Select User menu", false, WorldIcon, buttonImage);
            QMNestedMenu gunActions = new QMNestedMenu(murder4NestedMenu, 4, 0, "<color=#8d142b>Gun Actions</color>", "<color=#8d142b>Gun Actions</color>", "Opens Select User menu", false, GunIcon, buttonImage);
            QMNestedMenu exploits = new QMNestedMenu(murder4NestedMenu, 2.5f, 1, "<color=#8d142b>Exploits</color>", "<color=#8d142b>Exploits</color>", "Opens Select User menu", false, SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\ExploitIcon.png"), buttonImage);

            new QMSingleButton(exploits, 2.5f, 1.5f, "Crash All", () =>
            {
                Patches.PhotonPatches.BlockUdon = true;
                PlayerManager.prop_PlayerManager_0.field_Private_List_1_Player_0.ToArray().ToList().ForEach(player =>
                {
                    if (player.field_Private_APIUser_0.displayName == Networking.LocalPlayer.displayName) return;
                    if (player.field_Private_APIUser_0.isFriend) return;
                    for (int i = 0; i < 50; i++) Murder4Utils.SendTargetedPatreonUdonEvent(player, "ListPatrons");
                });

                Patches.PhotonPatches.BlockUdon = false;
            }, "Brings death to all players", false, WinIcon, buttonImage);

            new QMSingleButton(winTriggers, 2, 2, "Murder", () =>
            {
                Murder4Utils.MurderWin();
            }, "Brings death to all players", false, WinIcon, buttonImage);

            new QMSingleButton(winTriggers, 3, 2, "Bystanders", () =>
            {
                Murder4Utils.BystandersWin();
            }, "Brings death to all players", false, WinIcon, buttonImage);

            new QMSingleButton(playerActions, 1, 0, "Kill All", () =>
            {
                Murder4Utils.KillAll();
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(playerActions, 2, 0, "Blind All", () =>
            {
                Murder4Utils.BlindAll();
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(playerActions, 3, 0, "Become Murderer", () =>
            {
                Murder4Utils.BeARole(PlayerWrapper.LocalPlayer.field_Private_APIUser_0.displayName, "SyncAssignM");
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(playerActions, 4, 0, "Become Bystander", () =>
            {
                Murder4Utils.BeARole(PlayerWrapper.LocalPlayer.field_Private_APIUser_0.displayName, "SyncAssignB");
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(playerActions, 1, 1, "Become Detective", () =>
            {
                Murder4Utils.BeARole(PlayerWrapper.LocalPlayer.field_Private_APIUser_0.displayName, "SyncAssignD");
            }, "Brings death to all players", false, null, buttonImage);



            new QMSingleButton(worldActions, 1, 0, "Open Doors", () =>
            {
                Murder4Utils.OpenAllDoors();
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(worldActions, 2, 0, "Close Doors", () =>
            {
                Murder4Utils.CloseAllDoors();
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(worldActions, 3, 0, "Unlock Doors", () =>
            {
                Murder4Utils.UnlockAndOpenAllDoors();
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(worldActions, 4, 0, "Lock Doors", () =>
            {
                Murder4Utils.LockAllDoors();
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(worldActions, 1, 1, "Release Snake", () =>
            {
                Murder4Utils.ReleaseSnake();
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(worldActions, 2, 1, "Patreon Revolver", () =>
            {
                Murder4Utils.RevolverPatronSkin();
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(worldActions, 3, 1, "Start Match", () =>
            {
                Murder4Utils.StartMatch();
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(worldActions, 4, 1, "Find Murderer", () =>
            {
                Murder4Utils.FindMurder();
            }, "Brings death to all players", false, null, buttonImage);

            new QMToggleButton(worldActions, 1, 2, "Find Murderer", () =>
            {
                MelonCoroutines.Start(Murder4Utils.KnifeShieldCoroutine(PlayerWrapper.LocalPlayer._vrcplayer));
            }, delegate {
                MelonCoroutines.Stop(Murder4Utils.KnifeShieldCoroutine(PlayerWrapper.LocalPlayer._vrcplayer));
            }, "Brings death to all players", false, buttonImage);

            new QMSingleButton(gunActions, 1, 0, "Fire Revolver", () =>
            {
                Murder4Utils.firerevolver();
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(gunActions, 2, 0, "Fire Shotgun", () =>
            {
                Murder4Utils.fireShotgun();
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(gunActions, 3, 0, "Fire Luger", () =>
            {
                Murder4Utils.fireLuger();
            }, "Brings death to all players", false, null, buttonImage);

            QMNestedMenu FTAC = new QMNestedMenu(gameHacks, 2, 0, "<color=#8d142b>FTAC</color>", "<color=#8d142b>FTAC</color>", "Opens Select User menu", false, FTACIcon, buttonImage);

            new QMSingleButton(FTAC, 1, 0, "Trigger Group Board", () =>
            {
                FTACUdonUtils.SendEvent("OpenGroup");
            }, "Brings death to all players", false, null, buttonImage);
        }
    }
}
