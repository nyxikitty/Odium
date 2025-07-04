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
        private const int STD_INPUT_HANDLE = -10;
        private const int ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4;
        private const int ENABLE_ECHO_INPUT = 0x0004;
        private const int ENABLE_LINE_INPUT = 0x0002;
        private const int ENABLE_PROCESSED_INPUT = 0x0001;
        private static bool _isInitialized = false;

        public static void Initialize()
        {
            if (!ShouldAllocateConsole()) return;
            if (GetConsoleWindow() != IntPtr.Zero) return;

            try
            {
                AllocConsole();
                System.Threading.Thread.Sleep(200); // Give console time to initialize

                // Initialize standard streams
                InitializeStandardStreams();

                Console.Title = "Odium Console";
                EnableVirtualTerminalProcessing();
                EnableInputMode();
                Console.CursorVisible = true;

                DisplayBanner();
                Log("System", "Console initialized successfully", LogLevel.Info);
                Log("System", "Console ready for input commands", LogLevel.Info);

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Log("System", $"Failed to initialize console: {ex.Message}", LogLevel.Error);
            }
        }

        private static void InitializeStandardStreams()
        {
            try
            {
                // Initialize output
                var stdout = Console.OpenStandardOutput();
                var writer = new StreamWriter(stdout) { AutoFlush = true };
                Console.SetOut(writer);

                // Initialize input
                var stdin = Console.OpenStandardInput();
                var reader = new StreamReader(stdin);
                Console.SetIn(reader);
            }
            catch (Exception ex)
            {
                Log("System", $"Failed to initialize streams: {ex.Message}", LogLevel.Error);
            }
        }

        private static void EnableInputMode()
        {
            try
            {
                var inputHandle = GetStdHandle(STD_INPUT_HANDLE);
                if (inputHandle != IntPtr.Zero && GetConsoleMode(inputHandle, out var mode))
                {
                    mode |= ENABLE_ECHO_INPUT | ENABLE_LINE_INPUT | ENABLE_PROCESSED_INPUT;
                    SetConsoleMode(inputHandle, mode);
                }
            }
            catch (Exception ex)
            {
                Log("System", $"Failed to set input mode: {ex.Message}", LogLevel.Warning);
            }
        }

        private static bool ShouldAllocateConsole()
        {
            try
            {
                string odiumFolderPath = Components.ModSetup.GetOdiumFolderPath();
                string prefsFilePath = Path.Combine(odiumFolderPath, "odium_prefs.json");

                if (!File.Exists(prefsFilePath))
                {
                    CreateDefaultPreferencesFile(prefsFilePath);
                    return true;
                }

                string jsonContent = File.ReadAllText(prefsFilePath);
                var preferences = OdiumJsonHandler.ParsePreferences(jsonContent);
                return preferences?.AllocConsole ?? true;
            }
            catch
            {
                return true;
            }
        }

        private static void CreateDefaultPreferencesFile(string filePath)
        {
            try
            {
                var defaultPrefs = new OdiumPreferences { AllocConsole = true };
                string jsonString = OdiumJsonHandler.SerializePreferences(defaultPrefs);
                File.WriteAllText(filePath, jsonString);
            }
            catch { }
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

        public static string Readline(string category, string message, LogLevel level = LogLevel.Info)
        {
            string input = string.Empty;
            if (!_isInitialized) return input;
            try
            {
                input = Console.ReadLine();
            }
            catch
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{category}] {message}");
            }

            return input;
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
            Console.Write("\u001b[0m");
            if (addNewline) Console.WriteLine();
        }

        private static void LogWithGradient(string text, (int red, int green, int blue) startColor, (int red, int green, int blue) endColor)
        {
            int length = text.Length;
            for (int i = 0; i < length; i++)
            {
                int red = startColor.red + (i * (endColor.red - startColor.red) / length);
                int green = startColor.green + (i * (endColor.green - startColor.green) / length);
                int blue = startColor.blue + (i * (endColor.blue - startColor.blue) / length);

                Console.Write($"\u001b[38;2;{red};{green};{blue}m{text[i]}");
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
    }

    public static class OdiumJsonHandler
    {
        public static OdiumPreferences ParsePreferences(string jsonString)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jsonString))
                    return null;

                var preferences = new OdiumPreferences();
                string cleaned = jsonString.Trim().Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");
                if (cleaned.StartsWith("{"))
                    cleaned = cleaned.Substring(1);
                if (cleaned.EndsWith("}"))
                    cleaned = cleaned.Substring(0, cleaned.Length - 1);

                string[] properties = cleaned.Split(',');
                foreach (string property in properties)
                {
                    if (string.IsNullOrWhiteSpace(property))
                        continue;

                    string[] keyValue = property.Split(':');
                    if (keyValue.Length != 2)
                        continue;

                    string key = RemoveQuotes(keyValue[0].Trim());
                    string value = RemoveQuotes(keyValue[1].Trim());

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
            catch
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
            catch
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
    }

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