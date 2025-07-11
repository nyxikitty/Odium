using System;
using UnityEngine;
using VRC;

namespace Odium.Components
{
    // Awoochy added this fuck ass AI generated code
    public class ThirdPersonComponent
    {
        private static bool isThirdPerson = false;
        private static Camera mainCamera;
        private static VRC.Player localPlayer;
        private static Transform originalCameraParent;
        private static Vector3 originalCameraPosition;
        private static Quaternion originalCameraRotation;
        private static float cameraHeight = 1.8f;
        private static float cameraDistance = 2.5f;
        private static float smoothTime = 0.3f;
        private static bool initialized = false;
        private static Vector3 currentVelocity;
        private static Transform headBone;
        private static Transform chestBone;

        // NEW: VRChat-specific approach - use a separate camera
        private static Camera thirdPersonCamera;
        private static GameObject thirdPersonCameraObject;

        // NEW: Head visibility control
        private static Renderer[] headRenderers;
        private static bool headVisible = true;

        public static void Initialize()
        {
            if (initialized) return;

            try
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    MelonLoader.MelonLogger.Error("Main camera not found!");
                    return;
                }

                localPlayer = VRC.Player.prop_Player_0;
                if (localPlayer == null)
                {
                    MelonLoader.MelonLogger.Warning("Local player not found, will retry...");
                    return;
                }

                // Store original camera settings
                originalCameraParent = mainCamera.transform.parent;
                originalCameraPosition = mainCamera.transform.localPosition;
                originalCameraRotation = mainCamera.transform.localRotation;

                GetHeadBone();
                CreateThirdPersonCamera();

                initialized = true;
                MelonLoader.MelonLogger.Msg("Third Person Component initialized");
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error($"Failed to initialize Third Person Component: {e.Message}");
            }
        }

        private static void CreateThirdPersonCamera()
        {
            try
            {
                // Create a separate camera object for third person
                thirdPersonCameraObject = new GameObject("ThirdPersonCamera");
                thirdPersonCamera = thirdPersonCameraObject.AddComponent<Camera>();

                // Copy settings from main camera
                thirdPersonCamera.CopyFrom(mainCamera);
                thirdPersonCamera.enabled = false; // Start disabled

                // Make sure it's not destroyed on scene load
                UnityEngine.Object.DontDestroyOnLoad(thirdPersonCameraObject);

                MelonLoader.MelonLogger.Msg("Third person camera created");
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error($"Failed to create third person camera: {e.Message}");
            }
        }

        private static void GetHeadBone()
        {
            try
            {
                if (localPlayer != null)
                {
                    Animator animator = localPlayer.GetComponentInChildren<Animator>();
                    if (animator != null && animator.isHuman)
                    {
                        headBone = animator.GetBoneTransform(HumanBodyBones.Head);
                        if (headBone == null)
                        {
                            headBone = animator.GetBoneTransform(HumanBodyBones.Neck);
                        }

                        // NEW: Get chest bone for camera attachment
                        chestBone = animator.GetBoneTransform(HumanBodyBones.Chest);
                        if (chestBone == null)
                        {
                            chestBone = animator.GetBoneTransform(HumanBodyBones.Spine);
                        }
                        if (chestBone == null)
                        {
                            chestBone = animator.GetBoneTransform(HumanBodyBones.Hips);
                        }

                        MelonLoader.MelonLogger.Msg($"Head bone: {headBone?.name}, Chest bone: {chestBone?.name}");

                        // NEW: Find head renderers to hide them in third person
                        FindHeadRenderers();
                    }
                }
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error($"Error getting bones: {e.Message}");
            }
        }

        private static void FindHeadRenderers()
        {
            try
            {
                if (headBone != null)
                {
                    // Get all renderers in head bone and its children
                    headRenderers = headBone.GetComponentsInChildren<Renderer>();
                    MelonLoader.MelonLogger.Msg($"Found {headRenderers?.Length ?? 0} head renderers");
                }
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error($"Error finding head renderers: {e.Message}");
            }
        }

        private static void SetHeadVisibility(bool visible)
        {
            try
            {
                if (headRenderers != null)
                {
                    foreach (var renderer in headRenderers)
                    {
                        if (renderer != null)
                        {
                            renderer.enabled = visible;
                        }
                    }
                    headVisible = visible;
                    MelonLoader.MelonLogger.Msg($"Head visibility set to: {visible}");
                }
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error($"Error setting head visibility: {e.Message}");
            }
        }

        public static void SetThirdPerson(bool enabled)
        {
            if (!initialized)
            {
                Initialize();
                if (!initialized) return;
            }

            isThirdPerson = enabled;

            try
            {
                if (enabled)
                {
                    if (thirdPersonCamera == null)
                    {
                        CreateThirdPersonCamera();
                    }

                    if (thirdPersonCamera != null && mainCamera != null)
                    {
                        // Disable main camera, enable third person camera
                        mainCamera.enabled = false;
                        thirdPersonCamera.enabled = true;

                        // Position third person camera immediately at a reasonable location
                        SetInitialThirdPersonPosition();

                        MelonLoader.MelonLogger.Msg("Enabled third person view");
                    }
                }
                else
                {
                    RestoreFirstPerson();
                }
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error($"Error setting third person: {e.Message}");
            }
        }

        private static void SetInitialThirdPersonPosition()
        {
            if (thirdPersonCamera == null || localPlayer == null) return;

            try
            {
                Vector3 playerPosition = localPlayer.transform.position;
                Vector3 playerForward = localPlayer.transform.forward;

                // Use head position if available
                Vector3 followPosition = playerPosition + Vector3.up * 1.7f;
                if (headBone != null)
                {
                    followPosition = headBone.position;
                }

                // Position camera behind and above player
                Vector3 cameraPos = followPosition + (-playerForward * cameraDistance) + (Vector3.up * cameraHeight);
                thirdPersonCamera.transform.position = cameraPos;

                // Look at player
                Vector3 lookDirection = followPosition - cameraPos;
                if (lookDirection.magnitude > 0.01f)
                {
                    thirdPersonCamera.transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
                }
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error($"Error setting initial position: {e.Message}");
            }
        }

        public static void ToggleThirdPerson()
        {
            SetThirdPerson(!isThirdPerson);
        }

        public static void Update()
        {
            if (!initialized)
            {
                Initialize();
                if (!initialized) return;
            }

            // Check for F5 key to toggle third person
            if (Input.GetKeyDown(KeyCode.F5))
            {
                ToggleThirdPerson();
            }

            if (isThirdPerson && thirdPersonCamera != null && thirdPersonCamera.enabled)
            {
                UpdateThirdPersonCamera();
            }
        }

        private static void UpdateThirdPersonCamera()
        {
            if (thirdPersonCamera == null || localPlayer == null) return;

            try
            {
                Vector3 playerPosition = localPlayer.transform.position;
                Vector3 playerForward = localPlayer.transform.forward;

                // Use head bone position if available
                Vector3 followPosition = playerPosition + Vector3.up * 1.7f;
                if (headBone != null)
                {
                    followPosition = headBone.position;
                }

                // Calculate target position
                Vector3 targetPosition = followPosition + (-playerForward * cameraDistance) + (Vector3.up * cameraHeight);

                // Calculate target rotation
                Vector3 lookDirection = followPosition - targetPosition;
                Quaternion targetRotation = Quaternion.identity;
                if (lookDirection.magnitude > 0.01f)
                {
                    targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
                }

                // Smooth movement
                if (smoothTime > 0)
                {
                    thirdPersonCamera.transform.position = Vector3.SmoothDamp(
                        thirdPersonCamera.transform.position,
                        targetPosition,
                        ref currentVelocity,
                        smoothTime
                    );
                    thirdPersonCamera.transform.rotation = Quaternion.Slerp(
                        thirdPersonCamera.transform.rotation,
                        targetRotation,
                        Time.deltaTime * (1f / smoothTime)
                    );
                }
                else
                {
                    thirdPersonCamera.transform.position = targetPosition;
                    thirdPersonCamera.transform.rotation = targetRotation;
                }
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error($"Error updating third person camera: {e.Message}");
            }
        }

        private static void RestoreFirstPerson()
        {
            try
            {
                if (thirdPersonCamera != null)
                {
                    thirdPersonCamera.enabled = false;
                }

                if (mainCamera != null)
                {
                    mainCamera.enabled = true;
                }

                currentVelocity = Vector3.zero;
                MelonLoader.MelonLogger.Msg("Restored first person view");
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error($"Error restoring first person: {e.Message}");
            }
        }

        // Alternative approach: Camera offset method (if the above doesn't work)
        public static void SetThirdPersonOffset(bool enabled)
        {
            if (!initialized)
            {
                Initialize();
                if (!initialized) return;
            }

            try
            {
                if (enabled)
                {
                    // Don't detach camera, just offset it within VRChat's system
                    if (mainCamera != null && headBone != null)
                    {
                        // Keep camera parented to head but offset it to see the head
                        mainCamera.transform.SetParent(headBone, false);

                        // Position camera behind and above to see the head
                        mainCamera.transform.localPosition = new Vector3(0, 0.8f, -2.5f);

                        // Angle camera down slightly to look at the head/body
                        mainCamera.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);

                        isThirdPerson = true;
                        MelonLoader.MelonLogger.Msg("Enabled third person offset view");
                    }
                }
                else
                {
                    RestoreFirstPersonOffset();
                }
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error($"Error setting third person offset: {e.Message}");
            }
        }

        private static void RestoreFirstPersonOffset()
        {
            try
            {
                if (mainCamera != null)
                {
                    if (originalCameraParent != null)
                    {
                        mainCamera.transform.SetParent(originalCameraParent, false);
                    }
                    mainCamera.transform.localPosition = originalCameraPosition;
                    mainCamera.transform.localRotation = originalCameraRotation;
                }

                // NEW: Restore head visibility
                SetHeadVisibility(true);

                isThirdPerson = false;
                MelonLoader.MelonLogger.Msg("Restored first person offset view");
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error($"Error restoring first person offset: {e.Message}");
            }
        }

        public static void SetCameraDistance(float distance)
        {
            cameraDistance = Mathf.Clamp(distance, 0.5f, 10f);
        }

        public static void SetCameraHeight(float height)
        {
            cameraHeight = Mathf.Clamp(height, -2f, 5f);
        }

        public static void SetSmoothTime(float time)
        {
            smoothTime = Mathf.Clamp(time, 0f, 1f);
        }

        public static void SetOffsetCameraPosition(Vector3 localPosition)
        {
            if (isThirdPerson && mainCamera != null && headBone != null)
            {
                mainCamera.transform.localPosition = localPosition;
            }
        }

        public static void SetOffsetCameraRotation(Vector3 eulerAngles)
        {
            if (isThirdPerson && mainCamera != null && headBone != null)
            {
                mainCamera.transform.localRotation = Quaternion.Euler(eulerAngles);
            }
        }

        public static void SetThirdPersonPreset(string preset)
        {
            if (!isThirdPerson) return;

            switch (preset.ToLower())
            {
                case "default":
                    SetOffsetCameraPosition(new Vector3(0, 0.8f, -2.5f));
                    SetOffsetCameraRotation(new Vector3(15f, 0f, 0f));
                    break;
                case "close":
                    SetOffsetCameraPosition(new Vector3(0, 0.5f, -1.5f));
                    SetOffsetCameraRotation(new Vector3(10f, 0f, 0f));
                    break;
                case "high":
                    SetOffsetCameraPosition(new Vector3(0, 1.5f, -3f));
                    SetOffsetCameraRotation(new Vector3(25f, 0f, 0f));
                    break;
                case "side":
                    SetOffsetCameraPosition(new Vector3(1.5f, 0.5f, -1f));
                    SetOffsetCameraRotation(new Vector3(0f, -30f, 0f));
                    break;
                case "overhead":
                    SetOffsetCameraPosition(new Vector3(0, 3f, -1f));
                    SetOffsetCameraRotation(new Vector3(45f, 0f, 0f));
                    break;
            }
        }

        public static void ToggleHeadVisibility()
        {
            SetHeadVisibility(!headVisible);
        }

        public static void ForceHeadVisibility(bool visible)
        {
            SetHeadVisibility(visible);
        }

        public static void DebugAvatarStructure()
        {
            try
            {
                if (localPlayer != null)
                {
                    Animator animator = localPlayer.GetComponentInChildren<Animator>();
                    if (animator != null)
                    {
                        MelonLoader.MelonLogger.Msg("=== Avatar Debug Info ===");
                        MelonLoader.MelonLogger.Msg($"Head bone: {headBone?.name ?? "NULL"}");
                        MelonLoader.MelonLogger.Msg($"Chest bone: {chestBone?.name ?? "NULL"}");
                        MelonLoader.MelonLogger.Msg($"Head renderers found: {headRenderers?.Length ?? 0}");

                        if (headRenderers != null)
                        {
                            for (int i = 0; i < headRenderers.Length; i++)
                            {
                                var renderer = headRenderers[i];
                                MelonLoader.MelonLogger.Msg($"  Renderer {i}: {renderer?.name ?? "NULL"} - Enabled: {renderer?.enabled ?? false}");
                            }
                        }

                        MelonLoader.MelonLogger.Msg($"Camera parent: {mainCamera?.transform?.parent?.name ?? "NULL"}");
                        MelonLoader.MelonLogger.Msg($"Camera local pos: {mainCamera?.transform?.localPosition}");
                    }
                }
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error($"Error debugging avatar: {e.Message}");
            }
        }

        public static void TryDifferentBone(string boneName)
        {
            try
            {
                if (localPlayer != null && isThirdPerson)
                {
                    Animator animator = localPlayer.GetComponentInChildren<Animator>();
                    if (animator != null)
                    {
                        HumanBodyBones boneType;
                        if (System.Enum.TryParse(boneName, out boneType))
                        {
                            Transform bone = animator.GetBoneTransform(boneType);
                            if (bone != null && mainCamera != null)
                            {
                                mainCamera.transform.SetParent(bone, false);
                                mainCamera.transform.localPosition = new Vector3(0, 1f, -2f);
                                mainCamera.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
                                MelonLoader.MelonLogger.Msg($"Attached camera to {boneName} bone: {bone.name}");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error($"Error trying bone {boneName}: {e.Message}");
            }
        }

        public static void OnPlayerJoined()
        {
            if (localPlayer == null)
            {
                localPlayer = VRC.Player.prop_Player_0;
                GetHeadBone();
            }
        }

        public static void OnAvatarChanged()
        {
            GetHeadBone();

            if (isThirdPerson)
            {
                SetHeadVisibility(false);
            }
        }

        public static void Destroy()
        {
            if (isThirdPerson)
            {
                RestoreFirstPerson();
                RestoreFirstPersonOffset();
            }

            SetHeadVisibility(true);

            if (thirdPersonCameraObject != null)
            {
                UnityEngine.Object.Destroy(thirdPersonCameraObject);
                thirdPersonCamera = null;
                thirdPersonCameraObject = null;
            }

            initialized = false;
            isThirdPerson = false;
            mainCamera = null;
            localPlayer = null;
            headBone = null;
            chestBone = null;
            headRenderers = null;
            currentVelocity = Vector3.zero;

            MelonLoader.MelonLogger.Msg("Third Person Component destroyed");
        }

        public static bool IsThirdPerson => isThirdPerson;
        public static bool IsInitialized => initialized;
        public static bool IsHeadVisible => headVisible;
        public static float CameraDistance => cameraDistance;
        public static float CameraHeight => cameraHeight;
        public static float SmoothTime => smoothTime;
        public static string AttachedBone => mainCamera?.transform?.parent?.name ?? "None";
    }
}