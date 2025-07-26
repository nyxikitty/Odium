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
        private static bool colliderWasEnabled = true;
        private static float cachedAnimatorSpeed = 1f;
        private static Animator playerAnimator = null;

        public static bool FlyEnabled = false;
        public static float FlySpeed = 2.5f;
        public static float DefaultFlySpeed = 2.5f;
        public static bool DirectionalFlight = true;
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

        // New speed control methods
        public static void IncreaseFlySpeed()
        {
            FlySpeed += 0.5f;
            if (FlySpeed > 50f)
                FlySpeed = 50f;
        }

        public static void DecreaseFlySpeed()
        {
            FlySpeed -= 0.5f;
            // Prevent speed from going below 0.1f
            if (FlySpeed < 0.1f)
                FlySpeed = 0.1f;
        }

        public static void ResetFlySpeed()
        {
            FlySpeed = DefaultFlySpeed;
        }

        // Method to set custom default speed
        public static void SetDefaultFlySpeed(float newDefaultSpeed)
        {
            DefaultFlySpeed = newDefaultSpeed;
        }

        private static void EnableFly()
        {
            if (!setupedNormalFly) return;

            cachedGravity = PlayerWrapper.LocalPlayer.field_Private_VRCPlayerApi_0.GetGravityStrength();
            PlayerWrapper.LocalPlayer.field_Private_VRCPlayerApi_0.SetGravityStrength(0f);

            // Store original collider state and disable more carefully
            var collider = PlayerWrapper.LocalPlayer.gameObject.GetComponent<Collider>();
            if (collider != null)
            {
                colliderWasEnabled = collider.enabled;
                collider.enabled = false;
            }

            // Freeze avatar animations to keep player in standing pose
            playerAnimator = PlayerWrapper.LocalPlayer.gameObject.GetComponentInChildren<Animator>();
            if (playerAnimator != null)
            {
                cachedAnimatorSpeed = playerAnimator.speed;
                playerAnimator.speed = 0f; // Freeze animations instead of disabling
            }

            setupedNormalFly = false;
        }

        private static void DisableFly()
        {
            if (setupedNormalFly) return;

            PlayerWrapper.LocalPlayer.field_Private_VRCPlayerApi_0.SetGravityStrength(cachedGravity);

            // Restore original collider state
            var collider = PlayerWrapper.LocalPlayer.gameObject?.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = colliderWasEnabled;

            // Restore animator speed
            if (playerAnimator != null)
            {
                playerAnimator.speed = cachedAnimatorSpeed;
            }

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

            // VR Input handling - fixed to use proper axes
            float vrVertical = Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickVertical"); // Up/Down
            float vrHorizontal = Input.GetAxis("Oculus_CrossPlatform_PrimaryThumbstickHorizontal"); // Left/Right
            float vrForward = Input.GetAxis("Oculus_CrossPlatform_PrimaryThumbstickVertical"); // Forward/Back

            // Get camera directions for directional flight
            Vector3 forwardDirection, rightDirection, upDirection;

            if (DirectionalFlight)
            {
                // True directional flight - use camera's actual orientation
                forwardDirection = cameraTransform.forward;
                rightDirection = cameraTransform.right;
                upDirection = cameraTransform.up;
            }
            else
            {
                // Horizontal flight - flatten directions
                forwardDirection = cameraTransform.forward;
                forwardDirection.y = 0f;
                forwardDirection.Normalize();

                rightDirection = cameraTransform.right;
                rightDirection.y = 0f;
                rightDirection.Normalize();

                upDirection = Vector3.up;
            }

            // VR Movement
            if (Mathf.Abs(vrVertical) > 0.1f)
                move += upDirection * vrVertical * finalSpeed * Time.deltaTime;

            if (Mathf.Abs(vrHorizontal) > 0.1f)
                move += rightDirection * vrHorizontal * finalSpeed * Time.deltaTime;

            if (Mathf.Abs(vrForward) > 0.1f)
                move += forwardDirection * vrForward * finalSpeed * Time.deltaTime;

            // PC Keyboard Movement
            if (Input.GetKey(KeyCode.Q))
                move += -upDirection * finalSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.E))
                move += upDirection * finalSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.A))
                move += -rightDirection * finalSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.D))
                move += rightDirection * finalSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.S))
                move += -forwardDirection * finalSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.W))
                move += forwardDirection * finalSpeed * Time.deltaTime;

            // Apply movement
            if (move.magnitude > 0)
            {
                transform.position += move;
            }
        }
    }
}