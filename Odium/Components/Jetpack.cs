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
                Jetpackbool = true;
            }
            else
            {
                Jetpackbool = false;
            }
        }
        
        public static void Update()
        {
            if (Jetpackbool && (Bindings.Button_Jump.GetState(0) || Input.GetKey((KeyCode)32)))
            {
                System.Console.WriteLine("Jetpack ON");
                Vector3 velocity = Networking.LocalPlayer.GetVelocity();
                velocity.y = Networking.LocalPlayer.GetJumpImpulse();
                Networking.LocalPlayer.SetVelocity(velocity);
            }
            else
            {
                System.Console.WriteLine("Jetpack OFF");
            }
        }
    }
}