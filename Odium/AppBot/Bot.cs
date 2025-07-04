using MelonLoader;
using MelonLoader.TinyJSON;
using Odium.Components;
using Odium.Modules;
using Odium.Odium;
using Odium.QMPages;
using Odium.UX;
using Odium.Wrappers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.SDKBase;
using static Interop;

namespace Odium.ApplicationBot
{
    public class Bot
    {
        private const float MoveSpeed = 0.1f;
        public static int BotProfile { get; set; }
        public static string BotId { get; set; }
        public string SessionId { get; set; }
        public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastPing { get; set; } = DateTime.UtcNow;
        public static bool IsHeadlessBot { get; set; }
        public bool IsAlive { get; set; } = true;
        public static bool voiceMimic = false;
        public static bool movementMimic = false;

        public static int voiceMimicActorNr = 0;
        public static int movementMimicActorNr = 0;

        public static ApiWorld Current_World { get { return RoomManager.field_Internal_Static_ApiWorld_0; } }
        public static bool chatBoxLagger = false;

        private static Dictionary<string, Action<string, string>> Commands = new Dictionary<string, Action<string, string>>()
        {
            { "JoinWorld", (WorldID, botId) => {
                Console.WriteLine("[Client] Joining World " + WorldID);
                if (botId == BotId) {
                    if (Current_World != null)
                        Networking.GoToRoom(WorldID);
                }
            } },

            { "ToggleBlockAll", (UserID, botId) => {
                if (botId == BotId) {
                foreach (var player in PlayerWrapper.Players)
                    if (player.field_Private_APIUser_0.id != UserID)
                        player.Method_Public_Void_Boolean_0(UserID != string.Empty);
                }
            } },

            { "OrbitSelected", (UserID, botId) => {
                if (botId == BotId) {
                     OrbitTarget = UserID == string.Empty ? null : PlayerWrapper.GetVRCPlayerFromId(UserID)._player;
                }
            } },

            { "PortalSpam", (UserID, botId) => {
                if (botId == BotId) {
                    if (!voiceMimic) {
                        ActionWrapper.portalSpamPlayer = PlayerWrapper.GetVRCPlayerFromId(UserID)._player;
                        voiceMimic = true;
                    } else {
                        ActionWrapper.portalSpamPlayer = null;
                        voiceMimic = true;
                    }
                }
            } },

            { "MovementMimic", (UserID, botId) => {
                if (botId == BotId) {
                        OdiumConsole.Log("OdiumBot", $"Movement mimic called for actor -> {movementMimicActorNr} ({UserID})");
                    if (!movementMimic) {
                        movementMimic = true;
                        movementMimicActorNr = PlayerWrapper.GetLocalPlayerAPIUser(UserID).playerId;
                        OdiumConsole.Log("OdiumBot", $"Movement mimic enabled for actor -> {movementMimicActorNr} ({UserID})");
                    } else {
                        movementMimicActorNr = 0;
                        movementMimic = false;
                    }
                }
            } },

            { "ChatBoxLagger", (boolean, botId) => {
                if (botId == BotId) {
                if (!AssignedVariables.chatboxLagger)
                {
                    InternalConsole.LogIntoConsole("Chatbox lagger was enabled!");
                    AssignedVariables.chatboxLagger = true;
                    chatboxLaggerCoroutine = MelonCoroutines.Start(OptimizedChatboxLaggerCoroutine());
                } else
                {
                    InternalConsole.LogIntoConsole("Chatbox lagger was disabled!");
                    AssignedVariables.chatboxLagger = false;

                    if (chatboxLaggerCoroutine != null)
                    {
                        MelonCoroutines.Stop(chatboxLaggerCoroutine);
                        chatboxLaggerCoroutine = null;
                    }

                    preGeneratedMessages.Clear();
                    }
                }
            } },

            { "ClickTP", (Position, botId) => {
                if (PlayerWrapper.LocalPlayer != null) {
                    string[] Split = Position.Split(':');
                    float X = float.Parse(Split[0]);
                    float Y = float.Parse(Split[1]);
                    float Z = float.Parse(Split[2]);
                    PlayerWrapper.LocalPlayer.transform.position = new Vector3(X, Y, Z);
                }
            } },

            { "TeleportToPlayer", (UserID, botId) => {
                if (botId == BotId) {
                    if (PlayerWrapper.LocalPlayer != null) {
                        Networking.LocalPlayer.TeleportTo(PlayerWrapper.GetVRCPlayerFromId(UserID)._player.transform.position, PlayerWrapper.GetVRCPlayerFromId(UserID)._player.transform.rotation);
                    }
                }
            } },

            { "EventCachingDCToggle", (Enabled, botId) => {
                EventCachingDC = Enabled != string.Empty;
            } },

            { "SpinbotToggle", (Enabled, botId) => {
                if (botId == BotId) {
                 Spinbot = Enabled != string.Empty;
                }
            } },

            { "SpinbotSpeed", (Speed, botId) => {
                SpinbotSpeed = int.Parse(Speed);
            } },

            { "SetTargetFramerate", (Framerate, botId) => {
                if (int.TryParse(Framerate, out int n))
                    Application.targetFrameRate = n;
            } },
        };

        private static void MovePlayer(Vector3 pos)
        {
            if (PlayerWrapper.LocalPlayer != null)
                PlayerWrapper.LocalPlayer.transform.position += pos;
        }

        public static void ReceiveCommand(string Command)
        {
            var firstSpaceIndex = Command.IndexOf(" ");
            var CMD = Command.Substring(0, firstSpaceIndex);

            var remainingCommand = Command.Substring(firstSpaceIndex + 1);
            var secondSpaceIndex = remainingCommand.IndexOf(" ");

            if (secondSpaceIndex != -1)
            {
                var Parameters = remainingCommand.Substring(0, secondSpaceIndex);
                var Parameters2 = remainingCommand.Substring(secondSpaceIndex + 1);
                if (Parameters2.Contains(BotId))
                {
                    HandleActionOnMainThread(() => { Commands[CMD].Invoke(Parameters, Parameters2); });
                }
            }
        }

        private static void HandleActionOnMainThread(Action action)
        {
            LastActionOnMainThread = action;
        }

        private static List<string> preGeneratedMessages = new List<string>();
        private static System.Random random = new System.Random();
        private static object chatboxLaggerCoroutine = null;

        private static void PreGenerateMessages(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var chineseChars = new char[144];
                for (int j = 0; j < 144; j++)
                {
                    chineseChars[j] = (char)random.Next(0x4E00, 0x9FFF + 1);
                }
                preGeneratedMessages.Add(new string(chineseChars));
            }
        }

        private static IEnumerator OptimizedChatboxLaggerCoroutine()
        {
            while (AssignedVariables.chatboxLagger)
            {
                if (preGeneratedMessages.Count == 0)
                    PreGenerateMessages(10);

                Chatbox.SendCustomChatMessage(preGeneratedMessages[0]);
                preGeneratedMessages.RemoveAt(0);
                yield return new WaitForSecondsRealtime(0.5f);
            }
        }

        private static Player OrbitTarget;
        private static Action LastActionOnMainThread;
        private static bool EventCachingDC = false;
        private static bool Spinbot = false;
        private static int SpinbotSpeed = 20;

        public static void OnUpdate()
        {
            if (IsApplicationBot)
            {
                if (LastActionOnMainThread != null)
                {
                    LastActionOnMainThread();
                    LastActionOnMainThread = null;
                }
                HandleBotFunctions();
            }
        }

        private static IEnumerator RamClearLoop()
        {
            for (; ; )
            {
                yield return new WaitForSeconds(5f);
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
            }
        }

        private static bool EmojiSpam = false;
        private static bool WestCoastLagger = false;
        private static string _PrefabName = "";

        private static void HandleBotFunctions()
        {
            if (OrbitTarget != null && PlayerWrapper.LocalPlayer != null)
            {
                Physics.gravity = new Vector3(0, 0, 0);
                alpha += Time.deltaTime * OrbitSpeed;
                PlayerWrapper.LocalPlayer.transform.position = new Vector3(OrbitTarget.transform.position.x + a * (float)Math.Cos(alpha), OrbitTarget.transform.position.y, OrbitTarget.transform.position.z + b * (float)Math.Sin(alpha));
            }

            if (Spinbot && PlayerWrapper.LocalPlayer != null)
            {
                PlayerWrapper.LocalPlayer.transform.Rotate(new Vector3(0f, SpinbotSpeed, 0f));
            }
        }

        public static float OrbitSpeed = 5f;
        public static float alpha = 0f;
        public static float a = 1f;
        public static float b = 1f;
        public static float Range = 1f;
        public static float Height = 0f;
        public static VRCPlayer currentPlayer;
        public static Player selectedPlayer;

        public static Player LagTarget;

        public static void Start()
        {
            if (IsLaunchedAsBot())
            {
                IsApplicationBot = true;
                OdiumConsole.LogGradient("Odium", $"Running as Application Bot with assigned ID: {BotId}", LogLevel.Info);
                SocketConnection.Client();
                RamClearLoop();
                MelonCoroutines.Start(WaitForWorldJoin());
                return;
            } else
            {
                OdiumConsole.LogGradient("Odium", "Starting bot server...", LogLevel.Info);
                ApplicationBot.SocketConnection.StartServer();
            }
        }

        public static bool IsLaunchedAsBot()
        {
            try
            {
                string[] args = System.Environment.GetCommandLineArgs();

                bool isBot = args.Any(arg => arg.ToLower() == "--appbot");

                if (isBot)
                {
                    MelonLogger.Log("Found --appBot launch parameter");

                    ExtractLaunchParameters(args);
                }

                return isBot;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error checking launch parameters: {ex.Message}");
                return false;
            }
        }

        public static IEnumerator WaitForWorldJoin()
        {
            while (RoomManager.field_Private_Static_ApiWorldInstance_0 == null)
            {
                yield return new WaitForSeconds(0.5f);
            }

            try
            {
                string worldId = Current_World?.id ?? "unknown";
                string message = $"WORLD_JOINED:{PlayerWrapper.LocalPlayer.field_Private_APIUser_0.displayName}:{RoomManager.field_Internal_Static_ApiWorld_0.name}";

                SocketConnection.SendMessageToServer(message);

                OdiumConsole.LogGradient("OdiumBot", $"Notified server that bot {BotId} joined world {worldId}");

                string statusMessage = $"BOT_STATUS:{BotId}:READY";
                SocketConnection.SendMessageToServer(statusMessage);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex);
            }
        }

        public static void NotifyWorldLeave()
        {
            try
            {
                string message = $"WORLD_LEFT:{BotId}";
                SocketConnection.SendMessageToServer(message);
                OdiumConsole.LogGradient("OdiumBot", $"Notified server that bot {BotId} left world");
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex);
            }
        }

        public static void ExtractLaunchParameters(string[] args)
        {
            try
            {
                var profileArg = args.FirstOrDefault(arg => arg.StartsWith("--profile="));
                if (profileArg != null)
                {
                    string profileValue = profileArg.Split('=')[1];
                    if (int.TryParse(profileValue, out int profile))
                    {
                        BotProfile = profile;
                        MelonLogger.Log($"Bot Profile: {profile}");
                    }
                }

                var idArg = args.FirstOrDefault(arg => arg.StartsWith("--id="));
                if (idArg != null)
                {
                    BotId = idArg.Split('=')[1];
                    MelonLogger.Log($"Bot ID: {BotId}");
                }

                bool isHeadless = args.Any(arg => arg.ToLower() == "-batchmode");
                if (isHeadless)
                {
                    IsHeadlessBot = true;
                    MelonLogger.Log("Running in headless mode");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error extracting launch parameters: {ex.Message}");
            }
        }

        public static bool IsApplicationBot = false;
    }
}