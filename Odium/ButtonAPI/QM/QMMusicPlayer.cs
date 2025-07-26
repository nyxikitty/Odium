using System;
using System.Collections;
using System.IO;
using MelonLoader;
using Odium.Components;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Drawing;
using System.Linq;

namespace Odium.Modules
{
    public static class OdiumMusicPlayer
    {
        private static AssetBundle _musicPlayerBundle;
        private static GameObject _musicPlayerPrefab;
        private static GameObject _musicPlayerInstance;
        private static bool _isInitialized = false;

        private static RawImage _albumArt;
        private static TextMeshProUGUI _trackName;
        private static TextMeshProUGUI _artistName;
        private static TextMeshProUGUI _currentTrackTime;
        private static Slider _songProgress;
        private static Button _playButton;
        private static Button _rewindButton;

        // Add fields for image conversion
        private static byte[] cachedBytes;
        private static MultiStageLoadingJob currentJob;
        private static int[] imageSize = { 256, 256 }; // Default image size, adjust as needed

        private static readonly string MusicPlayerBundlePath = Path.Combine(Components.ModSetup.GetOdiumFolderPath(), "AssetBundles", "odiummusicplayer");

        public static void Initialize()
        {
            if (_isInitialized)
            {
                OdiumConsole.Log("MusicPlayerLoader", "Already initialized, skipping...", LogLevel.Info);
                return;
            }

            OdiumConsole.Log("MusicPlayerLoader", "Starting music player initialization...", LogLevel.Info);
            LoadMusicPlayer();
        }

        private static void LoadMusicPlayer()
        {
            try
            {
                if (!File.Exists(MusicPlayerBundlePath))
                {
                    OdiumConsole.Log("MusicPlayerLoader", $"Music player bundle file not found at: {MusicPlayerBundlePath}", LogLevel.Error);
                    return;
                }

                OdiumConsole.Log("MusicPlayerLoader", "Loading AssetBundle from file...", LogLevel.Info);
                _musicPlayerBundle = AssetBundle.LoadFromFile(MusicPlayerBundlePath);
                if (_musicPlayerBundle == null)
                {
                    OdiumConsole.Log("MusicPlayerLoader", "Failed to load AssetBundle!", LogLevel.Error);
                    return;
                }

                OdiumConsole.Log("MusicPlayerLoader", "AssetBundle loaded successfully!", LogLevel.Info);

                string[] assetNames = _musicPlayerBundle.GetAllAssetNames();
                OdiumConsole.Log("MusicPlayerLoader", $"Found {assetNames.Length} assets in bundle:", LogLevel.Info);
                foreach (string assetName in assetNames)
                {
                    OdiumConsole.Log("MusicPlayerLoader", $"  - {assetName}", LogLevel.Info);
                }

                string[] possibleNames = { "MusicPlayer", "musicplayer", "assets/musicplayer.prefab", "musicplayer.prefab" };

                foreach (string name in possibleNames)
                {
                    OdiumConsole.Log("MusicPlayerLoader", $"Trying to load prefab with name: '{name}'", LogLevel.Info);
                    _musicPlayerPrefab = _musicPlayerBundle.LoadAsset<GameObject>(name);
                    if (_musicPlayerPrefab != null)
                    {
                        OdiumConsole.Log("MusicPlayerLoader", $"Successfully loaded prefab with name: '{name}'", LogLevel.Info);
                        break;
                    }
                }

                if (_musicPlayerPrefab == null)
                {
                    OdiumConsole.Log("MusicPlayerLoader", "Standard names failed, trying to find any GameObject...", LogLevel.Warning);
                    foreach (string assetName in assetNames)
                    {
                        GameObject asset = _musicPlayerBundle.LoadAsset<GameObject>(assetName);
                        if (asset != null)
                        {
                            _musicPlayerPrefab = asset;
                            OdiumConsole.Log("MusicPlayerLoader", $"Using GameObject asset: {assetName}", LogLevel.Info);
                            break;
                        }
                    }
                }

                if (_musicPlayerPrefab == null)
                {
                    OdiumConsole.Log("MusicPlayerLoader", "Failed to load any GameObject from AssetBundle!", LogLevel.Error);
                    _musicPlayerBundle.Unload(true);
                    _musicPlayerBundle = null;
                    return;
                }

                _musicPlayerBundle.Unload(false);
                _musicPlayerBundle = null;

                CreateMusicPlayerInstance();

                _isInitialized = true;
                OdiumConsole.LogGradient("MusicPlayerLoader", "Music player system initialized successfully!", LogLevel.Info, true);
                OdiumConsole.Log("MusicPlayerLoader", $"Prefab reference: {_musicPlayerPrefab.name}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "LoadMusicPlayer");
                _isInitialized = false;
                _musicPlayerPrefab = null;
                if (_musicPlayerBundle != null)
                {
                    _musicPlayerBundle.Unload(true);
                    _musicPlayerBundle = null;
                }
            }
        }

        private static void ConvertImage(byte[] input)
        {
            if (input == cachedBytes) return;
            if (input == null)
            {
                cachedBytes = null;
                // Set default/fallback image if needed
                return;
            }
            if (cachedBytes != null && cachedBytes.SequenceEqual(input)) return;

            try
            {
                var ms = new MemoryStream(input);
                var image = System.Drawing.Image.FromStream(ms);
                var bmp = new System.Drawing.Bitmap(image);

                int width, height;
                if (image.Width == image.Height)
                {
                    width = imageSize[0];
                    height = imageSize[1];
                }
                else if (image.Width > image.Height)
                {
                    width = imageSize[0];
                    height = image.Height * imageSize[1] / image.Width;
                }
                else
                {
                    width = image.Width * imageSize[0] / image.Height;
                    height = imageSize[1];
                }

                bmp.SetResolution(width, height);
                Color32[,] array = new Color32[bmp.Height, bmp.Width];

                for (int i = 0; i < bmp.Height; i++)
                {
                    for (int j = 0; j < bmp.Width; j++)
                    {
                        var item = bmp.GetPixel(j, bmp.Height - 1 - i);
                        array[i, j] = new Color32(item.R, item.G, item.B, item.A);
                    }
                }

                // Create Texture2D from pixel array
                Texture2D texture2D = new Texture2D(bmp.Width, bmp.Height, TextureFormat.RGBA32, false);

                // Convert 2D array to 1D array for SetPixels32
                Color32[] pixels1D = new Color32[bmp.Width * bmp.Height];
                for (int i = 0; i < bmp.Height; i++)
                {
                    for (int j = 0; j < bmp.Width; j++)
                    {
                        pixels1D[i * bmp.Width + j] = array[i, j];
                    }
                }

                texture2D.SetPixels32(pixels1D);
                texture2D.Apply();

                currentJob = new MultiStageLoadingJob
                {
                    texture2D = texture2D,
                    pixels = array,
                    Height = bmp.Height,
                    Width = bmp.Width
                };

                // Set the album art in the music player
                SetAlbumArt(texture2D);

                cachedBytes = input;

                OdiumConsole.Log("MusicPlayerLoader", $"Album art converted and set - size: {bmp.Width}x{bmp.Height}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                cachedBytes = null;
                OdiumConsole.LogException(ex, "ConvertImage");
            }
        }

        // Public method to convert and set album art from byte array
        public static void SetAlbumArtFromBytes(byte[] imageBytes)
        {
            ConvertImage(imageBytes);
        }

        // Direct method to set album art from Unity Texture2D (bypasses conversion)
        public static void SetAlbumArtFromTexture2D(Texture2D sourceTexture)
        {
            if (sourceTexture == null) return;

            try
            {
                // Create a new texture with the same data to avoid reference issues
                Texture2D newTexture = new Texture2D(sourceTexture.width, sourceTexture.height, sourceTexture.format, false);
                newTexture.SetPixels32(sourceTexture.GetPixels32());
                newTexture.Apply();

                SetAlbumArt(newTexture);

                OdiumConsole.Log("MusicPlayerLoader", $"Album art set from Texture2D - size: {newTexture.width}x{newTexture.height}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "SetAlbumArtFromTexture2D");
            }
        }

        private static void CreateMusicPlayerInstance()
        {
            try
            {
                Transform quickMenuParent = FindQuickMenuParent();
                if (quickMenuParent == null)
                {
                    OdiumConsole.Log("MusicPlayerLoader", "Could not find QuickMenu parent! Music player will not be attached.", LogLevel.Error);
                    return;
                }

                _musicPlayerInstance = UnityEngine.Object.Instantiate(_musicPlayerPrefab, quickMenuParent);
                _musicPlayerInstance.name = "OdiumMusicPlayer";

                // Fix positioning and visibility issues
                FixPositioningAndVisibility();

                CacheUIComponents();
                SetupButtonListeners();

                OdiumConsole.Log("MusicPlayerLoader", "Music player instance created and attached to QuickMenu", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "CreateMusicPlayerInstance");
            }
        }

        private static void FixPositioningAndVisibility()
        {
            try
            {
                if (_musicPlayerInstance == null) return;

                // Set the music player to be active
                _musicPlayerInstance.SetActive(true);

                // Add or configure CanvasGroup for proper alpha and interaction
                CanvasGroup canvasGroup = _musicPlayerInstance.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = _musicPlayerInstance.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = 1.0f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;

                // Position the music player
                RectTransform rectTransform = _musicPlayerInstance.GetComponent<RectTransform>();
                if (rectTransform == null)
                {
                    rectTransform = _musicPlayerInstance.AddComponent<RectTransform>();
                }

                // Set anchoring to top-right corner and position it visibly
                rectTransform.anchorMin = new Vector2(1, 1);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.anchoredPosition = new Vector2(-200, -100); // Offset from top-right
                rectTransform.sizeDelta = new Vector2(350, 200); // Set a reasonable size

                // Ensure all child UI elements are also visible
                SetChildrenVisible(_musicPlayerInstance.transform);

                OdiumConsole.Log("MusicPlayerLoader", $"Music player positioned at: {rectTransform.anchoredPosition} with size: {rectTransform.sizeDelta}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "FixPositioningAndVisibility");
            }
        }

        private static void SetChildrenVisible(Transform parent)
        {
            try
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    Transform child = parent.GetChild(i);
                    GameObject childObj = child.gameObject;

                    // Ensure child is active
                    childObj.SetActive(true);

                    // If it has a CanvasGroup, make sure it's visible
                    CanvasGroup childCanvasGroup = childObj.GetComponent<CanvasGroup>();
                    if (childCanvasGroup != null)
                    {
                        childCanvasGroup.alpha = 1.0f;
                    }

                    // If it's a RawImage, make sure it's not transparent
                    RawImage rawImage = childObj.GetComponent<RawImage>();
                    if (rawImage != null && rawImage.color.a < 0.1f)
                    {
                        UnityEngine.Color color = rawImage.color;
                        color.a = 1.0f;
                        rawImage.color = color;
                    }

                    // If it's an Image, make sure it's not transparent
                    UnityEngine.UI.Image image = childObj.GetComponent<UnityEngine.UI.Image>();
                    if (image != null && image.color.a < 0.1f)
                    {
                        UnityEngine.Color color = image.color;
                        color.a = 1.0f;
                        image.color = color;
                    }

                    // If it's a TextMeshPro, make sure it's visible
                    TextMeshProUGUI text = childObj.GetComponent<TextMeshProUGUI>();
                    if (text != null && text.color.a < 0.1f)
                    {
                        UnityEngine.Color color = text.color;
                        color.a = 1.0f;
                        text.color = color;
                    }

                    // Recursively check children
                    SetChildrenVisible(child);
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "SetChildrenVisible");
            }
        }

        private static Transform FindQuickMenuParent()
        {
            try
            {
                GameObject[] rootObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

                foreach (GameObject obj in rootObjects)
                {
                    if (obj.name.Contains("Canvas_QuickMenu") && obj.name.Contains("Clone"))
                    {
                        Transform canvasGroup = obj.transform.Find("CanvasGroup");
                        if (canvasGroup != null)
                        {
                            Transform container = canvasGroup.Find("Container");
                            if (container != null)
                            {
                                Transform window = container.Find("Window");
                                if (window != null)
                                {
                                    Transform wingRight = window.Find("Wing_Right");
                                    if (wingRight != null)
                                    {
                                        OdiumConsole.Log("MusicPlayerLoader", $"Found QuickMenu Wing_Right: {wingRight.name}", LogLevel.Info);
                                        return wingRight;
                                    }
                                }
                            }
                        }
                    }
                }

                // Fallback: try to find the main Canvas if QuickMenu structure is different
                OdiumConsole.Log("MusicPlayerLoader", "Could not find Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/Wing_Right, trying fallback...", LogLevel.Warning);

                foreach (GameObject obj in rootObjects)
                {
                    Canvas canvas = obj.GetComponent<Canvas>();
                    if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        OdiumConsole.Log("MusicPlayerLoader", $"Using fallback canvas: {obj.name}", LogLevel.Info);
                        return obj.transform;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "FindQuickMenuParent");
                return null;
            }
        }

        private static void CacheUIComponents()
        {
            if (_musicPlayerInstance == null) return;

            try
            {
                // Look for components directly under the music player instance
                Transform root = _musicPlayerInstance.transform;

                _albumArt = root.Find("AlbumArt")?.GetComponent<RawImage>();
                _trackName = root.Find("TrackName")?.GetComponent<TextMeshProUGUI>();
                _artistName = root.Find("ArtistName")?.GetComponent<TextMeshProUGUI>();
                _currentTrackTime = root.Find("CurrentTrackTime")?.GetComponent<TextMeshProUGUI>();

                Transform songProgress = root.Find("SongProgress");
                if (songProgress != null)
                {
                    _songProgress = songProgress.GetComponent<Slider>();
                    if (_songProgress != null)
                    {
                        _songProgress.interactable = false;
                    }
                }

                _playButton = root.Find("PlayButton")?.GetComponent<Button>();
                _rewindButton = root.Find("RewindButton")?.GetComponent<Button>();

                // If not found at root level, try looking under Background
                if (_albumArt == null)
                {
                    Transform background = root.Find("Background");
                    if (background != null)
                    {
                        _albumArt = background.Find("AlbumArt")?.GetComponent<RawImage>();
                        _trackName = background.Find("TrackName")?.GetComponent<TextMeshProUGUI>();
                        _artistName = background.Find("ArtistName")?.GetComponent<TextMeshProUGUI>();
                        _currentTrackTime = background.Find("CurrentTrackTime")?.GetComponent<TextMeshProUGUI>();

                        Transform songProgressBg = background.Find("SongProgress");
                        if (songProgressBg != null)
                        {
                            _songProgress = songProgressBg.GetComponent<Slider>();
                            if (_songProgress != null)
                            {
                                _songProgress.interactable = false;
                            }
                        }

                        _playButton = background.Find("PlayButton")?.GetComponent<Button>();
                        _rewindButton = background.Find("RewindButton")?.GetComponent<Button>();
                    }
                }

                OdiumConsole.Log("MusicPlayerLoader", "UI Components cached successfully", LogLevel.Info);

                // Log what components were found for debugging
                OdiumConsole.Log("MusicPlayerLoader", $"Components found - AlbumArt: {_albumArt != null}, TrackName: {_trackName != null}, ArtistName: {_artistName != null}, CurrentTrackTime: {_currentTrackTime != null}, SongProgress: {_songProgress != null}, PlayButton: {_playButton != null}, RewindButton: {_rewindButton != null}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "CacheUIComponents");
            }
        }

        private static void SetupButtonListeners()
        {
            try
            {
                if (_playButton != null)
                {
                    _playButton.onClick.RemoveAllListeners();
                    _playButton.onClick.AddListener(new Action(() => { OnPlayButtonClicked(); }));
                }

                if (_rewindButton != null)
                {
                    _rewindButton.onClick.RemoveAllListeners();
                    _rewindButton.onClick.AddListener(new Action(() => { OnRewindButtonClicked(); }));
                }

                OdiumConsole.Log("MusicPlayerLoader", "Button listeners setup successfully", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "SetupButtonListeners");
            }
        }

        public static void SetAlbumArt(Texture2D texture)
        {
            try
            {
                OdiumConsole.Log("MusicPlayerLoader", $"SetAlbumArt called - _albumArt null: {_albumArt == null}, texture null: {texture == null}", LogLevel.Info);

                if (_albumArt != null && texture != null)
                {
                    _albumArt.texture = texture;
                    OdiumConsole.Log("MusicPlayerLoader", $"Album art updated - texture size: {texture.width}x{texture.height}", LogLevel.Info);
                }
                else
                {
                    OdiumConsole.Log("MusicPlayerLoader", "Failed to set album art - component or texture is null", LogLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "SetAlbumArt");
            }
        }

        // Keep the old Sprite method for backward compatibility, but convert Sprite to Texture2D
        public static void SetAlbumArt(Sprite sprite)
        {
            try
            {
                if (sprite != null && sprite.texture != null)
                {
                    SetAlbumArt(sprite.texture);
                }
                else
                {
                    OdiumConsole.Log("MusicPlayerLoader", "Failed to set album art - sprite or sprite texture is null", LogLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "SetAlbumArt");
            }
        }

        public static void SetTrackName(string trackName)
        {
            try
            {
                if (_trackName != null)
                {
                    _trackName.text = trackName ?? "";
                    OdiumConsole.Log("MusicPlayerLoader", $"Track name set to: {trackName}", LogLevel.Info);
                }
                else
                {
                    OdiumConsole.Log("MusicPlayerLoader", "Failed to set track name - component is null", LogLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "SetTrackName");
            }
        }

        public static void SetArtistName(string artistName)
        {
            try
            {
                if (_artistName != null)
                {
                    _artistName.text = artistName ?? "";
                    OdiumConsole.Log("MusicPlayerLoader", $"Artist name set to: {artistName}", LogLevel.Info);
                }
                else
                {
                    OdiumConsole.Log("MusicPlayerLoader", "Failed to set artist name - component is null", LogLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "SetArtistName");
            }
        }

        public static void SetCurrentTrackTime(string currentTime)
        {
            try
            {
                if (_currentTrackTime != null)
                {
                    _currentTrackTime.text = currentTime ?? "";
                    OdiumConsole.Log("MusicPlayerLoader", $"Current track time set to: {currentTime}", LogLevel.Info);
                }
                else
                {
                    OdiumConsole.Log("MusicPlayerLoader", "Failed to set current track time - component is null", LogLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "SetCurrentTrackTime");
            }
        }

        public static void SetSongProgress(float progress)
        {
            try
            {
                if (_songProgress != null)
                {
                    progress = Mathf.Clamp01(progress);
                    _songProgress.value = progress;
                    OdiumConsole.Log("MusicPlayerLoader", $"Song progress set to: {progress:F2}", LogLevel.Info);
                }
                else
                {
                    OdiumConsole.Log("MusicPlayerLoader", "Failed to set song progress - component is null", LogLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "SetSongProgress");
            }
        }

        private static void OnPlayButtonClicked()
        {
            try
            {
                OdiumConsole.LogGradient("MusicPlayerLoader", "Play button clicked!", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "OnPlayButtonClicked");
            }
        }

        private static void OnRewindButtonClicked()
        {
            try
            {
                OdiumConsole.LogGradient("MusicPlayerLoader", "Rewind button clicked!", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "OnRewindButtonClicked");
            }
        }

        public static bool IsInitialized()
        {
            bool result = _isInitialized && _musicPlayerPrefab != null && _musicPlayerInstance != null;
            OdiumConsole.Log("MusicPlayerLoader", $"IsInitialized check - _isInitialized: {_isInitialized}, _musicPlayerPrefab != null: {_musicPlayerPrefab != null}, _musicPlayerInstance != null: {_musicPlayerInstance != null}, Result: {result}", LogLevel.Info);
            return result;
        }

        public static void Show()
        {
            try
            {
                if (_musicPlayerInstance != null)
                {
                    _musicPlayerInstance.SetActive(true);
                    OdiumConsole.Log("MusicPlayerLoader", "Music player shown", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "Show");
            }
        }

        public static void Hide()
        {
            try
            {
                if (_musicPlayerInstance != null)
                {
                    _musicPlayerInstance.SetActive(false);
                    OdiumConsole.Log("MusicPlayerLoader", "Music player hidden", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "Hide");
            }
        }

        public static void SetParent(Transform newParent, bool worldPositionStays = false)
        {
            try
            {
                if (_musicPlayerInstance == null)
                {
                    OdiumConsole.Log("MusicPlayerLoader", "Cannot set parent - music player instance is null", LogLevel.Warning);
                    return;
                }

                if (newParent == null)
                {
                    OdiumConsole.Log("MusicPlayerLoader", "Cannot set parent - new parent is null", LogLevel.Warning);
                    return;
                }

                Transform oldParent = _musicPlayerInstance.transform.parent;
                _musicPlayerInstance.transform.SetParent(newParent, worldPositionStays);

                OdiumConsole.Log("MusicPlayerLoader", $"Music player parent changed from '{(oldParent != null ? oldParent.name : "null")}' to '{newParent.name}' (worldPositionStays: {worldPositionStays})", LogLevel.Info);

                // If not preserving world position, we might want to reapply positioning
                if (!worldPositionStays)
                {
                    FixPositioningAndVisibility();
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "SetParent");
            }
        }

        public static Transform GetParent()
        {
            try
            {
                if (_musicPlayerInstance != null)
                {
                    return _musicPlayerInstance.transform.parent;
                }
                return null;
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "GetParent");
                return null;
            }
        }

        public static void SetLocalPosition(Vector3 localPosition)
        {
            try
            {
                if (_musicPlayerInstance == null)
                {
                    OdiumConsole.Log("MusicPlayerLoader", "Cannot set local position - music player instance is null", LogLevel.Warning);
                    return;
                }

                Vector3 oldPosition = _musicPlayerInstance.transform.localPosition;
                _musicPlayerInstance.transform.localPosition = localPosition;

                OdiumConsole.Log("MusicPlayerLoader", $"Music player local position changed from {oldPosition} to {localPosition}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "SetLocalPosition");
            }
        }

        public static void SetAnchoredPosition(Vector2 anchoredPosition)
        {
            try
            {
                if (_musicPlayerInstance == null)
                {
                    OdiumConsole.Log("MusicPlayerLoader", "Cannot set anchored position - music player instance is null", LogLevel.Warning);
                    return;
                }

                RectTransform rectTransform = _musicPlayerInstance.GetComponent<RectTransform>();
                if (rectTransform == null)
                {
                    OdiumConsole.Log("MusicPlayerLoader", "Cannot set anchored position - no RectTransform component found", LogLevel.Warning);
                    return;
                }

                Vector2 oldPosition = rectTransform.anchoredPosition;
                rectTransform.anchoredPosition = anchoredPosition;

                OdiumConsole.Log("MusicPlayerLoader", $"Music player anchored position changed from {oldPosition} to {anchoredPosition}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "SetAnchoredPosition");
            }
        }

        public static Vector3 GetLocalPosition()
        {
            try
            {
                if (_musicPlayerInstance != null)
                {
                    return _musicPlayerInstance.transform.localPosition;
                }
                return Vector3.zero;
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "GetLocalPosition");
                return Vector3.zero;
            }
        }

        public static Vector2 GetAnchoredPosition()
        {
            try
            {
                if (_musicPlayerInstance != null)
                {
                    RectTransform rectTransform = _musicPlayerInstance.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        return rectTransform.anchoredPosition;
                    }
                }
                return Vector2.zero;
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "GetAnchoredPosition");
                return Vector2.zero;
            }
        }

        public static void Cleanup()
        {
            try
            {
                if (_musicPlayerInstance != null)
                {
                    UnityEngine.Object.Destroy(_musicPlayerInstance);
                    _musicPlayerInstance = null;
                    OdiumConsole.Log("MusicPlayerLoader", "Destroyed music player instance", LogLevel.Info);
                }

                _albumArt = null;
                _trackName = null;
                _artistName = null;
                _currentTrackTime = null;
                _songProgress = null;
                _playButton = null;
                _rewindButton = null;

                if (_musicPlayerBundle != null)
                {
                    _musicPlayerBundle.Unload(true);
                    _musicPlayerBundle = null;
                }

                _musicPlayerPrefab = null;
                _isInitialized = false;

                OdiumConsole.Log("MusicPlayerLoader", "Music player system cleaned up", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "MusicPlayerCleanup");
            }
        }
    }

    // Supporting class for image loading
    public class MultiStageLoadingJob
    {
        public Texture2D texture2D;
        public LoadingStep nextStep;
        public Color32[,] pixels;
        public int Height;
        public int Width;
    }

    public enum LoadingStep
    {
        // Add any loading steps you need
        None,
        Converting,
        Complete
    }
}