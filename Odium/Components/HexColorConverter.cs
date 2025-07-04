using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odium.Components
{
    using UnityEngine;
    using System;

    public static class HexColorConverter
    {
        /// <summary>
        /// Converts a hex color string to Unity Color
        /// Supports formats: #RGB, #RRGGBB, #RRGGBBAA, RGB, RRGGBB, RRGGBBAA
        /// </summary>
        /// <param name="hex">Hex color string</param>
        /// <returns>Unity Color object</returns>
        public static Color HexToColor(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                throw new ArgumentException("Hex string cannot be null or empty");

            hex = hex.TrimStart('#');

            if (!IsValidHex(hex))
                throw new ArgumentException($"Invalid hex color: #{hex}");

            switch (hex.Length)
            {
                case 3: // RGB
                    hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
                    break;
                case 4: // RGBA
                    hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}";
                    break;
                case 6: // RRGGBB
                    hex += "FF"; // Add full alpha
                    break;
                case 8: // RRGGBBAA
                    break;
                default:
                    throw new ArgumentException($"Invalid hex color length: #{hex}");
            }

            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            byte a = Convert.ToByte(hex.Substring(6, 2), 16);

            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        /// <summary>
        /// Converts a hex color string to Unity Color32
        /// </summary>
        /// <param name="hex">Hex color string</param>
        /// <returns>Unity Color32 object</returns>
        public static Color32 HexToColor32(string hex)
        {
            Color color = HexToColor(hex);
            return new Color32(
                (byte)(color.r * 255),
                (byte)(color.g * 255),
                (byte)(color.b * 255),
                (byte)(color.a * 255)
            );
        }

        /// <summary>
        /// Converts Unity Color to hex string
        /// </summary>
        /// <param name="color">Unity Color</param>
        /// <param name="includeAlpha">Include alpha channel in output</param>
        /// <returns>Hex color string with # prefix</returns>
        public static string ColorToHex(Color color, bool includeAlpha = true)
        {
            Color32 c = color;
            if (includeAlpha)
                return $"#{c.r:X2}{c.g:X2}{c.b:X2}{c.a:X2}";
            else
                return $"#{c.r:X2}{c.g:X2}{c.b:X2}";
        }

        /// <summary>
        /// Converts Unity Color32 to hex string
        /// </summary>
        /// <param name="color">Unity Color32</param>
        /// <param name="includeAlpha">Include alpha channel in output</param>
        /// <returns>Hex color string with # prefix</returns>
        public static string Color32ToHex(Color32 color, bool includeAlpha = true)
        {
            if (includeAlpha)
                return $"#{color.r:X2}{color.g:X2}{color.b:X2}{color.a:X2}";
            else
                return $"#{color.r:X2}{color.g:X2}{color.b:X2}";
        }

        /// <summary>
        /// Tries to convert hex string to Unity Color without throwing exceptions
        /// </summary>
        /// <param name="hex">Hex color string</param>
        /// <param name="color">Output Unity Color</param>
        /// <returns>True if conversion successful, false otherwise</returns>
        public static bool TryHexToColor(string hex, out Color color)
        {
            color = Color.white;
            try
            {
                color = HexToColor(hex);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates if a string is a valid hex color
        /// </summary>
        /// <param name="hex">Hex string to validate (without #)</param>
        /// <returns>True if valid hex color</returns>
        private static bool IsValidHex(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return false;

            if (hex.Length != 3 && hex.Length != 4 && hex.Length != 6 && hex.Length != 8)
                return false;

            foreach (char c in hex)
            {
                if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')))
                    return false;
            }

            return true;
        }
    }
}
