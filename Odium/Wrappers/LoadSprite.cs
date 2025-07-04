using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Odium.Wrappers
{
    internal static class SpriteLoader
    {
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
