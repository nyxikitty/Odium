using Odium.ButtonAPI.QM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Odium.Odium
{
    public class AssignedVariables
    {
        public static GameObject userInterface;
        public static GameObject quickMenu;
        public static bool welcomeNotificationShown = false;
        public static bool chatboxLagger = false;
        public static bool announceBlocks = false;
        public static bool announceMutes = false;
        public static bool desktopPlayerList = true;
        public static bool boneESP = false;
        public static bool autoDroneCrash = false;
        public static bool instanceLock = false;
        public static bool chatBoxAntis = true;
        public static int playerTagsCount = 0;
        public static int odiumUsersCount = 0;
        public static bool adminSpoof = false;
        public static QMNestedMenu playerList;
        public static bool preventPatreonCrash = true;
        public static bool preventM4EventFlooding = true;
        public static bool preventM4EventSpam = true;
        public static bool ratelimitM4Events = true;

    }
}
