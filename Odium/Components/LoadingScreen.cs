using System;
using System.Collections;
using System.IO;
using MelonLoader;
using Odium.Components;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Odium.Modules
{
    public static class OdiumAssetBundleLoader
    {
        private static AssetBundle _loadingScreenBundle;
        private static GameObject _loadingScreenPrefab;
        private static GameObject _instantiatedLoadingScreen;
        public static AudioSource _customAudioSource;
        public static AudioClip _customAudioClip;

        private static readonly string LoadingScreenPath = Path.Combine(Components.ModSetup.GetOdiumFolderPath(), "AssetBundles", "odium.loadingscreen");
        private static readonly string LoadingMusicPath = Path.Combine(Components.ModSetup.GetOdiumFolderPath(), "Audio", "loadingmusic.mp3");

        private static readonly Color FemboyPink = new Color(0.792f, 0.008f, 0.988f, 1f);

        public static void Initialize()
        {
            OdiumConsole.Log("AssetLoader", "Starting loading screen asset loader...", LogLevel.Info);
            LoadAndInstantiateLoadingScreen();
            GameObject.Find("MenuContent/Popups/LoadingPopup/3DElements").SetActive(false);
            GameObject.Find("MenuContent/Popups/LoadingPopup/ProgressPanel/Parent_Loading_Progress/GoButton").GetComponent<Image>().m_Color = Color.black;
            GameObject.Find("MenuContent/Popups/LoadingPopup/ProgressPanel/Parent_Loading_Progress/Decoration_Right").GetComponent<Image>().m_Color = Color.black;
            GameObject.Find("MenuContent/Popups/LoadingPopup/ProgressPanel/Parent_Loading_Progress/Decoration_Left").GetComponent<Image>().m_Color = Color.black;

            string image = Path.Combine(Environment.CurrentDirectory, "Odium", "ButtonBackground.png");
            Sprite sprite = SpriteUtil.LoadFromDisk(image);
            GameObject.Find("MenuContent/Popups/LoadingPopup/ProgressPanel/Parent_Loading_Progress/Panel_Backdrop").GetComponent<Image>().m_Color = Color.black;
            GameObject.Find("MenuContent/Popups/LoadingPopup/ButtonMiddle").GetComponent<Image>().m_Color = Color.black;
            GameObject.Find("MenuContent/Popups/LoadingPopup/ButtonMiddle/Text").GetComponent<TextMeshProUGUIEx>().m_fontColor = Color.white;
            GameObject.Find("MenuContent/Popups/LoadingPopup/ProgressPanel/Parent_Loading_Progress/GoButton/Text").GetComponent<TextMeshProUGUIEx>().m_fontColor = FemboyPink;
            GameObject.Find("MenuContent/Popups/LoadingPopup/ButtonMiddle/Text").GetComponent<TextMeshProUGUIEx>().m_fontColor = FemboyPink;
        }

        private static void LoadAndInstantiateLoadingScreen()
        {
            MelonCoroutines.Start(LoadAssetBundle());
            MelonCoroutines.Start(LoadPrefabFromBundle());
            ChangeLoadingScreen();
        }

        public static void ChangeLoadingScreen()
        {
            MelonCoroutines.Start(InstantiateLoadingScreen());
            MelonCoroutines.Start(ApplyToVRChatLoading());
        }

        private static IEnumerator LoadAssetBundle()
        {
            OdiumConsole.Log("AssetLoader", "Loading AssetBundle from file...", LogLevel.Info);

            if (!File.Exists(LoadingScreenPath))
            {
                OdiumConsole.Log("AssetLoader", $"Loading screen file not found at: {LoadingScreenPath}", LogLevel.Error);
                yield break;
            }

            var bundleRequest = AssetBundle.LoadFromFileAsync(LoadingScreenPath);
            yield return bundleRequest;

            try
            {
                _loadingScreenBundle = bundleRequest.assetBundle;

                if (_loadingScreenBundle != null)
                {
                    OdiumConsole.LogGradient("AssetLoader", "AssetBundle loaded successfully!", LogLevel.Info, true);

                    var assetNames = _loadingScreenBundle.GetAllAssetNames();
                    OdiumConsole.Log("AssetLoader", $"Found {assetNames.Length} assets in bundle:", LogLevel.Info);

                    foreach (var assetName in assetNames)
                    {
                        OdiumConsole.Log("AssetLoader", $"  - {assetName}", LogLevel.Info);
                    }
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "LoadAssetBundle");
            }
        }

        private static IEnumerator LoadPrefabFromBundle()
        {
            if (_loadingScreenBundle == null)
            {
                yield break;
            }

            OdiumConsole.Log("AssetLoader", "Loading prefab from AssetBundle...", LogLevel.Info);

            try
            {
                string[] possibleNames = {
                    "Loading Screen",
                    "ParticleSystem",
                    "CustomLoadingScreen",
                    "loadingscreen",
                    "Loading",
                    "Screen"
                };

                foreach (var name in possibleNames)
                {
                    _loadingScreenPrefab = _loadingScreenBundle.LoadAsset<GameObject>(name);
                    if (_loadingScreenPrefab != null)
                    {
                        OdiumConsole.LogGradient("AssetLoader", $"Found loading screen prefab: {name}", LogLevel.Info, true);
                        break;
                    }
                }

                if (_loadingScreenPrefab == null)
                {
                    var assetNames = _loadingScreenBundle.GetAllAssetNames();
                    foreach (var assetName in assetNames)
                    {
                        var asset = _loadingScreenBundle.LoadAsset<GameObject>(assetName);
                        if (asset != null)
                        {
                            _loadingScreenPrefab = asset;
                            OdiumConsole.LogGradient("AssetLoader", $"Using first GameObject asset: {assetName}", LogLevel.Info, true);
                            break;
                        }
                    }
                }

                if (_loadingScreenPrefab == null)
                {
                    OdiumConsole.Log("AssetLoader", "No GameObject prefab found in AssetBundle!", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "LoadPrefabFromBundle");
            }

            yield return null;
        }

        private static IEnumerator InstantiateLoadingScreen()
        {
            if (_loadingScreenPrefab == null)
            {
                yield break;
            }

            OdiumConsole.Log("AssetLoader", "Instantiating loading screen...", LogLevel.Info);

            try
            {
                var loadingPopup = GameObject.Find("MenuContent/Popups/LoadingPopup");
                if (loadingPopup == null)
                {
                    OdiumConsole.Log("AssetLoader", "Could not find LoadingPopup parent", LogLevel.Warning);
                    _instantiatedLoadingScreen = UnityEngine.Object.Instantiate(_loadingScreenPrefab);
                }
                else
                {
                    _instantiatedLoadingScreen = UnityEngine.Object.Instantiate(_loadingScreenPrefab, loadingPopup.transform);
                }

                if (_instantiatedLoadingScreen != null)
                {
                    _instantiatedLoadingScreen.name = "OdiumCustomLoadingScreen";
                    _instantiatedLoadingScreen.transform.localPosition = Vector3.zero;
                    _instantiatedLoadingScreen.transform.localRotation = Quaternion.identity;
                    _instantiatedLoadingScreen.transform.localScale = Vector3.one;

                    _instantiatedLoadingScreen.SetActive(true);

                    ApplyFemboyPinkToParticles();

                    OdiumConsole.LogGradient("AssetLoader", "Loading screen instantiated successfully!", LogLevel.Info);
                }
                else
                {
                    OdiumConsole.Log("AssetLoader", "Failed to instantiate loading screen!", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "InstantiateLoadingScreen");
            }

            yield return null;
        }

        private static void ApplyFemboyPinkToParticles()
        {
            if (_instantiatedLoadingScreen == null) return;

            try
            {
                var particleSystems = _instantiatedLoadingScreen.GetComponentsInChildren<ParticleSystem>(true);

                OdiumConsole.Log("AssetLoader", $"Found {particleSystems.Length} particle systems to colorize", LogLevel.Info);


                OdiumConsole.LogGradient("AssetLoader", "All particle systems colorized with femboy pink!", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "ApplyFemboyPinkToParticles");
            }
        }

        private static IEnumerator InitLoadingScreenAudio()
        {
            // Prevent multiple audio sources if one is already playing
            if (_customAudioSource != null && _customAudioSource.isPlaying)
            {
                OdiumConsole.Log("AssetLoader", "Custom audio already playing, skipping initialization", LogLevel.Info);
                yield break;
            }

            OdiumConsole.Log("AssetLoader", "Creating custom audio source...", LogLevel.Info);

            if (!File.Exists(LoadingMusicPath))
            {
                OdiumConsole.Log("AssetLoader", $"Audio file not found at: {LoadingMusicPath}", LogLevel.Warning);
                yield break;
            }

            // Stop and destroy any existing audio source to prevent duplicates
            if (_customAudioSource != null)
            {
                _customAudioSource.Stop();
                UnityEngine.Object.Destroy(_customAudioSource.gameObject);
                _customAudioSource = null;
                OdiumConsole.Log("AssetLoader", "Destroyed existing audio source", LogLevel.Info);
            }

            // Create a new GameObject with AudioSource for our custom music
            GameObject customAudioObject = new GameObject("OdiumCustomAudio");
            _customAudioSource = customAudioObject.AddComponent<AudioSource>();

            // Configure the audio source
            _customAudioSource.loop = true;
            _customAudioSource.volume = 0.7f;
            _customAudioSource.spatialBlend = 0f; // 2D audio

            // Don't destroy on load so it persists
            UnityEngine.Object.DontDestroyOnLoad(customAudioObject);

            OdiumConsole.Log("AssetLoader", "Custom audio source created!", LogLevel.Info);

            // Load audio using the VanishClient method
            UnityWebRequest www = UnityWebRequest.Get("file://" + LoadingMusicPath);
            www.SendWebRequest();

            while (!www.isDone)
            {
                yield return null;
            }

            _customAudioClip = WebRequestWWW.InternalCreateAudioClipUsingDH(www.downloadHandler, www.url, false, false, AudioType.UNKNOWN);

            while (!www.isDone || _customAudioClip.loadState == AudioDataLoadState.Loading)
            {
                yield return null;
            }

            if (_customAudioClip != null)
            {
                OdiumConsole.LogGradient("AssetLoader", "Custom audio loaded successfully!", LogLevel.Info);

                // Play our custom music through the new audio source
                _customAudioSource.clip = _customAudioClip;
                _customAudioSource.Play();

                OdiumConsole.LogGradient("AssetLoader", "Custom music now playing!", LogLevel.Info);
            }
            else
            {
                OdiumConsole.Log("AssetLoader", "Failed to create AudioClip", LogLevel.Error);
            }

            www.Dispose();
        }

        private static IEnumerator ApplyToVRChatLoading()
        {
            if (_instantiatedLoadingScreen == null)
            {
                yield break;
            }

            OdiumConsole.Log("AssetLoader", "Applying custom loading screen to VRChat...", LogLevel.Info);

            try
            {
                HideOriginalLoadingElements();
                PositionCustomLoadingScreen();
                OdiumConsole.LogGradient("AssetLoader", "Custom loading screen applied to VRChat!", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "ApplyToVRChatLoading");
            }

            yield return null;
        }

        private static void HideOriginalLoadingElements()
        {
            try
            {
                var originalElements = new string[]
                {
                    "MenuContent/Popups/LoadingPopup/3DElements/LoadingBackground_TealGradient/SkyCube_Baked",
                    "MenuContent/Popups/LoadingPopup/3DElements/LoadingBackground_TealGradient/_FX_ParticleBubbles",
                    "MenuContent/Popups/LoadingPopup/LoadingSound"
                };

                foreach (var elementPath in originalElements)
                {
                    var element = GameObject.Find(elementPath);
                    if (element != null)
                    {
                        element.SetActive(false);
                    }
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "HideOriginalLoadingElements");
            }
        }

        private static void PositionCustomLoadingScreen()
        {
            try
            {
                if (_instantiatedLoadingScreen != null)
                {
                    _instantiatedLoadingScreen.transform.localScale = new Vector3(400f, 400f, 400f);
                    _instantiatedLoadingScreen.transform.localPosition = Vector3.zero;
                    _instantiatedLoadingScreen.transform.localRotation = Quaternion.identity;

                    OdiumConsole.Log("AssetLoader", "Custom loading screen positioned", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "PositionCustomLoadingScreen");
            }
        }

        public static IEnumerator ChangeLoadingScreenAudio()
        {
            if (!File.Exists(LoadingMusicPath))
            {
                OdiumConsole.Log("AssetLoader", "No audio file to change to", LogLevel.Warning);
                yield break;
            }

            if (_customAudioSource == null)
            {
                OdiumConsole.Log("AssetLoader", "No custom audio source available", LogLevel.Warning);
                yield break;
            }

            UnityWebRequest www = UnityWebRequest.Get("file://" + LoadingMusicPath);
            www.SendWebRequest();

            while (!www.isDone)
            {
                yield return null;
            }

            _customAudioClip = WebRequestWWW.InternalCreateAudioClipUsingDH(www.downloadHandler, www.url, false, false, AudioType.UNKNOWN);

            while (!www.isDone || _customAudioClip.loadState == AudioDataLoadState.Loading)
            {
                yield return null;
            }

            if (_customAudioClip != null)
            {
                _customAudioSource.Stop();
                _customAudioSource.clip = _customAudioClip;
                _customAudioSource.Play();
                OdiumConsole.LogGradient("AssetLoader", "Custom audio changed!", LogLevel.Info);
            }

            www.Dispose();
        }

        public static void ShowLoadingScreen()
        {
            if (_instantiatedLoadingScreen != null)
            {
                _instantiatedLoadingScreen.SetActive(true);
                OdiumConsole.Log("AssetLoader", "Custom loading screen shown", LogLevel.Info);
            }
        }

        public static void HideLoadingScreen()
        {
            if (_instantiatedLoadingScreen != null)
            {
                _instantiatedLoadingScreen.SetActive(false);
                OdiumConsole.Log("AssetLoader", "Custom loading screen hidden", LogLevel.Info);
            }
        }

        public static bool IsLoadingScreenLoaded()
        {
            return _instantiatedLoadingScreen != null;
        }

        public static GameObject GetLoadingScreenInstance()
        {
            return _instantiatedLoadingScreen;
        }

        public static void StopCustomAudio()
        {
            if (_customAudioSource != null)
            {
                _customAudioSource.Stop();
                OdiumConsole.Log("AssetLoader", "Custom audio stopped", LogLevel.Info);
            }
        }

        public static void PlayCustomAudio()
        {
            if (_customAudioSource != null && _customAudioClip != null)
            {
                _customAudioSource.Play();
                OdiumConsole.Log("AssetLoader", "Custom audio playing", LogLevel.Info);
            }
        }

        public static void Cleanup()
        {
            try
            {
                if (_instantiatedLoadingScreen != null)
                {
                    UnityEngine.Object.Destroy(_instantiatedLoadingScreen);
                    _instantiatedLoadingScreen = null;
                    OdiumConsole.Log("AssetLoader", "Loading screen instance destroyed", LogLevel.Info);
                }

                if (_loadingScreenBundle != null)
                {
                    _loadingScreenBundle.Unload(true);
                    _loadingScreenBundle = null;
                    OdiumConsole.Log("AssetLoader", "AssetBundle unloaded", LogLevel.Info);
                }

                if (_customAudioSource != null)
                {
                    _customAudioSource.Stop();
                    UnityEngine.Object.Destroy(_customAudioSource.gameObject);
                    _customAudioSource = null;
                    OdiumConsole.Log("AssetLoader", "Custom audio source destroyed", LogLevel.Info);
                }

                if (_customAudioClip != null)
                {
                    UnityEngine.Object.Destroy(_customAudioClip);
                    _customAudioClip = null;
                    OdiumConsole.Log("AssetLoader", "Custom audio clip destroyed", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "Cleanup");
            }
        }

        public static void RestoreOriginalLoadingScreen()
        {
            try
            {
                OdiumConsole.Log("AssetLoader", "Restoring original loading screen...", LogLevel.Info);

                HideLoadingScreen();

                // Stop our custom audio
                if (_customAudioSource != null)
                {
                    _customAudioSource.Stop();
                    OdiumConsole.Log("AssetLoader", "Stopped custom audio", LogLevel.Info);
                }

                var originalElements = new string[]
                {
                    "MenuContent/Popups/LoadingPopup/3DElements/LoadingBackground_TealGradient/SkyCube_Baked",
                    "MenuContent/Popups/LoadingPopup/3DElements/LoadingBackground_TealGradient/_FX_ParticleBubbles",
                };

                foreach (var elementPath in originalElements)
                {
                    var element = GameObject.Find(elementPath);
                    if (element != null)
                    {
                        element.SetActive(true);
                        OdiumConsole.Log("AssetLoader", $"Restored original element: {elementPath}", LogLevel.Info);
                    }
                }

                OdiumConsole.LogGradient("AssetLoader", "Original loading screen restored!", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "RestoreOriginalLoadingScreen");
            }
        }
    }
}