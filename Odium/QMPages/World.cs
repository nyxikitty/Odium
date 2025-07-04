using Odium.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VampClient.Api;

namespace Odium.QMPages
{
    class World
    {
        public static QMToggleButton hidePickupsToggle = null;
        public static void InitializePage(QMNestedMenu worldButton, Sprite buttonImage)
        {
            QMToggleButton qMToggleButton = new QMToggleButton(worldButton, 1, 0, "Drone Swarm", () =>
            {
                DroneSwarmWrapper.isSwarmActive = true;
                DroneSwarmWrapper.ChangeSwarmTarget(PlayerWrapper.LocalPlayer.gameObject);
            }, delegate
            {
                DroneSwarmWrapper.isSwarmActive = false;
            }, "Swarms your player with every available drone in the instance", false, buttonImage);

            QMSingleButton dropDrones = new QMSingleButton(worldButton, 2, 0, "Drop Drones", () =>
            {
                PickupWrapper.DropDronePickups();
            }, "Drop all drones in the instance", false, null, buttonImage);

            QMSingleButton drop = new QMSingleButton(worldButton, 3, 0, "Drop Pickups", () =>
            {
                PickupWrapper.DropAllPickups();
            }, "Drop all pickups in the instance", false, null, buttonImage);

            QMSingleButton bring = new QMSingleButton(worldButton, 4, 0, "Bring Pickups", () =>
            {
                PickupWrapper.BringAllPickupsToPlayer(PlayerWrapper.LocalPlayer);
            }, "Brings all pickups in the instance", false, null, buttonImage);

            hidePickupsToggle = new QMToggleButton(worldButton, 1, 1, "Hide Pickups",
            () => {
                PickupWrapper.HideAllPickups();
                hidePickupsToggle.SetButtonText("Show Pickups");
            },
            () => {
                PickupWrapper.ShowAllPickups();
                hidePickupsToggle.SetButtonText("Hide Pickups");
            },
            "Toggle visibility of all pickups in the instance", false, buttonImage);
        }
    }
}
