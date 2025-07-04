using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Odium.Components
{
    class GradientColorTags
    {
        public static string GetAnimatedGradientText(string text, Color color1, Color color2, float speed = 1.0f, float waveLength = 2.0f)
        {
            if (string.IsNullOrEmpty(text)) return text;

            StringBuilder sb = new StringBuilder();
            float time = Time.time * speed;

            for (int i = 0; i < text.Length; i++)
            {
                float normalizedPosition = (float)i / (text.Length - 1);
                float animatedPosition = normalizedPosition + (Mathf.Sin(time + i * waveLength) * 0.5f + 0.5f) * 0.3f;
                animatedPosition = Mathf.Clamp01(animatedPosition);

                Color currentColor = Color.Lerp(color1, color2, animatedPosition);
                string hexColor = ColorUtility.ToHtmlStringRGB(currentColor);

                sb.Append($"<color=#{hexColor}>{text[i]}</color>");
            }

            return sb.ToString();
        }

        public static string GetWaveGradientText(string text, Color color1, Color color2, float speed = 2.0f, float frequency = 0.5f)
        {
            if (string.IsNullOrEmpty(text)) return text;

            StringBuilder sb = new StringBuilder();
            float time = Time.time * speed;

            for (int i = 0; i < text.Length; i++)
            {
                float wave = Mathf.Sin(time + i * frequency) * 0.5f + 0.5f;
                Color currentColor = Color.Lerp(color1, color2, wave);
                string hexColor = ColorUtility.ToHtmlStringRGB(currentColor);

                sb.Append($"<color=#{hexColor}>{text[i]}</color>");
            }

            return sb.ToString();
        }

        public static string GetPulseGradientText(string text, Color color1, Color color2, float speed = 1.5f)
        {
            if (string.IsNullOrEmpty(text)) return text;

            StringBuilder sb = new StringBuilder();
            float pulse = Mathf.Sin(Time.time * speed) * 0.5f + 0.5f;

            for (int i = 0; i < text.Length; i++)
            {
                float normalizedPosition = (float)i / (text.Length - 1);
                float gradientPosition = Mathf.Lerp(pulse * 0.3f, 1.0f - pulse * 0.3f, normalizedPosition);

                Color currentColor = Color.Lerp(color1, color2, gradientPosition);
                string hexColor = ColorUtility.ToHtmlStringRGB(currentColor);

                sb.Append($"<color=#{hexColor}>{text[i]}</color>");
            }

            return sb.ToString();
        }

        public static string GetRainbowText(string text, float speed = 1.0f, float frequency = 0.3f)
        {
            if (string.IsNullOrEmpty(text)) return text;

            StringBuilder sb = new StringBuilder();
            float time = Time.time * speed;

            for (int i = 0; i < text.Length; i++)
            {
                float hue = (time + i * frequency) % 1.0f;
                Color currentColor = Color.HSVToRGB(hue, 1.0f, 1.0f);
                string hexColor = ColorUtility.ToHtmlStringRGB(currentColor);

                sb.Append($"<color=#{hexColor}>{text[i]}</color>");
            }

            return sb.ToString();
        }

        public static string GetFireText(string text, float speed = 2.0f)
        {
            Color color1 = new Color(1.0f, 0.2f, 0.0f);
            Color color2 = new Color(1.0f, 1.0f, 0.0f);
            Color color3 = new Color(1.0f, 0.5f, 0.0f);

            if (string.IsNullOrEmpty(text)) return text;

            StringBuilder sb = new StringBuilder();
            float time = Time.time * speed;

            for (int i = 0; i < text.Length; i++)
            {
                float wave1 = Mathf.Sin(time + i * 0.5f) * 0.5f + 0.5f;
                float wave2 = Mathf.Cos(time * 1.3f + i * 0.3f) * 0.5f + 0.5f;

                Color tempColor = Color.Lerp(color1, color2, wave1);
                Color currentColor = Color.Lerp(tempColor, color3, wave2);

                string hexColor = ColorUtility.ToHtmlStringRGB(currentColor);
                sb.Append($"<color=#{hexColor}>{text[i]}</color>");
            }

            return sb.ToString();
        }

        public static string GetGlitchText(string text, float speed = 5.0f)
        {
            Color color1 = Color.red;
            Color color2 = Color.cyan;
            Color color3 = Color.white;

            if (string.IsNullOrEmpty(text)) return text;

            StringBuilder sb = new StringBuilder();
            float time = Time.time * speed;

            for (int i = 0; i < text.Length; i++)
            {
                float random = Mathf.Sin(time * 7.0f + i * 2.0f) * 0.5f + 0.5f;

                Color currentColor;
                if (random < 0.33f)
                    currentColor = color1;
                else if (random < 0.66f)
                    currentColor = color2;
                else
                    currentColor = color3;

                string hexColor = ColorUtility.ToHtmlStringRGB(currentColor);
                sb.Append($"<color=#{hexColor}>{text[i]}</color>");
            }

            return sb.ToString();
        }
    }
}