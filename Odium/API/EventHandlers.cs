using ExitGames.Client.Photon;
using Odium.ButtonAPI.QM;
using Odium.Components;
using Odium.Modules;
using Odium.Odium;
using Odium.Threadding;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;

namespace Odium.API
{
    public static class EventHandlers
    {
        public static EnterpriseWebSocketClient client;

        // Admin announcement settings
        public static bool ShowAnnouncements { get; set; } = true;
        public static bool ShowHighPriorityOnly { get; set; } = false;

        public static void Initialize()
        {
            client = new EnterpriseWebSocketClient("wss://odiumvrc.com");

            // Connection configuration
            client.MaxReconnectAttempts = 5;
            client.InitialReconnectDelay = TimeSpan.FromSeconds(2);
            client.MaxReconnectDelay = TimeSpan.FromMinutes(1);
            client.UseExponentialBackoff = true;

            // Reconnection events
            client.OnReconnecting += attempt => OdiumConsole.Log("ODIUM", $"Reconnecting... attempt {attempt}");
            client.OnReconnected += () => OdiumConsole.Log("ODIUM", "Successfully reconnected!");
            client.OnReconnectFailed += () => OdiumConsole.Log("ODIUM", "Reconnection failed after max attempts");

            // Enhanced connection event with new parameters
            client.OnConnected += (msg, clientId, serverVersion) =>
            {
                OdiumConsole.Log("ODIUM", $"Connected: {msg}");
                if (!string.IsNullOrEmpty(clientId))
                {
                    OdiumConsole.Log("ODIUM", $"Client ID: {clientId}, Server Version: {serverVersion}");
                }
                MainThreadDispatcher.Enqueue(() =>
                {
                    DebugUI.isConnectedToServer = true;
                });
            };

            // Active count from manual requests
            client.OnActiveCountReceived += (connections, rooms, timestamp) => {
                MainThreadDispatcher.Enqueue(() =>
                {
                    DebugUI.activeRoomCount = rooms;
                    DebugUI.cachedOdiumUsers = connections;

                    QM.activeUsersLabel.SetText($"Active Users: {connections}");
                    QM.activeRoomsLabel.SetText($"Active Rooms: {rooms}");

                    Console.WriteLine($"Manual request - Connections: {connections}, Rooms: {rooms}, Time: {timestamp}");
                });
            };

            // Enhanced server info with metrics support
            client.OnServerInfoReceived += (connections, rooms, timestamp, metrics) => {
                MainThreadDispatcher.Enqueue(() =>
                {
                    DebugUI.activeRoomCount = rooms;
                    DebugUI.cachedOdiumUsers = connections;

                    QM.activeUsersLabel.SetText($"Active Users: {connections}");
                    QM.activeRoomsLabel.SetText($"Active Rooms: {rooms}");

                    Console.WriteLine($"Server info - Connections: {connections}, Rooms: {rooms}, Time: {timestamp}");
                });
            };

            // Ping response handling
            client.OnPongReceived += (roundTripMs) => {
                DebugUI.msResponse = roundTripMs;
                MainThreadDispatcher.Enqueue(() =>
                {
                    QM.pingLabel.SetText($"Ping: {roundTripMs} ms");
                });
            };

            // Room events
            client.OnJoined += (msg) => OdiumConsole.Log("ODIUM", $"Joined: {msg}");

            // Enhanced audio communication with timestamps
            client.OnUSpeakReceived += (base64Data, sender, timestamp) => {
                MainThreadDispatcher.Enqueue(() =>
                {
                    if (AssignedVariables.conduit)
                    {
                        string senderStr = sender?.ToString() ?? "unknown";
                        OdiumConsole.Log("USPEAK", base64Data.Length > 100
                            ? $"Received large audio data from {senderStr}"
                            : $"Received audio data from {senderStr}: {base64Data.Substring(0, Math.Min(50, base64Data.Length))}...");

                        if (AssignedVariables.clientTalk)
                        {
                            try
                            {
                                byte[] audioBytes = Convert.FromBase64String(base64Data);
                                EventData eventData = new EventData
                                {
                                    Code = 1,
                                    customData = Serializer.FromManagedToIL2CPP<Il2CppSystem.Object>(audioBytes),
                                    sender = GetSenderAsInt(sender),
                                    Sender = GetSenderAsInt(sender),
                                };
                                PhotonNetwork.field_Public_Static_LoadBalancingClient_0.OnEvent(eventData);
                            }
                            catch (Exception ex)
                            {
                                OdiumConsole.Log("ERROR", $"Failed to process audio data: {ex.Message}");
                            }
                        }
                    }
                });
            };

            // Enhanced movement handling (new feature)
            client.OnMovementReceived += (base64Data, sender, timestamp) => {
                MainThreadDispatcher.Enqueue(() =>
                {
                    if (AssignedVariables.conduit)
                    {
                        string senderStr = sender?.ToString() ?? "unknown";
                        OdiumConsole.Log("MOVEMENT", $"Received movement data from {senderStr}");

                        if (AssignedVariables.proxyMovement)
                        {
                            try
                            {
                                byte[] audioBytes = Convert.FromBase64String(base64Data);
                                EventData eventData = new EventData
                                {
                                    Code = 12,
                                    customData = Serializer.FromManagedToIL2CPP<Il2CppSystem.Object>(audioBytes),
                                    sender = GetSenderAsInt(sender),
                                    Sender = GetSenderAsInt(sender),
                                };
                                PhotonNetwork.field_Public_Static_LoadBalancingClient_0.OnEvent(eventData);
                            }
                            catch (Exception ex)
                            {
                                OdiumConsole.Log("ERROR", $"Failed to process audio data: {ex.Message}");
                            }
                        }
                    }
                });
            };

            // Enhanced portal handling with timestamps
            client.OnPortalDropped += (base64Data, sender, timestamp) => {
                MainThreadDispatcher.Enqueue(() =>
                {
                    if (AssignedVariables.conduit)
                    {
                        string senderStr = sender?.ToString() ?? "unknown";
                        OdiumConsole.Log("PORTAL", base64Data.Length > 100
                            ? $"Received large portal data from {senderStr}"
                            : $"Received portal data from {senderStr}: {base64Data.Substring(0, Math.Min(50, base64Data.Length))}...");

                        if (AssignedVariables.proxyPortals)
                        {
                            try
                            {
                                byte[] portalData = Convert.FromBase64String(base64Data);
                                EventData eventData = new EventData
                                {
                                    Code = 74,
                                    customData = Serializer.FromManagedToIL2CPP<Il2CppSystem.Object>(portalData),
                                    sender = GetSenderAsInt(sender),
                                    Sender = GetSenderAsInt(sender),
                                };
                                PhotonNetwork.field_Public_Static_LoadBalancingClient_0.OnEvent(eventData);
                            }
                            catch (Exception ex)
                            {
                                OdiumConsole.Log("ERROR", $"Failed to process portal data: {ex.Message}");
                            }
                        }
                    }
                });
            };

            // NEW: Admin announcement handling
            client.OnAnnouncementReceived += (message, priority, category, timestamp, broadcastId, expiresAt) => {
                MainThreadDispatcher.Enqueue(() =>
                {
                    HandleAdminAnnouncement(message, priority, category, timestamp, broadcastId, expiresAt);
                });
            };

            // Enhanced error handling with error codes
            client.OnError += (message, errorCode) => {
                OdiumConsole.Log("ERROR", $"[{errorCode}] {message}");

                // Handle specific error cases
                switch (errorCode)
                {
                    case "RATE_LIMITED":
                        OdiumConsole.Log("WARNING", "Message rate limit exceeded. Slowing down...");
                        break;
                    case "NOT_CONNECTED":
                        MainThreadDispatcher.Enqueue(() =>
                        {
                            DebugUI.isConnectedToServer = false;
                        });
                        break;
                    case "DATA_TOO_LARGE":
                        OdiumConsole.Log("WARNING", "Data size too large. Consider compressing or splitting the data.");
                        break;
                }
            };

            // Rate limiting notifications
            client.OnRateLimited += (message) => {
                OdiumConsole.Log("WARNING", $"Rate limited: {message}");
            };

            // Disconnect handling
            client.OnDisconnected += () => {
                OdiumConsole.Log("ODIUM", "Disconnected from server");
                MainThreadDispatcher.Enqueue(() =>
                {
                    DebugUI.isConnectedToServer = false;
                });
            };

            // Start the connection
            client.ConnectAsync();
        }

        /// <summary>
        /// Handle incoming admin announcements
        /// </summary>
        private static void HandleAdminAnnouncement(string message, string priority, string category, DateTime timestamp, string broadcastId, DateTime? expiresAt)
        {
            // Check if announcements are enabled
            if (!ShowAnnouncements)
                return;

            // Check priority filter
            if (ShowHighPriorityOnly && priority != "high" && priority != "urgent")
                return;

            // Check if announcement has expired
            if (expiresAt.HasValue && DateTime.UtcNow > expiresAt.Value)
                return;

            // Log the announcement with appropriate emoji based on priority
            string priorityPrefix = priority.ToUpper() == "HIGH" ? "🔸" :
                       priority.ToUpper() == "URGENT" ? "🔴" :
                       "📢";

            OdiumConsole.Log("ANNOUNCEMENT", $"{priorityPrefix} [{category.ToUpper()}] {message}");
            OdiumBottomNotification.ShowNotification($"{priorityPrefix} [{category.ToUpper()}] {message}");
            // Optional: Display in UI or play sounds based on priority
            switch (priority.ToLower())
            {
                case "urgent":
                    // Handle urgent announcements (could flash UI, play loud sound, etc.)
                    Console.WriteLine($"🔴 URGENT ANNOUNCEMENT: {message}");
                    break;
                case "high":
                    // Handle high priority announcements
                    Console.WriteLine($"🔸 HIGH PRIORITY: {message}");
                    break;
                default:
                    // Handle normal announcements
                    Console.WriteLine($"📢 INFO: {message}");
                    break;
            }
        }

        /// <summary>
        /// Convert sender object to int for backward compatibility with existing Photon integration
        /// </summary>
        private static int GetSenderAsInt(object sender)
        {
            if (sender == null) return -1;

            if (sender is int intSender)
                return intSender;

            if (int.TryParse(sender.ToString(), out int parsedSender))
                return parsedSender;

            // Use hash code as fallback for string senders
            return Math.Abs(sender.ToString().GetHashCode());
        }

        /// <summary>
        /// Send a manual ping request to measure latency
        /// </summary>
        public static async Task PingServer()
        {
            if (client?.IsConnected == true)
            {
                await client.SendPingAsync();
            }
        }

        /// <summary>
        /// Request current active counts from server
        /// </summary>
        public static async Task RequestActiveCount()
        {
            if (client?.IsConnected == true)
            {
                await client.RequestActiveCountAsync();
            }
        }

        /// <summary>
        /// Join a specific room
        /// </summary>
        public static async Task JoinRoom(string roomId)
        {
            if (client?.IsConnected == true && !string.IsNullOrEmpty(roomId))
            {
                await client.JoinRoomAsync(roomId);
            }
        }

        /// <summary>
        /// Leave current room
        /// </summary>
        public static async Task LeaveRoom()
        {
            if (client?.IsConnected == true)
            {
                await client.LeaveRoomAsync();
            }
        }

        /// <summary>
        /// Send audio data to other clients in the current room
        /// </summary>
        public static async Task SendAudio(byte[] audioData, object sender)
        {
            if (client?.IsConnected == true && audioData != null && audioData.Length > 0)
            {
                await client.SendUSpeakAsync(audioData, sender);
            }
        }

        /// <summary>
        /// Send movement data to other clients in the current room
        /// </summary>
        public static async Task SendMovement(byte[] movementData, object sender)
        {
            if (client?.IsConnected == true && movementData != null && movementData.Length > 0)
            {
                await client.SendMovementAsync(movementData, sender);
            }
        }

        /// <summary>
        /// Drop a portal with data for other clients
        /// </summary>
        public static async Task DropPortal(byte[] portalData, object sender)
        {
            if (client?.IsConnected == true && portalData != null && portalData.Length > 0)
            {
                await client.DropPortalAsync(portalData, sender);
            }
        }

        /// <summary>
        /// Clean shutdown of the WebSocket client
        /// </summary>
        public static async Task Shutdown()
        {
            if (client != null)
            {
                await client.DisconnectAsync();
                client.Dispose();
                client = null;
            }
        }
    }
}