using OdiumLoader;
using Odium.Wrappers;
using UnityEngine;
using VRC.SDKBase;

namespace Odium.Modules
{
    public class SpinBotModule : OdiumModule
    {
        private const string DefaultAvatarObjectName = "Avatar";
        private const string FallbackAvatarObjectName = "avatar";

        private static Transform _avatarTransform;
        private static float _rotationSpeed = 500f;
        private static bool _isActive;

        public override void OnUpdate()
        {
            if (!_isActive) return;

            EnsureAvatarTransform();
            RotateAvatar();
        }

        public static void Toggle() => SetActive(!_isActive);

        public static void SetSpeed(float speed) => _rotationSpeed = Mathf.Clamp(speed, 0f, 2000f);

        public static void SetActive(bool state)
        {
            _isActive = state;

            if (state && _avatarTransform == null)
            {
                CacheAvatarTransform();
            }
        }

        private static void EnsureAvatarTransform()
        {
            if (_avatarTransform == null)
            {
                CacheAvatarTransform();

                if (_avatarTransform == null)
                {
                    SetActive(false);
                    Debug.LogWarning("[SpinBot] Failed to locate avatar transform - disabling");
                }
            }
        }

        private static void RotateAvatar()
        {
            if (_avatarTransform != null)
            {
                _avatarTransform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
            }
        }

        private static void CacheAvatarTransform()
        {
            var localPlayer = PlayerWrapper.LocalPlayer;
            if (localPlayer == null) return;

            var animator = localPlayer.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                _avatarTransform = animator.transform;
                return;
            }

            _avatarTransform = localPlayer.transform.Find(DefaultAvatarObjectName) ??
                             localPlayer.transform.Find(FallbackAvatarObjectName);
        }
    }
}