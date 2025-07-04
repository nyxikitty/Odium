using UnityEngine;
using VRC.SDKBase;

namespace Odium.Components
{
    public class SpyCamera : MonoBehaviour
    {
        public static SpyCamera Instance;
        public static Camera _spyCam;
        public static VRCPlayerApi _targetPlayer;
        public static bool _isActive;

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                DestroyImmediate(this);
            }
        }

        public static void Toggle(VRCPlayerApi player, bool state)
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

        private static void EnableSpyCamera(VRCPlayerApi player)
        {
            if (_isActive || player == null) return;

            _targetPlayer = player;

            // Create camera object
            GameObject camObj = new GameObject("SpyCamera");
            camObj.transform.SetParent(_targetPlayer.GetBoneTransform(HumanBodyBones.Head));
            _spyCam = camObj.AddComponent<Camera>();

            // Configure camera
            _spyCam.fieldOfView = 60f;
            _spyCam.nearClipPlane = 0.1f;
            _spyCam.farClipPlane = 1000f;
            _spyCam.depth = 1; // Make sure it renders on top

            _isActive = true;
        }

        private static void DisableSpyCamera()
        {
            if (!_isActive) return;

            if (_spyCam != null)
            {
                Destroy(_spyCam.gameObject);
                _spyCam = null;
            }

            _targetPlayer = null;
            _isActive = false;
        }

        public static void LateUpdate()
        {
            if (!_isActive || _targetPlayer == null || !_targetPlayer.IsValid()) return;

            // Get the player's tracking data for their head
            VRCPlayerApi.TrackingData headData = _targetPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

            // Update camera position and rotation to match the player's head
            if (_spyCam != null)
            {
                _spyCam.transform.position = headData.position;
                _spyCam.transform.rotation = headData.rotation;
            }
        }

        public static void OnDestroy()
        {
            DisableSpyCamera();
        }
    }
}