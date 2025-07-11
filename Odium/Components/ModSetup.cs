using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace Odium.Components
{
    public static class ModSetup
    {
        private static readonly string OdiumFolderPath = Path.Combine(Environment.CurrentDirectory, "Odium");
        private static readonly string AssetBundlesFolderPath = Path.Combine(Environment.CurrentDirectory, "Odium", "AssetBundles");
        private static readonly string OdiumPrefsPath = Path.Combine(OdiumFolderPath, "odium_prefs.json");

        private static readonly string QMBackgroundPath = Path.Combine(OdiumFolderPath, "ButtonBackground.png");
        private static readonly string ButtonBackgroundPath = Path.Combine(OdiumFolderPath, "QMBackground.png");
        private static readonly string QMHalfButtonPath = Path.Combine(OdiumFolderPath, "QMHalfButton.png");
        private static readonly string QMConsolePath = Path.Combine(OdiumFolderPath, "QMConsole.png");
        private static readonly string TabImagePath = Path.Combine(OdiumFolderPath, "OdiumIcon.png");
        private static readonly string NameplatePath = Path.Combine(OdiumFolderPath, "Nameplate.png");
        private static readonly string NotificationAssetBundlePath = Path.Combine(OdiumFolderPath, "AssetBundles", "notification");

        private const string AssetsZipUrl = "https://odiumvrc.com/files/odium-build-796.zip";
        private static readonly string TempZipPath = Path.Combine(Path.GetTempPath(), "odium_assets.zip");

        public static async Task Initialize()
        {
            try
            {
                OdiumConsole.Log("ModSetup", "Starting mod setup initialization...", LogLevel.Info);
                await CheckAndCreateOdiumFolder();
                await CheckAndCreatePreferencesFile();

                if (!AssetsDownloaded())
                {
                    await DownloadAndExtractAssets();
                }
                else
                {
                    OdiumConsole.LogGradient("ModSetup", "Assets found, skipping download!", LogLevel.Info);
                }

                OdiumConsole.LogGradient("ModSetup", "Mod setup completed successfully!", LogLevel.Info);
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "ModSetup.Initialize");
            }
        }

        private static async Task CheckAndCreateOdiumFolder()
        {
            try
            {
                if (!Directory.Exists(OdiumFolderPath))
                {
                    OdiumConsole.Log("ModSetup", "Odium folder couldn't be found.. Creating folder", LogLevel.Info);
                    Directory.CreateDirectory(OdiumFolderPath);
                    if (Directory.Exists(OdiumFolderPath))
                        OdiumConsole.LogGradient("ModSetup", $"Odium folder created successfully at: {OdiumFolderPath}", LogLevel.Info);
                    else
                        OdiumConsole.Log("ModSetup", "Failed to create Odium folder!", LogLevel.Error);
                }
                else
                {
                    OdiumConsole.LogGradient("ModSetup", "Odium folder found!", LogLevel.Info);
                }

                if (!Directory.Exists(AssetBundlesFolderPath))
                {
                    OdiumConsole.Log("ModSetup", "Odium's AssetBundles folder couldn't be found.. Creating folder", LogLevel.Info);
                    Directory.CreateDirectory(AssetBundlesFolderPath);
                    if (Directory.Exists(AssetBundlesFolderPath))
                        OdiumConsole.LogGradient("ModSetup", $"Odium's AssetBundles folder created successfully at: {AssetBundlesFolderPath}", LogLevel.Info, true);
                    else
                        OdiumConsole.Log("ModSetup", "Failed to create Odium's AssetBundles folder!", LogLevel.Error);
                }
                else
                {
                    OdiumConsole.LogGradient("ModSetup", "Odium's AssetBundles folder found!", LogLevel.Info);
                }
            }
            catch (Exception ex) { OdiumConsole.LogException(ex, "CheckAndCreateOdiumFolder"); }
        }

        private static async Task CheckAndCreatePreferencesFile()
        {
            try
            {
                if (!File.Exists(OdiumPrefsPath))
                {
                    OdiumConsole.Log("ModSetup", "odium_prefs.json not found. Creating default preferences file...", LogLevel.Info);
                    bool createSuccess = await CreateDefaultPreferencesFile();
                    if (createSuccess)
                        OdiumConsole.LogGradient("ModSetup", "Default preferences file created successfully!", LogLevel.Info);
                    else
                        OdiumConsole.Log("ModSetup", "Failed to create default preferences file!", LogLevel.Error);
                }
                else
                {
                    OdiumConsole.LogGradient("ModSetup", "odium_prefs.json found!", LogLevel.Info);
                    // Optionally validate the existing file
                    await ValidatePreferencesFile();
                }
            }
            catch (Exception ex) { OdiumConsole.LogException(ex, "CheckAndCreatePreferencesFile"); }
        }

        private static async Task<bool> CreateDefaultPreferencesFile()
        {
            try
            {
                var defaultPrefs = new OdiumPreferences
                {
                    AllocConsole = true
                };

                string jsonContent = OdiumJsonHandler.SerializePreferences(defaultPrefs);

                // Write the file
                File.WriteAllText(OdiumPrefsPath, jsonContent);

                // Verify the file was created successfully
                if (File.Exists(OdiumPrefsPath))
                {
                    var fileInfo = new FileInfo(OdiumPrefsPath);
                    OdiumConsole.LogGradient("ModSetup", $"Preferences file saved successfully! Size: {fileInfo.Length} bytes", LogLevel.Info);
                    return true;
                }
                else
                {
                    OdiumConsole.Log("ModSetup", "Preferences file creation completed but file verification failed!", LogLevel.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "CreateDefaultPreferencesFile");
                return false;
            }
        }

        private static async Task ValidatePreferencesFile()
        {
            try
            {
                string jsonContent = File.ReadAllText(OdiumPrefsPath);
                var preferences = OdiumJsonHandler.ParsePreferences(jsonContent);

                if (preferences != null)
                {
                    OdiumConsole.Log("ModSetup", $"Preferences validated - Console allocation: {preferences.AllocConsole}", LogLevel.Info);
                }
                else
                {
                    OdiumConsole.Log("ModSetup", "Warning: Preferences file exists but could not be parsed correctly", LogLevel.Warning);
                    // Optionally recreate the file with defaults
                    OdiumConsole.Log("ModSetup", "Attempting to recreate preferences file with defaults...", LogLevel.Info);
                    await CreateDefaultPreferencesFile();
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "ValidatePreferencesFile");
                // Try to recreate the file if validation fails
                OdiumConsole.Log("ModSetup", "Attempting to recreate preferences file due to validation error...", LogLevel.Info);
                await CreateDefaultPreferencesFile();
            }
        }

        private static bool AssetsDownloaded()
        {
            try
            {
                // Check if the assets have been downloaded by looking for a marker file or some key assets
                // Since we don't know all files in the zip, we'll check for a few key ones
                bool buttonBackgroundExists = File.Exists(ButtonBackgroundPath);
                bool qmBackgroundExists = File.Exists(QMBackgroundPath);
                bool assetBundleFolderExists = Directory.Exists(AssetBundlesFolderPath) && Directory.GetFiles(AssetBundlesFolderPath).Length > 0;

                OdiumConsole.Log("ModSetup", $"Asset check - ButtonBackground: {buttonBackgroundExists}, QMBackground: {qmBackgroundExists}, AssetBundles folder has files: {assetBundleFolderExists}", LogLevel.Info);

                // If these key files exist, assume the zip was extracted successfully
                return buttonBackgroundExists && qmBackgroundExists && assetBundleFolderExists;
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "AssetsDownloaded");
                return false;
            }
        }

        private static async Task<bool> DownloadAndExtractAssets()
        {
            try
            {
                OdiumConsole.Log("ModSetup", "Downloading assets package...", LogLevel.Info);

                // Download the zip file
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromMinutes(5); // Longer timeout for zip download
                    OdiumConsole.Log("ModSetup", "Connecting to download server...", LogLevel.Info);
                    var response = await httpClient.GetAsync(AssetsZipUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        OdiumConsole.Log("ModSetup", $"Download failed with status: {response.StatusCode}", LogLevel.Error);
                        return false;
                    }

                    var fileBytes = await response.Content.ReadAsByteArrayAsync();
                    OdiumConsole.Log("ModSetup", $"Downloaded {fileBytes.Length} bytes. Saving to temp location...", LogLevel.Info);

                    // Save to temp file
                    File.WriteAllBytes(TempZipPath, fileBytes);

                    if (!File.Exists(TempZipPath))
                    {
                        OdiumConsole.Log("ModSetup", "Failed to save zip file to temp location!", LogLevel.Error);
                        return false;
                    }

                    OdiumConsole.LogGradient("ModSetup", "Assets package downloaded successfully!", LogLevel.Info);
                }

                // Extract the zip file
                OdiumConsole.Log("ModSetup", "Extracting all assets...", LogLevel.Info);

                using (var archive = ZipFile.OpenRead(TempZipPath))
                {
                    int extractedCount = 0;
                    foreach (var entry in archive.Entries)
                    {
                        // Skip directories
                        if (string.IsNullOrEmpty(entry.Name))
                            continue;

                        // Determine the extraction path
                        string extractPath = Path.Combine(OdiumFolderPath, entry.FullName);

                        // Create directory if it doesn't exist
                        string directoryPath = Path.GetDirectoryName(extractPath);
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        // Extract the file
                        OdiumConsole.Log("ModSetup", $"Extracting: {entry.Name}", LogLevel.Info);
                        entry.ExtractToFile(extractPath, overwrite: true);
                        extractedCount++;
                    }

                    OdiumConsole.LogGradient("ModSetup", $"Extracted {extractedCount} files successfully!", LogLevel.Info);
                }

                // Clean up temp file
                try
                {
                    if (File.Exists(TempZipPath))
                    {
                        File.Delete(TempZipPath);
                        OdiumConsole.Log("ModSetup", "Temp zip file cleaned up", LogLevel.Info);
                    }
                }
                catch (Exception cleanupEx)
                {
                    OdiumConsole.Log("ModSetup", $"Warning: Could not delete temp file: {cleanupEx.Message}", LogLevel.Warning);
                }

                // Verify extraction
                bool assetsExtracted = AssetsDownloaded();
                if (assetsExtracted)
                {
                    OdiumConsole.LogGradient("ModSetup", "Assets verified successfully!", LogLevel.Info);
                }
                else
                {
                    OdiumConsole.Log("ModSetup", "Warning: Some key assets may not have been extracted correctly", LogLevel.Warning);
                }

                return assetsExtracted;
            }
            catch (HttpRequestException httpEx)
            {
                OdiumConsole.Log("ModSetup", $"Network error during download: {httpEx.Message}", LogLevel.Error);
                return false;
            }
            catch (TaskCanceledException)
            {
                OdiumConsole.Log("ModSetup", "Download timed out after 5 minutes", LogLevel.Error);
                return false;
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "DownloadAndExtractAssets");
                return false;
            }
        }

        // Getter methods
        public static string GetOdiumFolderPath() => OdiumFolderPath;
        public static string GetButtonBackgroundPath() => ButtonBackgroundPath;
        public static string GetQMBackgroundPath() => QMBackgroundPath;
        public static string GetOdiumPrefsPath() => OdiumPrefsPath;
        public static string GetQMHalfButtonPath() => QMHalfButtonPath;
        public static string GetQMConsolePath() => QMConsolePath;
        public static string GetTabImagePath() => TabImagePath;
        public static string GetNameplatePath() => NameplatePath;

        public static bool ValidateSetup()
        {
            try
            {
                bool folderExists = Directory.Exists(OdiumFolderPath);
                bool prefsExists = File.Exists(OdiumPrefsPath);
                bool assetsExist = AssetsDownloaded();

                OdiumConsole.Log("ModSetup", $"Setup validation - Folder: {folderExists}, Preferences: {prefsExists}, Assets: {assetsExist}", LogLevel.Info);

                return folderExists && prefsExists && assetsExist;
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex, "ValidateSetup");
                return false;
            }
        }

        public static void CleanUp()
        {
            try
            {
                if (Directory.Exists(OdiumFolderPath))
                {
                    Directory.Delete(OdiumFolderPath, true);
                    OdiumConsole.Log("ModSetup", "Odium folder and contents deleted", LogLevel.Info);
                }

                // Clean up temp file if it exists
                if (File.Exists(TempZipPath))
                {
                    File.Delete(TempZipPath);
                    OdiumConsole.Log("ModSetup", "Temp zip file cleaned up", LogLevel.Info);
                }
            }
            catch (Exception ex) { OdiumConsole.LogException(ex, "CleanUp"); }
        }

        // Force update methods
        public static async Task ForceUpdateAllAssets()
        {
            try
            {
                OdiumConsole.Log("ModSetup", "Starting force update of all assets...", LogLevel.Info);

                // Delete all files in the Odium folder except the preferences file
                if (Directory.Exists(OdiumFolderPath))
                {
                    string[] allFiles = Directory.GetFiles(OdiumFolderPath, "*", SearchOption.AllDirectories);
                    foreach (string file in allFiles)
                    {
                        // Skip the preferences file
                        if (file.Equals(OdiumPrefsPath, StringComparison.OrdinalIgnoreCase))
                            continue;

                        try
                        {
                            File.Delete(file);
                            OdiumConsole.Log("ModSetup", $"Deleted existing file: {Path.GetFileName(file)}", LogLevel.Info);
                        }
                        catch (Exception deleteEx)
                        {
                            OdiumConsole.Log("ModSetup", $"Warning: Could not delete {Path.GetFileName(file)}: {deleteEx.Message}", LogLevel.Warning);
                        }
                    }

                    // Delete empty directories (except the main Odium folder)
                    string[] allDirs = Directory.GetDirectories(OdiumFolderPath, "*", SearchOption.AllDirectories);
                    foreach (string dir in allDirs)
                    {
                        try
                        {
                            if (Directory.GetFiles(dir).Length == 0 && Directory.GetDirectories(dir).Length == 0)
                            {
                                Directory.Delete(dir);
                                OdiumConsole.Log("ModSetup", $"Deleted empty directory: {Path.GetFileName(dir)}", LogLevel.Info);
                            }
                        }
                        catch (Exception deleteEx)
                        {
                            OdiumConsole.Log("ModSetup", $"Warning: Could not delete directory {Path.GetFileName(dir)}: {deleteEx.Message}", LogLevel.Warning);
                        }
                    }
                }

                // Re-download and extract
                bool success = await DownloadAndExtractAssets();
                if (success)
                {
                    OdiumConsole.LogGradient("ModSetup", "All assets force updated successfully!", LogLevel.Info);
                }
                else
                {
                    OdiumConsole.Log("ModSetup", "Force update failed!", LogLevel.Error);
                }
            }
            catch (Exception ex) { OdiumConsole.LogException(ex, "ForceUpdateAllAssets"); }
        }

        public static async Task ForceRecreatePreferences()
        {
            try
            {
                if (File.Exists(OdiumPrefsPath))
                {
                    File.Delete(OdiumPrefsPath);
                    OdiumConsole.Log("ModSetup", "Existing preferences file deleted", LogLevel.Info);
                }
                await CheckAndCreatePreferencesFile();
            }
            catch (Exception ex) { OdiumConsole.LogException(ex, "ForceRecreatePreferences"); }
        }

        // Legacy individual update methods (kept for backwards compatibility, but they now use the zip approach)
        public static async Task ForceUpdateQMBackground() => await ForceUpdateAllAssets();
        public static async Task ForceUpdateButtonBackground() => await ForceUpdateAllAssets();
        public static async Task ForceUpdateQMHalfButton() => await ForceUpdateAllAssets();
        public static async Task ForceUpdateQMConsole() => await ForceUpdateAllAssets();
        public static async Task ForceUpdateTabImage() => await ForceUpdateAllAssets();
        public static async Task ForceUpdateNameplate() => await ForceUpdateAllAssets();
        public static async Task ForceUpdateAllImages() => await ForceUpdateAllAssets();
    }
}