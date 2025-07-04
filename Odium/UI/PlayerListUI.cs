using TMPro;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using VRC;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;

namespace Odium.UI
{
    public class PlayerRankTextDisplay
    {
        private static GameObject canvasObject;
        private static Canvas canvas;
        private static GameObject textDisplayObject;
        private static TextMeshProUGUI textComponent;
        private static List<PlayerInfo> playerList = new List<PlayerInfo>();
        private static HashSet<string> clientUsers = new HashSet<string>();
        private static Color gradientColor1 = ColorFromHex("#D37CFE");
        private static Color gradientColor2 = ColorFromHex("#8900CE");
        private static HttpClient httpClient = new HttpClient();

        public struct PlayerInfo
        {
            public string playerName;
            public Rank rank;
            public VRC.Player player;
            public string userId;

            public PlayerInfo(string name, Rank playerRank, VRC.Player plr, string id)
            {
                playerName = name;
                rank = playerRank;
                player = plr;
                userId = id;
            }
        }

        public enum Rank
        {
            Visitor,
            NewUser,
            User,
            Known,
            Trusted
        }

        public static void Initialize()
        {
            if (canvasObject != null)
                return;

            CreateStandaloneUI();
        }

        private static void CreateStandaloneUI()
        {
            try
            {
                canvasObject = new GameObject("PlayerRankOverlayCanvas");
                UnityEngine.Object.DontDestroyOnLoad(canvasObject);

                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 999;

                var canvasScaler = canvasObject.AddComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920, 1080);
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.matchWidthOrHeight = 0f;

                canvasObject.AddComponent<GraphicRaycaster>();

                textDisplayObject = new GameObject("PlayerRankText");
                textDisplayObject.transform.SetParent(canvasObject.transform, false);

                textComponent = textDisplayObject.AddComponent<TextMeshProUGUI>();

                textComponent.text = "";
                textComponent.fontSize = 18f;
                textComponent.richText = true;
                textComponent.enableAutoSizing = false;
                textComponent.alignment = TextAlignmentOptions.TopLeft;
                textComponent.verticalAlignment = VerticalAlignmentOptions.Top;
                textComponent.color = Color.white;
                textComponent.fontStyle = FontStyles.Bold;

                RectTransform rectTransform = textDisplayObject.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0f, 1f);
                rectTransform.anchorMax = new Vector2(0f, 1f);
                rectTransform.pivot = new Vector2(0f, 1f);
                rectTransform.anchoredPosition = new Vector2(20f, -20f);
                rectTransform.sizeDelta = new Vector2(350f, 600f);

                var shadow = textDisplayObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
                shadow.effectDistance = new Vector2(2f, -2f);

                var outline = textDisplayObject.AddComponent<Outline>();
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(1f, -1f);

                var canvasGroup = textDisplayObject.AddComponent<CanvasGroup>();
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;

                MelonLoader.MelonLogger.Msg("Standalone player rank overlay created successfully");
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error($"Failed to create standalone UI: {e.Message}");
            }
        }

        public static void AddPlayer(string playerName, VRC.Core.APIUser apiUser, VRC.Player plr)
        {
            if (canvasObject == null)
                Initialize();

            try
            {
                Rank rank = GetPlayerRank(apiUser);
                string userId = apiUser?.id ?? "";
                PlayerInfo newPlayer = new PlayerInfo(playerName, rank, plr, userId);

                int existingIndex = playerList.FindIndex(p => p.playerName == playerName);
                if (existingIndex >= 0)
                {
                    playerList[existingIndex] = newPlayer;
                }
                else
                {
                    playerList.Add(newPlayer);
                }

                if (!string.IsNullOrEmpty(userId))
                {
                    CheckClientUserAsync(userId);
                }

                UpdateDisplay();
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error($"Failed to add player {playerName}: {e.Message}");
            }
        }

        private static async void CheckClientUserAsync(string userId)
        {
            try
            {
                string url = $"http://api.snoofz.net:3778/api/odium/user/exists?id={userId}";

                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    if (jsonResponse.Contains("\"exists\":true"))
                    {
                        clientUsers.Add(userId);
                        MelonLoader.MelonLogger.Msg($"Client user detected: {userId}");
                    }
                }
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error($"Error checking client user: {e.Message}");
            }
        }

        private static string GetAnimatedGradientText(string text, Color color1, Color color2, float speed = 3f, float waveLength = 1.5f)
        {
            if (string.IsNullOrEmpty(text)) return text;

            StringBuilder sb = new StringBuilder();
            float time = Time.time * speed;

            for (int i = 0; i < text.Length; i++)
            {
                float normalizedPosition = (float)i / (text.Length - 1);
                float animatedPosition = normalizedPosition + (Mathf.Sin(time + i * waveLength) * 0.5f + 0.5f) * 0.3f;
                animatedPosition = Mathf.Clamp01(animatedPosition);

                Color currentColor = Color.Lerp(color1, color2, animatedPosition);
                string hexColor = ColorToHex(currentColor);

                sb.Append($"<color={hexColor}>{text[i]}</color>");
            }

            return sb.ToString();
        }

        public static void RemovePlayer(string playerName)
        {
            try
            {
                var playerToRemove = playerList.Find(p => p.playerName == playerName);
                if (!string.IsNullOrEmpty(playerToRemove.userId))
                {
                    clientUsers.Remove(playerToRemove.userId);
                }

                playerList.RemoveAll(p => p.playerName == playerName);
                UpdateDisplay();
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error($"Failed to remove player {playerName}: {e.Message}");
            }
        }

        public static void ClearAll()
        {
            try
            {
                playerList.Clear();
                clientUsers.Clear();
                UpdateDisplay();
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error($"Failed to clear player list: {e.Message}");
            }
        }

        private static string GetPlayerPlatform(Player player)
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
                    return "<size=8>[<color=#00BFFF>PC</color>]</size>";
                case "android":
                    return "<size=8>[<color=#32CD32>QUEST</color>]</size>";
                case "ios":
                    return "<size=8>[<color=#FF69B4>iOS</color>]</size>";
                default:
                    return "<size=8>[<color=#FFFFFF>UNK</color>]</size>";
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

        private static void UpdateDisplay()
        {
            if (textComponent == null) return;

            try
            {
                string displayText = "";

                if (playerList.Count > 0)
                {
                    foreach (var player in playerList)
                    {
                        string platform = GetPlayerPlatform(player.player);
                        string platformText = GetPlatformIcon(platform);
                        bool friend = IsFriend(player.player);
                        bool adult = IsAdult(player.player);
                        bool isClientUser = clientUsers.Contains(player.userId);

                        string friendText = friend ? "<size=8><color=#FFD700>[FRIEND]</color></size>" : "";
                        string adultText = adult ? "<size=8><color=#90EE90>[18+]</color></size>" : "";

                        if (isClientUser)
                        {
                            string animatedName = GetAnimatedGradientText(player.playerName, gradientColor1, gradientColor2);
                            string clientTag = "<size=8><color=#FF1493>[CLIENT]</color></size>";
                            displayText += $"<size=12>{animatedName}</size> {clientTag} {platformText} {friendText} {adultText}\n";
                        }
                        else
                        {
                            Color rankColor = GetRankColor(player.rank);
                            string hexColor = ColorToHex(rankColor);
                            string rankName = GetRankDisplayName(player.rank);
                            displayText += $"<size=12><color={hexColor}>{player.playerName}</color></size> <size=8><color=#CCCCCC>[{rankName}]</color></size> {platformText} {friendText} {adultText}\n";
                        }
                    }
                }

                textComponent.text = displayText;
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error($"Failed to update display: {e.Message}");
            }
        }

        private static string GetRankDisplayName(Rank rank)
        {
            switch (rank)
            {
                case Rank.Visitor: return "Visitor";
                case Rank.NewUser: return "New User";
                case Rank.User: return "User";
                case Rank.Known: return "Known User";
                case Rank.Trusted: return "Trusted User";
                default: return "Unknown";
            }
        }

        private static Rank GetPlayerRank(VRC.Core.APIUser apiUser)
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

        private static Color GetRankColor(Rank rank)
        {
            switch (rank)
            {
                case Rank.Visitor:
                    return new Color(1f, 1f, 1f, 0.9f);
                case Rank.NewUser:
                    return ColorFromHex("#96ECFF", 0.9f);
                case Rank.User:
                    return ColorFromHex("#96FFA9", 0.9f);
                case Rank.Known:
                    return ColorFromHex("#FF5E50", 0.9f);
                case Rank.Trusted:
                    return ColorFromHex("#A900FE", 0.9f);
                default:
                    return new Color(1f, 1f, 1f, 0.9f);
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

        private static string ColorToHex(Color color)
        {
            int r = Mathf.RoundToInt(color.r * 255);
            int g = Mathf.RoundToInt(color.g * 255);
            int b = Mathf.RoundToInt(color.b * 255);
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        public static void SetPosition(float x, float y)
        {
            if (textDisplayObject != null)
            {
                RectTransform rectTransform = textDisplayObject.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(x, y);
            }
        }

        public static void SetFontSize(float size)
        {
            if (textComponent != null)
            {
                textComponent.fontSize = size;
            }
        }

        public static void SetVisible(bool visible)
        {
            if (canvasObject != null)
            {
                canvasObject.SetActive(visible);
            }
        }

        public static void SetOpacity(float opacity)
        {
            if (textComponent != null)
            {
                var color = textComponent.color;
                color.a = opacity;
                textComponent.color = color;
            }
        }

        public static void Destroy()
        {
            if (canvasObject != null)
            {
                UnityEngine.Object.DestroyImmediate(canvasObject);
                canvasObject = null;
                canvas = null;
                textDisplayObject = null;
                textComponent = null;
                playerList.Clear();
                clientUsers.Clear();
                httpClient?.Dispose();
                MelonLoader.MelonLogger.Msg("Player rank overlay destroyed");
            }
        }
    }
}