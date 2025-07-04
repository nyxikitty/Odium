using System;
using System.Collections.Generic;
using UnityEngine;
using VRC;
using System.Linq;

namespace Odium.Components
{
    public class BoneESP
    {
        private static bool isEnabled = false;
        private static Color boneColor = Color.white;
        private static List<PlayerBoneData> playerBones = new List<PlayerBoneData>();
        private static Material lineMaterial;

        public struct PlayerBoneData
        {
            public Player player;
            public Animator animator;
            public List<BoneConnection> connections;
            public bool isValid;

            public PlayerBoneData(Player plr, Animator anim)
            {
                player = plr;
                animator = anim;
                connections = new List<BoneConnection>();
                isValid = anim != null;
            }
        }

        public struct BoneConnection
        {
            public Transform startBone;
            public Transform endBone;
            public HumanBodyBones startBoneType;
            public HumanBodyBones endBoneType;

            public BoneConnection(Transform start, Transform end, HumanBodyBones startType, HumanBodyBones endType)
            {
                startBone = start;
                endBone = end;
                startBoneType = startType;
                endBoneType = endType;
            }
        }

        private static readonly (HumanBodyBones, HumanBodyBones)[] boneConnections = {
            (HumanBodyBones.Hips, HumanBodyBones.Spine),
            (HumanBodyBones.Spine, HumanBodyBones.Chest),
            (HumanBodyBones.Chest, HumanBodyBones.UpperChest),
            (HumanBodyBones.UpperChest, HumanBodyBones.Neck),
            (HumanBodyBones.Neck, HumanBodyBones.Head),

            (HumanBodyBones.UpperChest, HumanBodyBones.LeftShoulder),
            (HumanBodyBones.LeftShoulder, HumanBodyBones.LeftUpperArm),
            (HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm),
            (HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand),

            (HumanBodyBones.UpperChest, HumanBodyBones.RightShoulder),
            (HumanBodyBones.RightShoulder, HumanBodyBones.RightUpperArm),
            (HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm),
            (HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand),

            (HumanBodyBones.Hips, HumanBodyBones.LeftUpperLeg),
            (HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg),
            (HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot),
            (HumanBodyBones.LeftFoot, HumanBodyBones.LeftToes),

            (HumanBodyBones.Hips, HumanBodyBones.RightUpperLeg),
            (HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg),
            (HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot),
            (HumanBodyBones.RightFoot, HumanBodyBones.RightToes),

            (HumanBodyBones.LeftHand, HumanBodyBones.LeftThumbProximal),
            (HumanBodyBones.LeftThumbProximal, HumanBodyBones.LeftThumbIntermediate),
            (HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.LeftThumbDistal),

            (HumanBodyBones.LeftHand, HumanBodyBones.LeftIndexProximal),
            (HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftIndexIntermediate),
            (HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftIndexDistal),

            (HumanBodyBones.LeftHand, HumanBodyBones.LeftMiddleProximal),
            (HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftMiddleIntermediate),
            (HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.LeftMiddleDistal),

            (HumanBodyBones.RightHand, HumanBodyBones.RightThumbProximal),
            (HumanBodyBones.RightThumbProximal, HumanBodyBones.RightThumbIntermediate),
            (HumanBodyBones.RightThumbIntermediate, HumanBodyBones.RightThumbDistal),

            (HumanBodyBones.RightHand, HumanBodyBones.RightIndexProximal),
            (HumanBodyBones.RightIndexProximal, HumanBodyBones.RightIndexIntermediate),
            (HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexDistal),

            (HumanBodyBones.RightHand, HumanBodyBones.RightMiddleProximal),
            (HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightMiddleIntermediate),
            (HumanBodyBones.RightMiddleIntermediate, HumanBodyBones.RightMiddleDistal)
        };

        public static void Initialize()
        {
            CreateLineMaterial();
            MelonLoader.MelonLogger.Msg("Bone ESP initialized");
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
                playerBones.Clear();
            }
        }

        public static void SetBoneColor(Color color)
        {
            boneColor = color;
        }

        public static void RefreshPlayerList()
        {
            if (!isEnabled) return;

            playerBones.Clear();

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
                        PlayerBoneData boneData = new PlayerBoneData(player, animator);
                        SetupBoneConnections(ref boneData);
                        playerBones.Add(boneData);
                    }
                }

                MelonLoader.MelonLogger.Msg($"Found {playerBones.Count} players with valid bone data");
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error($"Error refreshing player list: {e.Message}");
            }
        }

        private static void SetupBoneConnections(ref PlayerBoneData boneData)
        {
            boneData.connections.Clear();

            foreach (var connection in boneConnections)
            {
                Transform startBone = boneData.animator.GetBoneTransform(connection.Item1);
                Transform endBone = boneData.animator.GetBoneTransform(connection.Item2);

                if (startBone != null && endBone != null)
                {
                    BoneConnection boneConnection = new BoneConnection(startBone, endBone, connection.Item1, connection.Item2);
                    boneData.connections.Add(boneConnection);
                }
            }
        }

        public static void Update()
        {
            if (!isEnabled) return;

            for (int i = playerBones.Count - 1; i >= 0; i--)
            {
                if (playerBones[i].player == null || playerBones[i].animator == null)
                {
                    playerBones.RemoveAt(i);
                }
            }
        }

        public static void OnGUI()
        {
            if (!isEnabled || playerBones.Count == 0) return;

            Camera currentCamera = Camera.current;
            if (currentCamera == null) return;

            try
            {
                GL.PushMatrix();
                lineMaterial.SetPass(0);
                GL.LoadPixelMatrix();
                GL.Begin(1);
                GL.Color(boneColor);

                foreach (var playerBoneData in playerBones)
                {
                    if (!playerBoneData.isValid || playerBoneData.connections == null) continue;

                    foreach (var connection in playerBoneData.connections)
                    {
                        if (connection.startBone == null || connection.endBone == null) continue;

                        Vector3 startPos = currentCamera.WorldToScreenPoint(connection.startBone.position);
                        Vector3 endPos = currentCamera.WorldToScreenPoint(connection.endBone.position);

                        if (startPos.z > 0 && endPos.z > 0)
                        {
                            GL.Vertex3(startPos.x, startPos.y, 0);
                            GL.Vertex3(endPos.x, endPos.y, 0);
                        }
                    }
                }

                GL.End();
                GL.PopMatrix();
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error($"Error drawing bone ESP: {e.Message}");
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
            playerBones.RemoveAll(p => p.player == player);
        }

        public static void Destroy()
        {
            isEnabled = false;
            playerBones.Clear();
            if (lineMaterial != null)
            {
                UnityEngine.Object.DestroyImmediate(lineMaterial);
                lineMaterial = null;
            }
        }

        public static bool IsEnabled => isEnabled;
        public static int PlayerCount => playerBones.Count;
    }
}