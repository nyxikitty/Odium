using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Odium.ButtonAPI.QM;
using Odium;

public class MediaControls
{
    private static readonly HttpClient httpClient = new HttpClient();
    private static bool isSpotifyPlaying = false;
    private static bool isDiscordMuted = false;
    private static bool isDiscordDeafened = false;

    [DllImport("user32.dll")]
    static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    private const int KEYEVENTF_KEYUP = 0x0002;
    private const byte VK_MEDIA_PLAY_PAUSE = 0xB3;
    private const byte VK_MEDIA_PREV_TRACK = 0xB1;
    private const byte VK_MEDIA_NEXT_TRACK = 0xB0;

    public static async Task ToggleDiscordMute()
    {
        SendDiscordHotkey("^+m");
        isDiscordMuted = !isDiscordMuted;
        OdiumConsole.Log("Discord", $"Microphone {(isDiscordMuted ? "muted" : "unmuted")}");
    }

    public static async Task ToggleDiscordDeafen()
    {
        SendDiscordHotkey("^+d"); 
        isDiscordDeafened = !isDiscordDeafened;
        OdiumConsole.Log("Discord", $"Audio {(isDiscordDeafened ? "deafened" : "undeafened")}");
    }

    public static void SendDiscordHotkey(string hotkey)
    {
        var discordProcess = Process.GetProcessesByName("Discord");
        if (discordProcess.Length > 0)
        {
            SetForegroundWindow(discordProcess[0].MainWindowHandle);
            System.Threading.Thread.Sleep(100);

            System.Windows.Forms.SendKeys.SendWait(hotkey);
        }
    }

    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    public static async Task ToggleSpotifyPlayback()
    {
        try
        {
            await SpotifyWebAPIToggle();
        }
        catch
        {
            keybd_event(VK_MEDIA_PLAY_PAUSE, 0, 0, UIntPtr.Zero);
            keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        isSpotifyPlaying = !isSpotifyPlaying;
        OdiumConsole.Log("Spotify", $"Playback {(isSpotifyPlaying ? "resumed" : "paused")}");
    }

    public static async Task SpotifyWebAPIToggle()
    {
        var accessToken = GetSpotifyAccessToken();

        if (string.IsNullOrEmpty(accessToken)) return;

        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var action = isSpotifyPlaying ? "pause" : "play";
        var response = await httpClient.PutAsync($"https://api.spotify.com/v1/me/player/{action}", null);
    }

    public static void SpotifyRewind()
    {
        keybd_event(VK_MEDIA_PREV_TRACK, 0, 0, UIntPtr.Zero);
        keybd_event(VK_MEDIA_PREV_TRACK, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        OdiumConsole.Log("Spotify", "Previous track");
    }

    public static void SpotifySkip()
    {
        keybd_event(VK_MEDIA_NEXT_TRACK, 0, 0, UIntPtr.Zero);
        keybd_event(VK_MEDIA_NEXT_TRACK, 0, VK_MEDIA_NEXT_TRACK, UIntPtr.Zero);
        OdiumConsole.Log("Spotify", "Next track");
    }

    public static void SpotifyPause()
    {
        keybd_event(VK_MEDIA_PLAY_PAUSE, 0, 0, UIntPtr.Zero);
        keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        OdiumConsole.Log("Spotify", "Paused");
    }

    private static string GetSpotifyAccessToken()
    {
        return null;
    }
}