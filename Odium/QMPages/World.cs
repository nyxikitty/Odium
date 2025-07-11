using Odium.Components;
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
    class World
    {
        public static QMToggleButton hidePickupsToggle = null;
        public static void InitializePage(QMNestedMenu worldButton, Sprite buttonImage)
        {
            Sprite PickupsTabImage = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\PickupsIcon.png");
            QMNestedMenu pickupsButton = new QMNestedMenu(worldButton, 1f, 3f, "<color=#8d142b>Pickups</color>", "<color=#8d142b>Pickups</color>", "Opens Select User menu", false, null, buttonImage);

            new QMToggleButton(worldButton, 1, 0, "Drone Swarm", () =>
            {
                DroneSwarmWrapper.isSwarmActive = true;
                DroneSwarmWrapper.ChangeSwarmTarget(PlayerWrapper.LocalPlayer.gameObject);
            }, delegate
            {
                DroneSwarmWrapper.isSwarmActive = false;
            }, "Swarms your player with every available drone in the instance", false, buttonImage);

            new QMSingleButton(worldButton, 2, 0, "Drop Drones", () =>
            {
                PickupWrapper.DropDronePickups();
            }, "Drop all drones in the instance", false, null, buttonImage);

            new QMSingleButton(pickupsButton, 1, 0, "Drop Pickups", () =>
            {
                PickupWrapper.DropAllPickups();
            }, "Drop all pickups in the instance", false, null, buttonImage);

            new QMSingleButton(pickupsButton, 2, 0, "Bring Pickups", () =>
            {
                PickupWrapper.BringAllPickupsToPlayer(PlayerWrapper.LocalPlayer);
            }, "Brings all pickups in the instance", false, null, buttonImage);

            new QMSingleButton(pickupsButton, 3, 0, "Respawn Pickups", () =>
            {
                PickupWrapper.RespawnAllPickups();
            }, "Brings all pickups in the instance", false, null, buttonImage);

            hidePickupsToggle = new QMToggleButton(pickupsButton, 1, 3, "Hide Pickups",
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
