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

namespace Odium.Modules
{
    public static class OdiumInputDialog
    {
        private static AssetBundle _inputBundle;
        private static GameObject _inputDialogPrefab;
        private static bool _isInitialized = false;
        private static List<GameObject> _activeInputDialogs = new List<GameObject>();

        private static readonly string InputBundlePath = Path.Combine(Components.ModSetup.GetOdiumFolderPath(), "AssetBundles", "inputdialog");

        public delegate void InputCallback(string input, bool wasSubmitted);

        public static void Initialize()
        {
            if (_isInitialized)
            {
                OdiumConsole.Log("InputDialog", "Already initialized, skipping...", LogLevel.Info);
                return;
            }

            OdiumConsole.Log("InputDialog", "Starting input dialog initialization...", LogLevel.Info);
            LoadInputDialog();
        }

        private static void LoadInputDialog()
        {
            try
            {
                if (!File.Exists(InputBundlePath))
                {
                    OdiumConsole.Log("InputDialog", $"Input dialog bundle file not found at: {InputBundlePath}", LogLevel.Error);
                    return;
                }

                OdiumConsole.Log("InputDialog", "Loading AssetBundle from file...", LogLevel.Info);
                _inputBundle = AssetBundle.LoadFromFile(InputBundlePath);
                if (_inputBundle == null)
                {
                    OdiumConsole.Log("InputDialog", "Failed to load AssetBundle!", LogLevel.Error);
                    return;
                }

                OdiumConsole.Log("InputDialog", "AssetBundle loaded successfully!", LogLevel.Info);

                string[] assetNames = _inputBundle.GetAllAssetNames();
                OdiumConsole.Log("InputDialog", $"Found {assetNames.Length} assets in bundle:", LogLevel.Info);
                foreach (string assetName in assetNames)
                {
                    OdiumConsole.Log("InputDialog", $"  - {assetName}", LogLevel.Info);
                }

                string[] possibleNames = { "OdiumInputSystem", "inputdialog", "assets/inputdialog.prefab", "inputdialog.prefab" };

                foreach (string name in possibleNames)
                {
                    OdiumConsole.Log("InputDialog", $"Trying to load prefab with name: '{name}'", LogLevel.Info);
                    _inputDialogPrefab = _inputBundle.LoadAsset<GameObject>(name);
                    if (_inputDialogPrefab != null)
                    {
                        OdiumConsole.Log("InputDialog", $"Successfully loaded prefab with name: '{name}'", LogLevel.Info);
                        break;
                    }
                }

                if (_inputDialogPrefab == null)
                {
                    OdiumConsole.Log("InputDialog", "Standard names failed, trying to find any GameObject...", LogLevel.Warning);
                    foreach (string assetName in assetNames)
                    {
                        GameObject asset = _inputBundle.LoadAsset<GameObject>(assetName);
                        if (asset != null)
                        {
                            _inputDialogPrefab = asset;
                            OdiumConsole.Log("InputDialog", $"Using GameObject asset: {assetName}", LogLevel.Info);
                            break;
                        }
                    }
                }

                if (_inputDialogPrefab == null)
                {
                    OdiumConsole.Log("InputDialog", "Failed to load any GameObject from AssetBundle!", LogLevel.Error);
                    _inputBundle.Unload(true);
                    _inputBundle = null;
                    return;
                }

                _inputBundle.Unload(false);
                _inputBundle = null;

                _isInitialized = true;
                OdiumConsole.LogGradient("InputDialog", "Input dialog system initialized successfully!", LogLevel.Info, true);
                OdiumConsole.Log("InputDialog", $"Prefab reference: {_inputDialogPrefab.name}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "LoadInputDialog");
                _isInitialized = false;
                _inputDialogPrefab = null;
                if (_inputBundle != null)
                {
                    _inputBundle.Unload(true);
                    _inputBundle = null;
                }
            }
        }

        public static void ShowInputDialog(string promptText, InputCallback callback, string defaultValue = "", string placeholder = "Enter text...")
        {
            OdiumConsole.Log("InputDialog", $"ShowInputDialog called - Initialized: {_isInitialized}, Prefab null: {_inputDialogPrefab == null}", LogLevel.Info);

            if (!_isInitialized)
            {
                OdiumConsole.Log("InputDialog", "Input dialog system not initialized!", LogLevel.Error);
                callback?.Invoke("", false);
                return;
            }

            if (_inputDialogPrefab == null)
            {
                OdiumConsole.Log("InputDialog", "Input dialog prefab is null! Attempting to reinitialize...", LogLevel.Error);
                _isInitialized = false;
                Initialize();

                if (_inputDialogPrefab == null)
                {
                    OdiumConsole.Log("InputDialog", "Reinitialization failed - prefab still null!", LogLevel.Error);
                    callback?.Invoke("", false);
                    return;
                }
            }

            try
            {
                Scene activeScene = SceneManager.GetActiveScene();
                OdiumConsole.Log("InputDialog", $"Target scene: {activeScene.name}", LogLevel.Info);

                GameObject dialogInstance = UnityEngine.Object.Instantiate(_inputDialogPrefab);
                dialogInstance.name = $"OdiumInputDialog_{DateTime.Now.Ticks}";

                SceneManager.MoveGameObjectToScene(dialogInstance, activeScene);

                OdiumConsole.Log("InputDialog", $"Instantiated input dialog in scene: {dialogInstance.scene.name}", LogLevel.Info);

                Canvas dialogCanvas = dialogInstance.GetComponent<Canvas>();
                if (dialogCanvas != null)
                {
                    dialogCanvas.enabled = true;
                    dialogCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    dialogCanvas.sortingOrder = 32770;
                    dialogCanvas.overrideSorting = true;
                    dialogCanvas.pixelPerfect = false;

                    OdiumConsole.Log("InputDialog", $"Configured Canvas - Enabled: {dialogCanvas.enabled}, SortOrder: {dialogCanvas.sortingOrder}", LogLevel.Info);
                }
                else
                {
                    OdiumConsole.Log("InputDialog", "No Canvas component found on input dialog prefab!", LogLevel.Warning);
                }

                dialogInstance.SetActive(true);
                _activeInputDialogs.Add(dialogInstance);

                RectTransform rectTransform = dialogInstance.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.localScale = Vector3.one;
                    rectTransform.localPosition = Vector3.zero;
                    rectTransform.localEulerAngles = Vector3.zero;

                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.anchoredPosition = Vector2.zero;

                    if (rectTransform.sizeDelta.x <= 0 || rectTransform.sizeDelta.y <= 0)
                    {
                        rectTransform.sizeDelta = new Vector2(400, 200);
                    }

                    OdiumConsole.Log("InputDialog", $"Positioned input dialog - AnchoredPos: {rectTransform.anchoredPosition}, Size: {rectTransform.sizeDelta}", LogLevel.Info);
                }

                // Find and setup input field
                TMP_InputField inputField = dialogInstance.GetComponentInChildren<TMP_InputField>();
                if (inputField != null)
                {
                    inputField.text = defaultValue;

                    TextMeshProUGUI placeholderText = inputField.placeholder as TextMeshProUGUI;
                    if (placeholderText != null)
                    {
                        placeholderText.text = placeholder;
                    }

                    // Add submit listener to input field
                    inputField.onSubmit.AddListener(new Action<string>((value) => {
                        CloseInputDialog(dialogInstance);
                        callback?.Invoke(value, true);
                    }));

                    inputField.Select();
                    inputField.ActivateInputField();
                    OdiumConsole.Log("InputDialog", "Set up input field with listeners", LogLevel.Info);
                }
                else
                {
                    OdiumConsole.Log("InputDialog", "Could not find TMP_InputField component!", LogLevel.Warning);
                }

                // Find and setup button
                Button submitButton = dialogInstance.GetComponentInChildren<Button>();
                if (submitButton != null)
                {
                    submitButton.onClick.AddListener(new Action(() => {
                        string inputValue = inputField != null ? inputField.text : "";
                        CloseInputDialog(dialogInstance);
                        callback?.Invoke(inputValue, true);
                    }));
                    OdiumConsole.Log("InputDialog", "Set up button with click listener", LogLevel.Info);
                }
                else
                {
                    OdiumConsole.Log("InputDialog", "Could not find Button component!", LogLevel.Warning);
                }

                Transform[] allTransforms = dialogInstance.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in allTransforms)
                {
                    t.gameObject.SetActive(true);
                }

                Behaviour[] allBehaviours = dialogInstance.GetComponentsInChildren<Behaviour>(true);
                foreach (Behaviour behaviour in allBehaviours)
                {
                    behaviour.enabled = true;
                }

                Canvas.ForceUpdateCanvases();
                if (rectTransform != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                }

                OdiumConsole.Log("InputDialog", $"Final input dialog scene: {dialogInstance.scene.name}", LogLevel.Info);

                // Handle escape key
                MelonCoroutines.Start(HandleInputDialogInput(dialogInstance, callback));

                OdiumConsole.LogGradient("InputDialog", $"Input dialog shown with prompt: {promptText}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "ShowInputDialog");
                callback?.Invoke("", false);
            }
        }

        private static IEnumerator HandleInputDialogInput(GameObject dialog, InputCallback callback)
        {
            while (dialog != null && _activeInputDialogs.Contains(dialog))
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    CloseInputDialog(dialog);
                    callback?.Invoke("", false);
                    yield break;
                }
                yield return null;
            }
        }

        private static void CloseInputDialog(GameObject dialog)
        {
            try
            {
                if (dialog != null)
                {
                    _activeInputDialogs.Remove(dialog);
                    UnityEngine.Object.Destroy(dialog);
                    OdiumConsole.Log("InputDialog", "Input dialog closed", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "CloseInputDialog");
            }
        }

        public static bool IsInitialized()
        {
            bool result = _isInitialized && _inputDialogPrefab != null;
            OdiumConsole.Log("InputDialog", $"IsInitialized check - _isInitialized: {_isInitialized}, _inputDialogPrefab != null: {_inputDialogPrefab != null}, Result: {result}", LogLevel.Info);
            return result;
        }

        public static void CloseAllInputDialogs()
        {
            try
            {
                foreach (GameObject dialog in _activeInputDialogs.ToArray())
                {
                    if (dialog != null)
                    {
                        UnityEngine.Object.Destroy(dialog);
                    }
                }
                _activeInputDialogs.Clear();
                OdiumConsole.Log("InputDialog", "All input dialogs closed", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "CloseAllInputDialogs");
            }
        }

        public static void Cleanup()
        {
            try
            {
                CloseAllInputDialogs();

                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    if (scene.IsValid() && scene.isLoaded)
                    {
                        GameObject[] rootObjects = scene.GetRootGameObjects();
                        foreach (GameObject obj in rootObjects)
                        {
                            if (obj.name.StartsWith("OdiumInputDialog"))
                            {
                                UnityEngine.Object.Destroy(obj);
                                OdiumConsole.Log("InputDialog", $"Destroyed input dialog in scene: {scene.name}", LogLevel.Info);
                            }
                        }
                    }
                }

                if (_inputBundle != null)
                {
                    _inputBundle.Unload(true);
                    _inputBundle = null;
                }

                _inputDialogPrefab = null;
                _isInitialized = false;

                OdiumConsole.Log("InputDialog", "Input dialog system cleaned up", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "InputDialogCleanup");
            }
        }
    }
}