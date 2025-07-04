using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using MelonLoader;
using UnityEngine;
using Odium;
using Odium.Components;
using Odium.Modules;
using Odium.Odium;
using System.Collections;
using System.Linq;
using HarmonyLib;
using Odium.Wrappers;
using VRC.SDKBase;
using Odium.UX;
using Harmony;
using Odium.UI;
using Odium.ApplicationBot;

[assembly: MelonInfo(typeof(OdiumEntry), "Odium", "0.0.5", "Zuno")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace Odium
{
    public class OdiumEntry : MelonMod
    {
        public static HarmonyLib.Harmony HarmonyInstance;
        private float lastStatsUpdate = 0f;
        private const float STATS_UPDATE_INTERVAL = 1f;
        public static int loadIndex = 0;
        public override void OnInitializeMelon()
        {
            OdiumConsole.Initialize();

            OdiumConsole.LogGradient("Odium", "Starting mod initialization...", LogLevel.Info, true);

            ModSetup.Initialize().GetAwaiter();
            
            OdiumConsole.LogGradient("Odium", "Starting HTTP server", LogLevel.Info, true);
            
            ExternalMenu EXM = new ExternalMenu();
            EXM.StartServer();
            
            OdiumConsole.LogGradient("Odium", "External Menu Ready", LogLevel.Info, true);
            
            //On start set those gotta be off, so user enables em.
            //WE WILL REMOVE THIS SHIT WHEN WE MAKE A CONFIG FILE!!!!
            BoneESP.SetEnabled(false);
            BoxESP.SetEnabled(false);

            try
            {
                OdiumConsole.LogGradient("System", "Initialization complete!", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("Error", $"Failed to initialize: {ex.Message}", LogLevel.Error);
            }
        }

        public override void OnApplicationStart()
        {
            OdiumConsole.LogGradient("Odium", "Trying manual patch approach...", LogLevel.Info);

            MelonCoroutines.Start(QM.WaitForUI());
            MelonCoroutines.Start(OnNetworkManagerInit());
            QM.SetupMenu();
            PlayerRankTextDisplay.Initialize();
            PlayerRankTextDisplay.SetVisible(true);
            BoneESP.Initialize();
            BoneESP.SetEnabled(true);
            BoneESP.SetBoneColor(new UnityEngine.Color(0.584f, 0.008f, 0.996f, 1.0f));
            BoxESP.Initialize();
            BoxESP.SetEnabled(true);
            BoxESP.SetBoxColor(new UnityEngine.Color(0.584f, 0.008f, 0.996f, 1.0f));
            MainThreadDispatcher.Initialize();
        }

        public override void OnApplicationLateStart()
        {
            ApplicationBot.Bot.Start();
        }

        internal static IEnumerator OnNetworkManagerInit()
        {
            while (NetworkManager.field_Internal_Static_NetworkManager_0 == null)
                yield return new WaitForSecondsRealtime(2f);

            if (NetworkManager.field_Internal_Static_NetworkManager_0 != null)
            {
                NetworkManager.field_Internal_Static_NetworkManager_0.field_Private_ObjectPublicHa1UnT1Unique_1_IPlayer_1
                    .field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<IPlayer>(obj =>
                    {
                        OdiumConsole.LogGradient("PlayerJoin", obj.prop_IUser_0.prop_String_1);
                        Sprite nameplate = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\Nameplate.png");
                        NameplateModifier.ModifyPlayerNameplate(PlayerWrapper.GetVRCPlayerFromId(obj.prop_IUser_0.prop_String_0)._player, nameplate);

                        if (AssignedVariables.desktopPlayerList)
                        {
                            PlayerRankTextDisplay.AddPlayer(obj.prop_IUser_0.prop_String_1, PlayerWrapper.GetVRCPlayerFromId(obj.prop_IUser_0.prop_String_0)._player.field_Private_APIUser_0, PlayerWrapper.GetVRCPlayerFromId(obj.prop_IUser_0.prop_String_0)._player);
                        }
                        // Get the vrc player api
                        VRCPlayerApi vrcPlayerApi = PlayerWrapper.GetLocalPlayerAPIUser(obj.prop_IUser_0.prop_String_0);
                        VRC.Player player = PlayerWrapper.GetVRCPlayerFromId(obj.prop_IUser_0.prop_String_0)._player;

                        if (Networking.LocalPlayer.displayName == vrcPlayerApi.displayName)
                        {
                            PlayerWrapper.LocalPlayer = PlayerWrapper.GetVRCPlayerFromId(obj.prop_IUser_0.prop_String_0)._player;
                            IiIIiIIIiIIIIiIIIIiIiIiIIiIIIIiIiIIiiIiIiIIIiiIIiI.IiIIiIIIIIIIIIIIIiiiiiiIIIIiIIiIiIIIiiIiiIiIiIiiIiIiIiIIiIiIIIiiiIIIIIiIIiIiIiIiiIIIiiIiiiiiiiiIiiIIIiIiiiiIIIIIiII(obj.prop_IUser_0.prop_String_0, obj.prop_IUser_0.prop_String_1);
                        }

                        InternalConsole.LogIntoConsole($"{player.field_Private_APIUser_0.displayName} joined!");

                        PlayerWrapper.Players.Add(player);
                        BoneESP.OnPlayerJoined(player);
                        BoxESP.OnPlayerJoined(player);
                    }));

                NetworkManager.field_Internal_Static_NetworkManager_0.field_Private_ObjectPublicHa1UnT1Unique_1_IPlayer_0
                    .field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<IPlayer>(obj =>
                    {
                        OdiumConsole.LogGradient("PlayerJoin", obj.prop_IUser_0.prop_String_1);
                        Sprite nameplate = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\Nameplate.png");
                        NameplateModifier.ModifyPlayerNameplate(PlayerWrapper.GetVRCPlayerFromId(obj.prop_IUser_0.prop_String_0)._player, nameplate);

                        if (AssignedVariables.desktopPlayerList)
                        {
                            PlayerRankTextDisplay.AddPlayer(obj.prop_IUser_0.prop_String_1, PlayerWrapper.GetVRCPlayerFromId(obj.prop_IUser_0.prop_String_0)._player.field_Private_APIUser_0, PlayerWrapper.GetVRCPlayerFromId(obj.prop_IUser_0.prop_String_0)._player);
                        }

                        // Get the vrc player api
                        VRCPlayerApi vrcPlayerApi = PlayerWrapper.GetLocalPlayerAPIUser(obj.prop_IUser_0.prop_String_0);
                        VRC.Player player = PlayerWrapper.GetVRCPlayerFromId(obj.prop_IUser_0.prop_String_0)._player;

                        if (Networking.LocalPlayer.displayName == vrcPlayerApi.displayName)
                        {
                            PlayerWrapper.LocalPlayer = PlayerWrapper.GetVRCPlayerFromId(obj.prop_IUser_0.prop_String_0)._player;
                            IiIIiIIIiIIIIiIIIIiIiIiIIiIIIIiIiIIiiIiIiIIIiiIIiI.IiIIiIIIIIIIIIIIIiiiiiiIIIIiIIiIiIIIiiIiiIiIiIiiIiIiIiIIiIiIIIiiiIIIIIiIIiIiIiIiiIIIiiIiiiiiiiiIiiIIIiIiiiiIIIIIiII(obj.prop_IUser_0.prop_String_0, obj.prop_IUser_0.prop_String_1);
                        }

                        InternalConsole.LogIntoConsole($"{player.field_Private_APIUser_0.displayName} joined!");

                        PlayerWrapper.Players.Add(player);
                        BoneESP.OnPlayerJoined(player);
                        BoxESP.OnPlayerJoined(player);
                    }));

                NetworkManager.field_Internal_Static_NetworkManager_0.field_Private_ObjectPublicHa1UnT1Unique_1_IPlayer_2
                    .field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<IPlayer>(obj =>
                    {
                        OdiumConsole.LogGradient("PlayerLeave", obj.prop_IUser_0.prop_String_1);

                        if (AssignedVariables.desktopPlayerList)
                        {
                            PlayerRankTextDisplay.RemovePlayer(obj.prop_IUser_0.prop_String_1);
                        }

                        VRC.Player player = PlayerWrapper.GetVRCPlayerFromId(obj.prop_IUser_0.prop_String_0)._player;
                        InternalConsole.LogIntoConsole($"{player.field_Private_APIUser_0.displayName} joined!");
                        PlayerWrapper.Players.Remove(player);
                        BoneESP.OnPlayerLeft(player);
                        BoxESP.OnPlayerLeft(player);
                    }));
            }
        }

        public override void OnLevelWasLoaded(int level)
        {
            if (level == -1)
            {
                OdiumAssetBundleLoader._customAudioClip = null;
                OdiumAssetBundleLoader._customAudioSource = null;
                OdiumAssetBundleLoader.StopCustomAudio();
            }

            OdiumAssetBundleLoader.Initialize();
            PlayerRankTextDisplay.ClearAll();

            OdiumConsole.LogGradient("OnLevelWasLoaded", $"Level -> {level}");

            loadIndex += 1;
        }

        public override void OnGUI()
        {
            BoneESP.OnGUI();
            BoxESP.OnGUI();
        }

        public override void OnUpdate()
        {
            InternalConsole.ProcessLogCache();
            MainMenu.Setup();

            DroneSwarmWrapper.UpdateDroneSwarm();
            portalSpam.OnUpdate();
            portalTrap.OnUpdate();
            FlyComponent.OnUpdate();
            ApplicationBot.Bot.OnUpdate();
            BoneESP.Update();
            BoxESP.Update();
            if (Time.time - lastStatsUpdate >= STATS_UPDATE_INTERVAL)
            {
                NameplateModifier.UpdatePlayerStats();
                lastStatsUpdate = Time.time;
            }

            AdBlock.OnUpdate();
        }
    }
}