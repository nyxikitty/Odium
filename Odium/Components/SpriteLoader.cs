using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MelonLoader;
using System.Threading.Tasks;

public static class ImageLoader
{
    private static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    // WebClient coroutine version - Most compatible with Il2Cpp
    public static System.Collections.IEnumerator LoadSpriteFromURL(string url, Action<Sprite> onComplete, Action<string> onError = null)
    {
        // Check cache first
        if (spriteCache.ContainsKey(url))
        {
            onComplete?.Invoke(spriteCache[url]);
            yield break;
        }

        byte[] imageData = null;
        string errorMessage = null;
        bool completed = false;

        // Use WebClient in a separate task
        Task.Run(() => {
            try
            {
                using (var webClient = new System.Net.WebClient())
                {
                    imageData = webClient.DownloadData(url);
                }
                completed = true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                completed = true;
            }
        });

        // Wait for completion
        while (!completed)
        {
            yield return new WaitForSeconds(0.1f);
        }

        if (errorMessage != null)
        {
            onError?.Invoke($"Failed to download: {errorMessage}");
            yield break;
        }

        if (imageData != null)
        {
            try
            {
                // Create texture and load image data
                Texture2D texture = new Texture2D(2, 2);

                if (ImageConversion.LoadImage(texture, imageData))
                {
                    Sprite sprite = Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f),
                        100f
                    );

                    spriteCache[url] = sprite;
                    onComplete?.Invoke(sprite);
                }
                else
                {
                    onError?.Invoke("Failed to load image data into texture");
                }
            }
            catch (Exception ex)
            {
                onError?.Invoke($"Error creating sprite: {ex.Message}");
            }
        }
        else
        {
            onError?.Invoke("No image data received");
        }
    }

    // Alternative using UnityWebRequest (if available in your VRChat version)
    public static System.Collections.IEnumerator LoadSpriteFromURLWebRequest(string url, Action<Sprite> onComplete, Action<string> onError = null)
    {
        if (spriteCache.ContainsKey(url))
        {
            onComplete?.Invoke(spriteCache[url]);
            yield break;
        }

        var request = UnityEngine.Networking.UnityWebRequest.Get(url);
        {
            yield return request.SendWebRequest();

            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                try
                {
                    byte[] imageData = request.downloadHandler.data;

                    Texture2D texture = new Texture2D(2, 2);
                    if (ImageConversion.LoadImage(texture, imageData))
                    {
                        Sprite sprite = Sprite.Create(
                            texture,
                            new Rect(0, 0, texture.width, texture.height),
                            new Vector2(0.5f, 0.5f),
                            100f
                        );

                        spriteCache[url] = sprite;
                        onComplete?.Invoke(sprite);
                    }
                    else
                    {
                        onError?.Invoke("Failed to load image data into texture");
                    }
                }
                catch (Exception ex)
                {
                    onError?.Invoke($"Error creating sprite: {ex.Message}");
                }
            }
            else
            {
                onError?.Invoke($"Network error: {request.error}");
            }
        }
    }

    // Pure async version using WebClient
    public static async Task<Sprite> LoadSpriteFromURLAsync(string url)
    {
        // Check cache first
        if (spriteCache.ContainsKey(url))
        {
            return spriteCache[url];
        }

        try
        {
            using (var webClient = new System.Net.WebClient())
            {
                byte[] imageData = await webClient.DownloadDataTaskAsync(url);

                // Create texture and load image data
                Texture2D texture = new Texture2D(2, 2);

                if (ImageConversion.LoadImage(texture, imageData))
                {
                    Sprite sprite = Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f),
                        100f
                    );

                    // Cache the sprite
                    spriteCache[url] = sprite;

                    return sprite;
                }
                else
                {
                    throw new Exception("Failed to load image data into texture");
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to download image: {ex.Message}");
        }
    }

    // Synchronous version
    public static Sprite LoadSpriteFromURLSync(string url)
    {
        if (spriteCache.ContainsKey(url))
        {
            return spriteCache[url];
        }

        try
        {
            using (var webClient = new System.Net.WebClient())
            {
                byte[] imageData = webClient.DownloadData(url);

                Texture2D texture = new Texture2D(2, 2);

                if (ImageConversion.LoadImage(texture, imageData))
                {
                    Sprite sprite = Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f),
                        100f
                    );

                    spriteCache[url] = sprite;
                    return sprite;
                }
                else
                {
                    throw new Exception("Failed to load image data into texture");
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to download image: {ex.Message}");
        }
    }

    // Manual HTTP request approach (most compatible)
    public static System.Collections.IEnumerator LoadSpriteFromURLManual(string url, Action<Sprite> onComplete, Action<string> onError = null)
    {
        if (spriteCache.ContainsKey(url))
        {
            onComplete?.Invoke(spriteCache[url]);
            yield break;
        }

        byte[] imageData = null;
        string errorMessage = null;
        bool completed = false;

        // Manual HTTP request
        var request = System.Net.WebRequest.Create(url) as System.Net.HttpWebRequest;
        if (request != null)
        {
            request.Method = "GET";
            request.UserAgent = "Unity";

            request.BeginGetResponse((ar) => {
                try
                {
                    var response = request.EndGetResponse(ar);
                    using (var stream = response.GetResponseStream())
                    {
                        var memoryStream = new System.IO.MemoryStream();
                        byte[] buffer = new byte[4096];
                        int bytesRead;
                        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            memoryStream.Write(buffer, 0, bytesRead);
                        }
                        imageData = memoryStream.ToArray();
                    }
                    completed = true;
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    completed = true;
                }
            }, null);
        }
        else
        {
            onError?.Invoke("Failed to create HTTP request");
            yield break;
        }

        // Wait for completion
        float timeout = 30f; // 30 second timeout
        float elapsed = 0f;
        while (!completed && elapsed < timeout)
        {
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        if (!completed)
        {
            onError?.Invoke("Request timed out");
            yield break;
        }

        if (errorMessage != null)
        {
            onError?.Invoke($"Failed to download: {errorMessage}");
            yield break;
        }

        if (imageData != null && imageData.Length > 0)
        {
            try
            {
                Texture2D texture = new Texture2D(2, 2);
                if (ImageConversion.LoadImage(texture, imageData))
                {
                    Sprite sprite = Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f),
                        100f
                    );

                    spriteCache[url] = sprite;
                    onComplete?.Invoke(sprite);
                }
                else
                {
                    onError?.Invoke("Failed to load image data into texture");
                }
            }
            catch (Exception ex)
            {
                onError?.Invoke($"Error creating sprite: {ex.Message}");
            }
        }
        else
        {
            onError?.Invoke("No image data received");
        }
    }

    // Clear cache to free memory
    public static void ClearCache()
    {
        foreach (var sprite in spriteCache.Values)
        {
            if (sprite != null && sprite.texture != null)
            {
                UnityEngine.Object.Destroy(sprite.texture);
                UnityEngine.Object.Destroy(sprite);
            }
        }
        spriteCache.Clear();
    }

    // Remove specific URL from cache
    public static void RemoveFromCache(string url)
    {
        if (spriteCache.ContainsKey(url))
        {
            var sprite = spriteCache[url];
            if (sprite != null && sprite.texture != null)
            {
                UnityEngine.Object.Destroy(sprite.texture);
                UnityEngine.Object.Destroy(sprite);
            }
            spriteCache.Remove(url);
        }
    }
}