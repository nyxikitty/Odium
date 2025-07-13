using UnityEngine;
using VRC;
using MelonLoader;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using VRC.SDKBase;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections;
using Odium.Wrappers;
using Odium.Odium;
using Odium.Patches; // Add this for crash detection

namespace Odium.Components
{
    public enum Rank
    {
        Visitor,
        NewUser,
        User,
        Known,
        Trusted
    }

    public struct NameplateData
    {
        public string userId;
        public List<TextMeshProUGUI> statsComponents;
        public List<Transform> tagPlates;
        public float lastSeen;
        public Vector3 lastPosition;
        public string platform;
        public List<string> userTags;
    }

    [System.Serializable]
    public class TagResponse
    {
        public bool success;
        public string userId;
        public List<string> tags;
    }

    public static class NameplateModifier
    {
        private static List<NameplateData> playerStats = new List<NameplateData>();
        private static HttpClient httpClient = new HttpClient();
        private static string API_BASE = "https://odiumvrc.com/api/odium/tags";
        private static Dictionary<string, List<string>> tagCache = new Dictionary<string, List<string>>();

        private static bool autoRefreshEnabled = true;
        private static float lastRefreshTime = 0f;
        private static readonly float REFRESH_INTERVAL = 10f;

        public static void ModifyPlayerNameplate(Player player, Sprite newDevCircleSprite = null)
        {
            try
            {
                var apiUser = player.prop_APIUser_0;
                var nameplateContainer = player._vrcplayer.field_Public_GameObject_0;
                var playerNameplateCanvas = nameplateContainer.transform.FindChild("PlayerNameplate/Canvas");

                if (playerNameplateCanvas == null)
                {
                    MelonLogger.Warning("Could not find PlayerNameplate/Canvas");
                    return;
                }

                CleanupPlayerStats(apiUser.id);

                Rank rank = GetPlayerRank(apiUser);

                DestroyIconIfEnabled(playerNameplateCanvas);
                DisableBackground(playerNameplateCanvas);

                if (newDevCircleSprite != null)
                {
                    ChangeDevCircleSprite(playerNameplateCanvas, newDevCircleSprite, rank);
                }
                else
                {
                    ApplyRankColoring(playerNameplateCanvas, rank);
                }

                MelonCoroutines.Start(AddStatsToNameplateCoroutine(player, playerNameplateCanvas));
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error in ModifyPlayerNameplate: {ex.Message}");
            }
        }

        private static IEnumerator AddStatsToNameplateCoroutine(Player player, Transform playerNameplateCanvas)
        {
            yield return new WaitForSeconds(0.1f);

            try
            {
                var nameplateGroup = playerNameplateCanvas.FindChild("NameplateGroup/Nameplate");
                if (nameplateGroup == null)
                {
                    MelonLogger.Warning("Could not find NameplateGroup/Nameplate");
                    yield break;
                }

                var quickStats = nameplateGroup.FindChild("Contents/Quick Stats");
                if (quickStats == null)
                {
                    MelonLogger.Warning("Could not find Quick Stats to clone");
                    yield break;
                }

                string userId = player.field_Private_APIUser_0.id;

                CleanupPlayerStats(userId);

                var statsComponents = new List<TextMeshProUGUI>();
                var tagPlates = new List<Transform>();

                var mainStatsTransform = CreateStatsPlate(quickStats, nameplateGroup, "Player Stats Info", 0);
                if (mainStatsTransform != null)
                {
                    var mainStatsComponent = SetupStatsComponent(mainStatsTransform);
                    if (mainStatsComponent != null)
                    {
                        statsComponents.Add(mainStatsComponent);
                        tagPlates.Add(mainStatsTransform);
                    }
                }

                var statsData = new NameplateData
                {
                    userId = userId,
                    statsComponents = statsComponents,
                    tagPlates = tagPlates,
                    lastSeen = Time.time,
                    lastPosition = player.transform.position,
                    platform = GetPlayerPlatform(player),
                    userTags = new List<string>()
                };

                playerStats.Add(statsData);

                UpdateSinglePlayerStats(player, statsData);

                MelonCoroutines.Start(FetchAndApplyTagsCoroutine(userId, quickStats, nameplateGroup));

                MelonLogger.Msg($"Added basic stats display for player: {player.field_Private_APIUser_0.displayName}");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error in AddStatsToNameplateCoroutine: {ex.Message}");
            }
        }

        private static IEnumerator FetchAndApplyTagsCoroutine(string userId, Transform quickStats, Transform nameplateGroup)
        {
            Task<List<string>> tagTask = GetUserTags(userId);

            while (!tagTask.IsCompleted)
            {
                yield return new WaitForSeconds(0.1f);
            }

            List<string> userTags = new List<string>();
            try
            {
                userTags = tagTask.Result;
                tagCache[userId] = userTags;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Warning($"Failed to get tags for user {userId}: {ex.Message}");
            }

            ApplyTagsToNameplate(userId, userTags, quickStats, nameplateGroup);
        }

        private static void ApplyTagsToNameplate(string userId, List<string> userTags, Transform quickStats, Transform nameplateGroup)
        {
            try
            {
                var statsIndex = playerStats.FindIndex(s => s.userId == userId);
                if (statsIndex == -1) return;

                var statsData = playerStats[statsIndex];

                if (userId == PlayerWrapper.LocalPlayer.field_Private_APIUser_0.id)
                {
                    AssignedVariables.playerTagsCount = statsData.tagPlates.Count;
                }

                if (statsData.tagPlates.Count > 1)
                {
                    for (int i = statsData.tagPlates.Count - 1; i >= 1; i--)
                    {
                        try
                        {
                            if (statsData.tagPlates[i] != null && statsData.tagPlates[i].gameObject != null)
                            {
                                UnityEngine.Object.Destroy(statsData.tagPlates[i].gameObject);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            MelonLogger.Warning($"Error destroying old tag plate: {ex.Message}");
                        }
                    }

                    statsData.tagPlates.RemoveRange(1, statsData.tagPlates.Count - 1);
                    statsData.statsComponents.RemoveRange(1, statsData.statsComponents.Count - 1);
                }

                // Check if player has crashed and add it as the first tag
                bool playerCrashed = PhotonPatches.HasPlayerCrashed(userId);
                int tagStartIndex = 1;

                if (playerCrashed)
                {
                    var crashTagTransform = CreateStatsPlate(quickStats, nameplateGroup, "Crash Tag", 1);
                    if (crashTagTransform != null)
                    {
                        var crashTagComponent = SetupStatsComponent(crashTagTransform);
                        if (crashTagComponent != null)
                        {
                            crashTagComponent.text = "<color=#e91f42>CRASHED</color>";
                            statsData.statsComponents.Add(crashTagComponent);
                            statsData.tagPlates.Add(crashTagTransform);
                        }
                    }
                    tagStartIndex = 2; // Start user tags after crash tag
                }

                // Add user tags from API
                for (int i = 0; i < userTags.Count; i++)
                {
                    var tagStatsTransform = CreateStatsPlate(quickStats, nameplateGroup, $"Tag Stats {i}", tagStartIndex + i);
                    if (tagStatsTransform != null)
                    {
                        var tagStatsComponent = SetupStatsComponent(tagStatsTransform);
                        if (tagStatsComponent != null)
                        {
                            tagStatsComponent.text = $"<color=#e91e63>{userTags[i]}</color>";
                            statsData.statsComponents.Add(tagStatsComponent);
                            statsData.tagPlates.Add(tagStatsTransform);
                        }
                    }
                }

                statsData.userTags = userTags;
                playerStats[statsIndex] = statsData;

                int totalTags = userTags.Count + (playerCrashed ? 1 : 0);
                MelonLogger.Msg($"Applied {totalTags} tags for player: {userId} (Crashed: {playerCrashed})");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error applying tags to nameplate: {ex.Message}");
            }
        }

        public static string ColorToHex(Color color)
        {
            int r = Mathf.RoundToInt(color.r * 255);
            int g = Mathf.RoundToInt(color.g * 255);
            int b = Mathf.RoundToInt(color.b * 255);
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        public static void CheckAndRefreshTags()
        {
            if (!autoRefreshEnabled) return;

            if (Time.time - lastRefreshTime >= REFRESH_INTERVAL)
            {
                lastRefreshTime = Time.time;
                MelonCoroutines.Start(RefreshAllTagsCoroutine());
            }
        }

        private static IEnumerator RefreshAllTagsCoroutine()
        {
            MelonLogger.Msg("Starting auto-refresh of all player tags...");

            var playersToRefresh = new List<NameplateData>(playerStats);

            foreach (var statsData in playersToRefresh)
            {
                if (string.IsNullOrEmpty(statsData.userId)) continue;

                var player = GetPlayerById(statsData.userId);
                if (player == null) continue;

                var nameplateContainer = player._vrcplayer.field_Public_GameObject_0;
                var playerNameplateCanvas = nameplateContainer.transform.FindChild("PlayerNameplate/Canvas");
                if (playerNameplateCanvas == null) continue;

                var nameplateGroup = playerNameplateCanvas.FindChild("NameplateGroup/Nameplate");
                if (nameplateGroup == null) continue;

                var quickStats = nameplateGroup.FindChild("Contents/Quick Stats");
                if (quickStats == null) continue;

                MelonCoroutines.Start(FetchAndApplyTagsCoroutine(statsData.userId, quickStats, nameplateGroup));

                yield return new WaitForSeconds(0.1f);
            }

            tagCache.Clear();

            MelonLogger.Msg($"Initiated tag refresh for {playersToRefresh.Count} players");
        }

        private static Player GetPlayerById(string userId)
        {
            try
            {
                if (PlayerManager.prop_PlayerManager_0?.field_Private_List_1_Player_0 == null)
                    return null;

                var players = PlayerManager.prop_PlayerManager_0.field_Private_List_1_Player_0.ToArray();
                return players.FirstOrDefault(p => p?.field_Private_APIUser_0?.id == userId);
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error getting player by ID {userId}: {ex.Message}");
                return null;
            }
        }

        public static void EnableAutoRefresh()
        {
            autoRefreshEnabled = true;
            MelonLogger.Msg("Auto-refresh enabled");
        }

        public static void DisableAutoRefresh()
        {
            autoRefreshEnabled = false;
            MelonLogger.Msg("Auto-refresh disabled");
        }

        public static void SetRefreshInterval(float seconds)
        {
            if (seconds > 0)
            {
                var field = typeof(NameplateModifier).GetField("REFRESH_INTERVAL", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                field?.SetValue(null, seconds);
                MelonLogger.Msg($"Refresh interval set to {seconds} seconds");
            }
        }

        public static void ManualRefreshAllTags()
        {
            MelonCoroutines.Start(RefreshAllTagsCoroutine());
        }

        private static Transform CreateStatsPlate(Transform quickStats, Transform nameplateGroup, string plateName, int stackIndex)
        {
            try
            {
                var statsTransform = UnityEngine.Object.Instantiate(quickStats, nameplateGroup.FindChild("Contents"));
                if (statsTransform == null)
                {
                    MelonLogger.Warning($"Failed to instantiate {plateName} transform");
                    return null;
                }

                statsTransform.name = plateName;
                statsTransform.gameObject.SetActive(true);

                float yOffset = 180f + (stackIndex * 30f);
                statsTransform.localPosition = new Vector3(0f, yOffset, 0f);

                var trustIcon = statsTransform.FindChild("Trust Icon");
                if (trustIcon != null) trustIcon.gameObject.SetActive(false);

                var perfIcon = statsTransform.FindChild("Performance Icon");
                if (perfIcon != null) perfIcon.gameObject.SetActive(false);

                var perfText = statsTransform.FindChild("Performance Text");
                if (perfText != null) perfText.gameObject.SetActive(false);

                var friendAnchor = statsTransform.FindChild("Friend Anchor Stats");
                if (friendAnchor != null) friendAnchor.gameObject.SetActive(false);

                var imageComponent = statsTransform.GetComponent<ImageThreeSlice>();
                if (imageComponent != null)
                {
                    if (stackIndex == 0)
                    {
                        imageComponent.color = new Color(0f, 0f, 0f, 0.6f);
                    }
                    else
                    {
                        imageComponent.color = new Color(0.9f, 0.1f, 0.4f, 0.3f);
                    }
                }

                return statsTransform;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error creating stats plate {plateName}: {ex.Message}");
                return null;
            }
        }

        private static TextMeshProUGUI SetupStatsComponent(Transform statsTransform)
        {
            try
            {
                var trustText = statsTransform.FindChild("Trust Text");
                if (trustText == null)
                {
                    MelonLogger.Warning("Could not find Trust Text component");
                    return null;
                }

                var statsComponent = trustText.GetComponent<TextMeshProUGUI>();
                if (statsComponent == null)
                {
                    MelonLogger.Warning("Could not get TextMeshProUGUI component");
                    return null;
                }

                statsComponent.color = Color.white;
                statsComponent.fontSize = 12f;
                statsComponent.fontStyle = FontStyles.Bold;

                return statsComponent;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error setting up stats component: {ex.Message}");
                return null;
            }
        }

        private static async Task<List<string>> GetUserTags(string userId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = System.TimeSpan.FromSeconds(5);
                    var response = await client.GetStringAsync($"{API_BASE}/get?userId={userId}");
                    var tagResponse = JsonConvert.DeserializeObject<TagResponse>(response);

                    if (tagResponse.success && tagResponse.tags != null)
                    {
                        return tagResponse.tags;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Warning($"Failed to get tags for user {userId}: {ex.Message}");
            }

            return new List<string>();
        }

        public static void UpdatePlayerStats()
        {
            try
            {
                CheckAndRefreshTags();

                if (PlayerManager.prop_PlayerManager_0?.field_Private_List_1_Player_0 == null)
                    return;

                var players = PlayerManager.prop_PlayerManager_0.field_Private_List_1_Player_0.ToArray();

                foreach (var player in players)
                {
                    if (player?.field_Private_APIUser_0?.id == null) continue;

                    var statsIndex = playerStats.FindIndex(s => s.userId == player.field_Private_APIUser_0.id);
                    if (statsIndex == -1) continue;

                    var statsData = playerStats[statsIndex];
                    if (statsData.statsComponents == null || statsData.statsComponents.Count == 0) continue;

                    if (!ValidateStatsComponents(statsData))
                    {
                        MelonLogger.Warning($"Invalid stats components detected for player {player.field_Private_APIUser_0.displayName}, cleaning up...");
                        CleanupPlayerStats(player.field_Private_APIUser_0.id);
                        continue;
                    }

                    // Check if crash status has changed and refresh tags if needed
                    bool playerCurrentlyCrashed = PhotonPatches.HasPlayerCrashed(player.field_Private_APIUser_0.id);
                    bool hasCrashTag = statsData.tagPlates.Count > 1 &&
                                      statsData.statsComponents.Count > 1 &&
                                      statsData.statsComponents[1] != null &&
                                      statsData.statsComponents[1].text.Contains("CRASHED");

                    if (playerCurrentlyCrashed != hasCrashTag)
                    {
                        // Crash status changed, refresh the tags
                        var nameplateContainer = player._vrcplayer.field_Public_GameObject_0;
                        var playerNameplateCanvas = nameplateContainer.transform.FindChild("PlayerNameplate/Canvas");
                        if (playerNameplateCanvas != null)
                        {
                            var nameplateGroup = playerNameplateCanvas.FindChild("NameplateGroup/Nameplate");
                            if (nameplateGroup != null)
                            {
                                var quickStats = nameplateGroup.FindChild("Contents/Quick Stats");
                                if (quickStats != null)
                                {
                                    // Use cached tags if available, otherwise use empty list for immediate update
                                    List<string> cachedTags = tagCache.ContainsKey(player.field_Private_APIUser_0.id)
                                        ? tagCache[player.field_Private_APIUser_0.id]
                                        : statsData.userTags ?? new List<string>();

                                    ApplyTagsToNameplate(player.field_Private_APIUser_0.id, cachedTags, quickStats, nameplateGroup);
                                }
                            }
                        }
                    }

                    UpdateSinglePlayerStats(player, statsData);
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error in UpdatePlayerStats: {ex.Message}");
            }
        }

        private static bool ValidateStatsComponents(NameplateData statsData)
        {
            try
            {
                if (statsData.statsComponents.Count > 0 && statsData.statsComponents[0] != null)
                {
                    var _ = statsData.statsComponents[0].text;
                    return true;
                }
            }
            catch (System.Exception)
            {
                return false;
            }

            return false;
        }

        private static void UpdateSinglePlayerStats(Player player, NameplateData statsData)
        {
            try
            {
                var updatedStatsData = statsData;
                updatedStatsData.lastSeen = Time.time;
                updatedStatsData.lastPosition = player.transform.position;

                if (updatedStatsData.statsComponents.Count > 0 && updatedStatsData.statsComponents[0] != null)
                {
                    List<string> displayComponents = new List<string>();

                    string platformIcon = GetPlatformIcon(updatedStatsData.platform);
                    if (!string.IsNullOrEmpty(platformIcon))
                    {
                        displayComponents.Add(platformIcon);
                    }

                    if (player.field_Private_VRCPlayerApi_0?.isMaster == true)
                    {
                        displayComponents.Add("[<color=#FFD700>M</color>]");
                    }

                    if (IsFriend(player))
                    {
                        displayComponents.Add("[<color=#FF69B4>FRIEND</color>]");
                    }

                    if (IsAdult(player))
                    {
                        displayComponents.Add("[<color=#9966FF>18+</color>]");
                    }

                    string joinDate = GetAvatarReleaseStatus(player);
                    displayComponents.Add($"[<color=#9966FF>{joinDate}</color>]");

                    string displayText = string.Join(" | ", displayComponents);
                    updatedStatsData.statsComponents[0].text = displayText;
                }

                var statsIndex = playerStats.FindIndex(s => s.userId == updatedStatsData.userId);
                if (statsIndex != -1)
                {
                    playerStats[statsIndex] = updatedStatsData;
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error updating stats for player {player.field_Private_APIUser_0.displayName}: {ex.Message}");
                CleanupPlayerStats(player.field_Private_APIUser_0.id);
            }
        }

        public static void CleanupPlayerStats(string userId)
        {
            try
            {
                var statsIndex = playerStats.FindIndex(s => s.userId == userId);
                if (statsIndex != -1)
                {
                    var statsData = playerStats[statsIndex];

                    if (statsData.tagPlates != null)
                    {
                        foreach (var tagPlate in statsData.tagPlates)
                        {
                            try
                            {
                                if (tagPlate != null && tagPlate.gameObject != null)
                                {
                                    UnityEngine.Object.Destroy(tagPlate.gameObject);
                                }
                            }
                            catch (System.Exception ex)
                            {
                                MelonLogger.Warning($"Error destroying tag plate: {ex.Message}");
                            }
                        }
                    }

                    playerStats.RemoveAt(statsIndex);

                    MelonLogger.Msg($"Cleaned up stats for user: {userId}");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error cleaning up stats for user {userId}: {ex.Message}");
            }
        }

        public static void ClearTagCache(string userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    tagCache.Clear();
                    MelonLogger.Msg("Cleared all tag cache");
                }
                else if (tagCache.ContainsKey(userId))
                {
                    tagCache.Remove(userId);
                    MelonLogger.Msg($"Cleared tag cache for user: {userId}");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error clearing tag cache: {ex.Message}");
            }
        }

        public static string GetPlayerPlatform(Player player)
        {
            try
            {
                var apiUser = player.field_Private_APIUser_0;
                if (apiUser?.last_platform != null)
                {
                    return apiUser.last_platform;
                }

                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private static string GetPlatformIcon(string platform)
        {
            switch (platform?.ToLower())
            {
                case "standalonewindows":
                    return "[<color=#00BFFF>PC</color>]";
                case "android":
                    return "[<color=#32CD32>Q</color>]";
                case "ios":
                    return "[<color=#FF69B4>iOS</color>]";
                default:
                    return "[<color=#FFFFFF>UNK</color>]";
            }
        }

        private static bool IsFriend(Player player)
        {
            try
            {
                var apiUser = player.field_Private_APIUser_0;
                return apiUser?.isFriend == true;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsAdult(Player player)
        {
            try
            {
                var apiUser = player.field_Private_APIUser_0;
                return apiUser?.ageVerified == true ||
                       apiUser?.isAdult == true;
            }
            catch
            {
                return false;
            }
        }

        private static string GetAvatarReleaseStatus(Player player)
        {
            try
            {
                var apiUser = player.prop_ApiAvatar_0;
                return apiUser?.releaseStatus;
            }
            catch
            {
                return "ERR";
            }
        }

        public static Rank GetPlayerRank(VRC.Core.APIUser apiUser)
        {
            if (apiUser.hasLegendTrustLevel || apiUser.hasVeteranTrustLevel)
            {
                return Rank.Trusted;
            }
            else if (apiUser.hasTrustedTrustLevel)
            {
                return Rank.Known;
            }
            else if (apiUser.hasKnownTrustLevel)
            {
                return Rank.User;
            }
            else if (apiUser.hasBasicTrustLevel)
            {
                return Rank.NewUser;
            }
            else
            {
                return Rank.Visitor;
            }
        }


        private static void DestroyIconIfEnabled(Transform playerNameplateCanvas)
        {

        }

        private static void DisableBackground(Transform playerNameplateCanvas)
        {

        }

        private static void ChangeDevCircleSprite(Transform playerNameplateCanvas, Sprite newSprite, Rank rank)
        {
            var devCircle = playerNameplateCanvas.FindChild("NameplateGroup/Nameplate/Contents/Main/Dev Circle");

            if (devCircle != null)
            {
                var imageComponent = devCircle.GetComponent<ImageThreeSlice>();

                if (imageComponent != null)
                {
                    imageComponent.prop_Sprite_0 = newSprite;
                    imageComponent._sprite = newSprite;

                    var rankColor = GetRankColor(rank);
                    var canvasRenderer = devCircle.GetComponent<UnityEngine.CanvasRenderer>();
                    if (canvasRenderer != null)
                    {
                        canvasRenderer.SetColor(rankColor);
                    }

                    devCircle.gameObject.SetActive(true);

                    MelonLogger.Msg($"Changed Dev Circle sprite with {rank} coloring");
                }
                else
                {
                    MelonLogger.Warning("Dev Circle does not have an ImageThreeSlice component");
                }
            }
            else
            {
                MelonLogger.Warning("Could not find Dev Circle");
            }
        }

        private static void ApplyRankColoring(Transform playerNameplateCanvas, Rank rank)
        {
            var devCircle = playerNameplateCanvas.FindChild("NameplateGroup/Nameplate/Contents/Main/Dev Circle");

            if (devCircle != null)
            {
                var rankColor = GetRankColor(rank);
                var canvasRenderer = devCircle.GetComponent<UnityEngine.CanvasRenderer>();
                if (canvasRenderer != null)
                {
                    canvasRenderer.SetColor(rankColor);
                    devCircle.gameObject.SetActive(true);
                    MelonLogger.Msg($"Applied {rank} coloring to Dev Circle");
                }
            }
        }

        public static Color GetRankColor(Rank rank)
        {
            switch (rank)
            {
                case Rank.Visitor:
                    return new Color(1f, 1f, 1f, 0.8f);         // White
                case Rank.NewUser:
                    return ColorFromHex("#96ECFF", 0.8f);       // Light Blue
                case Rank.User:
                    return ColorFromHex("#96FFA9", 0.8f);       // Light Green
                case Rank.Known:
                    return ColorFromHex("#FF5E50", 0.8f);       // Orangish Red
                case Rank.Trusted:
                    return ColorFromHex("#A900FE", 0.8f);       // Purple
                default:
                    return new Color(1f, 1f, 1f, 0.8f);         // Default to white
            }
        }

        public static Color ColorFromHex(string hex, float alpha = 1f)
        {
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);

            if (hex.Length == 6)
            {
                float r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                float g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                float b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                return new Color(r, g, b, alpha);
            }

            return Color.white;
        }
    }
}