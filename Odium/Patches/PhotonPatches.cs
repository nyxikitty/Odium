using MelonLoader;
using Odium.Components;
using Odium.UX;
using Odium.Wrappers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    // Remove the [HarmonyPatch] attributes and keep all the utility methods and fields
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
        public static Dictionary<string, int> crashAttemptCounts = new Dictionary<string, int>();
        public static Dictionary<string, DateTime> lastLogTimes = new Dictionary<string, DateTime>();
        public static Dictionary<int, PlayerActivityData> playerActivityTracker = new Dictionary<int, PlayerActivityData>();
        public static HashSet<string> crashedPlayerIds = new HashSet<string>();
        private static object crashDetectionCoroutine;
        private const float CRASH_TIMEOUT = 5.0f;
        private const float CHECK_INTERVAL = 1.0f;
        public static Sprite LogoIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\OdiumIcon.png");
        public static readonly Dictionary<string, DateTime> syncAssignMCooldowns = new Dictionary<string, DateTime>();
        public static readonly TimeSpan SYNC_ASSIGN_M_COOLDOWN = TimeSpan.FromMinutes(2);
        public static Dictionary<string, DateTime> lastEventTimes = new Dictionary<string, DateTime>();

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

        public static void UpdatePlayerActivity(int actorId, int eventCode)
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

        public static bool TryUnboxInt(Il2CppSystem.Object obj, out int result)
        {
            result = 0;
            try
            {
                if (obj == null) return false;
                result = obj.Unbox<int>();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryUnboxByte(Il2CppSystem.Object obj, out byte result)
        {
            result = 0;
            try
            {
                if (obj == null) return false;
                result = obj.Unbox<byte>();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsKnownExploitPattern(string entryPointName, string behaviourName)
        {
            var knownExploits = new Dictionary<string, string[]>
            {
                { "ListPatrons", new[] { "Patreon Credits" } },
            };

            return knownExploits.ContainsKey(entryPointName) &&
                   knownExploits[entryPointName].Contains(behaviourName);
        }

        public static bool IsRapidFireEvent(string playerName)
        {
            DateTime currentTime = DateTime.Now;
            if (lastEventTimes.ContainsKey(playerName))
            {
                if ((currentTime - lastEventTimes[playerName]).TotalMilliseconds < 100)
                {
                    return true;
                }
            }
            lastEventTimes[playerName] = currentTime;
            return false;
        }

        public static string GetGameObjectPath(GameObject obj)
        {
            if (obj == null) return "null";

            string path = obj.name;
            Transform current = obj.transform.parent;

            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        private static void CleanupOldCooldowns()
        {
            DateTime cutoff = DateTime.Now - TimeSpan.FromHours(1);
            var keysToRemove = syncAssignMCooldowns.Where(kvp => kvp.Value < cutoff).Select(kvp => kvp.Key).ToList();

            foreach (var key in keysToRemove)
            {
                syncAssignMCooldowns.Remove(key);
            }

            if (keysToRemove.Count > 0)
            {
                InternalConsole.LogIntoConsole($"[CLEANUP] Removed {keysToRemove.Count} old cooldown entries");
            }
        }

        public static bool IsUnusualHash(uint eventHash)
        {
            if (eventHash == 0 || eventHash == uint.MaxValue) return true;

            var knownBadHashes = new uint[] { 236258089 };
            return knownBadHashes.Contains(eventHash);
        }
    }
}