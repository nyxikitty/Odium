using System.Reflection;
using HarmonyLib;
using ExitGames.Client.Photon;
using Odium.Components;
using Odium.Modules;
using Odium.Odium;
using Odium.UX;
using Odium.Wrappers;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using VampClient.Api;
using VRC.Udon;
using System;
using UnityEngine;

namespace Odium.Patches
{
    public static class PhotonPatchesManual
    {
        public static void ApplyPatches()
        {
            OdiumEntry.HarmonyInstance.Patch(
                typeof(LoadBalancingClient).GetMethod(
                    "OnEvent",
                    new[] { typeof(ExitGames.Client.Photon.EventData) }
                ),
                prefix: new HarmonyMethod(typeof(PhotonPatchesManual).GetMethod(nameof(OnEvent), BindingFlags.Static | BindingFlags.Public))
            );
        }

        public static bool OnEvent(LoadBalancingClient __instance, ref ExitGames.Client.Photon.EventData param_1)
        {
            var eventCode = param_1.Code;

            switch (eventCode)
            {
                case 12:
                    PhotonPatches.UpdatePlayerActivity(param_1.sender, eventCode);
                    break;
                case 34:
                    Ratelimit.ProcessRateLimit(ref param_1);
                    break;
                case 11:
                    if (PhotonPatches.BlockUdon)
                    {
                        return false;
                    }
                    break;
                case 1:
                    // Will be added back soon

                    //byte[] e = Serializer.Il2ToByteArray(param_1.CustomData);
                    //byte[] result = new byte[e.Length - 4];
                    //Array.Copy(e, 4, result, 0, e.Length - 4);
                    //string base64 = Convert.ToBase64String(result);

                    //string debugBytes = string.Join(" ", Array.ConvertAll(result.Take(16).ToArray(), b => b.ToString("X2")));
                    //OdiumConsole.Log("USpeak", $"First 16 bytes: {debugBytes}");
                    //OdiumConsole.Log("USpeak", $"Packet length: {result.Length}");

                    //VRC.Player plr = PlayerWrapper.GetVRCPlayerFromActorNr(param_1.sender);
                    //PhotonPatches.UpdatePlayerActivity(param_1.sender, eventCode);

                    //var parsed = USpeakPacketHandler.ParseUSpeakPacket(result);
                    //OdiumConsole.Log("USpeak", $@"{base64} - Gain: {parsed.gain}, Flags: {parsed.flags:X2}, Timestamp: {parsed.timestamp}");

                    //if (parsed.gain > 135)
                    //{
                    //    if (!PhotonPatches.blockedUSpeakPacketCount.ContainsKey(param_1.sender))
                    //    {
                    //        PhotonPatches.blockedUSpeakPacketCount[param_1.sender] = 0;
                    //        PhotonPatches.blockedUSpeakPackets[param_1.sender] = 0;
                    //    }

                    //    PhotonPatches.blockedUSpeakPacketCount[param_1.sender]++;
                    //    PhotonPatches.blockedUSpeakPackets[param_1.sender]++;

                    //    if (PhotonPatches.blockedUSpeakPacketCount[param_1.sender] == 1)
                    //    {
                    //        VRC.Player player = PlayerWrapper.GetVRCPlayerFromActorNr(param_1.sender);
                    //        InternalConsole.LogIntoConsole($"<color=red>Blocked USpeak packet from user -> {player.field_Private_APIUser_0.displayName}</color>", "[OdiumProtection]");
                    //        ToastBase.Toast("Odium Protection", $"Potentially harmful event blocked from user '{player.field_Private_APIUser_0.displayName}' (Reason: USpeak Spam)", PhotonPatches.LogoIcon, 5);
                    //    }
                    //    else if (PhotonPatches.blockedUSpeakPackets[param_1.sender] == 200)
                    //    {
                    //        VRC.Player player = PlayerWrapper.GetVRCPlayerFromActorNr(param_1.sender);
                    //        InternalConsole.LogIntoConsole(
                    //            $"<color=red>Blocked {PhotonPatches.blockedUSpeakPacketCount[param_1.sender]} total USpeak packets from user -> {player.field_Private_APIUser_0.displayName}</color>"
                    //        );
                    //        ToastBase.Toast("Odium Protection", $"Potentially harmful event blocked from user '{player.field_Private_APIUser_0.displayName}' (Reason: USpeak Spam)", PhotonPatches.LogoIcon, 5);

                    //        PhotonPatches.blockedUSpeakPackets[param_1.sender] = 0;
                    //    }
                    //    return false;
                    //}
                    //else
                    //{
                    //    return true;
                    //}
                    break;
                case 18:
                    try
                    {
                        if (param_1?.Parameters == null || !param_1.Parameters.ContainsKey(param_1.CustomDataKey))
                        {
                            InternalConsole.LogIntoConsole($"[BLOCK] Invalid event parameters - Parameters null: {param_1?.Parameters == null}, Missing key: {!param_1?.Parameters?.ContainsKey(param_1.CustomDataKey)}");
                            return false;
                        }

                        var dictionary = param_1.Parameters[param_1.CustomDataKey].Cast<Il2CppSystem.Collections.Generic.Dictionary<byte, Il2CppSystem.Object>>();

                        if (dictionary == null || !dictionary.ContainsKey(0) || !dictionary.ContainsKey(1) || !dictionary.ContainsKey(2))
                        {
                            InternalConsole.LogIntoConsole($"[BLOCK] Missing required dictionary keys - Dict null: {dictionary == null}, Has key 0: {dictionary?.ContainsKey(0)}, Has key 1: {dictionary?.ContainsKey(1)}, Has key 2: {dictionary?.ContainsKey(2)}");
                            return false;
                        }

                        if (!PhotonPatches.TryUnboxInt(dictionary[2], out int viewId) ||
                            !PhotonPatches.TryUnboxByte(dictionary[0], out byte eventType) ||
                            !PhotonPatches.TryUnboxInt(dictionary[1], out int eventHashInt))
                        {
                            InternalConsole.LogIntoConsole($"[BLOCK] Invalid data types - viewId unbox: {PhotonPatches.TryUnboxInt(dictionary[2], out _)}, eventType unbox: {PhotonPatches.TryUnboxByte(dictionary[0], out _)}, eventHash unbox: {PhotonPatches.TryUnboxInt(dictionary[1], out _)}");
                            return false;
                        }

                        if (viewId < 0 || viewId > 999999)
                        {
                            InternalConsole.LogIntoConsole($"[BLOCK] Suspicious viewId: {viewId} (range: 0-999999)");
                            return false;
                        }

                        if (eventType == 1 || eventType == 2)
                        {
                            PhotonView photonView = PhotonView.Method_Public_Static_PhotonView_Int32_0(viewId);
                            if (photonView == null || photonView.gameObject == null)
                            {
                                InternalConsole.LogIntoConsole($"[BLOCK] Invalid PhotonView for viewId: {viewId} - PhotonView null: {photonView == null}, GameObject null: {photonView?.gameObject == null}");
                                return false;
                            }

                            UdonBehaviour udonBehaviour = photonView.gameObject.GetComponent<UdonBehaviour>();
                            if (udonBehaviour == null)
                            {
                                return true;
                            }

                            string behaviourName = udonBehaviour.gameObject.name;
                            uint eventHash = (uint)eventHashInt;

                            VRC.Player vrcPlayer = PlayerWrapper.GetVRCPlayerFromActorNr(param_1.sender);
                            if (vrcPlayer?.field_Private_APIUser_0 == null)
                            {
                                InternalConsole.LogIntoConsole($"[BLOCK] Invalid player data - Player null: {vrcPlayer == null}, APIUser null: {vrcPlayer?.field_Private_APIUser_0 == null}");
                                return false;
                            }

                            string playerName = vrcPlayer.field_Private_APIUser_0.displayName ?? "Unknown";
                            string playerId = vrcPlayer.field_Private_APIUser_0.id ?? "Unknown";

                            if (!PhotonPatches.crashAttemptCounts.ContainsKey(playerName))
                            {
                                PhotonPatches.crashAttemptCounts[playerName] = 0;
                            }
                            if (!PhotonPatches.lastLogTimes.ContainsKey(playerName))
                            {
                                PhotonPatches.lastLogTimes[playerName] = DateTime.MinValue;
                            }

                            bool isSuspicious = false;
                            string suspiciousReason = "";

                            if (udonBehaviour.TryGetEntrypointNameFromHash(eventHash, out string entryPointName))
                            {
                                if (entryPointName == "SyncAssignM" || entryPointName == "SyncAssignD" || entryPointName == "SyncAssignB" || entryPointName == "KillSync")
                                {
                                    string rateLimitKey = $"{playerId}_{entryPointName}";
                                    DateTime currentTime = DateTime.Now;

                                    if (PhotonPatches.syncAssignMCooldowns.ContainsKey(rateLimitKey))
                                    {
                                        DateTime lastExecution = PhotonPatches.syncAssignMCooldowns[rateLimitKey];
                                        TimeSpan timeSinceLastExecution = currentTime - lastExecution;

                                        if (timeSinceLastExecution < PhotonPatches.SYNC_ASSIGN_M_COOLDOWN)
                                        {
                                            TimeSpan remainingCooldown = PhotonPatches.SYNC_ASSIGN_M_COOLDOWN - timeSinceLastExecution;
                                            InternalConsole.LogIntoConsole($"[RATELIMIT] SyncAssignM blocked for {playerName} - Cooldown remaining: {remainingCooldown.TotalSeconds:F1}s");

                                            if ((currentTime - PhotonPatches.lastLogTimes[playerName]).TotalSeconds >= 30)
                                            {
                                                ToastBase.Toast("Odium Protection", $"SyncAssignM rate limited for '{playerName}' - {remainingCooldown.TotalSeconds:F0}s remaining", PhotonPatches.LogoIcon, 3);
                                                PhotonPatches.lastLogTimes[playerName] = currentTime;
                                            }

                                            return false;
                                        }
                                    }

                                    PhotonPatches.syncAssignMCooldowns[rateLimitKey] = currentTime;
                                    InternalConsole.LogIntoConsole($"[RATELIMIT] SyncAssignM allowed for {playerName} - Cooldown set for 2 minutes");
                                }

                                OdiumConsole.Log("Udon", $@"
======= {playerName} =======
Player ID -> {playerId}
Behavior Name -> {behaviourName}
Entry Point Name -> {entryPointName}
Event Hash -> {eventHash}
Event Type -> {eventType}
GameObject Path -> {PhotonPatches.GetGameObjectPath(photonView.gameObject)}
Timestamp -> {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}
Rate Limited -> {(entryPointName == "SyncAssignM" ? "Yes (2min)" : "No")}
");

                                if (entryPointName == "ListPatrons" && behaviourName == "Patreon Credits")
                                {
                                    isSuspicious = true;
                                    suspiciousReason = "ListPatrons exploit";
                                }
                                else if (PhotonPatches.IsKnownExploitPattern(entryPointName, behaviourName))
                                {
                                    if (AssignedVariables.preventPatreonCrash == false)
                                    {
                                        isSuspicious = false;
                                    }
                                    else
                                    {
                                        isSuspicious = true;
                                        suspiciousReason = "Known exploit pattern";
                                    }
                                }
                            }

                            if (!isSuspicious)
                            {
                                if (PhotonPatches.IsRapidFireEvent(playerName))
                                {
                                    isSuspicious = true;
                                    suspiciousReason = "Rapid-fire events";
                                }

                                if (PhotonPatches.IsUnusualHash(eventHash))
                                {
                                    isSuspicious = true;
                                    suspiciousReason = "Unusual hash value";
                                }
                            }

                            if (isSuspicious)
                            {
                                PhotonPatches.crashAttemptCounts[playerName]++;
                                int attemptCount = PhotonPatches.crashAttemptCounts[playerName];
                                DateTime currentTime = DateTime.Now;

                                if ((currentTime - PhotonPatches.lastLogTimes[playerName]).TotalSeconds >= 15)
                                {
                                    InternalConsole.LogIntoConsole($"-> Prevented: {playerName} [Reason: {suspiciousReason}]");
                                    ToastBase.Toast("Odium Protection", $"Potentially harmful event blocked from user '{playerName}' (Reason: {suspiciousReason})", PhotonPatches.LogoIcon, 5);
                                    PhotonPatches.lastLogTimes[playerName] = currentTime;
                                }

                                return false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        InternalConsole.LogIntoConsole($"Error in event protection: {ex.Message}");
                        return false;
                    }
                    return true;

                case 43:
                    if (AssignedVariables.chatBoxAntis == false)
                    {
                        return true;
                    }
                    string incomingMessage = "";
                    try
                    {
                        byte[] byteArray = Serializer.Il2ToByteArray(param_1.CustomData);
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
                        if (!PhotonPatches.blockedMessagesCount.ContainsKey(param_1.sender))
                        {
                            PhotonPatches.blockedMessagesCount[param_1.sender] = 0;
                            PhotonPatches.blockedMessages[param_1.sender] = 0;
                        }

                        PhotonPatches.blockedMessagesCount[param_1.sender]++;
                        PhotonPatches.blockedMessages[param_1.sender]++;

                        if (PhotonPatches.blockedMessagesCount[param_1.sender] == 1)
                        {
                            VRC.Player player = PlayerWrapper.GetVRCPlayerFromActorNr(param_1.sender);
                            InternalConsole.LogIntoConsole(
                                $"<color=red>Blocked chatbox message from user -> {player.field_Private_APIUser_0.displayName}</color>"
                            );
                        }
                        else if (PhotonPatches.blockedMessages[param_1.sender] >= 100)
                        {
                            VRC.Player player = PlayerWrapper.GetVRCPlayerFromActorNr(param_1.sender);
                            InternalConsole.LogIntoConsole(
                                $"<color=red>Blocked {PhotonPatches.blockedMessagesCount[param_1.sender]} total chatbox messages from user -> {player.field_Private_APIUser_0.displayName}</color>"
                            );
                            PhotonPatches.blockedMessages[param_1.sender] = 0;
                        }
                        return false;
                    }

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

                                if (moderationDict.ContainsKey(10))
                                {
                                    var blockStatus = moderationDict[10].Unbox<bool>();

                                    if (blockStatus == true && !PhotonPatches.blockedUserIds.Contains(playerId.ToString()))
                                    {
                                        PhotonPatches.blockedUserIds.Add(playerId.ToString());

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
                                    else if (blockStatus == false && PhotonPatches.blockedUserIds.Contains(playerId.ToString()))
                                    {
                                        PhotonPatches.blockedUserIds.Remove(playerId.ToString());

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

                                if (moderationDict.ContainsKey(11))
                                {
                                    var muteStatus = moderationDict[11].Unbox<bool>();

                                    if (muteStatus == true && !PhotonPatches.mutedUserIds.Contains(playerId.ToString()))
                                    {
                                        PhotonPatches.mutedUserIds.Add(playerId.ToString());

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
                                    else if (muteStatus == false && PhotonPatches.mutedUserIds.Contains(playerId.ToString()))
                                    {
                                        PhotonPatches.mutedUserIds.Remove(playerId.ToString());

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