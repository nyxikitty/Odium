using Odium.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase;
using static Interop;

namespace Odium.Wrappers
{
    internal class PlayerWrapper
    {
        public static List<VRC.Player> Players = new List<VRC.Player>();
        public static VRC.Player LocalPlayer = null;
        public static int ActorId = 0;
        public static VRC.Player[] GetAllPlayers() => PlayerManager.prop_PlayerManager_0.field_Private_List_1_Player_0.ToArray();

        public static UnityEngine.Vector3 GetPosition(VRC.Player player)
        {
            var TargetPos = player.gameObject.transform.position;
            return TargetPos;
        }

        public static VRC.Player GetPlayerById(string playerId)
        {
            return Players.Find(player => player.field_Private_APIUser_0.id == playerId);
        }

        public static UnityEngine.Vector3 GetBonePosition(VRC.Player player, UnityEngine.HumanBodyBones bone)
        {
            var TargetPos = player.field_Private_VRCPlayerApi_0.GetBonePosition(bone);
            return TargetPos;
        }

        public static UnityEngine.Vector3 GetVelocity(VRC.Player player)
        {
            var TargetPos = player.field_Private_VRCPlayerApi_0.GetVelocity();
            return TargetPos;
        }

        public static Transform GetBoneTransform(VRC.Player player, UnityEngine.HumanBodyBones bone)
        {
            var TargetPos = player.field_Private_VRCPlayerApi_0.GetBoneTransform(bone);
            return TargetPos;
        }

        public static int GetViewID()
        {
            var vrcPlayer = LocalPlayer._vrcplayer;
            int viewID = vrcPlayer.Method_Public_Int32_0();
            return viewID;
        }

        public static GameObject GetNamePlateContainer(VRC.Player player)
        {
            var NameplateContainer = player._vrcplayer.field_Public_GameObject_0;
            return NameplateContainer;
        }

        public static VRCPlayerApi GetLocalPlayerAPIUser(string userId)
        {
            VRCPlayerApi vrcPlayerAPI = GetAllPlayers().ToList().Find(plr => plr.field_Private_APIUser_0.id == userId).field_Private_VRCPlayerApi_0;
            return vrcPlayerAPI;
        }

        public static VRC.Player GetPlayerByDisplayName(string name)
        {
            VRC.Player vrcPlayer = GetAllPlayers().ToList().Find(plr => plr.field_Private_APIUser_0.displayName == name);
            return vrcPlayer;
        }

        public static VRCPlayer GetVRCPlayerFromId(string userId)
        {
            VRCPlayer vrcPlayer = GetAllPlayers().ToList().Find(plr => plr.field_Private_APIUser_0.id == userId).prop_VRCPlayer_0;
            return vrcPlayer;
        }

        public static VRCPlayer GetVRCPlayerFromPhotonId(int plrId)
        {
            VRCPlayer vrcPlayer = GetAllPlayers().ToList().Find(plr => plr.field_Private_VRCPlayerApi_0.playerId == plrId).prop_VRCPlayer_0;
            return vrcPlayer;
        }

        public static VRCPlayer GetPlayerFromPhotonId(int id)
        {
            VRCPlayer vrcPlayer = GetAllPlayers().ToList().Find(plr => plr.field_Private_VRCPlayerApi_0.playerId == id).prop_VRCPlayer_0;
            return vrcPlayer;
        }

        public static Transform GetNamePlateCanvas(VRC.Player player)
        {
            var Container = GetNamePlateContainer(player);
            if (Container == null) return null;

            return Container.transform.FindChild("PlayerNameplate/Canvas"); ;
        }

        private static Rank GetPlayerRank(VRC.Core.APIUser apiUser)
        {
            if (apiUser.hasLegendTrustLevel || apiUser.hasVeteranTrustLevel)
            {
                return Rank.Trusted;
            }
            else if (apiUser.hasTrustedTrustLevel)
            {
                return Rank.Known;
            }
            else if (apiUser.hasKnownTrustLevel)
            {
                return Rank.User;
            }
            else if (apiUser.hasBasicTrustLevel)
            {
                return Rank.NewUser;
            }
            else
            {
                return Rank.Visitor;
            }
        }


        private static Color GetRankColor(Rank rank)
        {
            switch (rank)
            {
                case Rank.Visitor:
                    return new Color(1f, 1f, 1f, 0.8f);         // White
                case Rank.NewUser:
                    return ColorFromHex("#96ECFF", 0.8f);       // Light Blue
                case Rank.User:
                    return ColorFromHex("#96FFA9", 0.8f);       // Light Green
                case Rank.Known:
                    return ColorFromHex("#FF5E50", 0.8f);       // Orangish Red
                case Rank.Trusted:
                    return ColorFromHex("#A900FE", 0.8f);       // Purple
                default:
                    return new Color(1f, 1f, 1f, 0.8f);         // Default to white
            }
        }

        public static Color ColorFromHex(string hex, float alpha = 1f)
        {
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);

            if (hex.Length == 6)
            {
                float r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                float g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                float b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                return new Color(r, g, b, alpha);
            }

            return Color.white;
        }
    }
}
