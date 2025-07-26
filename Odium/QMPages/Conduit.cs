using Odium.ButtonAPI.QM;
using Odium.Modules;
using Odium.Odium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Odium.QMPages
{
    class Conduit
    {
        public static void InitializePage(QMNestedMenu exploitsButton, Sprite buttonImage)
        {
            new QMToggleButton(exploitsButton, 1f, 0, "Conduit", () =>
            {
                AssignedVariables.conduit = true;
                OdiumBottomNotification.ShowNotification("USpeak Proxy -> <color=green>enabled</color>");
            }, delegate
            {
                AssignedVariables.conduit = false;
                OdiumBottomNotification.ShowNotification("USpeak Proxy -> <color=red>disabled</color>");
            }, "Makes your microphone loud as hell", false, buttonImage);

            new QMToggleButton(exploitsButton, 2f, 0, "Proxy USpeak", () =>
            {
                AssignedVariables.clientTalk = true;
                OdiumBottomNotification.ShowNotification("USpeak Proxy -> <color=green>enabled</color>");
            }, delegate
            {
                AssignedVariables.clientTalk = false;
                OdiumBottomNotification.ShowNotification("USpeak Proxy -> <color=red>disabled</color>");
            }, "Makes your microphone loud as hell", false, buttonImage);

            new QMToggleButton(exploitsButton, 3f, 0, "Proxy Portals", () =>
            {
                AssignedVariables.proxyPortals = true;
                OdiumBottomNotification.ShowNotification("Portal Proxy <color=green>enabled</color>");
            }, delegate
            {
                AssignedVariables.proxyPortals = false;
                OdiumBottomNotification.ShowNotification("Portal Proxy <color=red>disabled</color>");
            }, "Makes your microphone loud as hell", false, buttonImage);

            new QMToggleButton(exploitsButton, 4f, 0, "Proxy Movement", () =>
            {
                AssignedVariables.proxyMovement = true;
                OdiumBottomNotification.ShowNotification("Movement Proxy <color=green>enabled</color>");
            }, delegate
            {
                AssignedVariables.proxyMovement = false;
                OdiumBottomNotification.ShowNotification("Movement Proxy <color=red>disabled</color>");
            }, "Makes your microphone loud as hell", false, buttonImage);
        }
    }
}
