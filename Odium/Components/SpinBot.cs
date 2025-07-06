using UnityEngine;
using VRC.SDKBase;

namespace Odium.Components
{
    public class SpinBot
    {
        public static float rotationSpeed = 500f;
        public static bool SpinBotbool;
        
        public static void Activate(bool state)
        {
            if (state)
            {
                SpinBotbool = true;
            }
            else
            {
                SpinBotbool = false;
            }
        }
        
        public static void Update()
        {
            if (!SpinBotbool)
            {
                Networking.LocalPlayer.gameObject.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
            }
            else
            {
            }
        }
    }
}