using ExitGames.Client.Photon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odium.Components
{
    class Ratelimit
    {
        private static List<byte> rateLimitedEvents = new List<byte>()
        {
            1, 33, 40, 42, 43, 44, 50, 52, 53, 60,
            4, 5, 6, 8, 16, 17, 18, 7, 19,
            12, 11, 13, 14, 15, 202, 209, 210, 21, 22,
            62, 63, 64, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 28,
            29, 30
        };

        public static void ProcessRateLimit(ref EventData eventData)
        {
            if (eventData.Code != 34) return;

            var customObj = new Dictionary<byte, object>();
            var rateLimitDict = new Dictionary<byte, int>();

            foreach (var ev in rateLimitedEvents)
                rateLimitDict.Add(ev, int.MaxValue);

            customObj.Add(0, rateLimitDict);
            customObj.Add(2, true);

            eventData.customData = Serializer.FromManagedToIL2CPP<Il2CppSystem.Object>(customObj);
        }
    }
}
