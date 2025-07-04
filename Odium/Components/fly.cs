using Odium.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Odium.Modules
{
    internal class FlyComponent
    {
        private static bool setupedNormalFly = true;
        private static float cachedGravity = 0f;

        public static bool FlyEnabled = false;
        public static float FlySpeed = 0.05f;
        public static DateTime LastKeyCheck = DateTime.Now;
        public static void OnUpdate()
        {
            var currentTime = DateTime.Now;
            if ((currentTime - LastKeyCheck).TotalMilliseconds >= 10)
            {
                if (Input.GetKeyDown(KeyCode.F) && Input.GetKey(KeyCode.LeftControl))
                {
                    FlyEnabled = !FlyEnabled;
                }

                LastKeyCheck = currentTime;
            }

            if (PlayerWrapper.LocalPlayer == null || PlayerWrapper.LocalPlayer == null) return;
            


            if (!FlyEnabled)
            {
                if (!setupedNormalFly)
                {
                    PlayerWrapper.LocalPlayer.field_Private_VRCPlayerApi_0.SetGravityStrength(cachedGravity);
                    var collider = PlayerWrapper.LocalPlayer.gameObject?.GetComponent<Collider>();
                    if (collider != null)
                        collider.enabled = true;

                    setupedNormalFly = true;
                }
                return;
            }

            if (setupedNormalFly)
            {
                cachedGravity = PlayerWrapper.LocalPlayer.field_Private_VRCPlayerApi_0.GetGravityStrength();
                PlayerWrapper.LocalPlayer.field_Private_VRCPlayerApi_0.SetGravityStrength(0f);
                var collider = PlayerWrapper.LocalPlayer.gameObject.GetComponent<Collider>();
                if (collider != null)
                    collider.enabled = false;

                setupedNormalFly = false;
            }

            var transform = PlayerWrapper.LocalPlayer.gameObject.transform;
            var cameraTransform = Camera.main?.transform;
            if (cameraTransform == null) return;

            float finalSpeed = FlySpeed / 2f;
            if (Input.GetKey(KeyCode.LeftShift))
                finalSpeed = FlySpeed + (0.5f / 70f);

            Vector3 move = Vector3.zero;

            if (Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickVertical") < 0f)
                move += Vector3.down * finalSpeed;
            if (Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickVertical") > 0f)
                move += Vector3.up * finalSpeed;

            if (Input.GetAxis("Oculus_CrossPlatform_PrimaryThumbstickHorizontal") < 0f)
                move += -transform.right * finalSpeed;
            if (Input.GetAxis("Oculus_CrossPlatform_PrimaryThumbstickHorizontal") > 0f)
                move += transform.right * finalSpeed;

            if (Input.GetAxis("Oculus_CrossPlatform_PrimaryThumbstickVertical") < 0f)
                move += -transform.forward * finalSpeed;
            if (Input.GetAxis("Oculus_CrossPlatform_PrimaryThumbstickVertical") > 0f)
                move += transform.forward * finalSpeed;

            if (Input.GetKey(KeyCode.Q))
                move += -cameraTransform.up * finalSpeed;
            if (Input.GetKey(KeyCode.E))
                move += cameraTransform.up * finalSpeed;
            if (Input.GetKey(KeyCode.A))
                move += -cameraTransform.right * finalSpeed;
            if (Input.GetKey(KeyCode.D))
                move += cameraTransform.right * finalSpeed;
            if (Input.GetKey(KeyCode.S))
                move += -cameraTransform.forward * finalSpeed;
            if (Input.GetKey(KeyCode.W))
                move += cameraTransform.forward * finalSpeed;

            transform.position += move;
        }
    }
}
