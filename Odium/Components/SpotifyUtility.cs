using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using MelonLoader;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using Microsoft.Win32;
using Odium;

namespace Odium.Modules
{
    [Serializable]
    public class MediaInfo
    {
        public string trackName = "";
        public string artistName = "";
        public string albumName = "";
        public string currentTime = "0:00";
        public string totalTime = "0:00";
        public float progress = 0f;
        public Texture2D albumArt = null;
        public bool isPlaying = false;
        public bool isAvailable = false;
        public string mediaApp = "";
    }

    public static class WindowsMediaController
    {
        #region Win32 API Imports

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("psapi.dll")]
        private static extern uint GetModuleBaseName(IntPtr hProcess, IntPtr hModule, StringBuilder lpBaseName, uint nSize);

        [DllImport("winmm.dll")]
        private static extern int mixerGetNumDevs();

        [DllImport("winmm.dll")]
        private static extern int mixerOpen(out IntPtr phmx, uint uMxId, uint dwCallback, uint dwInstance, uint fdwOpen);

        [DllImport("winmm.dll")]
        private static extern int mixerClose(IntPtr hmx);

        [DllImport("winmm.dll")]
        private static extern int mixerGetLineInfo(IntPtr hmxobj, ref MIXERLINE pmxl, uint fdwInfo);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private const uint PROCESS_QUERY_INFORMATION = 0x0400;
        private const uint PROCESS_VM_READ = 0x0010;

        // Windows messages for media keys
        private const uint WM_APPCOMMAND = 0x319;
        private const uint APPCOMMAND_MEDIA_NEXTTRACK = 11;
        private const uint APPCOMMAND_MEDIA_PREVIOUSTRACK = 12;
        private const uint APPCOMMAND_MEDIA_STOP = 13;
        private const uint APPCOMMAND_MEDIA_PLAY_PAUSE = 14;

        [StructLayout(LayoutKind.Sequential)]
        private struct MIXERLINE
        {
            public uint cbStruct;
            public uint dwDestination;
            public uint dwSource;
            public uint dwLineID;
            public uint fdwLine;
            public uint dwUser;
            public uint dwComponentType;
            public uint cChannels;
            public uint cConnections;
            public uint cControls;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string szShortName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szName;
            public uint dwType;
            public uint dwDeviceID;
            public uint dwManufacturerID;
            public uint dwProductID;
            public uint dwDriverVersion;
        }

        #endregion

        private static MediaInfo _currentMediaInfo = new MediaInfo();
        private static bool _isInitialized = false;
        private static object _updateCoroutine = null;
        private static List<string> _knownMediaApps = new List<string>();
        private static Dictionary<string, ProcessMediaInfo> _processCache = new Dictionary<string, ProcessMediaInfo>();

        // Events
        public static event Action<MediaInfo> OnMediaInfoChanged;
        public static event Action<bool> OnPlaybackStateChanged;
        public static event Action<Texture2D> OnAlbumArtChanged;

        private struct ProcessMediaInfo
        {
            public string processName;
            public string windowTitle;
            public IntPtr hwnd;
            public uint processId;
            public DateTime lastUpdate;
        }

        static WindowsMediaController()
        {
            // Known media applications
            _knownMediaApps.AddRange(new string[]
            {
                "spotify", "chrome", "firefox", "msedge", "vlc", "wmplayer", "groove",
                "iTunes", "musicbee", "foobar2000", "winamp", "aimp", "mediamonkey",
                "potplayer", "kmplayer", "gom", "youtube music", "discord", "steam"
            });
        }

        public static void Initialize()
        {
            if (_isInitialized)
            {
                OdiumConsole.Log("WindowsMediaController", "Already initialized", LogLevel.Info);
                return;
            }

            try
            {
                OdiumConsole.Log("WindowsMediaController", "Initializing Windows Media Controller...", LogLevel.Info);
                _isInitialized = true;

                // Start update loop
                if (_updateCoroutine != null)
                    MelonCoroutines.Stop(_updateCoroutine);
                _updateCoroutine = MelonCoroutines.Start(UpdateLoop());

                OdiumConsole.LogGradient("WindowsMediaController", "Successfully initialized Windows Media Controller!", LogLevel.Info, true);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "WindowsMediaController.Initialize");
                _isInitialized = false;
            }
        }

        private static IEnumerator UpdateLoop()
        {
            while (_isInitialized)
            {
                yield return new WaitForSeconds(2f); // Update every 2 seconds

                try
                {
                    ScanForMediaApplications();
                    UpdateMediaInfo();
                }
                catch (Exception ex)
                {
                    OdiumConsole.LogException(ex, "UpdateLoop");
                }
            }
        }

        private static void ScanForMediaApplications()
        {
            try
            {
                _processCache.Clear();
                EnumWindows(EnumWindowCallback, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "ScanForMediaApplications");
            }
        }

        private static bool EnumWindowCallback(IntPtr hWnd, IntPtr lParam)
        {
            try
            {
                if (!IsWindowVisible(hWnd))
                    return true;

                int length = GetWindowTextLength(hWnd);
                if (length == 0)
                    return true;

                StringBuilder windowText = new StringBuilder(length + 1);
                GetWindowText(hWnd, windowText, windowText.Capacity);
                string title = windowText.ToString();

                if (string.IsNullOrEmpty(title))
                    return true;

                GetWindowThreadProcessId(hWnd, out uint processId);

                IntPtr processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, processId);
                if (processHandle == IntPtr.Zero)
                    return true;

                StringBuilder processName = new StringBuilder(256);
                GetModuleBaseName(processHandle, IntPtr.Zero, processName, (uint)processName.Capacity);
                CloseHandle(processHandle);

                string procName = processName.ToString().ToLower();

                // Check if this is a known media application
                bool isMediaApp = false;
                foreach (string mediaApp in _knownMediaApps)
                {
                    if (procName.Contains(mediaApp.ToLower()) || title.ToLower().Contains(mediaApp.ToLower()))
                    {
                        isMediaApp = true;
                        break;
                    }
                }

                // Also check for common media indicators in window title
                if (!isMediaApp)
                {
                    string[] mediaIndicators = { "♫", "♪", "🎵", "🎶", "►", "⏸", "⏯", "⏭", "⏮", " - ", "now playing", "♬" };
                    foreach (string indicator in mediaIndicators)
                    {
                        if (title.Contains(indicator))
                        {
                            isMediaApp = true;
                            break;
                        }
                    }
                }

                if (isMediaApp && !string.IsNullOrEmpty(title) && title.Length > 3)
                {
                    ProcessMediaInfo mediaInfo = new ProcessMediaInfo
                    {
                        processName = procName,
                        windowTitle = title,
                        hwnd = hWnd,
                        processId = processId,
                        lastUpdate = DateTime.Now
                    };

                    string key = $"{procName}_{processId}";
                    _processCache[key] = mediaInfo;
                }
            }
            catch (Exception ex)
            {
                // Ignore individual window enumeration errors
            }

            return true;
        }

        private static void UpdateMediaInfo()
        {
            try
            {
                ProcessMediaInfo bestMatch = FindBestMediaProcess();

                if (bestMatch.processName != null)
                {
                    ParseMediaInfoFromTitle(bestMatch);
                    _currentMediaInfo.isAvailable = true;
                    _currentMediaInfo.mediaApp = bestMatch.processName;
                }
                else
                {
                    // No media found, clear info
                    if (_currentMediaInfo.isAvailable)
                    {
                        _currentMediaInfo = new MediaInfo();
                        OnMediaInfoChanged?.Invoke(_currentMediaInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "UpdateMediaInfo");
            }
        }

        private static ProcessMediaInfo FindBestMediaProcess()
        {
            ProcessMediaInfo bestMatch = new ProcessMediaInfo();
            int bestScore = 0;

            foreach (var kvp in _processCache)
            {
                ProcessMediaInfo info = kvp.Value;
                int score = 0;

                // Prioritize certain applications
                if (info.processName.Contains("spotify")) score += 10;
                else if (info.processName.Contains("chrome") || info.processName.Contains("firefox") || info.processName.Contains("msedge")) score += 8;
                else if (info.processName.Contains("vlc") || info.processName.Contains("wmplayer")) score += 7;
                else if (info.processName.Contains("groove") || info.processName.Contains("itunes")) score += 6;
                else score += 5;

                // Check for media indicators in title
                string title = info.windowTitle.ToLower();
                if (title.Contains(" - ")) score += 3;
                if (title.Contains("♫") || title.Contains("♪") || title.Contains("🎵")) score += 3;
                if (title.Contains("now playing")) score += 5;
                if (title.Contains("►") || title.Contains("⏸") || title.Contains("⏯")) score += 2;

                // Avoid generic titles
                if (title.Contains("new tab") || title.Contains("blank") || title == info.processName) score -= 5;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMatch = info;
                }
            }

            return bestMatch;
        }

        private static void ParseMediaInfoFromTitle(ProcessMediaInfo processInfo)
        {
            try
            {
                string title = processInfo.windowTitle;
                string oldTrackName = _currentMediaInfo.trackName;
                string oldArtistName = _currentMediaInfo.artistName;
                bool infoChanged = false;

                // Different parsing strategies based on application
                if (processInfo.processName.Contains("spotify"))
                {
                    ParseSpotifyTitle(title, ref infoChanged);
                }
                else if (processInfo.processName.Contains("chrome") || processInfo.processName.Contains("firefox") || processInfo.processName.Contains("msedge"))
                {
                    ParseBrowserTitle(title, ref infoChanged);
                }
                else if (processInfo.processName.Contains("vlc"))
                {
                    ParseVLCTitle(title, ref infoChanged);
                }
                else
                {
                    ParseGenericTitle(title, ref infoChanged);
                }

                // Simulate playback detection (very basic)
                _currentMediaInfo.isPlaying = !title.ToLower().Contains("paused") &&
                                           !title.ToLower().Contains("stopped") &&
                                           _currentMediaInfo.isAvailable;

                // Generate fake progress for demo (you'd need more advanced methods for real progress)
                if (_currentMediaInfo.isPlaying)
                {
                    UpdateFakeProgress();
                }

                if (infoChanged)
                {
                    OnMediaInfoChanged?.Invoke(_currentMediaInfo);

                    // Try to load album art (basic attempt)
                    LoadAlbumArtFromCache();
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "ParseMediaInfoFromTitle");
            }
        }

        private static void ParseSpotifyTitle(string title, ref bool infoChanged)
        {
            // Spotify format: "Artist - Song" or "Spotify"
            if (title.Contains(" - ") && title != "Spotify")
            {
                string[] parts = title.Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    string newArtist = parts[0].Trim();
                    string newTrack = parts[1].Trim();

                    if (_currentMediaInfo.artistName != newArtist)
                    {
                        _currentMediaInfo.artistName = newArtist;
                        infoChanged = true;
                    }

                    if (_currentMediaInfo.trackName != newTrack)
                    {
                        _currentMediaInfo.trackName = newTrack;
                        infoChanged = true;
                    }
                }
            }
            else if (title == "Spotify")
            {
                // Spotify is open but not playing
                if (_currentMediaInfo.isAvailable)
                {
                    _currentMediaInfo = new MediaInfo { mediaApp = "spotify" };
                    infoChanged = true;
                }
            }
        }

        private static void ParseBrowserTitle(string title, ref bool infoChanged)
        {
            // Browser format varies: "Song - Artist - YouTube" or "Artist - Song | YouTube Music"
            string[] separators = { " - ", " | ", " • ", " · " };

            foreach (string separator in separators)
            {
                if (title.Contains(separator))
                {
                    string[] parts = title.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        // Filter out browser-specific parts
                        List<string> relevantParts = new List<string>();
                        foreach (string part in parts)
                        {
                            string cleanPart = part.Trim().ToLower();
                            if (!cleanPart.Contains("youtube") && !cleanPart.Contains("spotify") &&
                                !cleanPart.Contains("google chrome") && !cleanPart.Contains("firefox") &&
                                !cleanPart.Contains("edge") && !cleanPart.Contains("music") &&
                                cleanPart.Length > 2)
                            {
                                relevantParts.Add(part.Trim());
                            }
                        }

                        if (relevantParts.Count >= 2)
                        {
                            string newTrack = relevantParts[0];
                            string newArtist = relevantParts[1];

                            if (_currentMediaInfo.trackName != newTrack)
                            {
                                _currentMediaInfo.trackName = newTrack;
                                infoChanged = true;
                            }

                            if (_currentMediaInfo.artistName != newArtist)
                            {
                                _currentMediaInfo.artistName = newArtist;
                                infoChanged = true;
                            }
                            break;
                        }
                    }
                }
            }
        }

        private static void ParseVLCTitle(string title, ref bool infoChanged)
        {
            // VLC format: "filename - VLC media player" or just "filename"
            if (title.Contains(" - VLC"))
            {
                string filename = title.Substring(0, title.IndexOf(" - VLC")).Trim();
                ParseFilename(filename, ref infoChanged);
            }
            else if (!title.Equals("VLC media player", StringComparison.OrdinalIgnoreCase))
            {
                ParseFilename(title, ref infoChanged);
            }
        }

        private static void ParseGenericTitle(string title, ref bool infoChanged)
        {
            // Try common separators
            string[] separators = { " - ", " | ", " • ", " · ", " : " };

            foreach (string separator in separators)
            {
                if (title.Contains(separator))
                {
                    string[] parts = title.Split(new string[] { separator }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        string newArtist = parts[0].Trim();
                        string newTrack = parts[1].Trim();

                        if (_currentMediaInfo.artistName != newArtist)
                        {
                            _currentMediaInfo.artistName = newArtist;
                            infoChanged = true;
                        }

                        if (_currentMediaInfo.trackName != newTrack)
                        {
                            _currentMediaInfo.trackName = newTrack;
                            infoChanged = true;
                        }
                        return;
                    }
                }
            }

            // If no separator found, treat as track name
            if (_currentMediaInfo.trackName != title)
            {
                _currentMediaInfo.trackName = title;
                _currentMediaInfo.artistName = "Unknown Artist";
                infoChanged = true;
            }
        }

        private static void ParseFilename(string filename, ref bool infoChanged)
        {
            // Remove file extension
            if (filename.Contains("."))
            {
                filename = Path.GetFileNameWithoutExtension(filename);
            }

            // Try to parse artist - title from filename
            if (filename.Contains(" - "))
            {
                string[] parts = filename.Split(new string[] { " - " }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    if (_currentMediaInfo.artistName != parts[0].Trim())
                    {
                        _currentMediaInfo.artistName = parts[0].Trim();
                        infoChanged = true;
                    }

                    if (_currentMediaInfo.trackName != parts[1].Trim())
                    {
                        _currentMediaInfo.trackName = parts[1].Trim();
                        infoChanged = true;
                    }
                    return;
                }
            }

            // Just use filename as track name
            if (_currentMediaInfo.trackName != filename)
            {
                _currentMediaInfo.trackName = filename;
                _currentMediaInfo.artistName = "Unknown Artist";
                infoChanged = true;
            }
        }

        private static void UpdateFakeProgress()
        {
            // This is a very basic fake progress simulation
            // In reality, you'd need more advanced techniques to get real progress
            if (_currentMediaInfo.progress >= 1.0f)
            {
                _currentMediaInfo.progress = 0f;
            }
            else
            {
                _currentMediaInfo.progress += 0.01f; // Increment by 1% each update
            }

            int totalSeconds = 180; // Assume 3 minute song
            int currentSeconds = (int)(_currentMediaInfo.progress * totalSeconds);

            _currentMediaInfo.currentTime = FormatTime(currentSeconds);
            _currentMediaInfo.totalTime = FormatTime(totalSeconds);
        }

        private static void LoadAlbumArtFromCache()
        {
            // Very basic album art loading - you could enhance this
            // to search for images in music folders, use APIs, etc.
            try
            {
                // For now, just create a placeholder colored texture
                if (_currentMediaInfo.albumArt == null)
                {
                    CreatePlaceholderAlbumArt();
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "LoadAlbumArtFromCache");
            }
        }

        private static void CreatePlaceholderAlbumArt()
        {
            try
            {
                // Create a simple colored texture as placeholder
                Texture2D texture = new Texture2D(256, 256);
                Color color = GetColorFromString(_currentMediaInfo.trackName + _currentMediaInfo.artistName);

                for (int x = 0; x < texture.width; x++)
                {
                    for (int y = 0; y < texture.height; y++)
                    {
                        texture.SetPixel(x, y, color);
                    }
                }

                texture.Apply();

                if (_currentMediaInfo.albumArt != null)
                {
                    UnityEngine.Object.Destroy(_currentMediaInfo.albumArt);
                }

                _currentMediaInfo.albumArt = texture;
                OnAlbumArtChanged?.Invoke(texture);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "CreatePlaceholderAlbumArt");
            }
        }

        private static Color GetColorFromString(string input)
        {
            if (string.IsNullOrEmpty(input)) return Color.gray;

            int hash = input.GetHashCode();
            UnityEngine.Random.InitState(hash);

            return new Color(
                UnityEngine.Random.Range(0.3f, 0.8f),
                UnityEngine.Random.Range(0.3f, 0.8f),
                UnityEngine.Random.Range(0.3f, 0.8f),
                1f
            );
        }

        private static string FormatTime(int seconds)
        {
            int minutes = seconds / 60;
            int remainingSeconds = seconds % 60;
            return $"{minutes}:{remainingSeconds:D2}";
        }

        #region Public API Methods

        public static MediaInfo GetCurrentMediaInfo()
        {
            return _currentMediaInfo;
        }

        public static bool IsInitialized()
        {
            return _isInitialized;
        }

        public static bool IsMediaAvailable()
        {
            return _isInitialized && _currentMediaInfo.isAvailable;
        }

        public static string GetTrackName()
        {
            return _currentMediaInfo.trackName;
        }

        public static string GetArtistName()
        {
            return _currentMediaInfo.artistName;
        }

        public static string GetAlbumName()
        {
            return _currentMediaInfo.albumName;
        }

        public static string GetCurrentTime()
        {
            return _currentMediaInfo.currentTime;
        }

        public static string GetTotalTime()
        {
            return _currentMediaInfo.totalTime;
        }

        public static float GetProgress()
        {
            return _currentMediaInfo.progress;
        }

        public static Texture2D GetAlbumArt()
        {
            return _currentMediaInfo.albumArt;
        }

        public static bool IsPlaying()
        {
            return _currentMediaInfo.isPlaying;
        }

        public static Sprite GetAlbumArtSprite()
        {
            if (_currentMediaInfo.albumArt != null)
            {
                return Sprite.Create(_currentMediaInfo.albumArt,
                    new Rect(0, 0, _currentMediaInfo.albumArt.width, _currentMediaInfo.albumArt.height),
                    Vector2.one * 0.5f);
            }
            return null;
        }

        public static string GetMediaAppName()
        {
            return _currentMediaInfo.mediaApp;
        }

        #endregion

        #region Media Control Methods

        public static void PlayPause()
        {
            try
            {
                // Send media key to system
                IntPtr hwnd = FindWindow("Shell_TrayWnd", null);
                SendMessage(hwnd, WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)APPCOMMAND_MEDIA_PLAY_PAUSE << 16));
                OdiumConsole.Log("WindowsMediaController", "Sent play/pause command", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "PlayPause");
            }
        }

        public static void NextTrack()
        {
            try
            {
                IntPtr hwnd = FindWindow("Shell_TrayWnd", null);
                SendMessage(hwnd, WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)APPCOMMAND_MEDIA_NEXTTRACK << 16));
                OdiumConsole.Log("WindowsMediaController", "Sent next track command", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "NextTrack");
            }
        }

        public static void PreviousTrack()
        {
            try
            {
                IntPtr hwnd = FindWindow("Shell_TrayWnd", null);
                SendMessage(hwnd, WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)APPCOMMAND_MEDIA_PREVIOUSTRACK << 16));
                OdiumConsole.Log("WindowsMediaController", "Sent previous track command", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "PreviousTrack");
            }
        }

        public static void Stop()
        {
            try
            {
                IntPtr hwnd = FindWindow("Shell_TrayWnd", null);
                SendMessage(hwnd, WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)APPCOMMAND_MEDIA_STOP << 16));
                OdiumConsole.Log("WindowsMediaController", "Sent stop command", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "Stop");
            }
        }

        #endregion

        #region Utility Methods

        public static void RefreshMediaInfo()
        {
            if (_isInitialized)
            {
                ScanForMediaApplications();
                UpdateMediaInfo();
            }
        }

        public static void Cleanup()
        {
            try
            {
                if (_updateCoroutine != null)
                {
                    MelonCoroutines.Stop(_updateCoroutine);
                    _updateCoroutine = null;
                }

                if (_currentMediaInfo.albumArt != null)
                {
                    UnityEngine.Object.Destroy(_currentMediaInfo.albumArt);
                    _currentMediaInfo.albumArt = null;
                }

                _processCache.Clear();
                _currentMediaInfo = new MediaInfo();
                _isInitialized = false;

                OdiumConsole.Log("WindowsMediaController", "Cleaned up Windows Media Controller", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "Cleanup");
            }
        }

        public static void DebugCurrentSession()
        {
            try
            {
                OdiumConsole.Log("WindowsMediaController", "=== Current Session Debug Info ===", LogLevel.Info);
                OdiumConsole.Log("WindowsMediaController", $"Initialized: {_isInitialized}", LogLevel.Info);
                OdiumConsole.Log("WindowsMediaController", $"Media Available: {_currentMediaInfo.isAvailable}", LogLevel.Info);
                OdiumConsole.Log("WindowsMediaController", $"Media App: {_currentMediaInfo.mediaApp}", LogLevel.Info);
                OdiumConsole.Log("WindowsMediaController", $"Track: {_currentMediaInfo.trackName}", LogLevel.Info);
                OdiumConsole.Log("WindowsMediaController", $"Artist: {_currentMediaInfo.artistName}", LogLevel.Info);
                OdiumConsole.Log("WindowsMediaController", $"Album: {_currentMediaInfo.albumName}", LogLevel.Info);
                OdiumConsole.Log("WindowsMediaController", $"Time: {_currentMediaInfo.currentTime} / {_currentMediaInfo.totalTime}", LogLevel.Info);
                OdiumConsole.Log("WindowsMediaController", $"Progress: {_currentMediaInfo.progress:F2}", LogLevel.Info);
                OdiumConsole.Log("WindowsMediaController", $"Playing: {_currentMediaInfo.isPlaying}", LogLevel.Info);
                OdiumConsole.Log("WindowsMediaController", $"Album Art: {(_currentMediaInfo.albumArt != null ? "Available" : "None")}", LogLevel.Info);
                OdiumConsole.Log("WindowsMediaController", $"Detected Processes: {_processCache.Count}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "DebugCurrentSession");
            }
        }

        #endregion
    }
}