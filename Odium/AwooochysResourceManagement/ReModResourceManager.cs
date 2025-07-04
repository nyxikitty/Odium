using System;
using System.IO;
using System.Reflection;
using UnhollowerBaseLib; // Keep this if you are working with Il2Cpp, as indicated by Il2CppStructArray
using UnityEngine;

namespace Odium.AwooochysResourceManagement
{
    public static class ReModResourceManager
    {
       
       public static Sprite LoadSpriteFromDisk(this string path, int width = 512, int height = 512)
       {
          bool flag = string.IsNullOrEmpty(path);
          Sprite sprite;
          if (flag)
          {
             sprite = null;
          }
          else
          {
             byte[] array = File.ReadAllBytes(path);
             bool flag2 = array == null || array.Length == 0;
             if (flag2)
             {
                sprite = null;
             }
             else
             {
                Texture2D texture2D = new Texture2D(width, height);
                bool flag3 = !ImageConversion.LoadImage(texture2D, array);
                if (flag3)
                {
                   sprite = null;
                }
                else
                {
                   Sprite sprite2 = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0f, 0f), 100000f, 1000U, SpriteMeshType.FullRect, Vector4.zero, false); // Changed 0 to SpriteMeshType.FullRect
                   sprite2.hideFlags |= (HideFlags)32; // <--- FIX IS HERE
                   sprite = sprite2;
                }
             }
          }
          return sprite;
       }

       
       public static Sprite LoadSpriteFromByteArray(this byte[] bytes, int width = 512, int height = 512)
       {
          bool flag = bytes == null || bytes.Length == 0;
          Sprite sprite;
          if (flag)
          {
             sprite = null;
          }
          else
          {
             Texture2D texture2D = new Texture2D(width, height);
             bool flag2 = !ImageConversion.LoadImage(texture2D, bytes);
             if (flag2)
             {
                sprite = null;
             }
             else
             {
                Sprite sprite2 = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0f, 0f), 100000f, 1000U, SpriteMeshType.FullRect, Vector4.zero, false); // Changed 0 to SpriteMeshType.FullRect
                sprite2.hideFlags |= (HideFlags)32; // <--- FIX IS HERE
                sprite = sprite2;
             }
          }
          return sprite;
       }

       
       public static Sprite LoadSpriteFromByteArray(this Il2CppStructArray<byte> bytes, int width = 512, int height = 512)
       {
          bool flag = bytes == null || bytes.Length == 0;
          Sprite sprite;
          if (flag)
          {
             sprite = null;
          }
          else
          {
             Texture2D texture2D = new Texture2D(width, height);
             bool flag2 = !ImageConversion.LoadImage(texture2D, bytes);
             if (flag2)
             {
                sprite = null;
             }
             else
             {
                Sprite sprite2 = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0f, 0f), 100000f, 1000U, SpriteMeshType.FullRect, Vector4.zero, false); // Changed 0 to SpriteMeshType.FullRect
                sprite2.hideFlags |= (HideFlags)32; // <--- FIX IS HERE
                sprite = sprite2;
             }
          }
          return sprite;
       }

       
       public static Sprite LoadSpriteFromBundledResource(this string path, int width = 512, int height = 512)
       {
          Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
          MemoryStream memoryStream = new MemoryStream((int)manifestResourceStream.Length);
          manifestResourceStream.CopyTo(memoryStream);
          Texture2D texture2D = new Texture2D(width, height);
          bool flag = !ImageConversion.LoadImage(texture2D, memoryStream.ToArray());
          Sprite sprite;
          if (flag)
          {
             sprite = null;
          }
          else
          {
             Sprite sprite2 = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0f, 0f), 100000f, 1000U, SpriteMeshType.FullRect, Vector4.zero, false); // Changed 0 to SpriteMeshType.FullRect
             sprite2.hideFlags |= (HideFlags)32; // <--- FIX IS HERE
             sprite = sprite2;
          }
          return sprite;
       }
    }
}