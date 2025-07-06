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
using Odium.Patches;
using OdiumLoader; // Add this using statement


[assembly: MelonInfo(typeof(OdiumEntry), "Odium", "0.0.5", "Zuno")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace Odium
{
    public class OdiumEntry : MelonMod
    {
        public static string Version = "0.0.1";
        public static bool wasKeyValid = false;
        public static HarmonyLib.Harmony HarmonyInstance;
        private float lastStatsUpdate = 0f;
        private const float STATS_UPDATE_INTERVAL = 1f;
        public static int loadIndex = 0;

        public override void OnInitializeMelon()
        {
            OdiumConsole.Initialize();

            OdiumConsole.LogGradient("Odium", "Starting mod initialization...", LogLevel.Info, true);

            ModSetup.Initialize().GetAwaiter();


            BoneESP.SetEnabled(false);
            BoxESP.SetEnabled(false);

            AwooochysPatchInitializer.Start();
            CoroutineManager.Init();

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
            OdiumModuleLoader.OnApplicationStart();
        }

        public static bool heartbeatRun = false;

        internal static IEnumerator OnNetworkManagerInit()
        {
            while (NetworkManager.field_Internal_Static_NetworkManager_0 == null)
                yield return new WaitForSecondsRealtime(2f);

            if (NetworkManager.field_Internal_Static_NetworkManager_0 != null)
            {
                NetworkManager.field_Internal_Static_NetworkManager_0.field_Private_ObjectPublicHa1UnT1Unique_1_IPlayer_1
                    .field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<IPlayer>(obj =>
                    {
                        if (!heartbeatRun)
                        {
                            heartbeatRun = true;
                            System.Threading.Thread thr = new System.Threading.Thread(() =>
                            {
                                while (true)
                                {
                                    var worldId = VRC.Core.APIUser.CurrentUser._location_k__BackingField;
                                    var players = PlayerWrapper.GetAllPlayers();
                                    foreach (var plr in players)
                                    {
                                        using (var client = new System.Net.Http.HttpClient())
                                        {
                                            var userId = plr.prop_APIUser_0._id_k__BackingField;
                                            var jsonObj = new
                                            {
                                                userId,
                                                worldId
                                            };
                                            string json = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj);
                                            var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");

                                            MelonLogger.Msg($"request");
                                            string endpoint = "https://track.niggaf.art/api/v1/user/heartbeat";
                                            var response = client.PostAsync(endpoint, content).Result;

                                            if (!response.IsSuccessStatusCode)
                                            {
                                                MelonLogger.Msg($"Failed to post user leave data: {response.StatusCode}");
                                            }

                                        }
                                    }
                                    System.Threading.Thread.Sleep(3 * 60 * 1000);

                                }
                            });
                            thr.IsBackground = true;
                            thr.Start();

                        }

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

                        if (!player.prop_VRCPlayerApi_0.isLocal)
                        {
                            System.Threading.Thread t = new System.Threading.Thread(() =>
                            {
                                try
                                {
                                    var apiUser = player.field_Private_APIUser_0;
                                    var avatarID = player.prop_ApiAvatar_0._id_k__BackingField;
                                    var bio = apiUser._bio_k__BackingField;
                                    var currentlocation = apiUser._location_k__BackingField;
                                    var dateJoined = apiUser.date_joined;
                                    var displayName = apiUser._displayName_k__BackingField;
                                    var userName = apiUser._username_k__BackingField;
                                    var userId = apiUser._id_k__BackingField;
                                    var platform = apiUser._last_platform;

                                    string type = "user-join";

                                    var jsonObj = new
                                    {
                                        type,
                                        avatarID,
                                        bio,
                                        currentlocation,
                                        dateJoined,
                                        displayName,
                                        userName,
                                        userId,
                                        platform
                                    };

                                    using (var client = new System.Net.Http.HttpClient())
                                    {
                                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj);
                                        var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");

                                        string endpoint = "https://track.niggaf.art/api/v1/user/join";
                                        var response = client.PostAsync(endpoint, content).Result;

                                        if (!response.IsSuccessStatusCode)
                                        {
                                            MelonLogger.Msg($"Failed to post user join data: {response.StatusCode}");
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MelonLogger.Msg($"Error posting user join data: {ex}");
                                }
                            });
                            t.IsBackground = true;
                            t.Start();
                        }
                    }));

                NetworkManager.field_Internal_Static_NetworkManager_0.field_Private_ObjectPublicHa1UnT1Unique_1_IPlayer_2
                    .field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<IPlayer>(obj =>
                    {
                        OdiumConsole.LogGradient("PlayerLeave", obj.prop_IUser_0.prop_String_1);

                        if (AssignedVariables.desktopPlayerList)
                        {
                            PlayerRankTextDisplay.RemovePlayer(obj.prop_IUser_0.prop_String_1);
                        }

                        var displayName = obj.prop_IUser_0.prop_String_1;
                        if (VRC.Core.APIUser.CurrentUser.displayName != displayName)
                        {
                            System.Threading.Thread t = new System.Threading.Thread(() =>
                            {
                                try
                                {

                                    var currentlocation = VRC.Core.APIUser.CurrentUser._location_k__BackingField;

                                    string type = "user-leave";

                                    System.Threading.Thread.Sleep(1000);
                                    var players = PlayerWrapper.GetAllPlayers();
                                    if (players.Length == 0)
                                    {
                                        type = "world-leave";
                                    }

                                    var jsonObj = new
                                    {
                                        type,
                                        currentlocation,
                                        displayName
                                    };
                                    MelonLogger.Msg($"{jsonObj}");
                                    using (var client = new System.Net.Http.HttpClient())
                                    {
                                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj);
                                        var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");

                                        MelonLogger.Msg($"request");
                                        string endpoint = "https://track.niggaf.art/api/v1/user/leave";
                                        var response = client.PostAsync(endpoint, content).Result;

                                        if (!response.IsSuccessStatusCode)
                                        {
                                            MelonLogger.Msg($"Failed to post user leave data: {response.StatusCode}");
                                        }

                                    }
                                }
                                catch (Exception ex)
                                {
                                    MelonLogger.Msg($"Error posting user leave data: {ex}");
                                }
                            });

                            t.IsBackground = true;
                            t.Start();
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

        public override void OnSceneWasLoaded(int buildindex, string sceneName)
        {
            // Call module loader for scene events
            OdiumModuleLoader.OnSceneWasLoaded(buildindex, sceneName);

            OnLoadedSceneManager.LoadedScene(buildindex, sceneName);
        }

        public override void OnSceneWasUnloaded(int buildindex, string sceneName)
        {
            // Call module loader for scene events
            OdiumModuleLoader.OnSceneWasUnloaded(buildindex, sceneName);
        }

        public override void OnApplicationQuit()
        {
            // Call module loader cleanup
            OdiumModuleLoader.OnApplicationQuit();
        }

        public override void OnGUI()
        {
            BoneESP.OnGUI();
            BoxESP.OnGUI();
        }

        public override void OnUpdate()
        {
            // Call module loader update FIRST
            OdiumModuleLoader.OnUpdate();

            InternalConsole.ProcessLogCache();
            MainMenu.Setup();

            DroneSwarmWrapper.UpdateDroneSwarm();
            portalSpam.OnUpdate();
            portalTrap.OnUpdate();
            ApplicationBot.Bot.OnUpdate();
            BoneESP.Update();
            BoxESP.Update();
            SwasticaOrbit.OnUpdate();
            Jetpack.Update();
            FlyComponent.OnUpdate();

            if (Time.time - lastStatsUpdate >= STATS_UPDATE_INTERVAL)
            {
                NameplateModifier.UpdatePlayerStats();
                lastStatsUpdate = Time.time;
            }

            AdBlock.OnUpdate();
        }

        public override void OnFixedUpdate()
        {
            // Call module loader fixed update
            OdiumModuleLoader.OnFixedUpdate();
        }

        public override void OnLateUpdate()
        {
            // Call module loader late update
            OdiumModuleLoader.OnLateUpdate();

            SpyCamera.LateUpdate();
        }
    }
}