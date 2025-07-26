using MelonLoader;
using Odium.API;
using System;
using UnityEngine;

namespace Odium.Core
{
    public static class PingLoop
    {
        private static float pingTimer = 0f;
        private const float PING_INTERVAL = 3f; // 3 seconds
        private static bool isEnabled = false;

        /// <summary>
        /// Starts the ping loop
        /// </summary>
        public static void Start()
        {
            isEnabled = true;
            pingTimer = 0f;
            OdiumConsole.Log("PingLoop", "Ping loop started");
        }

        /// <summary>
        /// Stops the ping loop
        /// </summary>
        public static void Stop()
        {
            isEnabled = false;
            OdiumConsole.Log("PingLoop", "Ping loop stopped");
        }

        /// <summary>
        /// Sets the ping interval in seconds
        /// </summary>
        public static void SetInterval(float seconds)
        {
            // You can modify this if you want dynamic interval support
            // For now, it's fixed at 3 seconds as requested
            OdiumConsole.Log("PingLoop", $"Ping interval change requested to {seconds}s (currently fixed at {PING_INTERVAL}s)");
        }

        /// <summary>
        /// Call this from your main mod's OnUpdate method
        /// </summary>
        public static void Update()
        {
            if (!isEnabled)
                return;

            // Increment timer by the time since last frame
            pingTimer += Time.deltaTime;

            // Check if the interval has passed
            if (pingTimer >= PING_INTERVAL)
            {
                // Reset timer
                pingTimer = 0f;

                // Send the ping
                SendPing();
            }
        }

        /// <summary>
        /// Sends a ping to the server
        /// </summary>
        private static void SendPing()
        {
            try
            {
                if (EventHandlers.client != null)
                {
                    EventHandlers.PingServer();
                    OdiumConsole.Log("PingLoop", "Ping sent successfully");
                }
                else
                {
                    OdiumConsole.Log("PingLoop", "Cannot send ping - client is null");
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("PingLoop", $"Error sending ping: {ex.Message}");
            }
        }

        /// <summary>
        /// Forces an immediate ping (useful for testing)
        /// </summary>
        public static void SendImmediatePing()
        {
            OdiumConsole.Log("PingLoop", "Sending immediate ping");
            SendPing();
        }

        /// <summary>
        /// Gets the current status of the ping loop
        /// </summary>
        public static bool IsRunning => isEnabled;

        /// <summary>
        /// Gets the time until next ping
        /// </summary>
        public static float TimeUntilNextPing => isEnabled ? (PING_INTERVAL - pingTimer) : -1f;
    }
}