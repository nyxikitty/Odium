using Odium.Components;
using Odium.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Odium.ButtonAPI.QM;

namespace Odium.QMPages
{
    class Movement
    {
        public static QMSingleButton gay;
        public static QMSingleButton flightType;
        public static void InitializePage(QMNestedMenu movementButton, Sprite buttonImage)
        {
            Sprite PlusIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\PlusIconOpacity.png");
            Sprite MinusIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\MinusIconOpacity.png");
            Sprite ResetIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\Reset.png");
            Sprite TransparentSprite = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\Transparent.png");
            Sprite HorizontalSprite = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\Horizontal.png");
            Sprite DirectionSprite = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\Direction.png");
            Sprite DisableSprite = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\Disable.png");

            QMToggleButton qMToggleButton2 = new QMToggleButton(movementButton, 2.5f, 0, "Jetpack", () =>
            {
                Jetpack.Activate(true);
            }, delegate
            {
                Jetpack.Activate(false);
            }, "Allows you to fly", false, buttonImage);

            var flySpeedButton = new QMExtendedBackground(
                movementButton,
                2.5f, 3f
            );

            var flightToggleButton = new QMExtendedBackground(
                movementButton,
                2.5f, 2f
            );


            flightType = new QMSingleButton(movementButton, 2.5f, 2f, "Flight: Directional", () => {
                FlyComponent.DirectionalFlight = true;
                FlyComponent.FlyEnabled = true;

                flightType.SetButtonText("Flight: Directional", true, false);
            }, "This is a placeholder for the button", false, DirectionSprite, TransparentSprite, true);

            new QMSingleButton(movementButton, 2f, 2f, "", () => {
                FlyComponent.DirectionalFlight = false;
                FlyComponent.FlyEnabled = true;
                flightType.SetButtonText("Flight: Horizontal", true, false);
                OdiumConsole.Log("Flight Module", $"Flight: Horizontal");
            }, "This is a placeholder for the button", false, HorizontalSprite, TransparentSprite, true);


            new QMSingleButton(movementButton, 3f, 2f, "", () => {
                FlyComponent.FlyEnabled = false;
                flightType.SetButtonText("Flight: Disabled", true, false);
                OdiumConsole.Log("Flight Module", $"Flight: Disabled");
            }, "This is a placeholder for the button", false, DisableSprite, TransparentSprite, true);


            gay = new QMSingleButton(movementButton, 2.5f, 3f, "Speed: " + FlyComponent.FlySpeed.ToString(), () => {
                FlyComponent.FlySpeed = 0.5f;
                gay.SetButtonText("Speed: " + FlyComponent.FlySpeed.ToString(), true, false);
            }, "This is a placeholder for the button", false, ResetIcon, TransparentSprite, true);

            new QMSingleButton(movementButton, 2f, 3f, "", () => {
                FlyComponent.FlySpeed -= 0.1f;
                gay.SetButtonText("Speed: " + FlyComponent.FlySpeed.ToString(), true, false);
                OdiumConsole.Log("Flight Module", $"Fly Speed: {FlyComponent.FlySpeed}");
            }, "This is a placeholder for the button", false, MinusIcon, TransparentSprite, true);


            new QMSingleButton(movementButton, 3f, 3f, "", () => {
                FlyComponent.FlySpeed += 0.1f;
                gay.SetButtonText("Speed: " + FlyComponent.FlySpeed.ToString(), true, false);
                OdiumConsole.Log("Flight Module", $"Fly Speed: {FlyComponent.FlySpeed}");
            }, "This is a placeholder for the button", false, PlusIcon, TransparentSprite, true);
        }
    }
}
