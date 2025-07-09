using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;
using Odium.Odium;
using Odium.Wrappers;

namespace Odium.Components
{
    class GhostMode
    {
        public static Vector3 originalGhostPosition;
        public static GameObject avatarClone;
        public static List<GameObject> clonedAvatarObjects = new List<GameObject>();
        public static Quaternion originalGhostRotation;

        public static void ToggleGhost(bool enable)
        {
            ActionWrapper.serialize = enable;
            bool flag = enable;
            if (flag)
            {
                GameObject gameObject = null;
                foreach (GameObject gameObject2 in SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    bool flag2 = gameObject2.name.StartsWith("VRCPlayer[Local]");
                    if (flag2)
                    {
                        gameObject = gameObject2;
                        break;
                    }
                }
                bool flag3 = gameObject == null;
                if (!flag3)
                {
                    originalGhostPosition = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position;
                    originalGhostRotation = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.rotation;
                    try
                    {
                        avatarClone = UnityEngine.Object.Instantiate<GameObject>(VRCPlayer.field_Internal_Static_VRCPlayer_0.field_Private_VRCAvatarManager_0.field_Private_GameObject_0, null, true);
                        Animator component = avatarClone.GetComponent<Animator>();
                        avatarClone.transform.position = originalGhostPosition;
                        avatarClone.transform.rotation = originalGhostRotation;
                        bool flag4 = component != null && component.isHuman;
                        if (flag4)
                        {
                            Transform boneTransform = component.GetBoneTransform(HumanBodyBones.Head);
                            bool flag5 = boneTransform != null;
                            if (flag5)
                            {
                                boneTransform.localScale = Vector3.one;
                            }
                        }
                        avatarClone.name = "Cloned Avatar";
                        component.enabled = false;
                        avatarClone.GetComponent<VRCVrIkController>().enabled = false;
                        avatarClone.transform.position = gameObject.transform.position;
                        avatarClone.transform.rotation = gameObject.transform.rotation;
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            else
            {
                GameObject.Destroy(avatarClone);
            }
        }
    }
}
