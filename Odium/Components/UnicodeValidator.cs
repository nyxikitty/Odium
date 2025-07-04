
using System;

public static class UnicodeValidator
{
    /// <summary>
    /// Checks if the input string contains anything other than basic ASCII characters.
    /// </summary>
    /// <param name="input">The string to check</param>
    /// <returns>True if the string contains non-ASCII characters, False if it's only basic ASCII</returns>
    public static bool Sanitize(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        foreach (char c in input)
        {
            if (c > 127)
                return true;

            if (char.IsControl(c) && !IsAllowedWhitespace(c))
                return true;
        }

        return false;
    }

    private static bool IsAllowedWhitespace(char c)
    {
        return c == ' ' || c == '\t' || c == '\r' || c == '\n';
    }
}