using ExitGames.Client.Photon;
using Odium.Components;
using Odium;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;

public static class ChatboxAntis
{
    private static readonly HashSet<string> blockedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "spam", "crash", "lag", "nigger", "faggot", "gang"
    };

    private static readonly Dictionary<int, DateTime> lastMessageTime = new Dictionary<int, DateTime>();
    private static readonly Dictionary<int, int> messageCount = new Dictionary<int, int>();

    public static bool IsMessageValid(string message, int senderId = -1)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        if (message.Length > 200)
        {
            return false;
        }

        if (message.Length < 2)
        {
            return false;
        }

        if (UnicodeValidator.Sanitize(message))
        {
            return false;
        }

        if (ContainsBlockedWords(message))
        {
            return false;
        }

        if (senderId != -1 && !PassesRateLimit(senderId))
        {
            return false;
        }

        if (IsRepeatedCharacterSpam(message))
        {
            return false;
        }

        return true;
    }

    private static bool ContainsBlockedWords(string message)
    {
        foreach (string blockedWord in blockedWords)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(message,
                $@"\b{System.Text.RegularExpressions.Regex.Escape(blockedWord)}\b",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                OdiumConsole.Log("ChatBox", $"Found blocked word: '{blockedWord}' in message: '{message}'", LogLevel.Debug);
                return true;
            }
        }
        return false;
    }

    private static bool PassesRateLimit(int senderId)
    {
        DateTime now = DateTime.Now;

        var keysToRemove = lastMessageTime.Where(kvp => (now - kvp.Value).TotalMinutes > 1).Select(kvp => kvp.Key).ToList();
        foreach (var key in keysToRemove)
        {
            lastMessageTime.Remove(key);
            messageCount.Remove(key);
        }

        if (lastMessageTime.ContainsKey(senderId))
        {
            var timeSinceLastMessage = now - lastMessageTime[senderId];
            if (timeSinceLastMessage.TotalSeconds < 2)
            {
                OdiumConsole.Log("ChatBox", $"Rate limit: Too fast (User {senderId})", LogLevel.Debug);
                return false;
            }

            if (messageCount.ContainsKey(senderId) && messageCount[senderId] >= 10)
            {
                OdiumConsole.Log("ChatBox", $"Rate limit: Too many messages (User {senderId})", LogLevel.Debug);
                return false;
            }
        }

        lastMessageTime[senderId] = now;
        messageCount[senderId] = messageCount.ContainsKey(senderId) ? messageCount[senderId] + 1 : 1;
        return true;
    }

    private static bool IsRepeatedCharacterSpam(string message)
    {
        for (int i = 0; i < message.Length - 4; i++)
        {
            char currentChar = message[i];
            int count = 1;

            for (int j = i + 1; j < message.Length && j < i + 10; j++)
            {
                if (message[j] == currentChar)
                    count++;
                else
                    break;
            }

            if (count > 5)
                return true;
        }
        return false;
    }
}