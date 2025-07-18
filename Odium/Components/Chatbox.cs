using ExitGames.Client.Photon;
using MelonLoader;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Odium.Components
{
    public class Chatbox
    {
        private static Dictionary<string, FrameEffect> activeFrameEffects = new Dictionary<string, FrameEffect>();

        private class FrameEffect
        {
            public string[] frames;
            public int currentFrameIndex;
            public float frameTimer;
            public Action onComplete;
            public bool shouldLoop;
            public bool isWaiting;
            public float waitTime;

            public FrameEffect(string[] frameArray, Action callback, bool loop = false, float loopWaitTime = 3f)
            {
                frames = frameArray;
                currentFrameIndex = 0;
                frameTimer = 0f;
                onComplete = callback;
                shouldLoop = loop;
                isWaiting = false;
                waitTime = loopWaitTime;
            }

            public void Reset()
            {
                frameTimer = 0f;
                currentFrameIndex = 0;
                isWaiting = false;
            }

            public string GetCurrentFrame()
            {
                if (frames != null && currentFrameIndex < frames.Length)
                    return frames[currentFrameIndex];
                return "";
            }

            public void MoveToNextFrame()
            {
                currentFrameIndex++;
            }

            public bool HasMoreFrames()
            {
                return currentFrameIndex < frames.Length;
            }
        }

        public static void SendFrameAnimation(string[] frames, string effectId = null,
                                            Action onComplete = null, bool loop = false, float loopWaitTime = 3f)
        {
            if (effectId != null && activeFrameEffects.ContainsKey(effectId))
            {
                activeFrameEffects.Remove(effectId);
            }

            var effect = new FrameEffect(frames, onComplete, loop, loopWaitTime);

            if (effectId != null)
            {
                activeFrameEffects[effectId] = effect;
            }
            else
            {
                string autoId = "auto_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                activeFrameEffects[autoId] = effect;
            }
        }

        public static void CancelFrameEffect(string effectId)
        {
            if (effectId != null && activeFrameEffects.ContainsKey(effectId))
            {
                activeFrameEffects.Remove(effectId);
            }
        }

        public static void UpdateFrameEffects()
        {
            var keysToRemove = new List<string>();

            foreach (var kvp in activeFrameEffects)
            {
                var effectId = kvp.Key;
                var effect = kvp.Value;

                effect.frameTimer += Time.deltaTime;

                if (effect.isWaiting)
                {
                    if (effect.frameTimer >= effect.waitTime)
                    {
                        effect.Reset();
                    }
                    continue;
                }

                if (effect.frameTimer >= 0.12f)
                {
                    effect.frameTimer = 0f;

                    if (!effect.HasMoreFrames())
                    {
                        effect.onComplete?.Invoke();

                        if (effect.shouldLoop)
                        {
                            effect.isWaiting = true;
                            effect.frameTimer = 0f;
                        }
                        else
                        {
                            keysToRemove.Add(effectId);
                        }
                        continue;
                    }

                    string currentFrame = effect.GetCurrentFrame();
                    SendCustomChatMessage(currentFrame);
                    effect.MoveToNextFrame();
                }
            }

            foreach (var key in keysToRemove)
            {
                activeFrameEffects.Remove(key);
            }
        }

        public static void SendCustomChatMessage(string message)
        {
            try
            {
                PhotonExtensions.SendLowLevelEvent(43, message);
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("PhotonEvent", $"Failed to send custom message: {ex.Message}", LogLevel.Error);
            }
        }
    }
}