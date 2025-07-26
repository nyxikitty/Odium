using UnityEngine;
using VRC;
using MelonLoader;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using VRC.SDKBase;
using System.Collections;
using Odium.Wrappers;
using Odium.Odium;
using Odium.Patches; // Add this for crash detection
using System.Threading.Tasks;

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
        public bool isClientUser; // Added for client user tracking
        public TextMeshProUGUI creationDateComponent; // New field for creation date component
        public Transform creationDatePlate; // New field for creation date plate
        public System.DateTime? joinDate; // Cache for fetched join date
        public bool isDateFetching; // Flag to prevent multiple API calls
    }

    public static class NameplateModifier
    {
        private static List<NameplateData> playerStats = new List<NameplateData>();

        // Added client user tracking
        private static HashSet<string> clientUsers = new HashSet<string>();

        // VRChat API client instance
        private static VRChatApiClient apiClient = new VRChatApiClient();

        private static bool autoRefreshEnabled = true;
        private static float lastRefreshTime = 0f;
        private static readonly float REFRESH_INTERVAL = 10f;

        // Toggle states
        private static bool plateStatsEnabled = true;
        private static bool crashDetectionEnabled = true;
        private static bool customBackgroundEnabled = true;
        private static bool creationDateEnabled = true; // New toggle for creation date plate

        // Toggle Functions
        public static void TogglePlateStats()
        {
            plateStatsEnabled = !plateStatsEnabled;
            MelonLogger.Msg($"Plate stats {(plateStatsEnabled ? "enabled" : "disabled")}");

            if (!plateStatsEnabled)
            {
                // Hide all existing stats plates
                foreach (var statsData in playerStats)
                {
                    if (statsData.statsComponents != null)
                    {
                        foreach (var component in statsData.statsComponents)
                        {
                            if (component != null && component.gameObject != null)
                            {
                                component.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
            else
            {
                // Show all stats plates and refresh
                RefreshAllNameplates();
            }
        }

        // Enhanced Join Date Visibility Toggle Methods
        public static void ToggleJoinDateVisibility()
        {
            creationDateEnabled = !creationDateEnabled;
            MelonLogger.Msg($"Join date visibility {(creationDateEnabled ? "enabled" : "disabled")}");

            UpdateJoinDateVisibility();
        }

        public static void SetJoinDateVisibility(bool visible)
        {
            if (creationDateEnabled == visible) return; // No change needed

            creationDateEnabled = visible;
            MelonLogger.Msg($"Join date visibility set to: {(visible ? "enabled" : "disabled")}");

            UpdateJoinDateVisibility();
        }

        private static void UpdateJoinDateVisibility()
        {
            try
            {
                foreach (var statsData in playerStats.ToList()) // ToList() to avoid modification during iteration
                {
                    if (statsData.creationDateComponent != null && statsData.creationDateComponent.gameObject != null)
                    {
                        // Set visibility based on toggle state
                        statsData.creationDateComponent.gameObject.SetActive(creationDateEnabled);

                        if (statsData.creationDatePlate != null && statsData.creationDatePlate.gameObject != null)
                        {
                            statsData.creationDatePlate.gameObject.SetActive(creationDateEnabled);
                        }
                    }
                }

                // If enabling, refresh to fetch any missing join dates
                if (creationDateEnabled)
                {
                    RefreshJoinDates();
                }

                MelonLogger.Msg($"Updated join date visibility for {playerStats.Count} players");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error updating join date visibility: {ex.Message}");
            }
        }

        private static void RefreshJoinDates()
        {
            try
            {
                MelonCoroutines.Start(RefreshJoinDatesCoroutine());
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error starting join date refresh: {ex.Message}");
            }
        }

        private static IEnumerator RefreshJoinDatesCoroutine()
        {
            MelonLogger.Msg("Refreshing join dates for all players...");

            var playersNeedingDates = playerStats.Where(s =>
                creationDateEnabled &&
                s.creationDateComponent != null &&
                !s.joinDate.HasValue &&
                !s.isDateFetching
            ).ToList();

            foreach (var statsData in playersNeedingDates)
            {
                // Start async fetch for each player that needs a date
                Task.Run(async () => await FetchUserJoinDate(statsData.userId));

                // Small delay to prevent overwhelming the API
                yield return new WaitForSeconds(0.2f);
            }

            MelonLogger.Msg($"Initiated join date fetch for {playersNeedingDates.Count} players");
        }

        public static bool IsJoinDateVisible() => creationDateEnabled;
        public static bool IsJoinDateEnabled() => creationDateEnabled; // Alias for consistency

        public static void HideAllJoinDatePlates()
        {
            try
            {
                foreach (var statsData in playerStats)
                {
                    if (statsData.creationDateComponent != null && statsData.creationDateComponent.gameObject != null)
                    {
                        statsData.creationDateComponent.gameObject.SetActive(false);
                    }

                    if (statsData.creationDatePlate != null && statsData.creationDatePlate.gameObject != null)
                    {
                        statsData.creationDatePlate.gameObject.SetActive(false);
                    }
                }

                MelonLogger.Msg("Hidden all join date plates");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error hiding join date plates: {ex.Message}");
            }
        }

        public static void ShowAllJoinDatePlates()
        {
            try
            {
                foreach (var statsData in playerStats)
                {
                    if (statsData.creationDateComponent != null && statsData.creationDateComponent.gameObject != null)
                    {
                        statsData.creationDateComponent.gameObject.SetActive(true);
                    }

                    if (statsData.creationDatePlate != null && statsData.creationDatePlate.gameObject != null)
                    {
                        statsData.creationDatePlate.gameObject.SetActive(true);
                    }
                }

                MelonLogger.Msg("Showed all join date plates");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error showing join date plates: {ex.Message}");
            }
        }

        public static void GetJoinDateStats()
        {
            try
            {
                int totalPlayers = playerStats.Count;
                int playersWithDates = playerStats.Count(s => s.joinDate.HasValue);
                int playersCurrentlyFetching = playerStats.Count(s => s.isDateFetching);
                int playersWithComponents = playerStats.Count(s => s.creationDateComponent != null);

                MelonLogger.Msg($"Join Date Statistics:");
                MelonLogger.Msg($"  Total players: {totalPlayers}");
                MelonLogger.Msg($"  Players with join dates: {playersWithDates}");
                MelonLogger.Msg($"  Players currently fetching: {playersCurrentlyFetching}");
                MelonLogger.Msg($"  Players with date components: {playersWithComponents}");
                MelonLogger.Msg($"  Join date feature enabled: {creationDateEnabled}");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error getting join date stats: {ex.Message}");
            }
        }

        // Original methods updated to use enhanced system
        public static void ToggleCreationDate()
        {
            ToggleJoinDateVisibility(); // Use the enhanced method
        }

        public static void SetCreationDate(bool enabled)
        {
            SetJoinDateVisibility(enabled); // Use the enhanced method
        }

        public static void SetPlateStats(bool enabled)
        {
            plateStatsEnabled = enabled;
            MelonLogger.Msg($"Plate stats set to: {(enabled ? "enabled" : "disabled")}");

            if (!enabled)
            {
                foreach (var statsData in playerStats)
                {
                    if (statsData.statsComponents != null)
                    {
                        foreach (var component in statsData.statsComponents)
                        {
                            if (component != null && component.gameObject != null)
                            {
                                component.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
            else
            {
                RefreshAllNameplates();
            }
        }

        public static void ToggleCrashDetection()
        {
            crashDetectionEnabled = !crashDetectionEnabled;
            MelonLogger.Msg($"Crash detection {(crashDetectionEnabled ? "enabled" : "disabled")}");
            RefreshAllNameplates();
        }

        public static void SetCrashDetection(bool enabled)
        {
            crashDetectionEnabled = enabled;
            MelonLogger.Msg($"Crash detection set to: {(enabled ? "enabled" : "disabled")}");
            RefreshAllNameplates();
        }

        public static void ToggleCustomBackground()
        {
            customBackgroundEnabled = !customBackgroundEnabled;
            MelonLogger.Msg($"Custom nameplate background {(customBackgroundEnabled ? "enabled" : "disabled")}");
            RefreshAllNameplates();
        }

        public static void SetCustomBackground(bool enabled)
        {
            customBackgroundEnabled = enabled;
            MelonLogger.Msg($"Custom nameplate background set to: {(enabled ? "enabled" : "disabled")}");
            RefreshAllNameplates();
        }

        public static bool IsPlateStatsEnabled() => plateStatsEnabled;
        public static bool IsCrashDetectionEnabled() => crashDetectionEnabled;
        public static bool IsCustomBackgroundEnabled() => customBackgroundEnabled;
        public static bool IsCreationDateEnabled() => creationDateEnabled; // New getter

        private static void RefreshAllNameplates()
        {
            MelonCoroutines.Start(RefreshAllNameplatesCoroutine());
        }

        private static IEnumerator RefreshAllNameplatesCoroutine()
        {
            if (PlayerManager.prop_PlayerManager_0?.field_Private_List_1_Player_0 == null)
                yield break;

            var players = PlayerManager.prop_PlayerManager_0.field_Private_List_1_Player_0.ToArray();

            foreach (var player in players)
            {
                if (player?.field_Private_APIUser_0?.id == null) continue;

                ModifyPlayerNameplate(player);
                yield return new WaitForSeconds(0.05f); // Small delay to prevent frame drops
            }

            MelonLogger.Msg($"Refreshed nameplates for {players.Length} players");
        }

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

                if (customBackgroundEnabled)
                {
                    DestroyIconIfEnabled(playerNameplateCanvas);
                    DisableBackground(playerNameplateCanvas);
                }

                if (newDevCircleSprite != null)
                {
                    ChangeDevCircleSprite(playerNameplateCanvas, newDevCircleSprite, rank);
                }
                else if (customBackgroundEnabled)
                {
                    ApplyRankColoring(playerNameplateCanvas, rank);
                }

                if (plateStatsEnabled || creationDateEnabled)
                {
                    MelonCoroutines.Start(AddStatsToNameplateCoroutine(player, playerNameplateCanvas));
                }
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
                if (!plateStatsEnabled && !creationDateEnabled) yield break;

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
                TextMeshProUGUI creationDateComponent = null;
                Transform creationDatePlate = null;

                int plateIndex = 0;

                // Create main stats plate if enabled
                if (plateStatsEnabled)
                {
                    var mainStatsTransform = CreateStatsPlate(quickStats, nameplateGroup, "Player Stats Info", plateIndex);
                    if (mainStatsTransform != null)
                    {
                        var mainStatsComponent = SetupStatsComponent(mainStatsTransform);
                        if (mainStatsComponent != null)
                        {
                            statsComponents.Add(mainStatsComponent);
                            tagPlates.Add(mainStatsTransform);
                        }
                    }
                    plateIndex++;
                }

                // Create creation date plate if enabled
                if (creationDateEnabled)
                {
                    creationDatePlate = CreateCreationDatePlate(quickStats, nameplateGroup, "Account Creation Date", plateIndex);
                    if (creationDatePlate != null)
                    {
                        creationDateComponent = SetupCreationDateComponent(creationDatePlate);
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
                    userTags = new List<string>(),
                    isClientUser = false,
                    creationDateComponent = creationDateComponent,
                    creationDatePlate = creationDatePlate,
                    joinDate = null,
                    isDateFetching = false
                };

                playerStats.Add(statsData);

                UpdateSinglePlayerStats(player, statsData);

                // Fetch join date via API if creation date is enabled
                if (creationDateEnabled && creationDateComponent != null)
                {
                    Task.Run(async () => await FetchUserJoinDate(userId));
                }

                // Apply tags to nameplate (now using local data only)
                if (plateStatsEnabled)
                {
                    ApplyTagsToNameplate(userId, new List<string>(), quickStats, nameplateGroup);
                }

                MelonLogger.Msg($"Added nameplate modifications for player: {player.field_Private_APIUser_0.displayName}");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error in AddStatsToNameplateCoroutine: {ex.Message}");
            }
        }

        // New method to fetch user join date via API
        private static async Task FetchUserJoinDate(string userId)
        {
            try
            {
                var statsIndex = playerStats.FindIndex(s => s.userId == userId);
                if (statsIndex == -1) return;

                var statsData = playerStats[statsIndex];
                if (statsData.isDateFetching) return; // Prevent multiple calls

                statsData.isDateFetching = true;
                playerStats[statsIndex] = statsData;

                MelonLogger.Msg($"Fetching join date for user: {userId}");

                // Add detailed error logging
                try
                {
                    var joinDate = await apiClient.GetUserDateJoinedAsync(userId);

                    statsData = playerStats[statsIndex]; // Re-get in case it was modified
                    statsData.joinDate = joinDate;
                    statsData.isDateFetching = false;
                    playerStats[statsIndex] = statsData;

                    MelonLogger.Msg($"Successfully fetched join date for {userId}: {joinDate?.ToString("yyyy-MM-dd") ?? "Unknown"}");
                }
                catch (System.Net.Http.HttpRequestException httpEx)
                {
                    MelonLogger.Error($"HTTP error fetching join date for {userId}: {httpEx.Message}");
                    if (httpEx.Message.Contains("401") || httpEx.Message.Contains("Unauthorized"))
                    {
                        MelonLogger.Error("API authentication failed - check cookies/auth token");
                    }
                    else if (httpEx.Message.Contains("403") || httpEx.Message.Contains("Forbidden"))
                    {
                        MelonLogger.Error("API access forbidden - user may be private or API rate limited");
                    }
                    else if (httpEx.Message.Contains("404"))
                    {
                        MelonLogger.Error("User not found - invalid user ID");
                    }

                    // Reset fetching flag on error
                    statsData = playerStats[statsIndex];
                    statsData.isDateFetching = false;
                    statsData.joinDate = null;
                    playerStats[statsIndex] = statsData;
                }
                catch (System.Threading.Tasks.TaskCanceledException timeoutEx)
                {
                    MelonLogger.Error($"API request timeout for {userId}: {timeoutEx.Message}");

                    // Reset fetching flag on timeout
                    statsData = playerStats[statsIndex];
                    statsData.isDateFetching = false;
                    playerStats[statsIndex] = statsData;
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Unexpected error fetching join date for {userId}: {ex.Message}");
                MelonLogger.Error($"Stack trace: {ex.StackTrace}");

                // Reset fetching flag on error
                var statsIndex = playerStats.FindIndex(s => s.userId == userId);
                if (statsIndex != -1)
                {
                    var statsData = playerStats[statsIndex];
                    statsData.isDateFetching = false;
                    playerStats[statsIndex] = statsData;
                }
            }
        }

        private static TextMeshProUGUI SetupCreationDateComponent(Transform creationDateTransform)
        {
            try
            {
                var trustText = creationDateTransform.FindChild("Trust Text");
                if (trustText == null)
                {
                    MelonLogger.Warning("Could not find Trust Text component for creation date");
                    return null;
                }

                var creationDateComponent = trustText.GetComponent<TextMeshProUGUI>();
                if (creationDateComponent == null)
                {
                    MelonLogger.Warning("Could not get TextMeshProUGUI component for creation date");
                    return null;
                }

                creationDateComponent.color = Color.white;
                creationDateComponent.fontSize = 12f;
                creationDateComponent.fontStyle = FontStyles.Bold;

                return creationDateComponent;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error setting up creation date component: {ex.Message}");
                return null;
            }
        }

        private static void UpdateCreationDateDisplay(Player player, NameplateData statsData)
        {
            try
            {
                if (!creationDateEnabled || statsData.creationDateComponent == null) return;

                string creationDateText;

                if (statsData.isDateFetching)
                {
                    creationDateText = "<color=#FFD700>Fetching join date...</color>";
                }
                else if (statsData.joinDate.HasValue)
                {
                    creationDateText = GetFormattedCreationDate(statsData.joinDate.Value);
                }
                else
                {
                    // More detailed error message and fallback option
                    creationDateText = "<color=#FF6B6B>Join Date: API Failed</color>";

                    // Try to get date from local APIUser as fallback
                    try
                    {
                        var apiUser = player.field_Private_APIUser_0;
                        if (apiUser?._date_joined_k__BackingField != null)
                        {
                            var fallbackDate = System.DateTime.Parse(apiUser._date_joined_k__BackingField);
                            creationDateText = GetFormattedCreationDate(fallbackDate) + " <color=#FFA500>(Local)</color>";
                            MelonLogger.Msg($"Using fallback local date for {apiUser.displayName}: {fallbackDate:yyyy-MM-dd}");
                        }
                    }
                    catch (System.Exception fallbackEx)
                    {
                        MelonLogger.Warning($"Fallback date parsing also failed: {fallbackEx.Message}");
                    }
                }

                statsData.creationDateComponent.text = creationDateText;
                statsData.creationDateComponent.gameObject.SetActive(creationDateEnabled);
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error updating creation date display: {ex.Message}");
            }
        }

        private static string GetFormattedCreationDate(System.DateTime joinDate)
        {
            try
            {
                var now = System.DateTime.Now;
                var timeSpan = now - joinDate;

                string dateColor = "#87CEEB"; // Light blue default
                string ageText = "";

                if (timeSpan.TotalDays < 30)
                {
                    dateColor = "#FF6B6B"; // Red for very new accounts
                    ageText = "New";
                }
                else if (timeSpan.TotalDays < 365)
                {
                    dateColor = "#FFD700"; // Gold for accounts under 1 year
                    ageText = "Recent";
                }
                else if (timeSpan.TotalDays < 365 * 2)
                {
                    dateColor = "#90EE90"; // Light green for 1-2 years
                    ageText = "Established";
                }
                else if (timeSpan.TotalDays < 365 * 4)
                {
                    dateColor = "#87CEEB"; // Light blue for 2-4 years
                    ageText = "Veteran";
                }
                else
                {
                    dateColor = "#DDA0DD"; // Plum for 4+ years
                    ageText = "Ancient";
                }

                string formattedDate = joinDate.ToString("MMM yyyy");
                return $"<color={dateColor}>Joined: {formattedDate} ({ageText})</color>";
            }
            catch (System.Exception ex)
            {
                MelonLogger.Warning($"Error formatting creation date: {ex.Message}");
                return "<color=#FF6B6B>Join Date: Error</color>";
            }
        }

        private static void ApplyTagsToNameplate(string userId, List<string> userTags, Transform quickStats, Transform nameplateGroup)
        {
            try
            {
                if (!plateStatsEnabled) return;

                var statsIndex = playerStats.FindIndex(s => s.userId == userId);
                if (statsIndex == -1) return;

                var statsData = playerStats[statsIndex];

                if (userId == PlayerWrapper.LocalPlayer.field_Private_APIUser_0.id)
                {
                    AssignedVariables.playerTagsCount = statsData.tagPlates.Count;
                }

                // Remove existing tag plates (keep the first one for stats)
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

                bool playerCrashed = crashDetectionEnabled && PhotonPatches.HasPlayerCrashed(userId);
                int tagStartIndex = 1;

                // Calculate offset based on creation date plate
                if (creationDateEnabled)
                {
                    tagStartIndex++;
                }

                // Add crash tag if player crashed and crash detection is enabled
                if (playerCrashed)
                {
                    var crashTagTransform = CreateStatsPlate(quickStats, nameplateGroup, "Crash Tag", tagStartIndex);
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
                    tagStartIndex++;
                }

                // Add user tags
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

                bool isClientUser = clientUsers.Contains(userId) || statsData.isClientUser;
                int totalTags = userTags.Count + (playerCrashed ? 1 : 0);
                MelonLogger.Msg($"Applied {totalTags} tags for player: {userId} (Client: {isClientUser}, Crashed: {playerCrashed})");
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
            if (!autoRefreshEnabled || (!plateStatsEnabled && !creationDateEnabled)) return;

            if (Time.time - lastRefreshTime >= REFRESH_INTERVAL)
            {
                lastRefreshTime = Time.time;
                MelonCoroutines.Start(RefreshAllTagsCoroutine());
            }
        }

        private static IEnumerator RefreshAllTagsCoroutine()
        {
            if (!plateStatsEnabled && !creationDateEnabled) yield break;

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

                // Apply tags using local data only
                if (plateStatsEnabled)
                {
                    ApplyTagsToNameplate(statsData.userId, statsData.userTags, quickStats, nameplateGroup);
                }

                yield return new WaitForSeconds(0.1f);
            }

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

        // Method to manually set client status for a user (since we can't check via API)
        public static void SetClientStatus(string userId, bool isClient)
        {
            if (isClient)
            {
                clientUsers.Add(userId);
                MelonLogger.Msg($"Manually set client user: {userId}");
            }
            else
            {
                clientUsers.Remove(userId);
                MelonLogger.Msg($"Removed client status for user: {userId}");
            }

            // Update the nameplate data
            var statsIndex = playerStats.FindIndex(s => s.userId == userId);
            if (statsIndex != -1)
            {
                var statsData = playerStats[statsIndex];
                statsData.isClientUser = isClient;
                playerStats[statsIndex] = statsData;

                // Trigger a refresh of the main nameplate to show/hide the client tag
                var player = GetPlayerById(userId);
                if (player != null)
                {
                    UpdateSinglePlayerStats(player, statsData);
                }
            }
        }

        // Method to add custom tags for a user (since we can't fetch from API)
        public static void AddUserTags(string userId, List<string> tags)
        {
            var statsIndex = playerStats.FindIndex(s => s.userId == userId);
            if (statsIndex != -1)
            {
                var statsData = playerStats[statsIndex];
                statsData.userTags = tags ?? new List<string>();
                playerStats[statsIndex] = statsData;

                // Refresh the nameplate to show the new tags
                var player = GetPlayerById(userId);
                if (player != null)
                {
                    var nameplateContainer = player._vrcplayer.field_Public_GameObject_0;
                    var playerNameplateCanvas = nameplateContainer.transform.FindChild("PlayerNameplate/Canvas");
                    if (playerNameplateCanvas != null)
                    {
                        var nameplateGroup = playerNameplateCanvas.FindChild("NameplateGroup/Nameplate");
                        if (nameplateGroup != null)
                        {
                            var quickStats = nameplateGroup.FindChild("Contents/Quick Stats");
                            if (quickStats != null && plateStatsEnabled)
                            {
                                ApplyTagsToNameplate(userId, tags, quickStats, nameplateGroup);
                            }
                        }
                    }
                }

                MelonLogger.Msg($"Added {tags.Count} tags for user: {userId}");
            }
        }

        // Method to clear client user cache
        public static void ClearClientCache()
        {
            clientUsers.Clear();
            MelonLogger.Msg("Cleared client user cache");
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

        public static void UpdatePlayerStats()
        {
            try
            {
                if (!plateStatsEnabled && !creationDateEnabled) return;

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

                    if (plateStatsEnabled && statsData.statsComponents != null && statsData.statsComponents.Count == 0) continue;

                    if (plateStatsEnabled && !ValidateStatsComponents(statsData))
                    {
                        MelonLogger.Warning($"Invalid stats components detected for player {player.field_Private_APIUser_0.displayName}, cleaning up...");
                        CleanupPlayerStats(player.field_Private_APIUser_0.id);
                        continue;
                    }

                    // Check if crash status or client status has changed and refresh tags if needed
                    bool playerCurrentlyCrashed = crashDetectionEnabled && PhotonPatches.HasPlayerCrashed(player.field_Private_APIUser_0.id);
                    bool isClientUser = clientUsers.Contains(player.field_Private_APIUser_0.id) || statsData.isClientUser;

                    // Check for crash tag
                    bool hasCrashTag = false;

                    if (plateStatsEnabled && statsData.statsComponents != null)
                    {
                        for (int i = 1; i < statsData.statsComponents.Count; i++)
                        {
                            if (statsData.statsComponents[i] != null)
                            {
                                if (statsData.statsComponents[i].text.Contains("CRASHED"))
                                    hasCrashTag = true;
                            }
                        }
                    }

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
                                if (quickStats != null && plateStatsEnabled)
                                {
                                    ApplyTagsToNameplate(player.field_Private_APIUser_0.id, statsData.userTags ?? new List<string>(), quickStats, nameplateGroup);
                                }
                            }
                        }
                    }

                    UpdateSinglePlayerStats(player, statsData);

                    // Update creation date display
                    if (creationDateEnabled)
                    {
                        UpdateCreationDateDisplay(player, statsData);
                    }
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
                if (!plateStatsEnabled) return;

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

                    // Add client tag to main plate if user is a client
                    bool isClientUser = clientUsers.Contains(updatedStatsData.userId) || updatedStatsData.isClientUser;
                    if (isClientUser)
                    {
                        displayComponents.Add("[<color=#e91f42>C</color>]");
                    }

                    if (player.field_Private_VRCPlayerApi_0?.isMaster == true)
                    {
                        displayComponents.Add("[<color=#FFD700>M</color>]");
                    }

                    if (IsFriend(player))
                    {
                        displayComponents.Add("[<color=#FF69B4>F</color>]");
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

                    // Clean up creation date plate
                    if (statsData.creationDatePlate != null && statsData.creationDatePlate.gameObject != null)
                    {
                        try
                        {
                            UnityEngine.Object.Destroy(statsData.creationDatePlate.gameObject);
                        }
                        catch (System.Exception ex)
                        {
                            MelonLogger.Warning($"Error destroying creation date plate: {ex.Message}");
                        }
                    }

                    playerStats.RemoveAt(statsIndex);
                    // Also remove from client users cache when cleaning up
                    clientUsers.Remove(userId);
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error cleaning up stats for user {userId}: {ex.Message}");
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
            if (!customBackgroundEnabled) return;

            // Add your icon destruction logic here if needed
        }

        private static void DisableBackground(Transform playerNameplateCanvas)
        {
            if (!customBackgroundEnabled) return;

            // Add your background disabling logic here if needed
        }

        private static void ChangeDevCircleSprite(Transform playerNameplateCanvas, Sprite newSprite, Rank rank)
        {
            if (!customBackgroundEnabled) return;

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
                }
            }
        }

        private static void ApplyRankColoring(Transform playerNameplateCanvas, Rank rank)
        {
            if (!customBackgroundEnabled) return;

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

        // Add these fields to your NameplateModifier class

        private static float statsYOffset = 180f; // Default Y offset for stats plates
        private static float plateSpacing = 30f;  // Spacing between plates

        // Add these methods to your NameplateModifier class

        /// <summary>
        /// Set the Y offset for nameplate stats positioning
        /// </summary>
        /// <param name="offset">Y position offset (default: 180f)</param>
        public static void SetStatsYOffset(float offset)
        {
            statsYOffset = offset;
            MelonLogger.Msg($"Stats Y offset set to: {offset}");

            // Apply to all existing stats immediately
            ApplyYOffsetToAllStats();
        }

        /// <summary>
        /// Set the spacing between nameplate plates
        /// </summary>
        /// <param name="spacing">Spacing between plates (default: 30f)</param>
        public static void SetPlateSpacing(float spacing)
        {
            plateSpacing = spacing;
            MelonLogger.Msg($"Plate spacing set to: {spacing}");

            // Apply to all existing stats immediately
            ApplyYOffsetToAllStats();
        }

        /// <summary>
        /// Get current Y offset value
        /// </summary>
        public static float GetStatsYOffset() => statsYOffset;

        /// <summary>
        /// Get current plate spacing value
        /// </summary>
        public static float GetPlateSpacing() => plateSpacing;

        /// <summary>
        /// Apply Y offset changes to all existing nameplate stats
        /// </summary>
        private static void ApplyYOffsetToAllStats()
        {
            try
            {
                MelonLogger.Msg("Applying Y offset changes to all existing stats...");

                foreach (var statsData in playerStats.ToList())
                {
                    try
                    {
                        // Update main stats plates
                        if (statsData.tagPlates != null)
                        {
                            for (int i = 0; i < statsData.tagPlates.Count; i++)
                            {
                                if (statsData.tagPlates[i] != null && statsData.tagPlates[i].gameObject != null)
                                {
                                    float yPos = statsYOffset + (i * plateSpacing);
                                    statsData.tagPlates[i].localPosition = new Vector3(0f, yPos, 0f);
                                }
                            }
                        }

                        // Update creation date plate
                        if (statsData.creationDatePlate != null && statsData.creationDatePlate.gameObject != null)
                        {
                            // Creation date plate comes after main stats
                            int creationDateIndex = (statsData.tagPlates?.Count ?? 0);
                            float creationYPos = statsYOffset + (creationDateIndex * plateSpacing);
                            statsData.creationDatePlate.localPosition = new Vector3(0f, creationYPos, 0f);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        MelonLogger.Warning($"Error updating Y offset for user {statsData.userId}: {ex.Message}");
                    }
                }

                MelonLogger.Msg($"Applied Y offset to {playerStats.Count} players");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error applying Y offset to all stats: {ex.Message}");
            }
        }

        /// <summary>
        /// Reset Y offset and spacing to default values
        /// </summary>
        public static void ResetStatsPositioning()
        {
            statsYOffset = 180f;
            plateSpacing = 30f;
            MelonLogger.Msg("Reset stats positioning to defaults");
            ApplyYOffsetToAllStats();
        }

        // Update your existing CreateStatsPlate method to use the new offset values
        private static Transform CreateStatsPlate(Transform quickStats, Transform nameplateGroup, string plateName, int stackIndex)
        {
            try
            {
                if (!plateStatsEnabled) return null;

                var statsTransform = UnityEngine.Object.Instantiate(quickStats, nameplateGroup.FindChild("Contents"));
                if (statsTransform == null)
                {
                    MelonLogger.Warning($"Failed to instantiate {plateName} transform");
                    return null;
                }

                statsTransform.name = plateName;
                statsTransform.gameObject.SetActive(true);

                // Use the configurable Y offset instead of hardcoded value
                float yOffset = statsYOffset + (stackIndex * plateSpacing);
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

        // Update your existing CreateCreationDatePlate method to use the new offset values
        private static Transform CreateCreationDatePlate(Transform quickStats, Transform nameplateGroup, string plateName, int stackIndex)
        {
            try
            {
                if (!creationDateEnabled) return null;

                var creationDateTransform = UnityEngine.Object.Instantiate(quickStats, nameplateGroup.FindChild("Contents"));
                if (creationDateTransform == null)
                {
                    MelonLogger.Warning($"Failed to instantiate {plateName} transform");
                    return null;
                }

                creationDateTransform.name = plateName;
                creationDateTransform.gameObject.SetActive(true);

                // Use the configurable Y offset instead of hardcoded value
                float yOffset = statsYOffset + (stackIndex * plateSpacing);
                creationDateTransform.localPosition = new Vector3(0f, yOffset, 0f);

                var trustIcon = creationDateTransform.FindChild("Trust Icon");
                if (trustIcon != null) trustIcon.gameObject.SetActive(false);

                var perfIcon = creationDateTransform.FindChild("Performance Icon");
                if (perfIcon != null) perfIcon.gameObject.SetActive(false);

                var perfText = creationDateTransform.FindChild("Performance Text");
                if (perfText != null) perfText.gameObject.SetActive(false);

                var friendAnchor = creationDateTransform.FindChild("Friend Anchor Stats");
                if (friendAnchor != null) friendAnchor.gameObject.SetActive(false);

                var imageComponent = creationDateTransform.GetComponent<ImageThreeSlice>();
                if (imageComponent != null)
                {
                    // Different color for creation date plate (purple/blue theme)
                    imageComponent.color = new Color(0.3f, 0.1f, 0.8f, 0.6f);
                }

                return creationDateTransform;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error creating creation date plate {plateName}: {ex.Message}");
                return null;
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

        // Method to test API connectivity and auth
        public static void TestApiConnection()
        {
            Task.Run(async () =>
            {
                try
                {
                    MelonLogger.Msg("Testing VRChat API connection...");

                    // Test with a known user ID (you can replace with any valid user ID)
                    string testUserId = "usr_c1644b5b-3ca4-45b2-b41c-ce589c39e96c"; // VRChat founder

                    var testResult = await apiClient.GetUserDataAsync(testUserId);

                    if (!string.IsNullOrEmpty(testResult))
                    {
                        MelonLogger.Msg("API connection successful!");
                        MelonLogger.Msg($"Test response length: {testResult.Length} characters");

                        // Try to parse and get date
                        var testDate = await apiClient.GetUserDateJoinedAsync(testUserId);
                        if (testDate.HasValue)
                        {
                            MelonLogger.Msg($"Date extraction successful: {testDate.Value:yyyy-MM-dd}");
                        }
                        else
                        {
                            MelonLogger.Warning("Date extraction failed - check JSON parsing");
                        }
                    }
                    else
                    {
                        MelonLogger.Error("API returned empty response");
                    }
                }
                catch (System.Exception ex)
                {
                    MelonLogger.Error($"API test failed: {ex.Message}");
                    MelonLogger.Error("Common fixes:");
                    MelonLogger.Error("1. Update auth cookie in VRChatApiClient");
                    MelonLogger.Error("2. Check if you're logged into VRChat website");
                    MelonLogger.Error("3. Verify internet connection");
                    MelonLogger.Error("4. Check if VRChat API is accessible");
                }
            });
        }

        // Method to manually retry fetching date for a specific user
        public static void RetryFetchDateForUser(string userId)
        {
            var statsIndex = playerStats.FindIndex(s => s.userId == userId);
            if (statsIndex != -1)
            {
                var statsData = playerStats[statsIndex];
                statsData.isDateFetching = false; // Reset flag
                statsData.joinDate = null; // Clear cached date
                playerStats[statsIndex] = statsData;

                Task.Run(async () => await FetchUserJoinDate(userId));
                MelonLogger.Msg($"Retrying date fetch for user: {userId}");
            }
        }

        // Cleanup method to dispose of API client when mod shuts down
        public static void Dispose()
        {
            try
            {
                apiClient?.Dispose();
                MelonLogger.Msg("VRChat API client disposed");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error disposing API client: {ex.Message}");
            }
        }

        // Add this method to your NameplateModifier class

        /// <summary>
        /// Comprehensive refresh function that completely rebuilds all nameplate modifications
        /// This will refresh colors, stats, tags, creation dates, crash detection, and all visual elements
        /// </summary>
        public static void RefreshEverything()
        {
            try
            {
                MelonLogger.Msg("Starting comprehensive nameplate refresh...");
                MelonCoroutines.Start(RefreshEverythingCoroutine());
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error starting comprehensive refresh: {ex.Message}");
            }
        }

        private static IEnumerator RefreshEverythingCoroutine()
        {
            MelonLogger.Msg("=== COMPREHENSIVE NAMEPLATE REFRESH STARTED ===");

            // Step 1: Clear all existing data and components
            yield return ClearAllNameplateDataCoroutine();

            // Step 2: Get all current players
            if (PlayerManager.prop_PlayerManager_0?.field_Private_List_1_Player_0 == null)
            {
                MelonLogger.Warning("No PlayerManager found, aborting refresh");
                yield break;
            }

            var players = PlayerManager.prop_PlayerManager_0.field_Private_List_1_Player_0.ToArray();
            MelonLogger.Msg($"Found {players.Length} players to refresh");

            if (players.Length == 0)
            {
                MelonLogger.Msg("No players found to refresh");
                yield break;
            }

            // Step 3: Refresh each player completely
            int refreshedCount = 0;
            int errorCount = 0;

            foreach (var player in players)
            {
                if (player?.field_Private_APIUser_0?.id == null)
                {
                    MelonLogger.Warning("Skipping player with null API user");
                    continue;
                }

                var refreshResult = RefreshSinglePlayerCompleteCoroutine(player);
                yield return refreshResult;

                if (refreshResult.Current != null && refreshResult.Current.ToString() == "success")
                {
                    refreshedCount++;
                }
                else
                {
                    errorCount++;
                }

                // Small delay to prevent frame drops and allow UI to update
                yield return new WaitForSeconds(0.1f);
            }

            // Step 4: Force update all visual elements
            yield return ForceUpdateAllVisualsCoroutine();

            // Step 5: Restart background processes
            RestartBackgroundProcesses();

            MelonLogger.Msg($"=== COMPREHENSIVE REFRESH COMPLETED ===");
            MelonLogger.Msg($"Successfully refreshed: {refreshedCount} players");
            MelonLogger.Msg($"Errors encountered: {errorCount}");
            MelonLogger.Msg($"Total nameplate data entries: {playerStats.Count}");

            // Step 6: Log current system status
            LogSystemStatus();
        }

        private static IEnumerator ClearAllNameplateDataCoroutine()
        {
            MelonLogger.Msg("Clearing all existing nameplate data...");

            // Clear all existing player stats and destroy GameObjects
            var statsToClean = new List<NameplateData>(playerStats);

            foreach (var statsData in statsToClean)
            {
                // Destroy all tag plates
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

                // Destroy creation date plate
                try
                {
                    if (statsData.creationDatePlate != null && statsData.creationDatePlate.gameObject != null)
                    {
                        UnityEngine.Object.Destroy(statsData.creationDatePlate.gameObject);
                    }
                }
                catch (System.Exception ex)
                {
                    MelonLogger.Warning($"Error destroying creation date plate: {ex.Message}");
                }

                yield return null; // Allow frame to process destructions
            }

            // Clear all collections
            playerStats.Clear();
            clientUsers.Clear();

            // Reset refresh timer
            lastRefreshTime = 0f;

            MelonLogger.Msg($"Cleared {statsToClean.Count} nameplate data entries");

            // Wait a moment for all destructions to complete
            yield return new WaitForSeconds(0.2f);
        }

        private static IEnumerator RefreshSinglePlayerCompleteCoroutine(Player player)
        {
            var apiUser = player.field_Private_APIUser_0;
            var nameplateContainer = player._vrcplayer.field_Public_GameObject_0;
            var playerNameplateCanvas = nameplateContainer.transform.FindChild("PlayerNameplate/Canvas");

            if (playerNameplateCanvas == null)
            {
                MelonLogger.Warning($"Could not find nameplate canvas for {apiUser.displayName}");
                yield return "error";
                yield break;
            }

            MelonLogger.Msg($"Refreshing nameplate for: {apiUser.displayName}");

            // Step 1: Clean up any existing modifications
            CleanupPlayerStats(apiUser.id);

            yield return new WaitForSeconds(0.05f);

            // Step 2: Determine player rank for coloring
            Rank rank;
            try
            {
                rank = GetPlayerRank(apiUser);
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error getting rank for {apiUser.displayName}: {ex.Message}");
                rank = Rank.Visitor; // Default fallback
            }

            // Step 3: Apply background modifications
            if (customBackgroundEnabled)
            {
                try
                {
                    DestroyIconIfEnabled(playerNameplateCanvas);
                    DisableBackground(playerNameplateCanvas);
                    ApplyRankColoring(playerNameplateCanvas, rank);
                }
                catch (System.Exception ex)
                {
                    MelonLogger.Error($"Error applying background modifications for {apiUser.displayName}: {ex.Message}");
                }
            }

            // Step 4: Add stats and creation date plates
            if (plateStatsEnabled || creationDateEnabled)
            {
                yield return AddStatsToNameplateCoroutine(player, playerNameplateCanvas);
            }

            // Step 5: Force update the display
            var statsIndex = playerStats.FindIndex(s => s.userId == apiUser.id);
            if (statsIndex != -1)
            {
                var statsData = playerStats[statsIndex];

                // Update main stats
                if (plateStatsEnabled)
                {
                    try
                    {
                        UpdateSinglePlayerStats(player, statsData);
                    }
                    catch (System.Exception ex)
                    {
                        MelonLogger.Error($"Error updating stats for {apiUser.displayName}: {ex.Message}");
                    }
                }

                // Update creation date
                if (creationDateEnabled)
                {
                    try
                    {
                        UpdateCreationDateDisplay(player, statsData);

                        // Fetch join date if not already cached
                        if (!statsData.joinDate.HasValue && !statsData.isDateFetching)
                        {
                            Task.Run(async () => await FetchUserJoinDate(apiUser.id));
                        }
                    }
                    catch (System.Exception ex)
                    {
                        MelonLogger.Error($"Error updating creation date for {apiUser.displayName}: {ex.Message}");
                    }
                }

                // Apply tags (including crash detection)
                if (plateStatsEnabled)
                {
                    try
                    {
                        var nameplateGroup = playerNameplateCanvas.FindChild("NameplateGroup/Nameplate");
                        if (nameplateGroup != null)
                        {
                            var quickStats = nameplateGroup.FindChild("Contents/Quick Stats");
                            if (quickStats != null)
                            {
                                ApplyTagsToNameplate(apiUser.id, statsData.userTags ?? new List<string>(), quickStats, nameplateGroup);
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        MelonLogger.Error($"Error applying tags for {apiUser.displayName}: {ex.Message}");
                    }
                }
            }

            MelonLogger.Msg($"Successfully refreshed: {apiUser.displayName}");
            yield return "success";
        }

        private static IEnumerator ForceUpdateAllVisualsCoroutine()
        {
            MelonLogger.Msg("Force updating all visual elements...");

            foreach (var statsData in playerStats.ToList())
            {
                var player = GetPlayerById(statsData.userId);
                if (player == null) continue;

                // Force update stats components visibility
                if (statsData.statsComponents != null)
                {
                    foreach (var component in statsData.statsComponents)
                    {
                        try
                        {
                            if (component != null && component.gameObject != null)
                            {
                                component.gameObject.SetActive(plateStatsEnabled);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            MelonLogger.Warning($"Error updating stats component visibility: {ex.Message}");
                        }
                    }
                }

                // Force update creation date component visibility
                try
                {
                    if (statsData.creationDateComponent != null && statsData.creationDateComponent.gameObject != null)
                    {
                        statsData.creationDateComponent.gameObject.SetActive(creationDateEnabled);
                    }

                    if (statsData.creationDatePlate != null && statsData.creationDatePlate.gameObject != null)
                    {
                        statsData.creationDatePlate.gameObject.SetActive(creationDateEnabled);
                    }
                }
                catch (System.Exception ex)
                {
                    MelonLogger.Warning($"Error updating creation date visibility: {ex.Message}");
                }

                // Force refresh the text content
                if (plateStatsEnabled)
                {
                    try
                    {
                        UpdateSinglePlayerStats(player, statsData);
                    }
                    catch (System.Exception ex)
                    {
                        MelonLogger.Warning($"Error updating player stats: {ex.Message}");
                    }
                }

                if (creationDateEnabled)
                {
                    try
                    {
                        UpdateCreationDateDisplay(player, statsData);
                    }
                    catch (System.Exception ex)
                    {
                        MelonLogger.Warning($"Error updating creation date display: {ex.Message}");
                    }
                }

                yield return null; // Allow frame processing
            }

            MelonLogger.Msg("Visual update completed");
        }

        private static void RestartBackgroundProcesses()
        {
            try
            {
                MelonLogger.Msg("Restarting background processes...");

                // Reset auto-refresh timer
                lastRefreshTime = Time.time;

                // Re-enable auto refresh if it was enabled
                if (autoRefreshEnabled)
                {
                    MelonLogger.Msg("Auto-refresh restarted");
                }

                // Restart join date fetching for players without dates
                if (creationDateEnabled)
                {
                    var playersNeedingDates = playerStats.Where(s =>
                        !s.joinDate.HasValue &&
                        !s.isDateFetching &&
                        s.creationDateComponent != null
                    ).ToList();

                    if (playersNeedingDates.Count > 0)
                    {
                        MelonLogger.Msg($"Restarting date fetching for {playersNeedingDates.Count} players");

                        Task.Run(async () =>
                        {
                            foreach (var statsData in playersNeedingDates)
                            {
                                try
                                {
                                    await FetchUserJoinDate(statsData.userId);
                                    await Task.Delay(200); // Small delay between requests
                                }
                                catch (System.Exception ex)
                                {
                                    MelonLogger.Warning($"Error fetching date for {statsData.userId}: {ex.Message}");
                                }
                            }
                        });
                    }
                }

                MelonLogger.Msg("Background processes restarted");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error restarting background processes: {ex.Message}");
            }
        }

        private static void LogSystemStatus()
        {
            try
            {
                MelonLogger.Msg("=== CURRENT SYSTEM STATUS ===");
                MelonLogger.Msg($"Plate Stats Enabled: {plateStatsEnabled}");
                MelonLogger.Msg($"Creation Date Enabled: {creationDateEnabled}");
                MelonLogger.Msg($"Crash Detection Enabled: {crashDetectionEnabled}");
                MelonLogger.Msg($"Custom Background Enabled: {customBackgroundEnabled}");
                MelonLogger.Msg($"Auto Refresh Enabled: {autoRefreshEnabled}");
                MelonLogger.Msg($"Total Players Tracked: {playerStats.Count}");
                MelonLogger.Msg($"Client Users Cached: {clientUsers.Count}");

                if (creationDateEnabled)
                {
                    int playersWithDates = playerStats.Count(s => s.joinDate.HasValue);
                    int playersCurrentlyFetching = playerStats.Count(s => s.isDateFetching);
                    MelonLogger.Msg($"Players with Join Dates: {playersWithDates}/{playerStats.Count}");
                    MelonLogger.Msg($"Players Currently Fetching: {playersCurrentlyFetching}");
                }

                if (crashDetectionEnabled)
                {
                    int crashedPlayers = playerStats.Count(s => PhotonPatches.HasPlayerCrashed(s.userId));
                    MelonLogger.Msg($"Currently Crashed Players: {crashedPlayers}");
                }

                MelonLogger.Msg("=== END STATUS ===");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error logging system status: {ex.Message}");
            }
        }

        // Additional helper methods for specific refresh scenarios

        /// <summary>
        /// Quick refresh that only updates text content without rebuilding components
        /// </summary>
        public static void QuickRefreshTextOnly()
        {
            try
            {
                MelonLogger.Msg("Starting quick text-only refresh...");

                foreach (var statsData in playerStats.ToList())
                {
                    try
                    {
                        var player = GetPlayerById(statsData.userId);
                        if (player == null) continue;

                        if (plateStatsEnabled)
                        {
                            UpdateSinglePlayerStats(player, statsData);
                        }

                        if (creationDateEnabled)
                        {
                            UpdateCreationDateDisplay(player, statsData);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        MelonLogger.Warning($"Error in quick refresh for user {statsData.userId}: {ex.Message}");
                    }
                }

                MelonLogger.Msg($"Quick refresh completed for {playerStats.Count} players");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error in quick refresh: {ex.Message}");
            }
        }

        /// <summary>
        /// Refresh only the visual styling (colors, backgrounds) without rebuilding plates
        /// </summary>
        public static void RefreshStylingOnly()
        {
            try
            {
                MelonLogger.Msg("Refreshing styling only...");
                MelonCoroutines.Start(RefreshStylingOnlyCoroutine());
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error starting styling refresh: {ex.Message}");
            }
        }

        private static IEnumerator RefreshStylingOnlyCoroutine()
        {
            if (PlayerManager.prop_PlayerManager_0?.field_Private_List_1_Player_0 == null)
                yield break;

            var players = PlayerManager.prop_PlayerManager_0.field_Private_List_1_Player_0.ToArray();

            foreach (var player in players)
            {
                if (player?.field_Private_APIUser_0?.id == null) continue;

                try
                {
                    var nameplateContainer = player._vrcplayer.field_Public_GameObject_0;
                    var playerNameplateCanvas = nameplateContainer.transform.FindChild("PlayerNameplate/Canvas");

                    if (playerNameplateCanvas != null && customBackgroundEnabled)
                    {
                        Rank rank = GetPlayerRank(player.field_Private_APIUser_0);
                        ApplyRankColoring(playerNameplateCanvas, rank);
                    }
                }
                catch (System.Exception ex)
                {
                    MelonLogger.Warning($"Error refreshing styling for player: {ex.Message}");
                }

                yield return new WaitForSeconds(0.02f);
            }

            MelonLogger.Msg("Styling refresh completed");
        }

        /// <summary>
        /// Force refresh join dates for all players (useful when API becomes available again)
        /// </summary>
        public static void ForceRefreshAllJoinDates()
        {
            try
            {
                if (!creationDateEnabled)
                {
                    MelonLogger.Warning("Creation date feature is disabled, cannot refresh join dates");
                    return;
                }

                MelonLogger.Msg("Force refreshing all join dates...");

                var playersToRefresh = playerStats.Where(s => s.creationDateComponent != null).ToList();

                Task.Run(async () =>
                {
                    foreach (var statsData in playersToRefresh)
                    {
                        try
                        {
                            // Reset fetching flags and cached dates
                            var index = playerStats.FindIndex(s => s.userId == statsData.userId);
                            if (index != -1)
                            {
                                var updatedData = playerStats[index];
                                updatedData.isDateFetching = false;
                                updatedData.joinDate = null;
                                playerStats[index] = updatedData;
                            }

                            await FetchUserJoinDate(statsData.userId);
                            await Task.Delay(300); // Slightly longer delay for force refresh
                        }
                        catch (System.Exception ex)
                        {
                            MelonLogger.Warning($"Error force refreshing date for {statsData.userId}: {ex.Message}");
                        }
                    }
                });

                MelonLogger.Msg($"Initiated force join date refresh for {playersToRefresh.Count} players");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error in force refresh join dates: {ex.Message}");
            }
        }
    }
}