using ExitGames.Client.Photon;
using HarmonyLib;
using Odium;
using Odium.ApplicationBot;
using Odium.Components;
using Odium.Modules;
using Odium.Odium;
using Odium.UX;
using Odium.Wrappers;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerBaseLib;
using UnityEngine;
using UnityEngine.UI;
using VRC;
using MelonLoader;
using System.Collections;
using Photon.Pun;
using System.Windows.Forms;
using static Il2CppSystem.Diagnostics.Tracing.EventSource;
using VRC.SDK3.Network;
using VRC.Udon;
using VampClient.Api;

namespace Odium.Patches
{
    public struct PlayerActivityData
    {
        public int actorId;
        public string userId;
        public string displayName;
        public float lastEvent1Time;
        public float lastEvent12Time;
        public bool hasCrashed;
        public bool wasActive;
    }

    [HarmonyPatch(typeof(LoadBalancingClient))]
    public class PhotonPatches
    {
        public static bool BlockUdon = false;
        public static Dictionary<int, int> blockedMessages = new Dictionary<int, int>();
        public static int blockedChatBoxMessages = 0;
        public static Dictionary<int, int> blockedMessagesCount = new Dictionary<int, int>();
        public static Dictionary<int, int> blockedUSpeakPacketCount = new Dictionary<int, int>();
        public static Dictionary<int, int> blockedUSpeakPackets = new Dictionary<int, int>();
        public static List<string> blockedUserIds = new List<string>();
        public static List<string> mutedUserIds = new List<string>();
        private static Dictionary<string, int> crashAttemptCounts = new Dictionary<string, int>();
        private static Dictionary<string, DateTime> lastLogTimes = new Dictionary<string, DateTime>();
        private static Dictionary<int, PlayerActivityData> playerActivityTracker = new Dictionary<int, PlayerActivityData>();
        private static HashSet<string> crashedPlayerIds = new HashSet<string>();
        private static object crashDetectionCoroutine;
        private const float CRASH_TIMEOUT = 5.0f; 
        private const float CHECK_INTERVAL = 1.0f;
        public static Sprite LogoIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\OdiumIcon.png");

        static PhotonPatches()
        {
            StartCrashDetection();
        }

        public static void StartCrashDetection()
        {
            if (crashDetectionCoroutine != null)
            {
                MelonCoroutines.Stop(crashDetectionCoroutine);
            }
            crashDetectionCoroutine = MelonCoroutines.Start(CrashDetectionLoop());
            OdiumConsole.Log("CrashDetection", "Crash detection system started");
        }

        public static void StopCrashDetection()
        {
            if (crashDetectionCoroutine != null)
            {
                MelonCoroutines.Stop(crashDetectionCoroutine);
                crashDetectionCoroutine = null;
                OdiumConsole.Log("CrashDetection", "Crash detection system stopped");
            }
        }

        private static IEnumerator CrashDetectionLoop()
        {
            while (true)
            {
                try
                {
                    CheckForCrashedPlayers();
                    CleanupDisconnectedPlayers();
                }
                catch (Exception ex)
                {
                    OdiumConsole.Log("CrashDetection", $"Error in crash detection loop: {ex.Message}");
                }

                yield return new WaitForSeconds(CHECK_INTERVAL);
            }
        }

        private static void CheckForCrashedPlayers()
        {
            float currentTime = Time.time;
            var keysToCheck = new List<int>(playerActivityTracker.Keys);

            foreach (int actorId in keysToCheck)
            {
                var playerData = playerActivityTracker[actorId];

                if (!playerData.wasActive || playerData.hasCrashed)
                    continue;

                float timeSinceEvent1 = currentTime - playerData.lastEvent1Time;
                float timeSinceEvent12 = currentTime - playerData.lastEvent12Time;

                if (timeSinceEvent1 > CRASH_TIMEOUT && timeSinceEvent12 > CRASH_TIMEOUT)
                {
                    playerData.hasCrashed = true;
                    playerActivityTracker[actorId] = playerData;

                    if (!string.IsNullOrEmpty(playerData.userId))
                    {
                        crashedPlayerIds.Add(playerData.userId);
                        OdiumConsole.LogGradient("CrashDetection",
                            $"Player CRASHED: {playerData.displayName} (Actor: {actorId}, UserID: {playerData.userId})");

                        InternalConsole.LogIntoConsole(
                            $"{playerData.displayName} has crashed (no events for {CRASH_TIMEOUT}s)"
                        );
                    }
                }
            }
        }

        private static void CleanupDisconnectedPlayers()
        {
            try
            {
                if (PlayerManager.prop_PlayerManager_0?.field_Private_List_1_Player_0 == null)
                    return;

                var currentPlayers = PlayerManager.prop_PlayerManager_0.field_Private_List_1_Player_0.ToArray()
                    .Where(p => p?.field_Private_APIUser_0?.id != null)
                    .Select(p => p.field_Private_APIUser_0.id)
                    .ToHashSet();

                var crashedToRemove = crashedPlayerIds.Where(userId => !currentPlayers.Contains(userId)).ToList();
                foreach (string userId in crashedToRemove)
                {
                    crashedPlayerIds.Remove(userId);
                    OdiumConsole.LogGradient("CrashDetection", $"Removed disconnected crashed player: {userId}");
                }

                var actorIdsToRemove = playerActivityTracker.Keys
                    .Where(actorId =>
                    {
                        var playerData = playerActivityTracker[actorId];
                        return !string.IsNullOrEmpty(playerData.userId) && !currentPlayers.Contains(playerData.userId);
                    })
                    .ToList();

                foreach (int actorId in actorIdsToRemove)
                {
                    playerActivityTracker.Remove(actorId);
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("CrashDetection", $"Error in cleanup: {ex.Message}");
            }
        }

        private static void UpdatePlayerActivity(int actorId, int eventCode)
        {
            try
            {
                VRC.Player player = PlayerWrapper.GetVRCPlayerFromActorNr(actorId);

                if (!playerActivityTracker.ContainsKey(actorId))
                {
                    var newPlayerData = new PlayerActivityData
                    {
                        actorId = actorId,
                        userId = player?.field_Private_APIUser_0?.id ?? "",
                        displayName = player?.field_Private_APIUser_0?.displayName ?? "Unknown",
                        lastEvent1Time = 0f,
                        lastEvent12Time = 0f,
                        hasCrashed = false,
                        wasActive = false
                    };
                    playerActivityTracker[actorId] = newPlayerData;
                }

                var playerData = playerActivityTracker[actorId];
                float currentTime = Time.time;

                if (eventCode == 1)
                {
                    playerData.lastEvent1Time = currentTime;
                    playerData.wasActive = true;
                }
                else if (eventCode == 12)
                {
                    playerData.lastEvent12Time = currentTime;
                    playerData.wasActive = true;
                }

                if (playerData.hasCrashed && playerData.wasActive)
                {
                    playerData.hasCrashed = false;
                    if (!string.IsNullOrEmpty(playerData.userId))
                    {
                        crashedPlayerIds.Remove(playerData.userId);
                        OdiumConsole.LogGradient("CrashDetection",
                            $"Player RECOVERED: {playerData.displayName} is active again");
                    }
                }

                playerActivityTracker[actorId] = playerData;
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("CrashDetection", $"Error updating player activity: {ex.Message}");
            }
        }

        public static bool HasPlayerCrashed(string userId)
        {
            return !string.IsNullOrEmpty(userId) && crashedPlayerIds.Contains(userId);
        }

        public static string GetCrashStatusInfo()
        {
            var activePlayers = playerActivityTracker.Values.Where(p => p.wasActive && !p.hasCrashed).Count();
            var crashedCount = crashedPlayerIds.Count;
            return $"Active: {activePlayers}, Crashed: {crashedCount}";
        }

        public static void MarkPlayerAsCrashed(string userId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                crashedPlayerIds.Add(userId);
            }
        }

        public static void UnmarkPlayerAsCrashed(string userId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                crashedPlayerIds.Remove(userId);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnEvent")]
        static bool OnEvent(LoadBalancingClient __instance, ExitGames.Client.Photon.EventData param_1)
        {
            var eventCode = param_1.Code;

            if (eventCode == 1 || eventCode == 12)
            {
                UpdatePlayerActivity(param_1.sender, eventCode);
            }

            switch (eventCode)
            {
                case 1:
                    byte[] e = Serializer.ToByteArray(param_1.CustomData);
                    string base64 = Convert.ToBase64String(e);

                    VRC.Player plr = PlayerWrapper.GetVRCPlayerFromActorNr(param_1.sender);

                    // This works fine dont touch
                    if ((int)USpeakPacketHandler.ParseUSpeakPacket(e).gain > 90)
                    {
                        if (!blockedUSpeakPacketCount.ContainsKey(param_1.sender))
                        {
                            blockedUSpeakPacketCount[param_1.sender] = 0;
                            blockedUSpeakPackets[param_1.sender] = 0;
                        }

                        blockedUSpeakPacketCount[param_1.sender]++;
                        blockedUSpeakPackets[param_1.sender]++;

                        if (blockedUSpeakPacketCount[param_1.sender] == 1)
                        {
                            VRC.Player player = PlayerWrapper.GetVRCPlayerFromActorNr(param_1.sender);
                            InternalConsole.LogIntoConsole($"<color=red>Blocked USpeak packet from user -> {player.field_Private_APIUser_0.displayName}</color>", "[OdiumProtection]");
                        }
                        else if (blockedUSpeakPackets[param_1.sender] == 200)
                        {
                            VRC.Player player = PlayerWrapper.GetVRCPlayerFromActorNr(param_1.sender);
                            InternalConsole.LogIntoConsole(
                                $"<color=red>Blocked {blockedUSpeakPacketCount[param_1.sender]} total USpeak packets from user -> {player.field_Private_APIUser_0.displayName}</color>"
                            );
                            blockedUSpeakPackets[param_1.sender] = 0;
                        }
                        return false;
                    } else
                    {
                        return true;
                    }
                case 18:
                    var dictionary = param_1.Parameters[param_1.CustomDataKey].Cast<Il2CppSystem.Collections.Generic.Dictionary<byte, Il2CppSystem.Object>>();
                    int viewId = dictionary[2].Unbox<int>();
                    var eventType = dictionary[0].Unbox<byte>();
                    if (eventType == 1)
                    {
                        PhotonView photonView = PhotonView.Method_Public_Static_PhotonView_Int32_0(viewId);
                        UdonBehaviour udonBehaviour = photonView.gameObject.GetComponent<UdonBehaviour>();
                        if (udonBehaviour != null)
                        {
                            string behaviourName = udonBehaviour.gameObject.name;
                            int eventHashInt = dictionary[1].Unbox<int>();
                            uint eventHash = (uint)eventHashInt;
                            if (udonBehaviour.TryGetEntrypointNameFromHash(eventHash, out string entryPointName))
                            {
                                if (entryPointName == "ListPatrons" && behaviourName == "Patreon Credits")
                                {
                                    VRC.Player vrcPlayer = PlayerWrapper.GetVRCPlayerFromActorNr(param_1.sender);
                                    string playerName = vrcPlayer.field_Private_APIUser_0.displayName;

                                    if (!crashAttemptCounts.ContainsKey(playerName))
                                    {
                                        crashAttemptCounts[playerName] = 0;
                                    }
                                    if (!lastLogTimes.ContainsKey(playerName))
                                    {
                                        lastLogTimes[playerName] = DateTime.MinValue;
                                    }

                                    crashAttemptCounts[playerName]++;
                                    int attemptCount = crashAttemptCounts[playerName];
                                    DateTime currentTime = DateTime.Now;

                                    if ((currentTime - lastLogTimes[playerName]).TotalSeconds >= 15)
                                    {
                                        InternalConsole.LogIntoConsole($"Prevented: {playerName} | Count: {attemptCount}", "[ListPatrons]");
                                        ToastBase.Toast("Odium Protection", $"Potentially harmful event blocked from user '{playerName}'", LogoIcon, 5);
                                        lastLogTimes[playerName] = currentTime;
                                    }

                                    return false;
                                } else
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    break;
                case 43:
                    string incomingMessage = "";
                    try
                    {
                        byte[] byteArray = Serializer.ToByteArray(param_1.CustomData);
                        incomingMessage = ChatboxLogger.ConvertBytesToText(byteArray);

                        incomingMessage = incomingMessage.Replace("\uFFFD", "")
                                                         .Replace("\u000B", "")
                                                         .Replace("\"", "")
                                                         .Trim();
                    }
                    catch (System.Exception ex)
                    {
                        OdiumConsole.Log("ChatBox", $"Error extracting message: {ex.Message}");
                        incomingMessage = "[Error extracting message]";
                    }

                    if (ChatboxAntis.IsMessageValid(incomingMessage))
                    {
                        return true;
                    }
                    else
                    {
                        if (!blockedMessagesCount.ContainsKey(param_1.sender))
                        {
                            blockedMessagesCount[param_1.sender] = 0;
                            blockedMessages[param_1.sender] = 0;
                        }

                        blockedMessagesCount[param_1.sender]++;
                        blockedMessages[param_1.sender]++;

                        if (blockedMessagesCount[param_1.sender] == 1)
                        {
                            VRC.Player player = PlayerWrapper.GetVRCPlayerFromActorNr(param_1.sender);
                            InternalConsole.LogIntoConsole(
                                $"<color=red>Blocked chatbox message from user -> {player.field_Private_APIUser_0.displayName}</color>"
                            );
                        }
                        else if (blockedMessages[param_1.sender] >= 100)
                        {
                            VRC.Player player = PlayerWrapper.GetVRCPlayerFromActorNr(param_1.sender);
                            InternalConsole.LogIntoConsole(
                                $"<color=red>Blocked {blockedMessagesCount[param_1.sender]} total chatbox messages from user -> {player.field_Private_APIUser_0.displayName}</color>"
                            );
                            blockedMessages[param_1.sender] = 0;
                        }
                        return false;
                    }
                    break;
                case 33:
                    if (param_1.Parameters != null && param_1.Parameters.ContainsKey(245))
                    {
                        var moderationDict = param_1.Parameters[245]
                            .TryCast<Il2CppSystem.Collections.Generic.Dictionary<byte, Il2CppSystem.Object>>();

                        if (moderationDict != null)
                        {
                            if (moderationDict.ContainsKey(0))
                            {
                                var moderationType = moderationDict[0].Unbox<byte>();
                            }

                            if (moderationDict.ContainsKey(1))
                            {
                                var playerId = moderationDict[1].Unbox<int>();


                                if (moderationDict.ContainsKey(10))
                                {
                                    var blockStatus = moderationDict[10].Unbox<bool>();

                                    if (blockStatus == true && !blockedUserIds.Contains(playerId.ToString()))
                                    {
                                        blockedUserIds.Add(playerId.ToString());

                                        VRCPlayer player = PlayerWrapper.GetPlayerFromPhotonId(playerId);
                                        if (AssignedVariables.announceBlocks)
                                        {
                                            Chatbox.SendCustomChatMessage($"[Odium] -> {player.field_Private_VRCPlayerApi_0.displayName} BLOCKED me");
                                        }
                                        Color rankColor = NameplateModifier.GetRankColor(NameplateModifier.GetPlayerRank(player._player.field_Private_APIUser_0));
                                        string hexColor = NameplateModifier.ColorToHex(rankColor);
                                        OdiumBottomNotification.ShowNotification($"<color=#FF5151>BLOCKED</color> by <color={hexColor}>{player.field_Private_VRCPlayerApi_0.displayName}");
                                        InternalConsole.LogIntoConsole(
                                            $"<color=#7B02FE>{player.field_Private_VRCPlayerApi_0.displayName}</color> <color=red>BLOCKED</color> you!"
                                        );
                                    }
                                    else if (blockStatus == false && blockedUserIds.Contains(playerId.ToString()))
                                    {
                                        blockedUserIds.Remove(playerId.ToString());

                                        VRCPlayer player = PlayerWrapper.GetPlayerFromPhotonId(playerId);
                                        if (AssignedVariables.announceBlocks)
                                        {
                                            Chatbox.SendCustomChatMessage($"[Odium] -> {player.field_Private_VRCPlayerApi_0.displayName} UNBLOCKED me");
                                        }
                                        Color rankColor = NameplateModifier.GetRankColor(NameplateModifier.GetPlayerRank(player._player.field_Private_APIUser_0));
                                        string hexColor = NameplateModifier.ColorToHex(rankColor);
                                        OdiumBottomNotification.ShowNotification($"<color=#FF5151>UNBLOCKED</color> by <color={hexColor}>{player.field_Private_VRCPlayerApi_0.displayName}");
                                        InternalConsole.LogIntoConsole(
                                            $"<color=#7B02FE>{player.field_Private_VRCPlayerApi_0.displayName}</color> <color=red>UNBLOCKED</color> you!"
                                        );
                                    }
                                }

                                if (moderationDict.ContainsKey(11))
                                {
                                    var muteStatus = moderationDict[11].Unbox<bool>();

                                    if (muteStatus == true && !mutedUserIds.Contains(playerId.ToString()))
                                    {
                                        mutedUserIds.Add(playerId.ToString());

                                        VRCPlayer player = PlayerWrapper.GetPlayerFromPhotonId(playerId);
                                        if (AssignedVariables.announceMutes)
                                        {
                                            Chatbox.SendCustomChatMessage($"[Odium] -> {player.field_Private_VRCPlayerApi_0.displayName} MUTED me");
                                        }
                                        Color rankColor = NameplateModifier.GetRankColor(NameplateModifier.GetPlayerRank(player._player.field_Private_APIUser_0));
                                        string hexColor = NameplateModifier.ColorToHex(rankColor);
                                        OdiumBottomNotification.ShowNotification($"<color=#FF5151>MUTED</color> by <color={hexColor}>{player.field_Private_VRCPlayerApi_0.displayName}");
                                        InternalConsole.LogIntoConsole(
                                            $"<color=#7B02FE>{player.field_Private_VRCPlayerApi_0.displayName}</color> <color=red>MUTED</color> you!"
                                        );
                                    }
                                    else if (muteStatus == false && mutedUserIds.Contains(playerId.ToString()))
                                    {
                                        mutedUserIds.Remove(playerId.ToString());

                                        VRCPlayer player = PlayerWrapper.GetPlayerFromPhotonId(playerId);
                                        if (AssignedVariables.announceMutes)
                                        {
                                            Chatbox.SendCustomChatMessage($"[Odium] -> {player.field_Private_VRCPlayerApi_0.displayName} unfortunately UNMUTED me");
                                        }
                                        Color rankColor = NameplateModifier.GetRankColor(NameplateModifier.GetPlayerRank(player._player.field_Private_APIUser_0));
                                        string hexColor = NameplateModifier.ColorToHex(rankColor);
                                        OdiumBottomNotification.ShowNotification($"<color=#FF5151>UNMUTED</color> by <color={hexColor}>{player.field_Private_VRCPlayerApi_0.displayName}");
                                        InternalConsole.LogIntoConsole(
                                            $"<color=#7B02FE>{player.field_Private_VRCPlayerApi_0.displayName}</color> <color=red>UNMUTED</color> you!"
                                        );
                                    }
                                }
                            }
                            else
                            {
                            }
                        }
                    }

                    if (param_1.Parameters != null && param_1.Parameters.ContainsKey(254))
                    {
                        var timestamp = param_1.Parameters[254];
                    }
                    break;

                case 208:
                    try
                    {
                        InternalConsole.LogIntoConsole(
                            $"<color=#31BCF0>[MasterClient]:</color> Master client switch detected"
                        );

                        if (param_1.Parameters != null && param_1.Parameters.ContainsKey(254))
                        {
                            var newMasterActorId = param_1.Parameters[254];
                            if (newMasterActorId != null)
                            {
                                var actorNumber = newMasterActorId.Unbox<int>();
                                VRCPlayer newMasterPlayer = PlayerWrapper.GetPlayerFromPhotonId(actorNumber);

                                if (newMasterPlayer != null)
                                {
                                    Color rankColor = NameplateModifier.GetRankColor(NameplateModifier.GetPlayerRank(newMasterPlayer._player.field_Private_APIUser_0));
                                    string hexColor = NameplateModifier.ColorToHex(rankColor);

                                    InternalConsole.LogIntoConsole(
                                        $"<color=#31BCF0>[MasterClient]:</color> New master: <color={hexColor}>{newMasterPlayer.field_Private_VRCPlayerApi_0.displayName}</color> (ID: {actorNumber})"
                                    );

                                    OdiumBottomNotification.ShowNotification($"<color={hexColor}>{newMasterPlayer.field_Private_VRCPlayerApi_0.displayName}</color> is the new <color=#FFECA1>Master</color>");
                                }
                                else
                                {
                                    InternalConsole.LogIntoConsole(
                                        $"<color=#31BCF0>[MasterClient]:</color> New master actor ID: {actorNumber} (Player not found)"
                                    );
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        InternalConsole.LogIntoConsole(
                            $"<color=#31BCF0>[MasterClient]:</color> <color=red>Error processing master client switch: {ex.Message}</color>"
                        );
                    }
                    break;
            }

            return true;
        }
    }
}