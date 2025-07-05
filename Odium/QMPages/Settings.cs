using Odium.Components;
using Odium.Odium;
using Odium.UI;
using Odium.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Odium.ButtonAPI.QM;

namespace Odium.QMPages
{
    class Settings
    {
        public static void InitializePage(QMNestedMenu movementButton, Sprite buttonImage)
        {
            new QMToggleButton(movementButton, 1.5f, 1.5f, "Announce Blocks", () =>
            {
                AssignedVariables.announceBlocks = true;
            }, delegate
            {
                AssignedVariables.announceBlocks = false;
            }, "Toggle Flight Mode", false, buttonImage);

            new QMToggleButton(movementButton, 2.5f, 1.5f, "Announce Mutes", () =>
            {
                AssignedVariables.announceMutes = true;
            }, delegate
            {
                AssignedVariables.announceMutes = false;
            }, "Toggle Flight Mode", false, buttonImage);

            new QMToggleButton(movementButton, 3.5f, 1.5f, "Desktop Playerlist", () =>
            {
                AssignedVariables.desktopPlayerList = true;
                PlayerWrapper.Players.ForEach(player =>
                {
                    PlayerRankTextDisplay.AddPlayer(player.prop_IUser_0.prop_String_1, PlayerWrapper.GetVRCPlayerFromId(player.prop_IUser_0.prop_String_0)._player.field_Private_APIUser_0, PlayerWrapper.GetVRCPlayerFromId(player.prop_IUser_0.prop_String_0)._player);
                });
            }, delegate
            {
                AssignedVariables.desktopPlayerList = false;
                PlayerWrapper.Players.ForEach(player =>
                {
                    PlayerRankTextDisplay.RemovePlayer(player.prop_IUser_0.prop_String_1);
                });
                PlayerRankTextDisplay.ClearAll();
            }, "Toggle Flight Mode", true, buttonImage);

            new QMSingleButton(movementButton, 2.5f, 2.5f, "Remove Ads", delegate
            {
                AdBlock.OnQMInit();
            }, "", false, null, buttonImage);
        }
    }
}
