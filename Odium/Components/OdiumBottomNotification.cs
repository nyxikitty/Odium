using System;
using System.Collections;
using System.IO;
using MelonLoader;
using Odium.Components;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// This shit was harder than I thought it would be to make
namespace Odium.Modules
{
    public static class OdiumBottomNotification
    {
        private static AssetBundle _notificationBundle;
        private static GameObject _notificationPrefab;
        private static bool _isInitialized = false;

        private static readonly string NotificationBundlePath = Path.Combine(Components.ModSetup.GetOdiumFolderPath(), "AssetBundles", "bottomnotification");

        public static void Initialize()
        {
            if (_isInitialized)
            {
                OdiumConsole.Log("NotificationLoader", "Already initialized, skipping...", LogLevel.Info);
                return;
            }

            OdiumConsole.Log("NotificationLoader", "Starting notification initialization...", LogLevel.Info);
            LoadNotifications();
        }

        private static void LoadNotifications()
        {
            try
            {
                if (!File.Exists(NotificationBundlePath))
                {
                    OdiumConsole.Log("NotificationLoader", $"Notification bundle file not found at: {NotificationBundlePath}", LogLevel.Error);
                    return;
                }

                OdiumConsole.Log("NotificationLoader", "Loading AssetBundle from file...", LogLevel.Info);
                _notificationBundle = AssetBundle.LoadFromFile(NotificationBundlePath);
                if (_notificationBundle == null)
                {
                    OdiumConsole.Log("NotificationLoader", "Failed to load AssetBundle!", LogLevel.Error);
                    return;
                }

                OdiumConsole.Log("NotificationLoader", "AssetBundle loaded successfully!", LogLevel.Info);

                string[] assetNames = _notificationBundle.GetAllAssetNames();
                OdiumConsole.Log("NotificationLoader", $"Found {assetNames.Length} assets in bundle:", LogLevel.Info);
                foreach (string assetName in assetNames)
                {
                    OdiumConsole.Log("NotificationLoader", $"  - {assetName}", LogLevel.Info);
                }

                string[] possibleNames = { "Notification", "notification", "assets/notification.prefab", "notification.prefab" };

                foreach (string name in possibleNames)
                {
                    OdiumConsole.Log("NotificationLoader", $"Trying to load prefab with name: '{name}'", LogLevel.Info);
                    _notificationPrefab = _notificationBundle.LoadAsset<GameObject>(name);
                    if (_notificationPrefab != null)
                    {
                        OdiumConsole.Log("NotificationLoader", $"Successfully loaded prefab with name: '{name}'", LogLevel.Info);
                        break;
                    }
                }

                if (_notificationPrefab == null)
                {
                    OdiumConsole.Log("NotificationLoader", "Standard names failed, trying to find any GameObject...", LogLevel.Warning);
                    foreach (string assetName in assetNames)
                    {
                        GameObject asset = _notificationBundle.LoadAsset<GameObject>(assetName);
                        if (asset != null)
                        {
                            _notificationPrefab = asset;
                            OdiumConsole.Log("NotificationLoader", $"Using GameObject asset: {assetName}", LogLevel.Info);
                            break;
                        }
                    }
                }

                if (_notificationPrefab == null)
                {
                    OdiumConsole.Log("NotificationLoader", "Failed to load any GameObject from AssetBundle!", LogLevel.Error);
                    _notificationBundle.Unload(true);
                    _notificationBundle = null;
                    return;
                }

                _notificationBundle.Unload(false);
                _notificationBundle = null;

                _isInitialized = true;
                OdiumConsole.LogGradient("NotificationLoader", "Notification system initialized successfully!", LogLevel.Info, true);
                OdiumConsole.Log("NotificationLoader", $"Prefab reference: {_notificationPrefab.name}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "LoadNotifications");
                _isInitialized = false;
                _notificationPrefab = null;
                if (_notificationBundle != null)
                {
                    _notificationBundle.Unload(true);
                    _notificationBundle = null;
                }
            }
        }

        private static Transform FindOrCreateNotificationCanvas()
        {
            try
            {
                Scene activeScene = SceneManager.GetActiveScene();
                OdiumConsole.Log("NotificationLoader", $"Current active scene: {activeScene.name}", LogLevel.Info);

                Canvas existingCanvas = FindBestExistingCanvas(activeScene);
                if (existingCanvas != null)
                {
                    OdiumConsole.Log("NotificationLoader", $"Using existing canvas: {existingCanvas.name} in scene: {activeScene.name}", LogLevel.Info);
                    return existingCanvas.transform;
                }

                GameObject canvasObj = new GameObject("OdiumNotificationCanvas");
                SceneManager.MoveGameObjectToScene(canvasObj, activeScene);

                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 32767;
                canvas.pixelPerfect = false;

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;

                GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
                raycaster.ignoreReversedGraphics = true;
                raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;

                OdiumConsole.Log("NotificationLoader", $"Created new notification canvas in active scene: {activeScene.name}", LogLevel.Info);
                return canvasObj.transform;
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "FindOrCreateNotificationCanvas");
                return null;
            }
        }

        private static Canvas FindBestExistingCanvas(Scene scene)
        {
            try
            {
                GameObject[] rootObjects = scene.GetRootGameObjects();
                Canvas bestCanvas = null;
                int highestSortOrder = -1;

                foreach (GameObject obj in rootObjects)
                {
                    Canvas[] canvases = obj.GetComponentsInChildren<Canvas>();
                    foreach (Canvas canvas in canvases)
                    {
                        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                        {
                            if (canvas.sortingOrder > highestSortOrder)
                            {
                                bestCanvas = canvas;
                                highestSortOrder = canvas.sortingOrder;
                            }
                        }
                    }
                }

                if (bestCanvas != null)
                {
                    OdiumConsole.Log("NotificationLoader", $"Found suitable existing canvas: {bestCanvas.name} with sort order: {highestSortOrder}", LogLevel.Info);
                }

                return bestCanvas;
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "FindBestExistingCanvas");
                return null;
            }
        }

        public static void ShowNotification(string text)
        {
            OdiumConsole.Log("NotificationLoader", $"ShowNotification called - Initialized: {_isInitialized}, Prefab null: {_notificationPrefab == null}", LogLevel.Info);

            if (!_isInitialized)
            {
                OdiumConsole.Log("NotificationLoader", "Notification system not initialized!", LogLevel.Error);
                return;
            }

            if (_notificationPrefab == null)
            {
                OdiumConsole.Log("NotificationLoader", "Notification prefab is null! Attempting to reinitialize...", LogLevel.Error);
                _isInitialized = false;
                Initialize();

                if (_notificationPrefab == null)
                {
                    OdiumConsole.Log("NotificationLoader", "Reinitialization failed - prefab still null!", LogLevel.Error);
                    return;
                }
            }

            try
            {
                Scene activeScene = SceneManager.GetActiveScene();
                OdiumConsole.Log("NotificationLoader", $"Target scene: {activeScene.name}", LogLevel.Info);

                GameObject notificationInstance = UnityEngine.Object.Instantiate(_notificationPrefab);
                notificationInstance.name = $"OdiumNotification_{DateTime.Now.Ticks}";

                SceneManager.MoveGameObjectToScene(notificationInstance, activeScene);

                OdiumConsole.Log("NotificationLoader", $"Instantiated notification in scene: {notificationInstance.scene.name}", LogLevel.Info);

                Canvas notificationCanvas = notificationInstance.GetComponent<Canvas>();
                if (notificationCanvas != null)
                {
                    notificationCanvas.enabled = true;
                    notificationCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    notificationCanvas.sortingOrder = 32768;
                    notificationCanvas.overrideSorting = true;
                    notificationCanvas.pixelPerfect = false;

                    OdiumConsole.Log("NotificationLoader", $"Configured Canvas - Enabled: {notificationCanvas.enabled}, SortOrder: {notificationCanvas.sortingOrder}", LogLevel.Info);
                }
                else
                {
                    OdiumConsole.Log("NotificationLoader", "No Canvas component found on notification prefab!", LogLevel.Warning);
                }

                notificationInstance.SetActive(true);

                RectTransform rectTransform = notificationInstance.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.localScale = Vector3.one;
                    rectTransform.localPosition = Vector3.zero;
                    rectTransform.localEulerAngles = Vector3.zero;

                    rectTransform.anchorMin = new Vector2(1, 1);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.pivot = new Vector2(1, 1);
                    rectTransform.anchoredPosition = new Vector2(-50, -50);

                    if (rectTransform.sizeDelta.x <= 0 || rectTransform.sizeDelta.y <= 0)
                    {
                        rectTransform.sizeDelta = new Vector2(300, 100);
                    }

                    rectTransform.localScale = new Vector3(1.1f, 1.1f, 1);

                    OdiumConsole.Log("NotificationLoader", $"Positioned notification - AnchoredPos: {rectTransform.anchoredPosition}, Size: {rectTransform.sizeDelta}", LogLevel.Info);
                }

                TextMeshProUGUI textComponent = notificationInstance.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = text;
                    textComponent.enabled = true;
                    textComponent.gameObject.SetActive(true);
                    OdiumConsole.Log("NotificationLoader", $"Set notification text: '{text}'", LogLevel.Info);
                }
                else
                {
                    Text legacyTextComponent = notificationInstance.GetComponentInChildren<Text>();
                    if (legacyTextComponent != null)
                    {
                        legacyTextComponent.text = text;
                        legacyTextComponent.enabled = true;
                        legacyTextComponent.gameObject.SetActive(true);
                        OdiumConsole.Log("NotificationLoader", $"Set notification text using legacy Text component: '{text}'", LogLevel.Info);
                    }
                    else
                    {
                        OdiumConsole.Log("NotificationLoader", "Could not find any text component!", LogLevel.Warning);
                    }
                }

                Transform[] allTransforms = notificationInstance.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in allTransforms)
                {
                    t.gameObject.SetActive(true);
                }

                Behaviour[] allBehaviours = notificationInstance.GetComponentsInChildren<Behaviour>(true);
                foreach (Behaviour behaviour in allBehaviours)
                {
                    behaviour.enabled = true;
                }

                Canvas.ForceUpdateCanvases();
                if (rectTransform != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                }

                OdiumConsole.Log("NotificationLoader", $"Final notification scene: {notificationInstance.scene.name}", LogLevel.Info);

                MelonCoroutines.Start(DestroyNotificationAfterDelay(notificationInstance, 9.0f));

                OdiumConsole.LogGradient("NotificationLoader", $"Notification shown: {text}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "ShowNotification");
            }
        }

        private static IEnumerator DestroyNotificationAfterDelay(GameObject notification, float delay)
        {
            yield return new WaitForSeconds(delay);

            try
            {
                if (notification != null)
                {
                    OdiumConsole.Log("NotificationLoader", $"Auto-destroying notification: {notification.name}", LogLevel.Info);
                    // UnityEngine.Object.Destroy(notification);
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "DestroyNotificationAfterDelay");
            }
        }

        public static bool IsInitialized()
        {
            bool result = _isInitialized && _notificationPrefab != null;
            OdiumConsole.Log("NotificationLoader", $"IsInitialized check - _isInitialized: {_isInitialized}, _notificationPrefab != null: {_notificationPrefab != null}, Result: {result}", LogLevel.Info);
            return result;
        }

        public static void Cleanup()
        {
            try
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    if (scene.IsValid() && scene.isLoaded)
                    {
                        GameObject[] rootObjects = scene.GetRootGameObjects();
                        foreach (GameObject obj in rootObjects)
                        {
                            if (obj.name == "OdiumNotificationCanvas")
                            {
                                UnityEngine.Object.Destroy(obj);
                                OdiumConsole.Log("NotificationLoader", $"Destroyed notification canvas in scene: {scene.name}", LogLevel.Info);
                            }
                        }
                    }
                }

                if (_notificationBundle != null)
                {
                    _notificationBundle.Unload(true);
                    _notificationBundle = null;
                }

                _notificationPrefab = null;
                _isInitialized = false;

                OdiumConsole.Log("NotificationLoader", "Notification system cleaned up", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "NotificationCleanup");
            }
        }
    }
}