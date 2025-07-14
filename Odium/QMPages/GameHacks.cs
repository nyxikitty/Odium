using MelonLoader;
using Odium.ButtonAPI.QM;
using Odium.Components;
using Odium.GameCheats;
using Odium.Odium;
using Odium.Wrappers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRC;
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
                try
                {
                    Patches.PhotonPatches.BlockUdon = true;
                    var localPlayerName = Networking.LocalPlayer.displayName;
                    var targets = PlayerManager.prop_PlayerManager_0.field_Private_List_1_Player_0.ToArray()
                        .Where(player =>
                            player != null &&
                            player.field_Private_APIUser_0 != null &&
                            player.field_Private_APIUser_0.displayName != localPlayerName &&
                            !player.field_Private_APIUser_0.isFriend)
                        .ToList();

                    MelonCoroutines.Start(CrashPlayersWithDelay(targets));
                }
                catch (Exception ex)
                {
                    Patches.PhotonPatches.BlockUdon = false;
                }
            }, "Brings death to all players", false, KillAllIcon, buttonImage);

        new QMSingleButton(winTriggers, 2, 2, "Murder", () =>
            {
                Murder4Utils.TriggerMurdererWin();
            }, "Brings death to all players", false, WinIcon, buttonImage);

            new QMSingleButton(winTriggers, 3, 2, "Bystanders", () =>
            {
                Murder4Utils.TriggerBystanderWin();
            }, "Brings death to all players", false, WinIcon, buttonImage);

            new QMSingleButton(playerActions, 1, 0, "Kill All", () =>
            {
                Murder4Utils.ExecuteAll();
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(playerActions, 2, 0, "Blind All", () =>
            {
                Murder4Utils.BlindAll();
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(playerActions, 3, 0, "Become Murderer", () =>
            {
                Murder4Utils.AssignRole(PlayerWrapper.LocalPlayer.field_Private_APIUser_0.displayName, "SyncAssignM");
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(playerActions, 4, 0, "Become Bystander", () =>
            {
                Murder4Utils.AssignRole(PlayerWrapper.LocalPlayer.field_Private_APIUser_0.displayName, "SyncAssignB");
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(playerActions, 1, 1, "Become Detective", () =>
            {
                Murder4Utils.AssignRole(PlayerWrapper.LocalPlayer.field_Private_APIUser_0.displayName, "SyncAssignD");
            }, "Brings death to all players", false, null, buttonImage);



            new QMSingleButton(worldActions, 1, 0, "Open Doors", () =>
            {
                Murder4Utils.OpenDoors();
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(worldActions, 2, 0, "Close Doors", () =>
            {
                Murder4Utils.CloseDoors();
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(worldActions, 3, 0, "Unlock Doors", () =>
            {
                Murder4Utils.ForceOpenDoors();
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(worldActions, 4, 0, "Lock Doors", () =>
            {
                Murder4Utils.LockDoors();
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(worldActions, 1, 1, "Release Snake", () =>
            {
                Murder4Utils.SpawnSnake();
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(worldActions, 2, 1, "Patreon Revolver", () =>
            {
                Murder4Utils.ApplyRevolverSkin();
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(worldActions, 3, 1, "Start Match", () =>
            {
                Murder4Utils.StartGame();
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(worldActions, 4, 1, "Find Murderer", () =>
            {
                Murder4Utils.IdentifyMurderer();
            }, "Brings death to all players", false, null, buttonImage);

            new QMToggleButton(worldActions, 1, 2, "Find Murderer", () =>
            {
                MelonCoroutines.Start(Murder4Utils.CreateKnifeShield(PlayerWrapper.LocalPlayer._vrcplayer));
            }, delegate {
                MelonCoroutines.Stop(Murder4Utils.CreateKnifeShield(PlayerWrapper.LocalPlayer._vrcplayer));
            }, "Brings death to all players", false, buttonImage);

            new QMSingleButton(gunActions, 1, 0, "Fire Revolver", () =>
            {
                Murder4Utils.FireRevolver();
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(gunActions, 2, 0, "Fire Shotgun", () =>
            {
                Murder4Utils.FireShotgun();
            }, "Brings death to all players", false, null, buttonImage);

            new QMSingleButton(gunActions, 3, 0, "Fire Luger", () =>
            {
                Murder4Utils.FireLuger();
            }, "Brings death to all players", false, null, buttonImage);

            QMNestedMenu FTAC = new QMNestedMenu(gameHacks, 2, 0, "<color=#8d142b>FTAC</color>", "<color=#8d142b>FTAC</color>", "Opens Select User menu", false, FTACIcon, buttonImage);

            new QMSingleButton(FTAC, 1, 0, "Trigger Group Board", () =>
            {
                FTACUdonUtils.SendEvent("OpenGroup");
            }, "Brings death to all players", false, null, buttonImage);
        }

        public static IEnumerator CrashPlayersWithDelay(List<Player> targets)
        {
            try
            {
                foreach (var player in targets)
                {
                    if (player == null) continue;

                    for (int i = 0; i < 150; i++)
                    {
                        Murder4Utils.SendTargetedPatreonEvent(player, "ListPatrons");
                        yield return new WaitForSeconds(0.4f);
                    }
                }
            }
            finally
            {
                Patches.PhotonPatches.BlockUdon = false;
            }
        }
    }
}
