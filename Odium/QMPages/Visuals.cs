using Odium.Components;
using Odium.Odium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Odium.ButtonAPI.QM;

namespace Odium.QMPages
{
    class Visuals
    {
        public static void InitializePage(QMNestedMenu movementButton, Sprite buttonImage)
        {
            new QMToggleButton(movementButton, 2f, 0, "Bone ESP", () =>
            {
                BoneESP.SetEnabled(true);
            }, delegate
            {
                BoneESP.SetEnabled(false);
            }, "Toggle Flight Mode", false, buttonImage);

            new QMToggleButton(movementButton, 3f, 0, "Box ESP", () =>
            {
                BoxESP.SetEnabled(true);
            }, delegate
            {
                BoxESP.SetEnabled(false);
            }, "Toggle Flight Mode", false, buttonImage);
        }
    }
}
