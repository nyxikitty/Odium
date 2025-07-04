using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace Odium
{
    public static class OdiumConsole
    {
        #region Native Imports
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, int dwMode);
        #endregion

        private const int STD_OUTPUT_HANDLE = -11;
        private const int ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4;
        private static bool _isInitialized = false;

        public static void Initialize()
        {
            // Check if console should be allocated based on JSON preferences
            if (!ShouldAllocateConsole())
            {
                return; // Don't allocate console if preference is false or file doesn't exist
            }

            if (GetConsoleWindow() != IntPtr.Zero) return;

            try
            {
                AllocConsole();
                Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
                Console.CursorVisible = false;
                Console.Title = "Odium Console";
                EnableVirtualTerminalProcessing();

                DisplayBanner();
                Log("System", "Console initialized successfully", LogLevel.Info);

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Log("System", $"Failed to initialize console: {ex.Message}", LogLevel.Error);
            }
        }

        private static bool ShouldAllocateConsole()
        {
            try
            {
                // Get the Odium folder path
                string odiumFolderPath = Components.ModSetup.GetOdiumFolderPath();
                string prefsFilePath = Path.Combine(odiumFolderPath, "odium_prefs.json");

                // Check if the preferences file exists
                if (!File.Exists(prefsFilePath))
                {
                    // If file doesn't exist, create a default one with allocConsole: true
                    CreateDefaultPreferencesFile(prefsFilePath);
                    return true; // Default to true if file doesn't exist
                }

                // Read and parse the JSON file using our custom parser
                string jsonContent = File.ReadAllText(prefsFilePath);
                var preferences = OdiumJsonHandler.ParsePreferences(jsonContent);

                return preferences?.AllocConsole ?? true; // Default to true if property is missing
            }
            catch (Exception)
            {
                // If any error occurs (file reading, JSON parsing, etc.), default to true
                return true;
            }
        }

        private static void CreateDefaultPreferencesFile(string filePath)
        {
            try
            {
                var defaultPrefs = new OdiumPreferences
                {
                    AllocConsole = true
                };

                string jsonString = OdiumJsonHandler.SerializePreferences(defaultPrefs);
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception)
            {
                // Silently fail if we can't create the default file
            }
        }

        public static void Log(string category, string message, LogLevel level = LogLevel.Info)
        {
            if (!_isInitialized) return;
            try
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                Console.ResetColor();
                Console.Write($"[{timestamp}] ");
                Console.ForegroundColor = GetCategoryColor(category);
                Console.Write($"[{category}] ");
                Console.ResetColor();
                Console.ForegroundColor = GetLevelColor(level);
                Console.WriteLine(message);
                Console.ResetColor();
            }
            catch
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{category}] {message}");
            }
        }

        public static void LogGradient(string category, string message, LogLevel level = LogLevel.Info, bool gradientCategory = false)
        {
            if (!_isInitialized) return;
            try
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                Console.ResetColor();
                Console.Write($"[{timestamp}] ");

                if (gradientCategory)
                {
                    Console.Write("[");
                    LogMessageWithGradient(category, false);
                    Console.Write("] ");
                    Console.ResetColor();
                    Console.WriteLine(message);
                }
                else
                {
                    Console.ForegroundColor = GetCategoryColor(category);
                    Console.Write($"[{category}] ");
                    Console.ResetColor();
                    LogMessageWithGradient(message);
                }
            }
            catch
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{category}] {message}");
            }
        }

        private static void LogMessageWithGradient(string text, bool addNewline = true)
        {
            int length = text.Length;
            for (int i = 0; i < length; i++)
            {
                int red = 255;
                int green = 204 - (i * 204 / length);
                int blue = 203 + (i * 52 / length);
                Console.Write($"\u001b[38;2;{red};{green};{blue}m{text[i]}");
            }
            if (addNewline)
            {
                Console.WriteLine("\u001b[0m");
            }
            else
            {
                Console.Write("\u001b[0m");
            }
        }

        // Tuples are amazing :3
        private static void LogWithGradient(string text, (int red, int green, int blue) startColor, (int red, int green, int blue) endColor)
        {
            int length = text.Length;
            for (int i = 0; i < length; i++)
            {
                int red = startColor.red + (i * (endColor.red - startColor.red) / length);
                int green = startColor.green + (i * (endColor.green - startColor.green) / length);
                int blue = startColor.blue + (i * (endColor.blue - startColor.blue) / length);

                Console.Write(string.Format("\u001b[38;2;{0};{1};{2}m{3}", new object[]
                {
            red,
            green,
            blue,
            text[i]
                }));
            }
            Console.WriteLine("\u001b[0m");
        }

        public static void LogException(Exception ex, string context = null)
        {
            if (!_isInitialized) return;

            var timestamp = DateTime.Now.ToString("HH:mm:ss");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[{timestamp}] ============ EXCEPTION ============");
            Console.WriteLine($"Context: {context ?? "None"}");
            Console.WriteLine($"Type: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine($"Stack Trace:\n{ex.StackTrace}");
            Console.WriteLine("===================================\n");
            Console.ResetColor();
        }

        #region Private Helpers
        private static void EnableVirtualTerminalProcessing()
        {
            try
            {
                var stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
                if (!GetConsoleMode(stdHandle, out var mode)) return;

                mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
                SetConsoleMode(stdHandle, mode);
            }
            catch
            {
                Log("System", "Failed to enable VT processing", LogLevel.Warning);
            }
        }

        private static ConsoleColor GetCategoryColor(string category)
        {
            if (category.StartsWith("System", StringComparison.OrdinalIgnoreCase))
                return ConsoleColor.Cyan;
            if (category.StartsWith("Network", StringComparison.OrdinalIgnoreCase))
                return ConsoleColor.Magenta;
            if (category.StartsWith("UI", StringComparison.OrdinalIgnoreCase))
                return ConsoleColor.Green;

            return ConsoleColor.White;
        }

        private static ConsoleColor GetLevelColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Error:
                    return ConsoleColor.Red;
                case LogLevel.Warning:
                    return ConsoleColor.Yellow;
                case LogLevel.Debug:
                    return ConsoleColor.Blue;
                default:
                    return ConsoleColor.Gray;
            }
        }

        private static void DisplayBanner()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            LogWithGradient(@"
                    /================================================================================\
                    ||                                                                              ||
                    ||                                                                              ||
                    ||                                                                              ||
                    ||                                                                              ||
                    ||                                                                              ||
                    ||                 ______    _______   __   __    __  .___  ___.                ||
                    ||                /  __  \  |       \ |  | |  |  |  | |   \/   |                ||
                    ||               |  |  |  | |  .--.  ||  | |  |  |  | |  \  /  |                ||
                    ||               |  |  |  | |  |  |  ||  | |  |  |  | |  |\/|  |                ||
                    ||               |  `--'  | |  '--'  ||  | |  `--'  | |  |  |  |                ||
                    ||                \______/  |_______/ |__|  \______/  |__|  |__|                ||
                    ||                                                                              ||
                    ||                                                                              ||
                    ||                                                                              ||
                    ||                                                                              ||
                    ||                                                                              ||
                    \================================================================================/
                         ", (255, 192, 203), (255, 20, 147));
            Console.ResetColor();
            LogWithGradient($"                    Odium Console - {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n", (255, 192, 203), (255, 20, 147));
        }
        #endregion
    }

    // Custom JSON handler class for Odium preferences
    public static class OdiumJsonHandler
    {
        public static OdiumPreferences ParsePreferences(string jsonString)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jsonString))
                    return null;

                var preferences = new OdiumPreferences();

                // Remove whitespace and braces
                string cleaned = jsonString.Trim().Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");
                if (cleaned.StartsWith("{"))
                    cleaned = cleaned.Substring(1);
                if (cleaned.EndsWith("}"))
                    cleaned = cleaned.Substring(0, cleaned.Length - 1);

                // Split by commas to get individual properties
                string[] properties = cleaned.Split(',');

                foreach (string property in properties)
                {
                    if (string.IsNullOrWhiteSpace(property))
                        continue;

                    // Split by colon to get key-value pair
                    string[] keyValue = property.Split(':');
                    if (keyValue.Length != 2)
                        continue;

                    string key = RemoveQuotes(keyValue[0].Trim());
                    string value = RemoveQuotes(keyValue[1].Trim());

                    // Parse the allocConsole property
                    if (key.Equals("allocConsole", StringComparison.OrdinalIgnoreCase))
                    {
                        if (bool.TryParse(value, out bool boolValue))
                        {
                            preferences.AllocConsole = boolValue;
                        }
                    }
                }

                return preferences;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string SerializePreferences(OdiumPreferences preferences)
        {
            try
            {
                if (preferences == null)
                    return "{}";

                var sb = new StringBuilder();
                sb.AppendLine("{");
                sb.AppendLine($"  \"allocConsole\": {preferences.AllocConsole.ToString().ToLower()}");
                sb.AppendLine("}");

                return sb.ToString();
            }
            catch (Exception)
            {
                return "{}";
            }
        }

        private static string RemoveQuotes(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            if (input.StartsWith("\"") && input.EndsWith("\"") && input.Length >= 2)
                return input.Substring(1, input.Length - 2);

            return input;
        }

        // Helper method to add more properties in the future
        public static void AddProperty(Dictionary<string, object> properties, string key, object value)
        {
            if (!properties.ContainsKey(key))
                properties.Add(key, value);
            else
                properties[key] = value;
        }

        // Helper method to serialize any simple object to JSON
        public static string SerializeSimpleObject(Dictionary<string, object> properties)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("{");

                bool first = true;
                foreach (var kvp in properties)
                {
                    if (!first)
                        sb.AppendLine(",");

                    string valueString;
                    if (kvp.Value is bool boolVal)
                        valueString = boolVal.ToString().ToLower();
                    else if (kvp.Value is string stringVal)
                        valueString = $"\"{stringVal}\"";
                    else if (kvp.Value is int || kvp.Value is float || kvp.Value is double)
                        valueString = kvp.Value.ToString();
                    else
                        valueString = $"\"{kvp.Value}\"";

                    sb.Append($"  \"{kvp.Key}\": {valueString}");
                    first = false;
                }

                sb.AppendLine();
                sb.AppendLine("}");

                return sb.ToString();
            }
            catch (Exception)
            {
                return "{}";
            }
        }
    }

    // Helper class for preferences
    public class OdiumPreferences
    {
        public bool AllocConsole { get; set; } = true;
    }

    public enum LogLevel
    {
        Info,
        Debug,
        Warning,
        Error
    }
}