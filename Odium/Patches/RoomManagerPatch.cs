using MelonLoader;
using UnityEngine.SceneManagement;
using VRC.Core;
using Odium.Components;

namespace Odium.Patches
{
    internal class RoomManagerPatch
    {
        public static void Patch()
        {
            EasyPatching.EasyPatchMethodPost(typeof(RoomManager), "Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_String_Int32_0", typeof(RoomManagerPatch), "EnterWorldPatch");
        }
        private static void EnterWorldPatch(ApiWorld __0, ApiWorldInstance __1)
        {
            bool flag = __0 == null || __1 == null;
            if (!flag)
            {
                System.Console.WriteLine("RoomManager: " + $"Joining {RoomManager.field_Internal_Static_ApiWorld_0.name} by {RoomManager.field_Internal_Static_ApiWorld_0.authorName}...");
                MelonCoroutines.Start(Components.OnLoadedSceneManager.WaitForLocalPlayer());
                
                if (__0.tags.Contains("feature_avatar_scaling_disabled"))
                {
                    __0.tags.Remove("feature_avatar_scaling_disabled");
                }
                
            }
        }
    }
}