using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Odium.Patches
{
    public class PhotonDebugger
    {
        public static bool IsOnEventSendDebug = false;
        public static bool OnEventSent(byte code, object data, RaiseEventOptions options, SendOptions sendOptions)
        {
            System.Console.WriteLine("Photon:OnEventSent" + $"----------------------");
            System.Console.WriteLine("Photon:OnEventSent" + $"Code:{code}");
            System.Console.WriteLine("Photon:OnEventSent" + $"Data:{data}");
            System.Console.WriteLine("Photon:OnEventSent" + $"Data:{data}");
            System.Console.WriteLine("Photon:OnEventSent" + $"RaiseEventOptions:{options}");
            System.Console.WriteLine("Photon:OnEventSent" + $"SendOptions:{sendOptions}");
            System.Console.WriteLine("Photon:OnEventSent" + $"----------------------");
            return true;
        }
    }
}