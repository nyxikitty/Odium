/*
 * Odium Module Loader System
 * 
 * This system allows loading external modules from DLL files placed in the "OdiumModules" folder.
 * 
 * SETUP:
 * 1. Place this file in your MelonLoader mod project
 * 2. Make sure OdiumModuleLoader is your main MelonMod class
 * 3. The system will automatically create an "OdiumModules" folder in your game directory
 * 
 * FOR MODULE DEVELOPERS:
 * 1. Create a new Class Library project targeting the same .NET version as your game
 * 2. Reference this file or create modules that inherit from OdiumModule
 * 3. Compile your module as a DLL
 * 4. Place the DLL in the "OdiumModules" folder
 * 5. The module will be automatically loaded when the game starts
 * 
 * EXAMPLE MODULE:
 * [OdiumModule("My Module", "1.0.0", "Author")]
 * public class MyModule : OdiumModule
 * {
 *     public override void OnApplicationStart() { ... }
 *     public override void OnUpdate() { ... }
 * }
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MelonLoader;
using Odium;
using UnityEngine;

namespace OdiumLoader
{
    /// <summary>
    /// Base class for all Odium modules
    /// </summary>
    public abstract class OdiumModule
    {
        /// <summary>
        /// Called when the module is loaded and initialized
        /// </summary>
        public virtual void OnModuleLoad() { }

        /// <summary>
        /// Called when the application starts
        /// </summary>
        public virtual void OnApplicationStart() { }

        /// <summary>
        /// Called on every frame update
        /// </summary>
        public virtual void OnUpdate() { }

        /// <summary>
        /// Called on every fixed update (physics update)
        /// </summary>
        public virtual void OnFixedUpdate() { }

        /// <summary>
        /// Called on every late update
        /// </summary>
        public virtual void OnLateUpdate() { }

        /// <summary>
        /// Called when the application is quitting
        /// </summary>
        public virtual void OnApplicationQuit() { }

        /// <summary>
        /// Called when a scene is loaded
        /// </summary>
        public virtual void OnSceneLoaded(int buildIndex, string sceneName) { }

        /// <summary>
        /// Called when a scene is unloaded
        /// </summary>
        public virtual void OnSceneUnloaded(int buildIndex, string sceneName) { }

        /// <summary>
        /// Module name for identification
        /// </summary>
        public virtual string ModuleName => GetType().Name;

        /// <summary>
        /// Module version
        /// </summary>
        public virtual string ModuleVersion => "1.0.0";

        /// <summary>
        /// Module author
        /// </summary>
        public virtual string ModuleAuthor => "Unknown";

        /// <summary>
        /// Whether this module is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Logger instance for this module
        /// </summary>
        protected MelonLogger.Instance Logger { get; private set; }

        internal void SetLogger(MelonLogger.Instance logger)
        {
            Logger = logger;
        }
    }

    /// <summary>
    /// Manages loading and execution of Odium modules
    /// </summary>
    public class OdiumModuleLoader
    {
        private static List<OdiumModule> loadedModules = new List<OdiumModule>();
        private static Dictionary<Type, object> typeCache = new Dictionary<Type, object>();

        public static void OnApplicationStart()
        {
            OdiumConsole.LogGradient("ModuleLoader", "Odium Module Loader starting...");
            LoadModules();

            foreach (var module in loadedModules)
            {
                if (module.IsEnabled)
                {
                    try
                    {
                        module.OnApplicationStart();
                    }
                    catch (Exception ex)
                    {
                        OdiumConsole.LogGradient("ModuleLoader", $"Error in {module.ModuleName}.OnApplicationStart(): {ex}", LogLevel.Error);
                    }
                }
            }
        }

        public static void OnUpdate()
        {
            foreach (var module in loadedModules)
            {
                if (module.IsEnabled)
                {
                    try
                    {
                        module.OnUpdate();
                    }
                    catch (Exception ex)
                    {
                        OdiumConsole.LogGradient("ModuleLoader", $"Error in {module.ModuleName}.OnUpdate(): {ex}", LogLevel.Error);
                    }
                }
            }
        }

        public static void OnFixedUpdate()
        {
            foreach (var module in loadedModules)
            {
                if (module.IsEnabled)
                {
                    try
                    {
                        module.OnFixedUpdate();
                    }
                    catch (Exception ex)
                    {
                        OdiumConsole.LogGradient("ModuleLoader", $"Error in {module.ModuleName}.OnFixedUpdate(): {ex}", LogLevel.Error);
                    }
                }
            }
        }

        public static void OnLateUpdate()
        {
            foreach (var module in loadedModules)
            {
                if (module.IsEnabled)
                {
                    try
                    {
                        module.OnLateUpdate();
                    }
                    catch (Exception ex)
                    {
                        OdiumConsole.LogGradient("ModuleLoader", $"Error in {module.ModuleName}.OnLateUpdate(): {ex}", LogLevel.Error);
                    }
                }
            }
        }

        public static void OnApplicationQuit()
        {
            foreach (var module in loadedModules)
            {
                if (module.IsEnabled)
                {
                    try
                    {
                        module.OnApplicationQuit();
                    }
                    catch (Exception ex)
                    {
                        OdiumConsole.LogGradient("ModuleLoader", $"Error in {module.ModuleName}.OnApplicationQuit(): {ex}", LogLevel.Error);
                    }
                }
            }
        }

        public static void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            foreach (var module in loadedModules)
            {
                if (module.IsEnabled)
                {
                    try
                    {
                        module.OnSceneLoaded(buildIndex, sceneName);
                    }
                    catch (Exception ex)
                    {
                        OdiumConsole.LogGradient("ModuleLoader", $"Error in {module.ModuleName}.OnSceneLoaded(): {ex}", LogLevel.Error);
                    }
                }
            }
        }

        public static void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            foreach (var module in loadedModules)
            {
                if (module.IsEnabled)
                {
                    try
                    {
                        module.OnSceneUnloaded(buildIndex, sceneName);
                    }
                    catch (Exception ex)
                    {
                        OdiumConsole.LogGradient("ModuleLoader", $"Error in {module.ModuleName}.OnSceneUnloaded(): {ex}", LogLevel.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Loads all modules that inherit from OdiumModule from the OdiumModules folder
        /// </summary>
        public static void LoadModules()
        {
            try
            {
                string modulesPath = Path.Combine(Environment.CurrentDirectory, "OdiumModules");

                // Create the OdiumModules folder if it doesn't exist
                if (!Directory.Exists(modulesPath))
                {
                    Directory.CreateDirectory(modulesPath);
                    OdiumConsole.LogGradient("ModuleLoader", $"Created OdiumModules directory at: {modulesPath}");
                    OdiumConsole.LogGradient("ModuleLoader", "Place your module DLL files in this folder to load them.");
                    return;
                }

                OdiumConsole.LogGradient("ModuleLoader", $"Loading modules from: {modulesPath}");

                // Get all DLL files in the OdiumModules folder
                var dllFiles = Directory.GetFiles(modulesPath, "*.dll", SearchOption.TopDirectoryOnly);

                if (dllFiles.Length == 0)
                {
                    OdiumConsole.LogGradient("ModuleLoader", "No module DLL files found in OdiumModules folder.", LogLevel.Warning);
                    return;
                }

                var loadedAssemblies = new List<Assembly>();

                // Load each DLL file as an assembly
                foreach (var dllFile in dllFiles)
                {
                    try
                    {
                        var fileName = Path.GetFileName(dllFile);
                        OdiumConsole.LogGradient("ModuleLoader", $"Loading assembly: {fileName}");

                        var assembly = Assembly.LoadFrom(dllFile);
                        loadedAssemblies.Add(assembly);

                        OdiumConsole.LogGradient("ModuleLoader", $"Successfully loaded assembly: {fileName}");
                    }
                    catch (Exception ex)
                    {
                        OdiumConsole.LogGradient("ModuleLoader", $"Failed to load assembly {Path.GetFileName(dllFile)}: {ex.Message}");
                    }
                }

                var currentAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && a.Location.Contains("OdiumModules"))
                    .ToList();

                loadedAssemblies.AddRange(currentAssemblies);

                foreach (var assembly in loadedAssemblies)
                {
                    try
                    {
                        var moduleTypes = assembly.GetTypes()
                            .Where(t => t.IsSubclassOf(typeof(OdiumModule)) && !t.IsAbstract)
                            .ToList();

                        if (moduleTypes.Any())
                        {
                            MelonLogger.Msg($"Found {moduleTypes.Count} module(s) in {assembly.GetName().Name}");
                        }

                        foreach (var moduleType in moduleTypes)
                        {
                            try
                            {
                                var moduleAttribute = moduleType.GetCustomAttribute<OdiumModuleAttribute>();

                                if (moduleAttribute?.AutoLoad == false)
                                {
                                    OdiumConsole.LogGradient("ModuleLoader", $"Skipping module {moduleType.Name} (AutoLoad disabled)");
                                    continue;
                                }

                                var module = (OdiumModule)Activator.CreateInstance(moduleType);
                                var logger = new MelonLogger.Instance($"[{module.ModuleName}]");
                                module.SetLogger(logger);

                                loadedModules.Add(module);

                                OdiumConsole.LogGradient("ModuleLoader", $"Loaded module: {module.ModuleName} v{module.ModuleVersion} by {module.ModuleAuthor}");

                                module.OnModuleLoad();
                            }
                            catch (Exception ex)
                            {
                                OdiumConsole.LogGradient("ModuleLoader", $"Failed to instantiate module {moduleType.Name}: {ex}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        OdiumConsole.LogGradient("ModuleLoader", $"Could not process assembly {assembly.GetName().Name}: {ex.Message}", LogLevel.Error);
                    }
                }

                MelonLogger.Msg($"Successfully loaded {loadedModules.Count} modules total from OdiumModules folder");

                if (loadedModules.Count == 0)
                {
                    OdiumConsole.LogGradient("ModuleLoader", "No valid modules found. Make sure your module DLLs:");
                    OdiumConsole.LogGradient("ModuleLoader", "  - Are placed in the OdiumModules folder");
                    OdiumConsole.LogGradient("ModuleLoader", "  - Contain classes that inherit from OdiumModule");
                    OdiumConsole.LogGradient("ModuleLoader", "  - Are compiled for the same .NET version");
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogGradient("ModuleLoader", $"Error loading modules: {ex}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Get a specific module by type
        /// </summary>
        public static T GetModule<T>() where T : OdiumModule
        {
            return loadedModules.OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// Get all loaded modules
        /// </summary>
        public static List<OdiumModule> GetAllModules()
        {
            return new List<OdiumModule>(loadedModules);
        }

        /// <summary>
        /// Enable or disable a specific module
        /// </summary>
        public static void SetModuleEnabled<T>(bool enabled) where T : OdiumModule
        {
            var module = GetModule<T>();
            if (module != null)
            {
                module.IsEnabled = enabled;
                OdiumConsole.LogGradient("ModuleLoader", $"Module {module.ModuleName} {(enabled ? "enabled" : "disabled")}");
            }
        }

        /// <summary>
        /// Get or create a cached instance of a type (useful for VRC types)
        /// </summary>
        public static T GetOrCreateCachedType<T>() where T : class, new()
        {
            var type = typeof(T);
            if (!typeCache.ContainsKey(type))
            {
                typeCache[type] = new T();
            }
            return (T)typeCache[type];
        }

        /// <summary>
        /// Find objects of a specific type in the scene
        /// </summary>
        public static T[] FindObjectsOfType<T>() where T : UnityEngine.Object
        {
            return UnityEngine.Object.FindObjectsOfType<T>();
        }

        /// <summary>
        /// Find a single object of a specific type in the scene
        /// </summary>
        public static T FindObjectOfType<T>() where T : UnityEngine.Object
        {
            return UnityEngine.Object.FindObjectOfType<T>();
        }

        /// <summary>
        /// Reload all modules from the OdiumModules folder
        /// </summary>
        public static void ReloadModules()
        {
            OdiumConsole.LogGradient("ModuleLoader", "Reloading modules...");

            // Call OnApplicationQuit for all existing modules
            foreach (var module in loadedModules)
            {
                try
                {
                    module.OnApplicationQuit();
                }
                catch (Exception ex)
                {
                    OdiumConsole.LogGradient("ModuleLoader", $"Error during {module.ModuleName} cleanup: {ex}", LogLevel.Error);
                }
            }

            // Clear existing modules
            loadedModules.Clear();
            typeCache.Clear();

            // Reload modules (static method call)
            LoadModules();

            // Call OnApplicationStart for newly loaded modules
            foreach (var module in loadedModules)
            {
                if (module.IsEnabled)
                {
                    try
                    {
                        module.OnApplicationStart();
                    }
                    catch (Exception ex)
                    {
                        OdiumConsole.LogGradient("ModuleLoader", $"Error in {module.ModuleName}.OnApplicationStart() during reload: {ex}", LogLevel.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Get the path to the OdiumModules folder
        /// </summary>
        public static string GetModulesPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "OdiumModules");
        }
    }

    /// <summary>
    /// Attribute to mark modules with metadata
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class OdiumModuleAttribute : Attribute
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public bool AutoLoad { get; set; } = true;

        public OdiumModuleAttribute(string name, string version = "1.0.0", string author = "Unknown")
        {
            Name = name;
            Version = version;
            Author = author;
        }
    }
}