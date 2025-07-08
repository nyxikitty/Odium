using Odium;
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
using VRC.SDKBase;

namespace Odium.IUserPage.MM
{
    class Functions
    {
        public static void Initialize()
        {
            Sprite InfoIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\InfoIcon.png");
            Sprite JoinMeIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\JoinMeIcon.png");
            Sprite PlusIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\PlusIcon.png");
            Sprite MinusIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\MinusIcon.png");
            Sprite DownloadIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\DownloadIcon.png");

            var odiumUserRow = new MMUserActionRow("Odium Actions");
            var odiumTagsRow = new MMUserActionRow("Odium Tags");

            new MMUserButton(odiumUserRow, "Copy ID", async () =>
            {
                OdiumConsole.Log("Odium", $"Attempting to copy user ID...");
                try
                {
                    string displayName = ApiUtils.GetMMIUser();
                    if (string.IsNullOrEmpty(displayName))
                    {
                        OdiumConsole.Log("Odium", $"No display name found");
                        return;
                    }
                    OdiumConsole.Log("Odium", $"Fetching user ID for: {displayName}");

                    MainThreadDispatcher.Enqueue(() =>
                    {
                        try
                        {
                            var httpClient = new System.Net.Http.HttpClient();
                            string apiUrl = $"http://api.snoofz.net:3778/api/odium/vrc/getByDisplayName?displayName={System.Uri.EscapeDataString(displayName)}";

                            var response = httpClient.GetAsync(apiUrl).Result;
                            if (response.IsSuccessStatusCode)
                            {
                                string jsonResponse = response.Content.ReadAsStringAsync().Result;
                                var userData = Newtonsoft.Json.Linq.JObject.Parse(jsonResponse);
                                string userID = userData["id"]?.ToString();
                                if (!string.IsNullOrEmpty(userID))
                                {
                                    Clipboard.SetText(userID);
                                    OdiumConsole.Log("Odium", $"Copied user ID to clipboard: {userID}");
                                    OdiumBottomNotification.ShowNotification("Copied User ID");
                                }
                                else
                                {
                                    OdiumConsole.Log("Odium", $"No user ID found in API response");
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
                    OdiumConsole.Log("Odium", $"Error fetching user ID: {ex.Message}");
                }
            }, InfoIcon);

            new MMUserButton(odiumUserRow, "Copy Last Platform", async () =>
            {
                OdiumConsole.Log("Odium", $"Attempting to copy Last Platform...");
                try
                {
                    string displayName = ApiUtils.GetMMIUser();
                    if (string.IsNullOrEmpty(displayName))
                    {
                        OdiumConsole.Log("Odium", $"No display name found");
                        return;
                    }
                    OdiumConsole.Log("Odium", $"Fetching user Last Platform for: {displayName}");

                    MainThreadDispatcher.Enqueue(() =>
                    {
                        try
                        {
                            var httpClient = new System.Net.Http.HttpClient();
                            string apiUrl = $"http://api.snoofz.net:3778/api/odium/vrc/getByDisplayName?displayName={System.Uri.EscapeDataString(displayName)}";

                            var response = httpClient.GetAsync(apiUrl).Result;
                            if (response.IsSuccessStatusCode)
                            {
                                string jsonResponse = response.Content.ReadAsStringAsync().Result;
                                var userData = Newtonsoft.Json.Linq.JObject.Parse(jsonResponse);
                                string last_platform = userData["last_platform"]?.ToString();
                                if (!string.IsNullOrEmpty(last_platform))
                                {
                                    Clipboard.SetText(last_platform);
                                    OdiumBottomNotification.ShowNotification($"User was last on -> {last_platform}");
                                }
                                else
                                {
                                    OdiumConsole.Log("Odium", $"No user ID found in API response");
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
                    OdiumConsole.Log("Odium", $"Error fetching user ID: {ex.Message}");
                }
            }, InfoIcon);

            new MMUserButton(odiumUserRow, "Join User", async () =>
            {
                try
                {
                    string displayName = ApiUtils.GetMMIUser();
                    if (string.IsNullOrEmpty(displayName))
                    {
                        OdiumConsole.Log("Odium", $"No display name found");
                        return;
                    }
                    OdiumConsole.Log("Odium", $"Fetching location for -> {displayName}");
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        try
                        {
                            var httpClient = new System.Net.Http.HttpClient();
                            string apiUrl = $"http://api.snoofz.net:3778/api/odium/vrc/getUserLocation?displayName={System.Uri.EscapeDataString(displayName)}";
                            var response = httpClient.GetAsync(apiUrl).Result;
                            if (response.IsSuccessStatusCode)
                            {
                                string jsonResponse = response.Content.ReadAsStringAsync().Result;
                                var userData = Newtonsoft.Json.Linq.JObject.Parse(jsonResponse);
                                OdiumConsole.Log("Odium", $"Joining user at location: {jsonResponse}");

                                string location = userData["location"]?.ToString();
                                if (!string.IsNullOrEmpty(location) && location != "none")
                                {
                                    OdiumConsole.Log("Odium", $"Joining user at location: {location}");
                                    Networking.GoToRoom(location);
                                    OdiumBottomNotification.ShowNotification($"Joining {displayName}");
                                }
                                else
                                {
                                    OdiumConsole.Log("Odium", $"User location is 'none' or empty - cannot join");
                                    OdiumBottomNotification.ShowNotification("User location unavailable");
                                }
                            }
                            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                            {
                                OdiumConsole.Log("Odium", $"User location not tracked");
                                OdiumBottomNotification.ShowNotification("User location not tracked");
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
                    OdiumConsole.Log("Odium", $"Error fetching user location: {ex.Message}");
                }
            }, JoinMeIcon);

            new MMUserButton(odiumTagsRow, "Add Tag", async () =>
            {
                OdiumInputDialog.ShowInputDialog(
                    "Enter tag",
                    (input, wasSubmitted) =>
                    {
                        if (wasSubmitted)
                        {
                            try
                            {
                                string displayName = ApiUtils.GetMMIUser();
                                if (string.IsNullOrEmpty(displayName))
                                {
                                    OdiumConsole.Log("Odium", $"No display name found");
                                    return;
                                }
                                MainThreadDispatcher.Enqueue(() =>
                                {
                                    try
                                    {
                                        var httpClient = new System.Net.Http.HttpClient();
                                        string apiUrl = $"http://api.snoofz.net:3778/api/odium/vrc/getByDisplayName?displayName={System.Uri.EscapeDataString(displayName)}";
                                        var response = httpClient.GetAsync(apiUrl).Result;
                                        if (response.IsSuccessStatusCode)
                                        {
                                            string jsonResponse = response.Content.ReadAsStringAsync().Result;
                                            var userData = Newtonsoft.Json.Linq.JObject.Parse(jsonResponse);
                                            string userID = userData["id"]?.ToString();
                                            if (!string.IsNullOrEmpty(userID))
                                            {
                                                var tagData = new
                                                {
                                                    userId = userID,
                                                    tag = input
                                                };

                                                string tagJson = Newtonsoft.Json.JsonConvert.SerializeObject(tagData);
                                                var content = new System.Net.Http.StringContent(tagJson, System.Text.Encoding.UTF8, "application/json");

                                                string addTagUrl = "https://snoofz.net/api/odium/tags/add";
                                                var tagResponse = httpClient.PostAsync(addTagUrl, content).Result;

                                                if (tagResponse.IsSuccessStatusCode)
                                                {
                                                    string tagResponseJson = tagResponse.Content.ReadAsStringAsync().Result;
                                                    var tagResult = Newtonsoft.Json.Linq.JObject.Parse(tagResponseJson);

                                                    OdiumInputDialog.CloseAllInputDialogs();
                                                    OdiumBottomNotification.ShowNotification($"Tag '{input}' added to {displayName}");
                                                    OdiumConsole.Log("Odium", $"Tag '{input}' successfully added to user {userID}");
                                                }
                                                else
                                                {
                                                    string errorResponse = tagResponse.Content.ReadAsStringAsync().Result;
                                                    var errorResult = Newtonsoft.Json.Linq.JObject.Parse(errorResponse);
                                                    string errorMessage = errorResult["message"]?.ToString() ?? "Failed to add tag";

                                                    OdiumInputDialog.CloseAllInputDialogs();
                                                    OdiumBottomNotification.ShowNotification($"Error: {errorMessage}");
                                                    OdiumConsole.Log("Odium", $"Failed to add tag: {errorMessage}");
                                                }
                                            }
                                            else
                                            {
                                                OdiumInputDialog.CloseAllInputDialogs();
                                                OdiumBottomNotification.ShowNotification("Error: Could not find user ID");
                                                OdiumConsole.Log("Odium", "User ID not found in response");
                                            }
                                        }
                                        else
                                        {
                                            OdiumInputDialog.CloseAllInputDialogs();
                                            OdiumBottomNotification.ShowNotification("Error: Could not find user");
                                            OdiumConsole.Log("Odium", $"Failed to get user data: {response.StatusCode}");
                                        }
                                    }
                                    catch (System.Exception ex)
                                    {
                                        OdiumInputDialog.CloseAllInputDialogs();
                                        OdiumBottomNotification.ShowNotification($"Error: {ex.Message}");
                                        OdiumConsole.Log("Odium", $"Exception in tag operation: {ex.Message}");
                                    }
                                });
                            }
                            catch (System.Exception ex)
                            {
                                OdiumConsole.Log("Odium", $"Error fetching user ID: {ex.Message}");
                                OdiumInputDialog.CloseAllInputDialogs();
                                OdiumBottomNotification.ShowNotification($"Error: {ex.Message}");
                            }
                        }
                        else
                        {
                            OdiumConsole.Log("Input", "User cancelled input", LogLevel.Info);
                            OdiumInputDialog.CloseAllInputDialogs();
                        }
                    },
                    defaultValue: "Enter tag",
                    placeholder: "Enter tag"
                );
            }, PlusIcon);

            new MMUserButton(odiumTagsRow, "Remove Tag", async () =>
            {
                OdiumInputDialog.ShowInputDialog(
                    "Enter tag",
                    (input, wasSubmitted) =>
                    {
                        if (wasSubmitted)
                        {
                            try
                            {
                                string displayName = ApiUtils.GetMMIUser();
                                if (string.IsNullOrEmpty(displayName))
                                {
                                    OdiumConsole.Log("Odium", $"No display name found");
                                    return;
                                }
                                MainThreadDispatcher.Enqueue(() =>
                                {
                                    try
                                    {
                                        var httpClient = new System.Net.Http.HttpClient();
                                        string apiUrl = $"http://api.snoofz.net:3778/api/odium/vrc/getByDisplayName?displayName={System.Uri.EscapeDataString(displayName)}";
                                        var response = httpClient.GetAsync(apiUrl).Result;
                                        if (response.IsSuccessStatusCode)
                                        {
                                            string jsonResponse = response.Content.ReadAsStringAsync().Result;
                                            var userData = Newtonsoft.Json.Linq.JObject.Parse(jsonResponse);
                                            string userID = userData["id"]?.ToString();
                                            if (!string.IsNullOrEmpty(userID))
                                            {
                                                var tagData = new
                                                {
                                                    userId = userID,
                                                    tag = input
                                                };

                                                string tagJson = Newtonsoft.Json.JsonConvert.SerializeObject(tagData);
                                                var content = new System.Net.Http.StringContent(tagJson, System.Text.Encoding.UTF8, "application/json");

                                                string addTagUrl = "https://snoofz.net/api/odium/tags/remove";
                                                var tagResponse = httpClient.PostAsync(addTagUrl, content).Result;

                                                if (tagResponse.IsSuccessStatusCode)
                                                {
                                                    string tagResponseJson = tagResponse.Content.ReadAsStringAsync().Result;
                                                    var tagResult = Newtonsoft.Json.Linq.JObject.Parse(tagResponseJson);

                                                    OdiumInputDialog.CloseAllInputDialogs();
                                                    OdiumBottomNotification.ShowNotification($"Tag '{input}' removed from {displayName}");
                                                    OdiumConsole.Log("Odium", $"Tag '{input}' successfully removed from user {userID}");
                                                }
                                                else
                                                {
                                                    string errorResponse = tagResponse.Content.ReadAsStringAsync().Result;
                                                    var errorResult = Newtonsoft.Json.Linq.JObject.Parse(errorResponse);
                                                    string errorMessage = errorResult["message"]?.ToString() ?? "Failed to removed tag";

                                                    OdiumInputDialog.CloseAllInputDialogs();
                                                    OdiumBottomNotification.ShowNotification($"Error: {errorMessage}");
                                                    OdiumConsole.Log("Odium", $"Failed to removed tag: {errorMessage}");
                                                }
                                            }
                                            else
                                            {
                                                OdiumInputDialog.CloseAllInputDialogs();
                                                OdiumBottomNotification.ShowNotification("Error: Could not find user ID");
                                                OdiumConsole.Log("Odium", "User ID not found in response");
                                            }
                                        }
                                        else
                                        {
                                            OdiumInputDialog.CloseAllInputDialogs();
                                            OdiumBottomNotification.ShowNotification("Error: Could not find user");
                                            OdiumConsole.Log("Odium", $"Failed to get user data: {response.StatusCode}");
                                        }
                                    }
                                    catch (System.Exception ex)
                                    {
                                        OdiumInputDialog.CloseAllInputDialogs();
                                        OdiumBottomNotification.ShowNotification($"Error: {ex.Message}");
                                        OdiumConsole.Log("Odium", $"Exception in tag operation: {ex.Message}");
                                    }
                                });
                            }
                            catch (System.Exception ex)
                            {
                                OdiumConsole.Log("Odium", $"Error fetching user ID: {ex.Message}");
                                OdiumInputDialog.CloseAllInputDialogs();
                                OdiumBottomNotification.ShowNotification($"Error: {ex.Message}");
                            }
                        }
                        else
                        {
                            OdiumConsole.Log("Input", "User cancelled input", LogLevel.Info);
                            OdiumInputDialog.CloseAllInputDialogs();
                        }
                    },
                    defaultValue: "Enter tag",
                    placeholder: "Enter tag"
                );
            }, MinusIcon);

            new MMUserButton(odiumUserRow, "Download VRCA", async () =>
            {
            }, DownloadIcon);
        }
    }
}