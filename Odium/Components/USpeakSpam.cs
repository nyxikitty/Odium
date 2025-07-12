using ExitGames.Client.Photon;
using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRC.SDKBase;

namespace Odium.Components
{
    class USpeakSpam
    {
        public static bool isEnabled = false;
        public static void ToggleUSpeakSpam(bool state)
        {
            isEnabled = state;
            if (state)
            {
                MelonCoroutines.Start(USpeakSpamLoop());
            }
            else
            {
                MelonCoroutines.Stop(USpeakSpamLoop());
            }
        }
        public static IEnumerator USpeakSpamLoop()
        {
            while (isEnabled)
            {
                byte[] array = Convert.FromBase64String("QwYAAEWhESjzrD0A+PTUA4+bi+0LaUxtDTCBf75zt9hhu0RMSn256S+Z5UFCa3TTpz7Vn+dqmsK22eM1c3QV2OkEnvb+V/VMgfSsPQD49NQ67K6//enjQ8caLBaso6feWZyjV1q6GQ09u6w6bw91CJzBBv8QxGNMEa8S0ZHgYsGLpNZYHzhn03iA9aw9APj0cbv6WD3sl6rbmZYvfDksrFMhDuaBoQeYWfXNDDFik9egcVcvAPfocJkwpJ7vRPS5QgCfiNUdn/AGbIH2rD0A+PTUNLlMaIau6JuUEFFVYpv/yWOVDLSshOI1mmUB9ujkr8KEmIu3keB87DekOFGRmaNgu8TWVvVXjTLogPesPQD49HQVoW8ADMH2KouFZ8eZB3tv/2X+ld6MklOeIE7HE+cY+m1QEkeUgdM0Fc+vQi5ZI21+sAEnmaXx1WqB");

                // This is for older packets, remove the first 4 bytes because they handle actorNr on the server now
                byte[] trimmedArray = new byte[array.Length - 4];
                Buffer.BlockCopy(array, 4, trimmedArray, 0, array.Length - 4);

                byte[] bytes = BitConverter.GetBytes(Networking.GetServerTimeInMilliseconds());
                Buffer.BlockCopy(bytes, 0, trimmedArray, 0, 4);

                RaiseEventOptions raiseEventOptions = new RaiseEventOptions
                {
                    field_Public_EventCaching_0 = 0,
                    field_Public_ReceiverGroup_0 = 0
                };
                PhotonExtensions.OpRaiseEvent(1, trimmedArray, raiseEventOptions, default(SendOptions));
                yield return new WaitForSecondsRealtime(0.1f);
            }
            yield break;
        }
    }
}
