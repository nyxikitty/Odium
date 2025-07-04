using ExitGames.Client.Photon;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odium.Components
{
    class Chatbox
    {
        public static void SendCustomChatMessage(string message)
        {
            try
            {
                PhotonExtensions.OpRaiseEvent(43, message, new RaiseEventOptions
                {
                    field_Public_EventCaching_0 = EventCaching.DoNotCache,
                    field_Public_ReceiverGroup_0 = ReceiverGroup.Others
                }, default(SendOptions));

                OdiumConsole.LogGradient("PhotonEvent", $"Sent custom message: {message}");
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("PhotonEvent", $"Failed to send custom message: {ex.Message}", LogLevel.Error);
            }
        }
    }
}
