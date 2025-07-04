using System;
using Newtonsoft.Json;

namespace Odium.ApplicationBot
{
    public class BotMessage
    {
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        public string Command { get; set; }
        public string TargetBotId { get; set; } // Empty for all bots, specific ID for individual bot
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public object Parameters { get; set; }
    }

    public class BotResponse
    {
        public string MessageId { get; set; }
        public string BotId { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    // Command parameter classes for type safety
    public class JoinWorldParams
    {
        public string WorldId { get; set; }
    }

    public class TeleportParams
    {
        public string UserId { get; set; }
    }

    public class PositionParams
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    public class ToggleParams
    {
        public bool Enabled { get; set; }
    }

    public class SpeedParams
    {
        public int Speed { get; set; }
    }

    public class FramerateParams
    {
        public int Framerate { get; set; }
    }
}