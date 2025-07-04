using MelonLoader;
using Odium.Odium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;

namespace Odium.Components
{
    class SpriteUtil
    {
        public static Texture2D CreateTextureFromBase64(string base64)
        {
            byte[] imageData = Convert.FromBase64String(base64);
            Texture2D texture = new Texture2D(2, 2);

            UnityEngine.ImageConversion.LoadImage(texture, imageData);
            return texture;
        }

        public static Sprite Create(Texture2D texture, UnityEngine.Rect rect, UnityEngine.Vector2 size)
        {
            return Sprite.Create(texture, rect, size);
        }

        public static void ApplySpriteToButton(string objectName, string image)
        {
            try
            {
                Sprite sprite;

                if (FileHelper.IsPath(image))
                {
                    sprite = LoadFromDisk(image);
                }
                else
                {
                    image = Path.Combine(Environment.CurrentDirectory, "Odium", image);
                    sprite = LoadFromDisk(image);
                }

                AssignedVariables.userInterface.transform.Find(objectName).transform.Find("Background").GetComponent<Image>().overrideSprite = sprite;

                OdiumConsole.LogGradient("Odium", $"Sprite applied to {objectName} successfully!");
            } catch (Exception ex)
            {
                OdiumConsole.LogException(ex);
            }
        }

        public static void ApplySpriteToMenu(string objectName, string image)
        {
            try
            {
                Sprite sprite;

                if (FileHelper.IsPath(image))
                {
                    sprite = LoadFromDisk(image);
                }
                else
                {
                    image = Path.Combine(Environment.CurrentDirectory, "Odium", image);
                    sprite = LoadFromDisk(image);
                }

                AssignedVariables.userInterface.transform.Find(objectName).GetComponent<Image>().overrideSprite = sprite;

                OdiumConsole.LogGradient("Odium", $"Sprite applied to {objectName} successfully!");
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex);
            }
        }

        public static void ApplyColorToMenu(string objectName, Color color)
        {
            try
            {
                AssignedVariables.userInterface.transform.Find(objectName).GetComponent<Image>().m_Color = color;
                OdiumConsole.LogGradient("Odium", $"Color applied to {objectName} successfully!");
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex);
            }
        }

        public static void ApplyColorToButton(string objectName, Color color)
        {
            try
            {
                AssignedVariables.userInterface.transform.Find(objectName).GetComponent<Image>().m_Color = color;
                OdiumConsole.LogGradient("Odium", $"Color applied to {objectName} successfully!");
            }
            catch (Exception ex)
            {
                OdiumConsole.LogException(ex);
            }
        }

        public static Sprite LoadFromDisk(string path, float pixelsPerUnit = 100f)
        {
            if (string.IsNullOrEmpty(path))
            {
                MelonLogger.Warning("Cannot load sprite: Path is null or empty");
                return null;
            }

            try
            {
                byte[] fileData = File.ReadAllBytes(path);
                if (fileData == null || fileData.Length == 0)
                {
                    MelonLogger.Warning($"Cannot load sprite: No data found at path '{path}'");
                    return null;
                }

                Texture2D texture = new Texture2D(2, 2);
                if (!ImageConversion.LoadImage(texture, fileData))
                {
                    MelonLogger.Error($"Failed to convert image data to texture from path '{path}'");
                    return null;
                }

                Rect rect = new Rect(0f, 0f, texture.width, texture.height);
                Vector2 pivot = new Vector2(0.5f, 0.5f);

                Sprite sprite = Sprite.Create(
                    texture,
                    rect,
                    pivot,
                    pixelsPerUnit,
                    0,
                    SpriteMeshType.FullRect,
                    Vector4.zero,
                    false);

                sprite.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                texture.hideFlags |= HideFlags.DontUnloadUnusedAsset;

                return sprite;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error loading sprite from '{path}': {ex.Message}", ex);
                return null;
            }
        }
    }
}
