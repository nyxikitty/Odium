using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRC.Udon;

namespace Odium.GameCheats
{
    class FTACUdonUtils
    {
        public static void SendEvent(string eventName) => GameObject.Find("Partner Button  (4)")?.GetComponent<UdonBehaviour>()?.SendCustomNetworkEvent(0, eventName);

    }
}
