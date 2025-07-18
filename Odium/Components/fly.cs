using Odium.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Odium.Modules
{
    public class FlyComponent
    {
        private static bool setupedNormalFly = true;
        private static float cachedGravity = 0f;

        public static bool FlyEnabled = false;
        public static float FlySpeed = 5f;
        public static DateTime LastKeyCheck = DateTime.Now;

        public static void OnUpdate()
        {
            var currentTime = DateTime.Now;
            if ((currentTime - LastKeyCheck).TotalMilliseconds >= 10)
            {
                if (Input.GetKeyDown(KeyCode.F) && Input.GetKey(KeyCode.LeftControl))
                {
                    ToggleFly();
                }

                LastKeyCheck = currentTime;
            }

            if (PlayerWrapper.LocalPlayer == null || PlayerWrapper.LocalPlayer.field_Private_VRCPlayerApi_0 == null)
                return;

            if (!FlyEnabled)
            {
                DisableFly();
                return;
            }

            EnableFly();
            HandleMovement();
        }

        private static void ToggleFly()
        {
            FlyEnabled = !FlyEnabled;
            if (!FlyEnabled)
            {
                DisableFly();
            }
        }

        private static void EnableFly()
        {
            if (!setupedNormalFly) return;

            cachedGravity = PlayerWrapper.LocalPlayer.field_Private_VRCPlayerApi_0.GetGravityStrength();
            PlayerWrapper.LocalPlayer.field_Private_VRCPlayerApi_0.SetGravityStrength(0f);

            var collider = PlayerWrapper.LocalPlayer.gameObject.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;

            setupedNormalFly = false;
        }

        private static void DisableFly()
        {
            if (setupedNormalFly) return;

            PlayerWrapper.LocalPlayer.field_Private_VRCPlayerApi_0.SetGravityStrength(cachedGravity);
            var collider = PlayerWrapper.LocalPlayer.gameObject?.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = true;

            setupedNormalFly = true;
        }

        private static void HandleMovement()
        {
            var transform = PlayerWrapper.LocalPlayer.gameObject.transform;
            var cameraTransform = Camera.main?.transform;
            if (cameraTransform == null) return;

            float finalSpeed = FlySpeed;
            if (Input.GetKey(KeyCode.LeftShift))
                finalSpeed *= 2f;

            Vector3 move = Vector3.zero;

            float vrVertical = Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickVertical");
            float vrHorizontal = Input.GetAxis("Oculus_CrossPlatform_PrimaryThumbstickHorizontal");
            float vrForward = Input.GetAxis("Oculus_CrossPlatform_PrimaryThumbstickVertical");

            Vector3 forwardDirection = cameraTransform.forward;
            forwardDirection.y = 0f;
            forwardDirection.Normalize();

            Vector3 rightDirection = cameraTransform.right;
            rightDirection.y = 0f;
            rightDirection.Normalize();

            if (vrVertical != 0f)
                move += Vector3.up * vrVertical * finalSpeed * Time.deltaTime;

            if (vrHorizontal != 0f)
                move += rightDirection * vrHorizontal * finalSpeed * Time.deltaTime;

            if (vrForward != 0f)
                move += forwardDirection * vrForward * finalSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.Q))
                move += Vector3.down * finalSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.E))
                move += Vector3.up * finalSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.A))
                move += -rightDirection * finalSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.D))
                move += rightDirection * finalSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.S))
                move += -forwardDirection * finalSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.W))
                move += forwardDirection * finalSpeed * Time.deltaTime;

            transform.position += move;
        }
    }
}