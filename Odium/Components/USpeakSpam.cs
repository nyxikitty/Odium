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
    public class USpeakSpam
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
                byte[] array = Convert.FromBase64String("AAAAAGfp+Lv2GRkA+MrI08yxTwBkxqwATk9LRU0wTk9LM00wTg==");

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
                PhotonExtensions.SendLowLevelEvent(1, trimmedArray);
                yield return new WaitForSecondsRealtime(0.05f);
            }
            yield break;
        }
    }
}
