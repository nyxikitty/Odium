using System;
using System.Collections.Generic;
using UnityEngine;
using VRC;
using System.Linq;

namespace Odium.Components
{
    public class BoxESP
    {
        private static bool isEnabled = false;
        private static Color boxColor = Color.white;
        private static List<PlayerBoxData> playerBoxes = new List<PlayerBoxData>();
        private static Material lineMaterial;
        private static float boxHeight = 2.0f;
        private static float boxWidth = 0.6f;
        private static float boxDepth = 0.4f;
        private static bool showOnlyVisible = false;
        private static bool showPlayerNames = true;
        private static bool showDistance = true;

        public struct PlayerBoxData
        {
            public Player player;
            public Animator animator;
            public Transform rootBone;
            public Transform headBone;
            public bool isValid;
            public Bounds boundingBox;
            public float distanceToPlayer;
            public string playerName;

            public PlayerBoxData(Player plr, Animator anim)
            {
                player = plr;
                animator = anim;
                rootBone = null;
                headBone = null;
                isValid = anim != null;
                boundingBox = new Bounds();
                distanceToPlayer = 0f;
                playerName = plr?.field_Private_APIUser_0?.displayName ?? "Unknown";

                if (anim != null && anim.isHuman)
                {
                    rootBone = anim.GetBoneTransform(HumanBodyBones.Hips);
                    headBone = anim.GetBoneTransform(HumanBodyBones.Head);
                }
            }
        }

        public static void Initialize()
        {
            CreateLineMaterial();
            MelonLoader.MelonLogger.Msg("Box ESP initialized");
        }

        private static void CreateLineMaterial()
        {
            if (lineMaterial == null)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                lineMaterial = new Material(shader);
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                lineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
                lineMaterial.SetInt("_ZWrite", 0);
            }
        }

        public static void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
            if (enabled)
            {
                RefreshPlayerList();
            }
            else
            {
                playerBoxes.Clear();
            }
        }

        public static void SetBoxColor(Color color)
        {
            boxColor = color;
        }

        public static void SetBoxDimensions(float height, float width, float depth)
        {
            boxHeight = height;
            boxWidth = width;
            boxDepth = depth;
        }

        public static void SetShowOnlyVisible(bool onlyVisible)
        {
            showOnlyVisible = onlyVisible;
        }

        public static void SetShowPlayerNames(bool show)
        {
            showPlayerNames = show;
        }

        public static void SetShowDistance(bool show)
        {
            showDistance = show;
        }

        public static void RefreshPlayerList()
        {
            if (!isEnabled) return;

            playerBoxes.Clear();

            try
            {
                Player[] allPlayers = UnityEngine.Object.FindObjectsOfType<Player>();

                foreach (Player player in allPlayers)
                {
                    if (player == null || player.gameObject == null) continue;

                    VRC.Player localPlayer = VRC.Player.prop_Player_0;
                    if (localPlayer != null && player.gameObject == localPlayer.gameObject) continue;

                    Animator animator = player.GetComponentInChildren<Animator>();
                    if (animator != null && animator.isHuman)
                    {
                        PlayerBoxData boxData = new PlayerBoxData(player, animator);
                        CalculateBoundingBox(ref boxData);
                        playerBoxes.Add(boxData);
                    }
                }

                MelonLoader.MelonLogger.Msg($"Found {playerBoxes.Count} players for box ESP");
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error($"Error refreshing player list: {e.Message}");
            }
        }

        private static void CalculateBoundingBox(ref PlayerBoxData boxData)
        {
            if (boxData.rootBone == null) return;

            Vector3 center = boxData.rootBone.position;
            if (boxData.headBone != null)
            {
                // Center the box between hips and head
                center = Vector3.Lerp(boxData.rootBone.position, boxData.headBone.position, 0.5f);
            }

            Vector3 size = new Vector3(boxWidth, boxHeight, boxDepth);
            boxData.boundingBox = new Bounds(center, size);

            // Calculate distance to local player
            VRC.Player localPlayer = VRC.Player.prop_Player_0;
            if (localPlayer != null)
            {
                boxData.distanceToPlayer = Vector3.Distance(localPlayer.transform.position, center);
            }
        }

        public static void Update()
        {
            if (!isEnabled) return;

            // Update bounding boxes for existing players
            for (int i = playerBoxes.Count - 1; i >= 0; i--)
            {
                if (playerBoxes[i].player == null || playerBoxes[i].animator == null)
                {
                    playerBoxes.RemoveAt(i);
                    continue;
                }

                PlayerBoxData boxData = playerBoxes[i];
                CalculateBoundingBox(ref boxData);
                playerBoxes[i] = boxData;
            }
        }

        public static void OnGUI()
        {
            if (!isEnabled || playerBoxes.Count == 0) return;

            Camera currentCamera = Camera.current;
            if (currentCamera == null) return;

            try
            {
                // Draw wireframe boxes
                GL.PushMatrix();
                lineMaterial.SetPass(0);
                GL.LoadPixelMatrix();
                GL.Begin(1); // GL_LINES = 1
                GL.Color(boxColor);

                foreach (var playerBoxData in playerBoxes)
                {
                    if (!playerBoxData.isValid) continue;

                    DrawBoundingBox(playerBoxData.boundingBox, currentCamera);
                }

                GL.End();
                GL.PopMatrix();

                // Draw text overlays
                if (showPlayerNames || showDistance)
                {
                    DrawTextOverlays(currentCamera);
                }
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error($"Error drawing box ESP: {e.Message}");
            }
        }

        private static void DrawBoundingBox(Bounds bounds, Camera camera)
        {
            Vector3 center = bounds.center;
            Vector3 size = bounds.size;

            // Calculate the 8 corners of the bounding box
            Vector3[] corners = new Vector3[8];
            corners[0] = center + new Vector3(-size.x, -size.y, -size.z) * 0.5f; // Bottom-back-left
            corners[1] = center + new Vector3(size.x, -size.y, -size.z) * 0.5f;  // Bottom-back-right
            corners[2] = center + new Vector3(size.x, -size.y, size.z) * 0.5f;   // Bottom-front-right
            corners[3] = center + new Vector3(-size.x, -size.y, size.z) * 0.5f;  // Bottom-front-left
            corners[4] = center + new Vector3(-size.x, size.y, -size.z) * 0.5f;  // Top-back-left
            corners[5] = center + new Vector3(size.x, size.y, -size.z) * 0.5f;   // Top-back-right
            corners[6] = center + new Vector3(size.x, size.y, size.z) * 0.5f;    // Top-front-right
            corners[7] = center + new Vector3(-size.x, size.y, size.z) * 0.5f;   // Top-front-left

            // Convert to screen space
            Vector3[] screenCorners = new Vector3[8];
            bool allBehindCamera = true;

            for (int i = 0; i < 8; i++)
            {
                screenCorners[i] = camera.WorldToScreenPoint(corners[i]);
                if (screenCorners[i].z > 0)
                {
                    allBehindCamera = false;
                }
            }

            // Don't draw if all corners are behind the camera
            if (allBehindCamera && showOnlyVisible) return;

            // Draw bottom face
            DrawLine(screenCorners[0], screenCorners[1]);
            DrawLine(screenCorners[1], screenCorners[2]);
            DrawLine(screenCorners[2], screenCorners[3]);
            DrawLine(screenCorners[3], screenCorners[0]);

            // Draw top face
            DrawLine(screenCorners[4], screenCorners[5]);
            DrawLine(screenCorners[5], screenCorners[6]);
            DrawLine(screenCorners[6], screenCorners[7]);
            DrawLine(screenCorners[7], screenCorners[4]);

            // Draw vertical edges
            DrawLine(screenCorners[0], screenCorners[4]);
            DrawLine(screenCorners[1], screenCorners[5]);
            DrawLine(screenCorners[2], screenCorners[6]);
            DrawLine(screenCorners[3], screenCorners[7]);
        }

        private static void DrawLine(Vector3 start, Vector3 end)
        {
            // Only draw if both points are in front of camera
            if (start.z > 0 && end.z > 0)
            {
                GL.Vertex3(start.x, start.y, 0);
                GL.Vertex3(end.x, end.y, 0);
            }
        }

        private static void DrawTextOverlays(Camera camera)
        {
            foreach (var playerBoxData in playerBoxes)
            {
                if (!playerBoxData.isValid) continue;

                Vector3 topOfBox = playerBoxData.boundingBox.center + Vector3.up * (playerBoxData.boundingBox.size.y * 0.5f);
                Vector3 screenPos = camera.WorldToScreenPoint(topOfBox);

                if (screenPos.z > 0)
                {
                    // Convert to GUI coordinates
                    screenPos.y = Screen.height - screenPos.y;

                    string text = "";
                    if (showPlayerNames)
                    {
                        text += playerBoxData.playerName;
                    }
                    if (showDistance)
                    {
                        if (text.Length > 0) text += "\n";
                        text += $"{playerBoxData.distanceToPlayer:F1}m";
                    }

                    if (text.Length > 0)
                    {
                        GUI.color = boxColor;
                        GUI.Label(new Rect(screenPos.x - 50, screenPos.y - 30, 100, 50), text);
                        GUI.color = Color.white;
                    }
                }
            }
        }

        public static void OnPlayerJoined(Player player)
        {
            if (!isEnabled) return;
            RefreshPlayerList();
        }

        public static void OnPlayerLeft(Player player)
        {
            if (!isEnabled) return;
            playerBoxes.RemoveAll(p => p.player == player);
        }

        public static void Destroy()
        {
            isEnabled = false;
            playerBoxes.Clear();
            if (lineMaterial != null)
            {
                UnityEngine.Object.DestroyImmediate(lineMaterial);
                lineMaterial = null;
            }
        }

        // Configuration methods
        public static void SetBoxHeight(float height)
        {
            boxHeight = Mathf.Clamp(height, 0.5f, 4.0f);
        }

        public static void SetBoxWidth(float width)
        {
            boxWidth = Mathf.Clamp(width, 0.2f, 2.0f);
        }

        public static void SetBoxDepth(float depth)
        {
            boxDepth = Mathf.Clamp(depth, 0.2f, 2.0f);
        }

        // Properties
        public static bool IsEnabled => isEnabled;
        public static int PlayerCount => playerBoxes.Count;
        public static float BoxHeight => boxHeight;
        public static float BoxWidth => boxWidth;
        public static float BoxDepth => boxDepth;
        public static bool ShowPlayerNames => showPlayerNames;
        public static bool ShowDistance => showDistance;
    }
}