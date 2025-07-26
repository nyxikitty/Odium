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
        public static bool clientTalk = false;
        public static bool proxyPortals = false;
        public static bool proxyMovement = false;
        public static bool conduit = false;
        public static bool debugUI = false;
        public static bool playerlistUI = false;
        public static bool customNameplates = false;
        public static bool plateStats = false;
        public static int playerTagsCount = 0;
        public static int odiumUsersCount = 0;
        public static bool adminSpoof = false;
        public static QMNestedMenu playerList;
        public static bool preventPatreonCrash = true;
        public static bool preventM4EventFlooding = true;
        public static bool preventM4EventSpam = true;
        public static bool ratelimitM4Events = true;

        // Debug logging toggles for Udon event filtering
        public static bool udonParamCheck = false;
        public static bool udonDictCheck = false;
        public static bool dataTypeCheck = false;
        public static bool filterViewIds = false;
        public static bool udonDataCheck = false;
    }
}