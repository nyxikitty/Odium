using System;
using System.Collections;
using System.IO;
using MelonLoader;
using Odium.Components;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using Odium.ButtonAPI.QM;

namespace Odium.Modules
{
    public static class OdiumPerformancePanel
    {
        private static AssetBundle _performanceBundle;
        private static GameObject _performancePanelPrefab;
        private static bool _isInitialized = false;
        private static GameObject _activePerformancePanel;
        private static TextMeshProUGUI _canvasText;
        private static bool _isUpdating = false;

        private static readonly string PerformanceBundlePath = Path.Combine(Components.ModSetup.GetOdiumFolderPath(), "AssetBundles", "performancepanel");

        public static void Initialize()
        {
            if (_isInitialized)
            {
                OdiumConsole.Log("PerformancePanel", "Already initialized, skipping...", LogLevel.Info);
                return;
            }

            OdiumConsole.Log("PerformancePanel", "Starting performance panel initialization...", LogLevel.Info);
            LoadPerformancePanel();
        }

        private static void LoadPerformancePanel()
        {
            try
            {
                if (!File.Exists(PerformanceBundlePath))
                {
                    OdiumConsole.Log("PerformancePanel", $"Performance panel bundle file not found at: {PerformanceBundlePath}", LogLevel.Error);
                    return;
                }

                OdiumConsole.Log("PerformancePanel", "Loading AssetBundle from file...", LogLevel.Info);
                _performanceBundle = AssetBundle.LoadFromFile(PerformanceBundlePath);
                if (_performanceBundle == null)
                {
                    OdiumConsole.Log("PerformancePanel", "Failed to load AssetBundle!", LogLevel.Error);
                    return;
                }

                OdiumConsole.Log("PerformancePanel", "AssetBundle loaded successfully!", LogLevel.Info);

                string[] assetNames = _performanceBundle.GetAllAssetNames();
                OdiumConsole.Log("PerformancePanel", $"Found {assetNames.Length} assets in bundle:", LogLevel.Info);
                foreach (string assetName in assetNames)
                {
                    OdiumConsole.Log("PerformancePanel", $"  - {assetName}", LogLevel.Info);
                }

                string[] possibleNames = { "PerformancePanel", "performancepanel", "assets/performancepanel.prefab", "performancepanel.prefab" };

                foreach (string name in possibleNames)
                {
                    OdiumConsole.Log("PerformancePanel", $"Trying to load prefab with name: '{name}'", LogLevel.Info);
                    _performancePanelPrefab = _performanceBundle.LoadAsset<GameObject>(name);
                    if (_performancePanelPrefab != null)
                    {
                        OdiumConsole.Log("PerformancePanel", $"Successfully loaded prefab with name: '{name}'", LogLevel.Info);
                        break;
                    }
                }

                if (_performancePanelPrefab == null)
                {
                    OdiumConsole.Log("PerformancePanel", "Standard names failed, trying to find any GameObject...", LogLevel.Warning);
                    foreach (string assetName in assetNames)
                    {
                        GameObject asset = _performanceBundle.LoadAsset<GameObject>(assetName);
                        if (asset != null)
                        {
                            _performancePanelPrefab = asset;
                            OdiumConsole.Log("PerformancePanel", $"Using GameObject asset: {assetName}", LogLevel.Info);
                            break;
                        }
                    }
                }

                if (_performancePanelPrefab == null)
                {
                    OdiumConsole.Log("PerformancePanel", "Failed to load any GameObject from AssetBundle!", LogLevel.Error);
                    _performanceBundle.Unload(true);
                    _performanceBundle = null;
                    return;
                }

                _performanceBundle.Unload(false);
                _performanceBundle = null;

                _isInitialized = true;
                OdiumConsole.LogGradient("PerformancePanel", "Performance panel system initialized successfully!", LogLevel.Info, true);
                OdiumConsole.Log("PerformancePanel", $"Prefab reference: {_performancePanelPrefab.name}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "LoadPerformancePanel");
                _isInitialized = false;
                _performancePanelPrefab = null;
                if (_performanceBundle != null)
                {
                    _performanceBundle.Unload(true);
                    _performanceBundle = null;
                }
            }
        }

        public static void ShowPerformancePanel()
        {
            OdiumConsole.Log("PerformancePanel", $"ShowPerformancePanel called - Initialized: {_isInitialized}, Prefab null: {_performancePanelPrefab == null}", LogLevel.Info);

            if (!_isInitialized)
            {
                OdiumConsole.Log("PerformancePanel", "Performance panel system not initialized!", LogLevel.Error);
                return;
            }

            if (_performancePanelPrefab == null)
            {
                OdiumConsole.Log("PerformancePanel", "Performance panel prefab is null! Attempting to reinitialize...", LogLevel.Error);
                _isInitialized = false;
                Initialize();

                if (_performancePanelPrefab == null)
                {
                    OdiumConsole.Log("PerformancePanel", "Reinitialization failed - prefab still null!", LogLevel.Error);
                    return;
                }
            }

            if (_activePerformancePanel != null)
            {
                ClosePerformancePanel();
            }

            try
            {
                Scene activeScene = SceneManager.GetActiveScene();
                OdiumConsole.Log("PerformancePanel", $"Target scene: {activeScene.name}", LogLevel.Info);

                _activePerformancePanel = UnityEngine.Object.Instantiate(_performancePanelPrefab);
                _activePerformancePanel.name = $"OdiumPerformancePanel_{DateTime.Now.Ticks}";

                SceneManager.MoveGameObjectToScene(_activePerformancePanel, activeScene);

                OdiumConsole.Log("PerformancePanel", $"Instantiated performance panel in scene: {_activePerformancePanel.scene.name}", LogLevel.Info);

                Canvas panelCanvas = _activePerformancePanel.GetComponent<Canvas>();
                if (panelCanvas != null)
                {
                    panelCanvas.enabled = true;
                    panelCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    panelCanvas.sortingOrder = 32769;
                    panelCanvas.overrideSorting = true;
                    panelCanvas.pixelPerfect = false;

                    OdiumConsole.Log("PerformancePanel", $"Configured Canvas - Enabled: {panelCanvas.enabled}, SortOrder: {panelCanvas.sortingOrder}", LogLevel.Info);
                }
                else
                {
                    OdiumConsole.Log("PerformancePanel", "No Canvas component found on performance panel prefab!", LogLevel.Warning);
                }

                _activePerformancePanel.SetActive(true);

                RectTransform rectTransform = _activePerformancePanel.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.localScale = Vector3.one;
                    rectTransform.localPosition = Vector3.zero;
                    rectTransform.localEulerAngles = Vector3.zero;

                    rectTransform.anchorMin = new Vector2(0f, 1f);
                    rectTransform.anchorMax = new Vector2(0f, 1f);
                    rectTransform.pivot = new Vector2(0f, 1f);
                    rectTransform.anchoredPosition = new Vector2(10f, -10f);

                    OdiumConsole.Log("PerformancePanel", $"Positioned performance panel - AnchoredPos: {rectTransform.anchoredPosition}, Size: {rectTransform.sizeDelta}", LogLevel.Info);
                }

                FindCanvasTextComponent();

                Transform[] allTransforms = _activePerformancePanel.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in allTransforms)
                {
                    t.gameObject.SetActive(true);
                }

                Behaviour[] allBehaviours = _activePerformancePanel.GetComponentsInChildren<Behaviour>(true);
                foreach (Behaviour behaviour in allBehaviours)
                {
                    behaviour.enabled = true;
                }

                Canvas.ForceUpdateCanvases();
                if (rectTransform != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                }

                StartPerformanceUpdate();

                OdiumConsole.LogGradient("PerformancePanel", "Performance panel shown successfully!", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "ShowPerformancePanel");
            }
        }

        private static void FindCanvasTextComponent()
        {
            try
            {
                if (_activePerformancePanel == null) return;

                Transform[] allTransforms = _activePerformancePanel.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in allTransforms)
                {
                    if (t.name == "Canvas" && t.name != "ServerInfoCanvas")
                    {
                        _canvasText = t.GetComponentInChildren<TextMeshProUGUI>();
                        if (_canvasText != null)
                        {
                            OdiumConsole.Log("PerformancePanel", $"Found Canvas text component: {_canvasText.name}", LogLevel.Info);
                            return;
                        }

                        Text regularText = t.GetComponentInChildren<Text>();
                        if (regularText != null)
                        {
                            OdiumConsole.Log("PerformancePanel", $"Found Canvas regular text component: {regularText.name}", LogLevel.Info);
                            return;
                        }
                    }
                }

                if (_canvasText == null)
                {
                    _canvasText = _activePerformancePanel.GetComponentInChildren<TextMeshProUGUI>();
                    if (_canvasText != null)
                    {
                        OdiumConsole.Log("PerformancePanel", $"Found TextMeshProUGUI component: {_canvasText.name}", LogLevel.Info);
                    }
                }

                if (_canvasText == null)
                {
                    OdiumConsole.Log("PerformancePanel", "Could not find Canvas text component!", LogLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "FindCanvasTextComponent");
            }
        }

        private static void StartPerformanceUpdate()
        {
            if (_isUpdating) return;

            _isUpdating = true;
            MelonCoroutines.Start(PerformanceUpdateLoop());
            OdiumConsole.Log("PerformancePanel", "Started performance update loop", LogLevel.Info);
        }

        private static IEnumerator PerformanceUpdateLoop()
        {
            while (_activePerformancePanel != null && _isUpdating)
            {
                try
                {
                    UpdatePerformanceText();
                }
                catch (Exception ex)
                {
                    OdiumConsole.LogException(ex, "PerformanceUpdateLoop");
                }

                yield return new WaitForSeconds(1f);
            }

            _isUpdating = false;
            OdiumConsole.Log("PerformancePanel", "Performance update loop stopped", LogLevel.Info);
        }

        private static void UpdatePerformanceText()
        {
            if (_canvasText == null) return;

            try
            {

                string performanceInfo = $"FPS:  {ApiUtils.GetFPS()}   |     PING:  {ApiUtils.GetPing()}   ";

                _canvasText.text = performanceInfo;
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "UpdatePerformanceText");
            }
        }

        public static void ClosePerformancePanel()
        {
            try
            {
                _isUpdating = false;

                if (_activePerformancePanel != null)
                {
                    UnityEngine.Object.Destroy(_activePerformancePanel);
                    _activePerformancePanel = null;
                    _canvasText = null;
                    OdiumConsole.Log("PerformancePanel", "Performance panel closed", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "ClosePerformancePanel");
            }
        }

        public static bool IsInitialized()
        {
            bool result = _isInitialized && _performancePanelPrefab != null;
            OdiumConsole.Log("PerformancePanel", $"IsInitialized check - _isInitialized: {_isInitialized}, _performancePanelPrefab != null: {_performancePanelPrefab != null}, Result: {result}", LogLevel.Info);
            return result;
        }

        public static bool IsShowing()
        {
            return _activePerformancePanel != null;
        }

        public static void TogglePerformancePanel()
        {
            if (IsShowing())
            {
                ClosePerformancePanel();
            }
            else
            {
                ShowPerformancePanel();
            }
        }

        public static void Cleanup()
        {
            try
            {
                _isUpdating = false;
                ClosePerformancePanel();

                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    if (scene.IsValid() && scene.isLoaded)
                    {
                        GameObject[] rootObjects = scene.GetRootGameObjects();
                        foreach (GameObject obj in rootObjects)
                        {
                            if (obj.name.StartsWith("OdiumPerformancePanel"))
                            {
                                UnityEngine.Object.Destroy(obj);
                                OdiumConsole.Log("PerformancePanel", $"Destroyed performance panel in scene: {scene.name}", LogLevel.Info);
                            }
                        }
                    }
                }

                if (_performanceBundle != null)
                {
                    _performanceBundle.Unload(true);
                    _performanceBundle = null;
                }

                _performancePanelPrefab = null;
                _canvasText = null;
                _isInitialized = false;

                OdiumConsole.Log("PerformancePanel", "Performance panel system cleaned up", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "PerformancePanelCleanup");
            }
        }
    }
}