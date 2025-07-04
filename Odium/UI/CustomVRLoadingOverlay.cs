using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Odium.AwooochysResourceManagement;


namespace Odium.UI
{
    public class CustomVRLoadingOverlay
    {
        public static IEnumerator Init()
        {
            float timeout = 2f;
            float elapsedTime = 0f;

            while (GameObject.Find("TrackingVolume/VRLoadingOverlay/FlatLoadingOverlay(Clone)") == null)
            {
                if (elapsedTime >= timeout)
                {
                    Debug.LogWarning("GameObject not found! Stopping coroutine.");
                    System.Console.WriteLine("Startup: " + "Can't find FlatLoadingOverlay(Clone), User may being in VR.");
                    yield break;
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            GameObject overlay = GameObject.Find("TrackingVolume/VRLoadingOverlay/FlatLoadingOverlay(Clone)/Container/Canvas/Background");
            if (overlay == null)
            {
                System.Console.WriteLine("Startup: " + "Can't find Overlay background.");
                yield break;
            }

            Image overlayImage = overlay.GetComponent<Image>();
            if (overlayImage == null)
            {
                System.Console.WriteLine("Startup: " + "Image component missing on overlay background !");
                yield break;
            }

            string filePath = "DeepCoreV2/LoadingBackgrund.png";
            if (File.Exists(filePath))
            {
                overlayImage.color = Color.white;
                overlayImage.overrideSprite = AwooochysResourceManagement.ReModResourceManager.LoadSpriteFromDisk(filePath);
            }
            else
            {
                System.Console.WriteLine("CustomVRLoadingOverlay: " + "Failed to find: " + filePath);
                System.Console.WriteLine("CustomVRLoadingOverlay: " + "Calling ClientResourceManager");
                try
                {
                    ClientResourceManager.EnsureAllResourcesExist();
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e);
                }
                
            }
        }
    }
}