using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRC.Localization;

namespace VampClient.Api
{
    public class ToastBase
    {
        public static void Toast(string content, string description = null, Sprite icon = null, float duration = 5f)
        {
            LocalizableString localizableString = LocalizableStringExtensions.Localize(content, null, null, null);
            LocalizableString localizableString2 = LocalizableStringExtensions.Localize(description, null, null, null);
            VRCUiManager.field_Private_Static_VRCUiManager_0.field_Private_HudController_0.notification.Method_Public_Void_Sprite_LocalizableString_LocalizableString_Single_Object1PublicTBoTUnique_1_Boolean_0(icon, localizableString, localizableString2, duration, null);
            MelonLogger.Msg(string.Concat(new string[]
            {
                "\n",
                content,
                "\n",
                description,
                "\n\n"
            }));
        }

        public static IEnumerator DelayToast(float delay, string content, string description = null, Sprite icon = null, float duration = 5f)
        {
            float startTime = Time.time;
            while (Time.time < startTime + delay)
            {
                yield return null;
            }
            float time = Time.time;
            LocalizableString param_ = LocalizableStringExtensions.Localize(content, null, null, null);
            LocalizableString param_2 = LocalizableStringExtensions.Localize(description, null, null, null);
            VRCUiManager.field_Private_Static_VRCUiManager_0.field_Private_HudController_0.notification.Method_Public_Void_Sprite_LocalizableString_LocalizableString_Single_Object1PublicTBoTUnique_1_Boolean_0(icon, param_, param_2, duration, null);
            MelonLogger.Msg(string.Concat(new string[]
            {
                "\n",
                content,
                "\n",
                description,
                "\n\n"
            }));
            yield break;
        }
    }
}
