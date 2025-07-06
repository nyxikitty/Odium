using System;
using UnityEngine;
using VRC.SDKBase;
using Odium.Components;

namespace Odium.Components
{
    public class Jetpack
    {
        public static bool Jetpackbool;

        public static void Activate(bool state)
        {
            if (state)
            {
                System.Console.WriteLine("Jetpack ON");
                Jetpackbool = true;
            }
            else
            {
                System.Console.WriteLine("Jetpack OFF");
                Jetpackbool = false;
            }
        }

        public static void Update()
        {
            
            if (Jetpackbool && Networking.LocalPlayer != null && (Bindings.Button_Jump.GetState(0) || Input.GetKey((KeyCode)32)))
            {
                try
                {
                    Vector3 velocity = Networking.LocalPlayer.GetVelocity();
                    velocity.y = Networking.LocalPlayer.GetJumpImpulse();
                    Networking.LocalPlayer.SetVelocity(velocity);
                }
                catch(Exception e)
                {
                    System.Console.WriteLine("FUCK IT" + e);
                }
            }
            else
            {
            }
        }
    }
}