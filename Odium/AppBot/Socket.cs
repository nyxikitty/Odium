using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using System.Threading;
using Il2CppSystem.Threading.Tasks;
using Odium.Modules;

namespace Odium.ApplicationBot
{
    class SocketConnection
    {
        private static readonly int botCount = 8;
        private static Socket clientSocket;

        public static void SendCommandToClients(string Command)
        {
            OdiumConsole.LogGradient($"BotServer", $"[{DateTime.Now}] Sending Message ({Command})");
            ServerHandlers.Where(s => s != null).ToList().ForEach(s => s.Send(Encoding.ASCII.GetBytes(Command)));
        }

        public static void SendMessageToServer(string message)
        {
            if (clientSocket != null && clientSocket.Connected)
            {
                try
                {
                    byte[] data = Encoding.ASCII.GetBytes(message);
                    clientSocket.Send(data);
                    OdiumConsole.LogGradient($"BotClient", $"[{DateTime.Now}] Sent Message to Server ({message})");
                }
                catch (Exception e)
                {
                    OdiumConsole.LogException(e);
                }
            }
        }

        public static void OnClientReceiveCommand(string Command)
        {
            OdiumConsole.LogGradient($"BotServer", $"[{DateTime.Now}] Received Message ({Command})");
            Bot.ReceiveCommand(Command);
        }

        public static void OnServerReceiveMessage(string message, Socket clientSocket)
        {
            OdiumConsole.LogGradient($"BotServer", $"[{DateTime.Now}] Received from Bot ({message})");

            if (message.StartsWith("WORLD_JOINED:"))
            {
                string[] parts = message.Split(':');
                if (parts.Length >= 3)
                {
                    string botName = parts[1];
                    string worldName = parts[2];
                    OdiumConsole.LogGradient($"BotServer", $"Bot {botName} joined world {worldName}");

                    MainThreadDispatcher.Enqueue(() =>
                    {
                        OdiumBottomNotification.ShowNotification($"[<color=#7A00FE>Bot</color>] <color=#FC7C93>{botName}</color> joined <color=#00FE9C>{worldName}</color>");
                    });
                }
            }
            else if (message.StartsWith("BOT_STATUS:"))
            {
                string[] parts = message.Split(':');
                if (parts.Length >= 3)
                {
                    string botId = parts[1];
                    string status = parts[2];
                    OdiumConsole.LogGradient($"BotServer", $"Bot {botId} status: {status}");
                }
            }
        }

        private static List<Socket> ServerHandlers = new List<Socket>();

        public static void StartServer()
        {
            ServerHandlers.Clear();
            System.Threading.Tasks.Task.Run(HandleServer);
        }

        private static void HandleServer()
        {
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
            try
            {
                Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEndPoint);
                listener.Listen(10);
                OdiumConsole.LogGradient("BotServer", $"Waiting for {botCount} connections...");

                for (int i = 0; i < botCount; i++)
                {
                    Socket handler = listener.Accept();
                    ServerHandlers.Add(handler);

                    System.Threading.Tasks.Task.Run(() => HandleClientMessages(handler));
                }
            }
            catch (Exception e)
            {
                OdiumConsole.LogException(e);
            }
        }

        private static void HandleClientMessages(Socket clientHandler)
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (clientHandler.Connected)
                {
                    int bytesReceived = clientHandler.Receive(buffer);
                    if (bytesReceived > 0)
                    {
                        string message = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
                        OnServerReceiveMessage(message, clientHandler);
                    }
                }
            }
            catch (Exception e)
            {
                OdiumConsole.LogGradient("BotServer", $"Client disconnected or error: {e.Message}");
            }
        }

        public static void Client()
        {
            System.Threading.Tasks.Task.Run(HandleClient);
        }

        private static void HandleClient()
        {
            byte[] bytes = new byte[1024];
            OdiumConsole.LogGradient("BotServer", $"Connecting to server!");
            try
            {
                IPHostEntry host = Dns.GetHostEntry("localhost");
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);
                clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    clientSocket.Connect(remoteEP);
                    OdiumConsole.LogGradient("BotServer", $"Socket connected to {clientSocket.RemoteEndPoint.ToString()}");

                    for (; ; )
                    {
                        int bytesRec = clientSocket.Receive(bytes);
                        if (bytesRec > 0)
                        {
                            OnClientReceiveCommand(Encoding.ASCII.GetString(bytes, 0, bytesRec));
                        }
                    }
                }
                catch (ArgumentNullException ane)
                {
                    OdiumConsole.LogException(ane);
                }
                catch (SocketException se)
                {
                    OdiumConsole.LogException(se);
                }
                catch (Exception e)
                {
                    OdiumConsole.LogException(e);
                }
            }
            catch (Exception e)
            {
                OdiumConsole.LogException(e);
            }
        }
    }
}