using System;
using System.IO;
using System.Net;

using UnityEngine;
namespace Odium.AwooochysResourceManagement
{
    public static class ClientResourceManager
    {
        // Base directories
        private const string ClientDirectory = "Odium";

        
        private const string ResourceBaseUrl = "https://nigga.rest/where/DownloadableResources/";
        
        // File structure definition
        private static readonly (string FileName, string TargetDirectory)[] RequiredResources = 
        {
            // Root directory files
            ("LoadingBackgrund.png", ClientDirectory)
        };

        public static void EnsureAllResourcesExist()
        {
            EnsureDirectoryStructure();
            
            foreach (var resource in RequiredResources)
            {
                string filePath = Path.Combine(resource.TargetDirectory, resource.FileName);
                if (!File.Exists(filePath))
                {
                    DownloadFile(resource.FileName, resource.TargetDirectory);
                }
            }
        }

        public static bool TryGetResourcePath(string fileName, string subDirectory, out string fullPath)
        {
            string targetDir = string.IsNullOrEmpty(subDirectory) ? 
                ClientDirectory : 
                Path.Combine(ClientDirectory, subDirectory);
            
            fullPath = Path.Combine(targetDir, fileName);
            return File.Exists(fullPath);
        }

        private static void EnsureDirectoryStructure()
        {
            CreateDirectoryIfNotExists(ClientDirectory);
        }

        private static void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                System.Console.WriteLine("ClientResourceManager: " + $"Created directory: {path}");
            }
        }

        private static void DownloadFile(string fileName, string targetDirectory)
        {
            string filePath = Path.Combine(targetDirectory, fileName);
            string downloadUrl = $"{ResourceBaseUrl}{fileName}";
            
            System.Console.WriteLine("ClientResourceManager: " + $"File not found: {filePath}, Downloading...");
            
            try
            {
                byte[] fileData = DownloadFileData(downloadUrl);
                File.WriteAllBytes(filePath, fileData);
                System.Console.WriteLine("ClientResourceManager: " + $"Successfully downloaded: {filePath}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("ClientResourceManager: " + $"Failed to download {fileName} to {targetDirectory}: {ex.Message}");
                
                // For DLLs, the error might be critical
                if (fileName.EndsWith(".dll"))
                {
                    System.Console.WriteLine("ClientResourceManager: " + "CRITICAL: Failed to download a dependency DLL. Some features may not work.");
                }
            }
        }

        private static byte[] DownloadFileData(string url)
        {
            using (WebClient webClient = new WebClient())
            {
                return webClient.DownloadData(url);
            }
        }
    }
}