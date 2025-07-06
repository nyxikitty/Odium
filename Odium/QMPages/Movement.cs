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
        public static void InitializePage(QMNestedMenu movementButton, Sprite buttonImage)
        {
            Sprite PlusIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\PlusIcon.png");
            Sprite MinusIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\MinusIcon.png");

            QMToggleButton qMToggleButton = new QMToggleButton(movementButton, 2.5f, 1, "Flight", () =>
            {
                FlyComponent.FlyEnabled = true;
            }, delegate
            {
                FlyComponent.FlyEnabled = false;
            }, "Toggle Flight Mode", false, buttonImage);
            
            
            QMToggleButton qMToggleButton2 = new QMToggleButton(movementButton, 2f, 0, "Jetpack", () =>
            {
                Jetpack.Activate(true);
            }, delegate
            {
                Jetpack.Activate(false);
            }, "Allows you to fly", false, buttonImage);
            
            QMToggleButton qMToggleButton3 = new QMToggleButton(movementButton, 3f, 0, "SpinBot", () =>
            {
                SpinBot.Activate(true);
            }, delegate
            {
                SpinBot.Activate(false);
            }, "HvH mode", false, buttonImage);
            

            QMSingleButton speed = new QMSingleButton(movementButton, 2, 3, "Fly Speed", () =>
            {
                FlyComponent.FlySpeed += 0.1f;
            }, "Increase Fly Speed", false, PlusIcon, buttonImage);

            QMSingleButton slow = new QMSingleButton(movementButton, 3, 3, "Fly Speed", () =>
            {
                FlyComponent.FlySpeed -= 0.1f;
            }, "Decrease Fly Speed", false, MinusIcon, buttonImage);
        }
    }
}
