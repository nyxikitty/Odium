using System;
using UnityEngine;
using VRC.SDKBase;
using MelonLoader;

namespace Odium.Components
{
    public static class Jetpack
    {
        private static bool jetpackEnabled = false;

        public static bool IsEnabled => jetpackEnabled;

        public static void Activate(bool state)
        {
            jetpackEnabled = state;

            if (state)
            {
                MelonLogger.Msg("Jetpack ON");
            }
            else
            {
                MelonLogger.Msg("Jetpack OFF");
            }
        }

        public static void Update()
        {
            if (!jetpackEnabled || Networking.LocalPlayer == null)
                return;

            bool jumpPressed = Input.GetKey(KeyCode.Space);

            try
            {
                if (Bindings.Button_Jump != null)
                    jumpPressed = jumpPressed || Bindings.Button_Jump.GetState(0);
            }
            catch
            {
            }

            if (jumpPressed)
            {
                ApplyJetpack();
            }
        }

        private static void ApplyJetpack()
        {
            try
            {
                var player = Networking.LocalPlayer;
                Vector3 velocity = player.GetVelocity();

                velocity.y = player.GetJumpImpulse();
                player.SetVelocity(velocity);
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Jetpack error: {e.Message}");
            }
        }

        public static void Toggle()
        {
            Activate(!jetpackEnabled);
        }
    }
}