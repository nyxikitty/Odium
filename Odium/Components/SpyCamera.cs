using System;
using UnityEngine;
using VRC;
using VRC.SDKBase;
using MelonLoader;

namespace Odium.Components
{
    public static class SpyCamera
    {
        private static Camera _spyCam;
        private static Player _targetPlayer;
        private static bool _isActive;
        private static MelonMod _modInstance;
        private const float UpdateInterval = 0.0167f; // ~60fps (1/60)

        public static void Initialize(MelonMod modInstance)
        {
            _modInstance = modInstance;
            MelonCoroutines.Start(UpdateCameraPosition());
        }

        public static void Toggle(Player player, bool state)
        {
            if (state)
            {
                EnableSpyCamera(player);
            }
            else
            {
                DisableSpyCamera();
            }
        }

        private static void EnableSpyCamera(Player player)
        {
            if (_isActive || player == null) return;

            _targetPlayer = player;

            // Create camera object
            _spyCam = new GameObject("SpyCamera").AddComponent<Camera>();

            // Configure camera
            _spyCam.fieldOfView = 60f;
            _spyCam.nearClipPlane = 0.1f;
            _spyCam.farClipPlane = 1000f;
            _spyCam.depth = 1; // Make sure it renders on top

            _isActive = true;
            MelonLogger.Msg("Spy camera enabled");
        }

        private static void DisableSpyCamera()
        {
            if (!_isActive) return;

            if (_spyCam != null)
            {
                UnityEngine.Object.Destroy(_spyCam.gameObject);
                _spyCam = null;
            }

            _targetPlayer = null;
            _isActive = false;
            MelonLogger.Msg("Spy camera disabled");
        }

        private static System.Collections.IEnumerator UpdateCameraPosition()
        {
            while (true)
            {
                try
                {
                    if (_isActive && _targetPlayer != null)
                    {
                        var vrcPlayer = _targetPlayer.GetComponent<VRCPlayer>();
                        if (vrcPlayer != null)
                        {
                            var head = vrcPlayer.transform.Find("TrackingData/Head");
                            if (head != null && _spyCam != null)
                            {
                                _spyCam.transform.SetPositionAndRotation(head.position, head.rotation);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MelonLogger.Error($"Error updating spy camera: {e}");
                }
                yield return new WaitForSeconds(UpdateInterval); // 60fps update
            }
        }
    }
}