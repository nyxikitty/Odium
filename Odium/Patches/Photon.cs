using ExitGames.Client.Photon;
using HarmonyLib;
using Odium.ApplicationBot;
using Odium.Components;
using Odium.Modules;
using Odium.Odium;
using Odium.UX;
using Odium.Wrappers;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerBaseLib;
using UnityEngine;
using UnityEngine.UI;
using VRC;

namespace Odium.Patches
{
    [HarmonyPatch(typeof(LoadBalancingClient))]
    public class PhotonPatches
    {
        public static bool BlockUdon = false;
        public static Dictionary<int, int> blockedMessages = new Dictionary<int, int>();
        public static int blockedChatBoxMessages = 0;
        public static Dictionary<int, int> blockedMessagesCount = new Dictionary<int, int>();
        public static Dictionary<int, int> blockedUSpeakPacketCount = new Dictionary<int, int>();

        public static Dictionary<int, int> blockedUSpeakPackets = new Dictionary<int, int>();

        private static Dictionary<int, bool> blocks = new Dictionary<int, bool>();
        private static Dictionary<int, bool> mutes = new Dictionary<int, bool>();

        [HarmonyPrefix]
        [HarmonyPatch("OnEvent")]
        static bool OnEvent(LoadBalancingClient __instance, EventData param_1)
        {
            var eventCode = param_1.Code;
            switch (eventCode)
            {
                case 1:
                    byte[] e = Serializer.ToByteArray(param_1.CustomData);
                    string base64 = Convert.ToBase64String(e);

                    VRC.Player plr = PlayerWrapper.GetVRCPlayerFromActorNr(param_1.sender);

                    if (base64.Contains("ABOT0tFTTBOT0szTTBOCw=="))
                    {
                        if (!blockedUSpeakPacketCount.ContainsKey(param_1.sender))
                        {
                            blockedUSpeakPacketCount[param_1.sender] = 0;
                            blockedUSpeakPackets[param_1.sender] = 0;
                        }

                        blockedUSpeakPacketCount[param_1.sender]++;
                        blockedUSpeakPackets[param_1.sender]++;
                        
                        if (blockedUSpeakPacketCount[param_1.sender] == 1)
                        {
                            VRC.Player player = PlayerWrapper.GetVRCPlayerFromActorNr(param_1.sender);
                            InternalConsole.LogIntoConsole($"<color=red>Blocked USpeak packet from user -> {player.field_Private_APIUser_0.displayName}</color>", "[OdiumProtection]");
                        }
                        else if (blockedUSpeakPackets[param_1.sender] == 200)
                        {
                            VRC.Player player = PlayerWrapper.GetVRCPlayerFromActorNr(param_1.sender);
                            InternalConsole.LogIntoConsole(
                                $"<color=red>Blocked {blockedUSpeakPacketCount[param_1.sender]} total USpeak packets from user -> {player.field_Private_APIUser_0.displayName}</color>"
                            );
                            blockedUSpeakPackets[param_1.sender] = 0;
                        }
                        return false;
                    } else
                    {
                        return true;
                    }
                    break;
                case 12:
                    if (Bot.movementMimic && Bot.movementMimicActorNr == param_1.sender)
                    {
                        PhotonExtensions.OpRaiseEvent(12, param_1.customData, new RaiseEventOptions
                        {
                            field_Public_EventCaching_0 = EventCaching.DoNotCache,
                            field_Public_ReceiverGroup_0 = ReceiverGroup.Others
                        }, default(SendOptions));
                        return false;
                    }
                    break;
                case 11:
                    if (BlockUdon)
                    {
                        InternalConsole.LogIntoConsole(
                            $"<color=#31BCF0>[Udon]:</color> Event <color=#00AAFF>blocked</color>!"
                        );
                        return false;
                    }
                    break;
                case 43:
                    string incomingMessage = "";
                    try
                    {
                        byte[] byteArray = Serializer.ToByteArray(param_1.CustomData);
                        incomingMessage = ChatboxLogger.ConvertBytesToText(byteArray);

                        incomingMessage = incomingMessage.Replace("\uFFFD", "")
                                                         .Replace("\u000B", "")
                                                         .Replace("\"", "")
                                                         .Trim();
                    }
                    catch (System.Exception ex)
                    {
                        OdiumConsole.Log("ChatBox", $"Error extracting message: {ex.Message}");
                        incomingMessage = "[Error extracting message]";
                    }

                    if (ChatboxAntis.IsMessageValid(incomingMessage))
                    {
                        return true;
                    }
                    else
                    {
                        if (!blockedMessagesCount.ContainsKey(param_1.sender))
                        {
                            blockedMessagesCount[param_1.sender] = 0;
                            blockedMessages[param_1.sender] = 0;
                        }

                        blockedMessagesCount[param_1.sender]++;
                        blockedMessages[param_1.sender]++;

                        if (blockedMessagesCount[param_1.sender] == 1)
                        {
                            VRC.Player player = PlayerWrapper.GetVRCPlayerFromActorNr(param_1.sender);
                            InternalConsole.LogIntoConsole(
                                $"<color=red>Blocked chatbox message from user -> {player.field_Private_APIUser_0.displayName}</color>"
                            );
                        }
                        else if (blockedMessages[param_1.sender] >= 100)
                        {
                            VRC.Player player = PlayerWrapper.GetVRCPlayerFromActorNr(param_1.sender);
                            InternalConsole.LogIntoConsole(
                                $"<color=red>Blocked {blockedMessagesCount[param_1.sender]} total chatbox messages from user -> {player.field_Private_APIUser_0.displayName}</color>"
                            );
                            blockedMessages[param_1.sender] = 0;
                        }
                        return false;
                    }
                    break;
                case 33:
                    if (param_1.Parameters != null && param_1.Parameters.ContainsKey(245))
                    {
                        var moderationDict = param_1.Parameters[245]
                            .TryCast<Il2CppSystem.Collections.Generic.Dictionary<byte, Il2CppSystem.Object>>();

                        if (moderationDict != null)
                        {
                            if (moderationDict.ContainsKey(0))
                            {
                                var moderationType = moderationDict[0].Unbox<byte>();
                            }

                            if (moderationDict.ContainsKey(1))
                            {
                                var playerId = moderationDict[1].Unbox<int>();

                                bool blockStatusChanged = false;

                                if (moderationDict.ContainsKey(10))
                                {
                                    var blockStatus = moderationDict[10].Unbox<bool>();

                                    if (!blocks.ContainsKey(playerId) || blocks[playerId] != blockStatus)
                                    {
                                        blocks[playerId] = blockStatus;
                                        blockStatusChanged = true;

                                        if (blockStatus == true)
                                        {
                                            VRCPlayer player = PlayerWrapper.GetPlayerFromPhotonId(playerId);
                                            if (AssignedVariables.announceBlocks)
                                            {
                                                Chatbox.SendCustomChatMessage($"[Odium] -> {player.field_Private_VRCPlayerApi_0.displayName} BLOCKED me");
                                            }
                                            Color rankColor = NameplateModifier.GetRankColor(NameplateModifier.GetPlayerRank(player._player.field_Private_APIUser_0));
                                            string hexColor = NameplateModifier.ColorToHex(rankColor);
                                            OdiumBottomNotification.ShowNotification($"<color=#FF5151>BLOCKED</color> by <color={hexColor}>{player.field_Private_VRCPlayerApi_0.displayName}");
                                            InternalConsole.LogIntoConsole(
                                                $"<color=#7B02FE>{player.field_Private_VRCPlayerApi_0.displayName}</color> <color=red>BLOCKED</color> you!"
                                            );
                                        }
                                        else
                                        {
                                            VRCPlayer player = PlayerWrapper.GetPlayerFromPhotonId(playerId);
                                            if (AssignedVariables.announceBlocks)
                                            {
                                                Chatbox.SendCustomChatMessage($"[Odium] -> {player.field_Private_VRCPlayerApi_0.displayName} UNBLOCKED me");
                                            }
                                            Color rankColor = NameplateModifier.GetRankColor(NameplateModifier.GetPlayerRank(player._player.field_Private_APIUser_0));
                                            string hexColor = NameplateModifier.ColorToHex(rankColor);
                                            OdiumBottomNotification.ShowNotification($"<color=#FF5151>UNBLOCKED</color> by <color={hexColor}>{player.field_Private_VRCPlayerApi_0.displayName}");
                                            InternalConsole.LogIntoConsole(
                                                $"<color=#7B02FE>{player.field_Private_VRCPlayerApi_0.displayName}</color> <color=red>UNBLOCKED</color> you!"
                                            );
                                        }
                                    }
                                }

                                if (!blockStatusChanged && moderationDict.ContainsKey(11))
                                {
                                    var muteStatus = moderationDict[11].Unbox<bool>();

                                    if (!mutes.ContainsKey(playerId) || mutes[playerId] != muteStatus)
                                    {
                                        mutes[playerId] = muteStatus;

                                        if (muteStatus == true)
                                        {
                                            VRCPlayer player = PlayerWrapper.GetPlayerFromPhotonId(playerId);
                                            if (AssignedVariables.announceMutes)
                                            {
                                                Chatbox.SendCustomChatMessage($"[Odium] -> {player.field_Private_VRCPlayerApi_0.displayName} MUTED me");
                                            }
                                            Color rankColor = NameplateModifier.GetRankColor(NameplateModifier.GetPlayerRank(player._player.field_Private_APIUser_0));
                                            string hexColor = NameplateModifier.ColorToHex(rankColor);
                                            OdiumBottomNotification.ShowNotification($"<color=#FF5151>MUTED</color> by <color={hexColor}>{player.field_Private_VRCPlayerApi_0.displayName}");
                                            InternalConsole.LogIntoConsole(
                                                $"<color=#7B02FE>{player.field_Private_VRCPlayerApi_0.displayName}</color> <color=red>MUTED</color> you!"
                                            );
                                        }
                                        else
                                        {
                                            VRCPlayer player = PlayerWrapper.GetPlayerFromPhotonId(playerId);
                                            if (AssignedVariables.announceMutes)
                                            {
                                                Chatbox.SendCustomChatMessage($"[Odium] -> {player.field_Private_VRCPlayerApi_0.displayName} unfortunately UNMUTED me");
                                            }
                                            Color rankColor = NameplateModifier.GetRankColor(NameplateModifier.GetPlayerRank(player._player.field_Private_APIUser_0));
                                            string hexColor = NameplateModifier.ColorToHex(rankColor);
                                            OdiumBottomNotification.ShowNotification($"<color=#FF5151>UNMUTED</color> by <color={hexColor}>{player.field_Private_VRCPlayerApi_0.displayName}");
                                            InternalConsole.LogIntoConsole(
                                                $"<color=#7B02FE>{player.field_Private_VRCPlayerApi_0.displayName}</color> <color=red>UNMUTED</color> you!"
                                            );
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (moderationDict.ContainsKey(10))
                                {
                                    var blocks = Il2CppArrayBase<int>.WrapNativeGenericArrayPointer(moderationDict[10].Pointer);
                                    if (blocks != null && blocks.Length > 0)
                                    {
                                        foreach (var blockId in blocks)
                                        {
                                        }
                                    }
                                }

                                if (moderationDict.ContainsKey(11))
                                {
                                    var mutes = Il2CppArrayBase<int>.WrapNativeGenericArrayPointer(moderationDict[11].Pointer);
                                    if (mutes != null && mutes.Length > 0)
                                    {
                                        foreach (var muteId in mutes)
                                        {
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (param_1.Parameters != null && param_1.Parameters.ContainsKey(254))
                    {
                        var timestamp = param_1.Parameters[254];
                    }
                    break;

                case 208:
                    try
                    {
                        InternalConsole.LogIntoConsole(
                            $"<color=#31BCF0>[MasterClient]:</color> Master client switch detected"
                        );

                        if (param_1.Parameters != null && param_1.Parameters.ContainsKey(254))
                        {
                            var newMasterActorId = param_1.Parameters[254];
                            if (newMasterActorId != null)
                            {
                                var actorNumber = newMasterActorId.Unbox<int>();
                                VRCPlayer newMasterPlayer = PlayerWrapper.GetPlayerFromPhotonId(actorNumber);

                                if (newMasterPlayer != null)
                                {
                                    Color rankColor = NameplateModifier.GetRankColor(NameplateModifier.GetPlayerRank(newMasterPlayer._player.field_Private_APIUser_0));
                                    string hexColor = NameplateModifier.ColorToHex(rankColor);

                                    InternalConsole.LogIntoConsole(
                                        $"<color=#31BCF0>[MasterClient]:</color> New master: <color={hexColor}>{newMasterPlayer.field_Private_VRCPlayerApi_0.displayName}</color> (ID: {actorNumber})"
                                    );

                                    OdiumBottomNotification.ShowNotification($"<color={hexColor}>{newMasterPlayer.field_Private_VRCPlayerApi_0.displayName}</color> is the new <color=#FFECA1>Master</color>");
                                }
                                else
                                {
                                    InternalConsole.LogIntoConsole(
                                        $"<color=#31BCF0>[MasterClient]:</color> New master actor ID: {actorNumber} (Player not found)"
                                    );
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        InternalConsole.LogIntoConsole(
                            $"<color=#31BCF0>[MasterClient]:</color> <color=red>Error processing master client switch: {ex.Message}</color>"
                        );
                    }
                    break;
            }

            return true;
        }
    }
}