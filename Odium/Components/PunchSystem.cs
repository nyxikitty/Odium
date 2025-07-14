using System;
using System.Collections;
using System.Collections.Generic;
using MelonLoader;
using Odium;
using Odium.GameCheats;
using UnityEngine;
using VRC.SDKBase;

public static class PunchSystem
{
    private const float COOLDOWN_TIME = 0.3f;
    private const float DETECTION_INTERVAL = 0.05f;
    private const float PUNCH_DISTANCE = 0.25f;
    private static float _lastPunchTime;
    private static bool _isInitialized;
    private static Transform _leftHand;
    private static Transform _rightHand;

    public static void Initialize()
    {
        if (_isInitialized) return;
        MelonCoroutines.Start(SetupPunchDetection());
        _isInitialized = true;
    }

    private static IEnumerator SetupPunchDetection()
    {
        OdiumConsole.Log("PunchSystem", "Initializing punch detection...");

        while (VRCPlayer.field_Internal_Static_VRCPlayer_0 == null)
            yield return null;

        var vrcPlayer = VRCPlayer.field_Internal_Static_VRCPlayer_0;
        while (vrcPlayer.GetComponent<VRCAvatarManager>().field_Private_GameObject_0 == null)
            yield return null;

        var animator = vrcPlayer.GetComponent<VRCAvatarManager>()
            .field_Private_GameObject_0.GetComponent<Animator>();

        if (animator == null)
        {
            OdiumConsole.Log("PunchSystem", "No animator found!");
            yield break;
        }

        _leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        _rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);

        if (_leftHand == null || _rightHand == null)
        {
            OdiumConsole.Log("PunchSystem", "Could not find hand transforms!");
            yield break;
        }

        MelonCoroutines.Start(PunchDetectionLoop());
        OdiumConsole.Log("PunchSystem", "Punch detection ready!");
    }

    private static IEnumerator PunchDetectionLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(DETECTION_INTERVAL);
            CheckForPunches();
        }
    }

    private static void CheckForPunches()
    {
        if (Time.time - _lastPunchTime < COOLDOWN_TIME) return;

        var players = GetNonLocalPlayers();
        if (players.Count == 0) return;

        if (_rightHand != null)
        {
            foreach (var player in players)
            {
                if (IsHandNearPlayer(_rightHand, player))
                {
                    HandlePunch(player);
                    return;
                }
            }
        }

        if (_leftHand != null)
        {
            foreach (var player in players)
            {
                if (IsHandNearPlayer(_leftHand, player))
                {
                    HandlePunch(player);
                    return;
                }
            }
        }
    }

    private static bool IsHandNearPlayer(Transform hand, VRC.Player player)
    {
        try
        {
            var head = player.field_Private_VRCPlayerApi_0.GetBonePosition(HumanBodyBones.Head);
            var chest = player.field_Private_VRCPlayerApi_0.GetBonePosition(HumanBodyBones.Chest);

            return Vector3.Distance(hand.position, head) < PUNCH_DISTANCE ||
                   Vector3.Distance(hand.position, chest) < PUNCH_DISTANCE;
        }
        catch
        {
            return false;
        }
    }

    private static List<VRC.Player> GetNonLocalPlayers()
    {
        var players = new List<VRC.Player>();
        foreach (var player in PlayerManager.prop_PlayerManager_0.field_Private_List_1_Player_0)
        {
            if (player != null && !player.field_Private_VRCPlayerApi_0.isLocal)
                players.Add(player);
        }
        return players;
    }

    private static void HandlePunch(VRC.Player targetPlayer)
    {
        _lastPunchTime = Time.time;
        OdiumConsole.Log("PunchSystem", $"Punched {targetPlayer.field_Private_VRCPlayerApi_0.displayName}!");
        SendUdonEvent(targetPlayer);
    }

    private static void SendUdonEvent(VRC.Player targetPlayer)
    {
        Odium.Patches.PhotonPatches.BlockUdon = true;
        for (int i = 0; i < 100; i++)
        {
            Murder4Utils.SendTargetedPatreonEvent(targetPlayer, "ListPatrons");
        }
        Odium.Patches.PhotonPatches.BlockUdon = false;
    }
}