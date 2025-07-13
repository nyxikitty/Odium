using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using VRC.Udon;

public static class UdonExtensions
{
    public static void SendUdon(this GameObject go, string evt, VRC.Player player = null, bool check = false)
    {
        var udon = go.GetComponent<UdonBehaviour>();
        if (player == null)
        {
            if (check) return;
            if (player == VRCPlayer.field_Internal_Static_VRCPlayer_0._player)
                udon.SendCustomEvent(evt);
            else
                udon.SendCustomNetworkEvent(0, evt);
        }
        else
        {
            SetOwner(go, player);
            udon.SendCustomNetworkEvent(NetworkEventTarget.Owner, evt);
        }
    }

    public static void SetOwner(this GameObject go, VRC.Player player)
    {
        if (GetOwner(go) == player) return;
        Networking.SetOwner(player.field_Private_VRCPlayerApi_0, go);
    }

    public static VRC.Player GetOwner(this GameObject go)
    {
        foreach (VRC.Player player in PlayerManager.prop_PlayerManager_0.field_Private_List_1_Player_0)
        {
            if (player.field_Private_VRCPlayerApi_0.IsOwner(go))
                return player;
        }
        return null;
    }
}