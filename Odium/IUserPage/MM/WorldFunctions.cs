using Odium.ButtonAPI.MM;
using Odium.ButtonAPI.QM;
using Odium.Components;
using Odium.Modules;
using Odium.Threadding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UnityEngine;

namespace Odium.IUserPage.MM
{
    class WorldFunctions
    {
        public static void Initialize()
        {
            Sprite InfoIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\InfoIcon.png");
            Sprite DownloadIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\DownloadIcon.png");

            MMWorldActionRow actionRow = new MMWorldActionRow("Odium Actions");
            new MMWorldButton(actionRow, "Copy World ID", () =>
            {
                OdiumConsole.Log("Odium", $"Attempting to copy world ID...");
                try
                {
                    string worldName = ApiUtils.GetMMWorldName();
                    if (string.IsNullOrEmpty(worldName))
                    {
                        OdiumConsole.Log("Odium", $"No world name found");
                        return;
                    }
                    OdiumConsole.Log("Odium", $"Fetching world ID for: {worldName}");
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        try
                        {
                            var httpClient = new System.Net.Http.HttpClient();
                            string apiUrl = $"http://api.snoofz.net:3778/api/odium/vrc/getWorldByName?worldName={System.Uri.EscapeDataString(worldName)}";
                            var response = httpClient.GetAsync(apiUrl).Result;
                            if (response.IsSuccessStatusCode)
                            {
                                string jsonResponse = response.Content.ReadAsStringAsync().Result;
                                var worldData = Newtonsoft.Json.Linq.JObject.Parse(jsonResponse);
                                string worldID = worldData["id"]?.ToString();
                                if (!string.IsNullOrEmpty(worldID))
                                {
                                    Clipboard.SetText(worldID);
                                    OdiumConsole.Log("Odium", $"Copied world ID to clipboard: {worldID}");
                                    OdiumBottomNotification.ShowNotification("Copied World ID");
                                }
                                else
                                {
                                    OdiumConsole.Log("Odium", $"No world ID found in API response");
                                }
                            }
                            else
                            {
                                OdiumConsole.Log("Odium", $"API request failed with status: {response.StatusCode}");
                            }
                        }
                        catch (System.Exception ex)
                        {
                            OdiumConsole.Log("Odium", $"Error in main thread execution: {ex.Message}");
                        }
                    });
                }
                catch (System.Exception ex)
                {
                    OdiumConsole.Log("Odium", $"Error fetching world ID: {ex.Message}");
                }
            }, InfoIcon);

            new MMWorldButton(actionRow, "Download VRCW", () =>
            {

            }, DownloadIcon);
        }
    }
}
