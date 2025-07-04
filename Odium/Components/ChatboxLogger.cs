using Odium.UX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class ChatboxLogger
{
    private static readonly Dictionary<int, DateTime> lastLogTime = new Dictionary<int, DateTime>();
    private static readonly Dictionary<int, int> blockedCount = new Dictionary<int, int>();
    private static readonly Dictionary<int, int> sessionBlockedCount = new Dictionary<int, int>();
    private static readonly TimeSpan logCooldown = TimeSpan.FromSeconds(30);

    public static void LogBlockedMessage(int senderId, string playerName)
    {
        DateTime now = DateTime.Now;

        if (blockedCount.ContainsKey(senderId))
            blockedCount[senderId]++;
        else
            blockedCount[senderId] = 1;

        if (sessionBlockedCount.ContainsKey(senderId))
            sessionBlockedCount[senderId]++;
        else
            sessionBlockedCount[senderId] = 1;

        bool shouldLog = false;
        if (!lastLogTime.ContainsKey(senderId))
        {
            shouldLog = true;
        }
        else if ((now - lastLogTime[senderId]) >= logCooldown)
        {
            shouldLog = true;
        }

        if (shouldLog)
        {
            int totalBlocked = blockedCount[senderId];
            int sessionBlocked = sessionBlockedCount[senderId];
            string message;

            if (totalBlocked == 1)
            {
                message = $"<color=#31BCF0>[ChatBox]:</color> <color=red>Blocked message from {playerName} (ID: {senderId})</color>";
            }
            else if (sessionBlocked == 1)
            {
                message = $"<color=#31BCF0>[ChatBox]:</color> <color=red>Blocked message from {playerName} (ID: {senderId}) - Total blocked: {totalBlocked}</color>";
            }
            else
            {
                message = $"<color=#31BCF0>[ChatBox]:</color> <color=red>Blocked {sessionBlocked} messages from {playerName} (ID: {senderId}) - Total blocked: {totalBlocked}</color>";
            }

            InternalConsole.LogIntoConsole(message);
            lastLogTime[senderId] = now;
            sessionBlockedCount[senderId] = 0;
        }

        if (UnityEngine.Random.Range(0, 100) < 5)
        {
            CleanupOldEntries();
        }
    }

    public static string PrintByteArray(byte[] bytes)
    {
        var sb = new StringBuilder("");
        foreach (var b in bytes)
        {
            sb.Append(b.ToString("X2")).Append(" ");
        }
        return sb.ToString().TrimEnd();
    }

    public static string ConvertBytesToText(byte[] bytes)
    {
        try
        {
            string result = Encoding.UTF8.GetString(bytes).TrimEnd('\0');

            result = new string(result.Where(c => !char.IsControl(c) || char.IsWhiteSpace(c)).ToArray());

            if (string.IsNullOrWhiteSpace(result))
            {
                return "[Empty or whitespace-only message]";
            }

            return result;
        }
        catch (Exception ex)
        {
            return $"[Invalid Encoding: {ex.Message}]";
        }
    }

    public static void CleanupOldEntries()
    {
        DateTime now = DateTime.Now;
        var keysToRemove = lastLogTime.Where(kvp => (now - kvp.Value).TotalMinutes > 5).Select(kvp => kvp.Key).ToList();

        foreach (var key in keysToRemove)
        {
            lastLogTime.Remove(key);
            blockedCount.Remove(key);
            sessionBlockedCount.Remove(key);
        }
    }

    // Optional: Manual cleanup method you can call
    public static void ResetCounts()
    {
        lastLogTime.Clear();
        blockedCount.Clear();
        sessionBlockedCount.Clear();
    }
}