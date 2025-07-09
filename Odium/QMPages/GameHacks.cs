using MelonLoader;
using Odium.ButtonAPI.QM;
using Odium.Components;
using Odium.GameCheats;
using Odium.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

            QMNestedMenu murder4NestedMenu = new QMNestedMenu(gameHacks, 1, 0, "Murder 4", "Murder 4", "Opens Select User menu", false, M4Icon, buttonImage);
            QMNestedMenu winTriggers = new QMNestedMenu(murder4NestedMenu, 1, 0, "Win Triggers", "Win Triggers", "Opens Select User menu", false, WinIcon, buttonImage);
            QMNestedMenu playerActions = new QMNestedMenu(murder4NestedMenu, 2, 0, "Player Actions", "Player Actions", "Opens Select User menu", false, PeopleIcon, buttonImage);
            QMNestedMenu worldActions = new QMNestedMenu(murder4NestedMenu, 3, 0, "World Actions", "World Actions", "Opens Select User menu", false, WorldIcon, buttonImage);
            QMNestedMenu gunActions = new QMNestedMenu(murder4NestedMenu, 4, 0, "Gun Actions", "Gun Actions", "Opens Select User menu", false, GunIcon, buttonImage);

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
        }
    }
}
