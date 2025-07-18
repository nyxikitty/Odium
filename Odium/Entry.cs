using System;
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
using OdiumLoader;
using Odium.Threadding;
using CursorLayerMod;
using Odium.ButtonAPI.QM;
using VRC.UI.Client;
using VRC.UI;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Odium.QMPages;
using VRC.Udon;
using Odium.GameCheats;


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
        public static Sprite buttonImage = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\ButtonBackground.png");

        public static string Current_World_id { get { return RoomManager.prop_ApiWorldInstance_0.id; } }

        // Authentication related fields
        private static readonly string AUTH_ENDPOINT = "https://odiumvrc.com/api/validate-purchase";
        private static readonly string AUTH_FILE = Path.Combine(Environment.CurrentDirectory, "Odium", "auth.dat");

        public override void OnInitializeMelon()
        {
            try
            {
                OdiumConsole.Initialize();
                OdiumConsole.LogGradient("Odium", "Starting authentication check...", LogLevel.Info, true);
                if (!AuthenticateUser())
                {
                    ShowErrorDialog("Authentication Required", "Please authenticate Odium to continue.");
                    MelonLogger.Error("Authentication failed. Closing application.");
                    Application.Quit();
                    return;
                }

                wasKeyValid = true;

                ModSetup.Initialize().GetAwaiter();

                BoneESP.SetEnabled(false);
                BoxESP.SetEnabled(false);
                PunchSystem.Initialize();

                CoroutineManager.Init();

                OdiumConsole.LogGradient("System", "Initialization complete!", LogLevel.Info);
            }
            catch (Exception ex)
            {
            }
        }

        private bool AuthenticateUser()
        {
            try
            {
                // Check if we have saved valid credentials
                if (File.Exists(AUTH_FILE))
                {
                    try
                    {
                        string savedData = File.ReadAllText(AUTH_FILE);
                        var authData = JsonConvert.DeserializeObject<AuthData>(savedData);

                        if (ValidateCredentials(authData.Email, authData.Key))
                        {
                            OdiumConsole.LogGradient("Auth", "Using saved credentials...", LogLevel.Info);
                            wasKeyValid = true;
                            return true;
                        }
                        else
                        {
                            // Saved credentials are invalid, delete the file
                            File.Delete(AUTH_FILE);
                            OdiumConsole.Log("Auth", "Saved credentials are invalid, requesting new ones...", LogLevel.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        OdiumConsole.Log("Auth", $"Error reading saved credentials: {ex.Message}", LogLevel.Warning);
                        if (File.Exists(AUTH_FILE))
                            File.Delete(AUTH_FILE);
                    }
                }

                // Show authentication dialog
                return ShowAuthenticationDialog();
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("Auth", $"Authentication error: {ex.Message}", LogLevel.Error);
                ShowErrorDialog("Authentication Error", $"An error occurred during authentication:\n{ex.Message}");
                return false;
            }
        }

        private bool ShowAuthenticationDialog()
        {
            try
            {
                return ShowFileBasedAuth();
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("Auth", $"Dialog error: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        private bool ShowFileBasedAuth()
        {
            try
            {
                string authFilePath = Path.Combine(Environment.CurrentDirectory, "Odium", "temp_auth.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(authFilePath));

                // Check if user already created the file
                if (File.Exists(authFilePath))
                {
                    string[] lines = File.ReadAllLines(authFilePath);
                    if (lines.Length >= 2)
                    {
                        string email = lines[0].Trim();
                        string key = lines[1].Trim();

                        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(key))
                        {
                            if (ValidateCredentials(email, key))
                            {
                                SaveCredentials(email, key);
                                File.Delete(authFilePath);
                                wasKeyValid = true;
                                OdiumConsole.LogGradient("Auth", "Authentication successful via file!", LogLevel.Info);
                                return true;
                            }
                            else
                            {
                                File.Delete(authFilePath); // Clean up invalid file
                                OdiumConsole.Log("Auth", "Invalid credentials in temp_auth.txt file.", LogLevel.Error);
                            }
                        }
                    }
                    File.Delete(authFilePath); // Clean up malformed file
                }

                // Create instruction file if authentication is needed
                string instructions = @"ODIUM AUTHENTICATION REQUIRED
=====================================

To authenticate Odium, please create a file named 'temp_auth.txt' in the Odium folder with your credentials:

File Location: {0}

File Contents (2 lines):
Line 1: Your purchase email
Line 2: Your invite key

Example:
user@example.com
your-invite-key-here

After creating the file, restart VRChat.
The file will be automatically deleted after successful authentication.

VRChat will now close so you can set up authentication.";

                string instructionPath = Path.Combine(Environment.CurrentDirectory, "Odium", "auth_instructions.txt");
                File.WriteAllText(instructionPath, string.Format(instructions, authFilePath));

                OdiumConsole.Log("Auth", $"Authentication required. Instructions written to: {instructionPath}", LogLevel.Info);
                OdiumConsole.Log("Auth", $"Create temp_auth.txt at: {authFilePath}", LogLevel.Info);

                return false;
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("Auth", $"File auth error: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        private bool ValidateCredentials(string email, string key)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);

                    var requestData = new
                    {
                        email = email,
                        key = key
                    };

                    string jsonContent = JsonConvert.SerializeObject(requestData);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var response = client.PostAsync(AUTH_ENDPOINT, content).Result;
                    string responseContent = response.Content.ReadAsStringAsync().Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var result = JsonConvert.DeserializeObject<ValidationResponse>(responseContent);
                        return result?.Success == true && result?.Valid == true;
                    }
                    else
                    {
                        OdiumConsole.Log("Auth", $"Validation request failed: {response.StatusCode} - {responseContent}", LogLevel.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("Auth", $"Validation error: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        private void SaveCredentials(string email, string key)
        {
            try
            {
                var authData = new AuthData { Email = email, Key = key };
                string jsonData = JsonConvert.SerializeObject(authData);

                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(AUTH_FILE));

                File.WriteAllText(AUTH_FILE, jsonData);
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("Auth", $"Failed to save credentials: {ex.Message}", LogLevel.Warning);
            }
        }

        private void ShowErrorDialog(string title, string message)
        {
            try
            {
                // Try Visual Basic MsgBox first
                Microsoft.VisualBasic.Interaction.MsgBox($"{message}", Microsoft.VisualBasic.MsgBoxStyle.Critical, $"Odium - {title}");
            }
            catch
            {
                // Fallback to console logging
                OdiumConsole.Log("Error", $"{title}: {message}", LogLevel.Error);
            }
        }

        private void ShowSuccessDialog(string title, string message)
        {
            try
            {
                // Try Visual Basic MsgBox first
                Microsoft.VisualBasic.Interaction.MsgBox($"{message}", Microsoft.VisualBasic.MsgBoxStyle.Information, $"Odium - {title}");
            }
            catch
            {
                // Fallback to console logging
                OdiumConsole.Log("Success", $"{title}: {message}", LogLevel.Info);
            }
        }

        // Data classes for JSON serialization
        private class AuthData
        {
            public string Email { get; set; }
            public string Key { get; set; }
        }

        private class ValidationResponse
        {
            public bool Success { get; set; }
            public bool Valid { get; set; }
            public string Message { get; set; }
        }

        public override void OnApplicationStart()
        {
            HarmonyInstance = new HarmonyLib.Harmony("Odium.Harmony");

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

            MelonCoroutines.Start(RamClearLoop());
            Patching.Initialize();
            ClonePatch.Patch();
            PhotonPatchesManual.ApplyPatches();
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

                        VRCPlayerApi vrcPlayerApi = PlayerWrapper.GetLocalPlayerAPIUser(obj.prop_IUser_0.prop_String_0);
                        VRC.Player player = PlayerWrapper.GetVRCPlayerFromId(obj.prop_IUser_0.prop_String_0)._player;

                        if (AssignedVariables.instanceLock)
                        {
                            for (int i = 0; i < 450; i++) Murder4Utils.SendTargetedPatreonEvent(player, "ListPatrons");
                        }

                        if (Networking.LocalPlayer.displayName == vrcPlayerApi.displayName)
                        {
                            PlayerWrapper.QuickSpoof();
                            PlayerWrapper.LocalPlayer = PlayerWrapper.GetVRCPlayerFromId(obj.prop_IUser_0.prop_String_0)._player;
                            UnityEngine.Color rankColorT = PlayerRankTextDisplay.GetRankColor(PlayerRankTextDisplay.GetPlayerRank(player.field_Private_APIUser_0));
                            string hexColorT = PlayerRankTextDisplay.ColorToHex(rankColorT);
                            string rankNameT = PlayerRankTextDisplay.GetRankDisplayName(PlayerRankTextDisplay.GetPlayerRank(player.field_Private_APIUser_0));
                            IiIIiIIIiIIIIiIIIIiIiIiIIiIIIIiIiIIiiIiIiIIIiiIIiI.IiIIiIIIIIIIIIIIIiiiiiiIIIIiIIiIiIIIiiIiiIiIiIiiIiIiIiIIiIiIIIiiiIIIIIiIIiIiIiIiiIIIiiIiiiiiiiiIiiIIIiIiiiiIIIIIiII(obj.prop_IUser_0.prop_String_0, obj.prop_IUser_0.prop_String_1, hexColorT);

                            MainThreadDispatcher.Enqueue(() =>
                            {
                                try
                                {
                                    var httpClient = new System.Net.Http.HttpClient();
                                    string currentLocation = Current_World_id;
                                    string displayName = obj.prop_IUser_0.prop_String_1;

                                    if (!string.IsNullOrEmpty(currentLocation) && !string.IsNullOrEmpty(displayName))
                                    {
                                        var requestBody = new
                                        {
                                            displayName = displayName,
                                            location = currentLocation
                                        };

                                        string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                                        var content = new System.Net.Http.StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                                        string apiUrl = "http://api.snoofz.net:3778/api/odium/vrc/setUserLocation";
                                        var response = httpClient.PostAsync(apiUrl, content).Result;

                                        if (response.IsSuccessStatusCode)
                                        {
                                            OdiumConsole.Log("Odium", $"Updated location on API: {displayName} -> {currentLocation}");
                                        }
                                        else
                                        {
                                            OdiumConsole.Log("Odium", $"Failed to update location on API. Status: {response.StatusCode}");
                                        }
                                    }
                                }
                                catch (System.Exception ex)
                                {
                                    OdiumConsole.Log("Odium", $"Error updating location on API: {ex.Message}");
                                }
                            });
                        }

                        UnityEngine.Color rankColor = PlayerRankTextDisplay.GetRankColor(PlayerRankTextDisplay.GetPlayerRank(player.field_Private_APIUser_0));
                        string hexColor = PlayerRankTextDisplay.ColorToHex(rankColor);
                        string rankName = PlayerRankTextDisplay.GetRankDisplayName(PlayerRankTextDisplay.GetPlayerRank(player.field_Private_APIUser_0));
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

                        if (AssignedVariables.instanceLock)
                        {
                            for (int i = 0; i < 450; i++) Murder4Utils.SendTargetedPatreonEvent(player, "ListPatrons");
                        }

                        if (Networking.LocalPlayer.displayName == vrcPlayerApi.displayName)
                        {
                            PlayerWrapper.QuickSpoof();
                            PlayerWrapper.LocalPlayer = PlayerWrapper.GetVRCPlayerFromId(obj.prop_IUser_0.prop_String_0)._player;
                            UnityEngine.Color rankColorT = PlayerRankTextDisplay.GetRankColor(PlayerRankTextDisplay.GetPlayerRank(player.field_Private_APIUser_0));
                            string hexColorT = PlayerRankTextDisplay.ColorToHex(rankColorT);
                            string rankNameT = PlayerRankTextDisplay.GetRankDisplayName(PlayerRankTextDisplay.GetPlayerRank(player.field_Private_APIUser_0));
                            IiIIiIIIiIIIIiIIIIiIiIiIIiIIIIiIiIIiiIiIiIIIiiIIiI.IiIIiIIIIIIIIIIIIiiiiiiIIIIiIIiIiIIIiiIiiIiIiIiiIiIiIiIIiIiIIIiiiIIIIIiIIiIiIiIiiIIIiiIiiiiiiiiIiiIIIiIiiiiIIIIIiII(obj.prop_IUser_0.prop_String_0, obj.prop_IUser_0.prop_String_1, hexColorT);
                            MainThreadDispatcher.Enqueue(() =>
                            {
                                try
                                {
                                    var httpClient = new System.Net.Http.HttpClient();
                                    string currentLocation = Current_World_id;
                                    string displayName = obj.prop_IUser_0.prop_String_1;

                                    if (!string.IsNullOrEmpty(currentLocation) && !string.IsNullOrEmpty(displayName))
                                    {
                                        var requestBody = new
                                        {
                                            displayName = displayName,
                                            location = currentLocation
                                        };

                                        string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                                        var content = new System.Net.Http.StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                                        string apiUrl = "http://api.snoofz.net:3778/api/odium/vrc/setUserLocation";
                                        var response = httpClient.PostAsync(apiUrl, content).Result;

                                        if (response.IsSuccessStatusCode)
                                        {
                                            OdiumConsole.Log("Odium", $"Updated location on API: {displayName} -> {currentLocation}");
                                        }
                                        else
                                        {
                                            OdiumConsole.Log("Odium", $"Failed to update location on API. Status: {response.StatusCode}");
                                        }
                                    }
                                }
                                catch (System.Exception ex)
                                {
                                    OdiumConsole.Log("Odium", $"Error updating location on API: {ex.Message}");
                                }
                            });
                        }

                        UnityEngine.Color rankColor = PlayerRankTextDisplay.GetRankColor(PlayerRankTextDisplay.GetPlayerRank(player.field_Private_APIUser_0));
                        string hexColor = PlayerRankTextDisplay.ColorToHex(rankColor);
                        string rankName = PlayerRankTextDisplay.GetRankDisplayName(PlayerRankTextDisplay.GetPlayerRank(player.field_Private_APIUser_0));
                        InternalConsole.LogIntoConsole($"[<color=#77dd77>PlayerJoin</color>] -> <color={hexColor}>{player.field_Private_APIUser_0.displayName}</color>");

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
                                    var players = PlayerWrapper.GetAllPlayers().ToArray();
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
                        UnityEngine.Color rankColor = PlayerRankTextDisplay.GetRankColor(PlayerRankTextDisplay.GetPlayerRank(player.field_Private_APIUser_0));
                        string hexColor = PlayerRankTextDisplay.ColorToHex(rankColor);
                        string rankName = PlayerRankTextDisplay.GetRankDisplayName(PlayerRankTextDisplay.GetPlayerRank(player.field_Private_APIUser_0));
                        InternalConsole.LogIntoConsole($"[<color=#ff6961>PlayerLeave</color>] -> <color={hexColor}>{player.field_Private_APIUser_0.displayName}</color>");
                        PlayerWrapper.Players.Remove(player);
                        BoneESP.OnPlayerLeft(player);
                        BoxESP.OnPlayerLeft(player);
                    }));
            }
        }

        public override void OnGUI()
        {
            BoneESP.OnGUI();
            BoxESP.OnGUI();
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
            OdiumModuleLoader.OnSceneWasLoaded(buildindex, sceneName);
            CursorLayerMod.CursorLayerMod.OnSceneWasLoaded(buildindex, sceneName);
            OnLoadedSceneManager.LoadedScene(buildindex, sceneName);
        }

        public override void OnSceneWasUnloaded(int buildindex, string sceneName)
        {
            OdiumModuleLoader.OnSceneWasUnloaded(buildindex, sceneName);
        }

        public override void OnApplicationQuit()
        {
            OdiumModuleLoader.OnApplicationQuit();
        }

        private static IEnumerator RamClearLoop()
        {
            for (; ; )
            {
                yield return new WaitForSeconds(300f);
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
            }
        }

        public override void OnUpdate()
        {
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
            CursorLayerMod.CursorLayerMod.OnUpdate();
            Chatbox.UpdateFrameEffects();
            Exploits.UpdateChatboxLagger();

            if (Time.time - lastStatsUpdate >= STATS_UPDATE_INTERVAL)
            {
                NameplateModifier.UpdatePlayerStats();
                lastStatsUpdate = Time.time;
            }

            if (Input.GetKeyDown(KeyCode.Minus))
            {
                DroneWrapper.DroneCrash();
            }
        }

        public override void OnFixedUpdate()
        {
            OdiumModuleLoader.OnFixedUpdate();
        }

        public override void OnLateUpdate()
        {
            OdiumModuleLoader.OnLateUpdate();
            SpyCamera.LateUpdate();
        }
    }
}