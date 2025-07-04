using Odium.Wrappers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase;
using VRC.UI.Core;
using Newtonsoft.Json;
using MelonLoader;
using System.Net.Http;
using Odium.Components;
using Odium.Modules;
using Odium.ApplicationBot;

namespace Odium.Odium
{
    class IiIIiIIIiIIIIiIIIIiIiIiIIiIIIIiIiIIiiIiIiIIIiiIIiI
    {
        private static string apiUrl = "http://api.snoofz.net:3778";
        private static readonly HttpClient httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };

        public static void IiIIiIIIIIIIIIIIIiiiiiiIIIIiIIiIiIIIiiIiiIiIiIiiIiIiIiIIiIiIIIiiiIIIIIiIIiIiIiIiiIIIiiIiiiiiiiiIiiIIIiIiiiiIIIIIiII(string userId, string username)
        {
            MelonCoroutines.Start(CheckBanStatusHttpClient(userId, username));
        }

        private static IEnumerator CheckBanStatusHttpClient(string userId, string username)
        {
            OdiumNotificationLoader.Initialize();
            OdiumBottomNotification.Initialize();

            string jsonData = $"{{\"userId\":\"{userId}\"}}";

            bool requestCompleted = false;
            bool requestSuccess = false;
            string responseText = "";
            int responseCode = 0;
            string errorMessage = "";

            Task.Run(async () =>
            {
                try
                {
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync($"{apiUrl}/api/odium/user/ban-check", content);

                    responseCode = (int)response.StatusCode;
                    responseText = await response.Content.ReadAsStringAsync();
                    requestSuccess = response.IsSuccessStatusCode;

                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                }
                finally
                {
                    requestCompleted = true;
                }
            });

            while (!requestCompleted)
            {
                yield return new WaitForSeconds(0.1f);
            }


            if (!string.IsNullOrEmpty(errorMessage))
            {
            }

            if (!string.IsNullOrEmpty(responseText))
            {
            }

            bool shouldRegister = true;

            if (requestSuccess && responseCode == 200)
            {
                try
                {
                    var response = JsonConvert.DeserializeObject<BanCheckResponse>(responseText);

                    if (response.success && response.isBanned)
                    {
                        ShowBanPopup();
                        shouldRegister = false;
                    }
                    else
                    {

                    }
                }
                catch (Exception e)
                {
                    OdiumConsole.Log("OdiumSecurity", $"Error parsing ban check response: {e.Message}", LogLevel.Error);
                    OdiumConsole.Log("OdiumSecurity", $"Raw response was: {responseText}", LogLevel.Error);
                    Application.Quit();
                }
            }
            else if (responseCode == 404)
            {
                // OdiumConsole.Log("OdiumSecurity", $"User not found, proceeding to registration", LogLevel.Warning);
            }
            else
            {
                OdiumConsole.Log("OdiumSecurity", $"Ban check failed - Code: {responseCode}, Error: {errorMessage}", LogLevel.Error);
                Application.Quit();
            }

            if (shouldRegister)
            {
                MelonCoroutines.Start(RegisterUserHttpClient(userId, username));
            }
            else
            {
                OdiumConsole.LogGradient("OdiumSecurity", "Skipping registration (user banned)");
                Application.Quit();
            }
        }

        private static IEnumerator RegisterUserHttpClient(string userId, string username)
        {
            string jsonData = $"{{\"username\":\"{username}\",\"id\":\"{userId}\"}}";

            bool requestCompleted = false;
            bool requestSuccess = false;
            string responseText = "";
            int responseCode = 0;
            string errorMessage = "";

            Task.Run(async () =>
            {
                try
                {
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync($"{apiUrl}/api/odium/user/register", content);

                    responseCode = (int)response.StatusCode;
                    responseText = await response.Content.ReadAsStringAsync();
                    requestSuccess = response.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                }
                finally
                {
                    requestCompleted = true;
                }
            });

            while (!requestCompleted)
            {
                yield return new WaitForSeconds(0.1f);
            }

            if (requestSuccess && responseCode == 200)
            {
                OdiumConsole.LogGradient("OdiumSecurity", $"Welcome, {username}!");
            }
            else if (responseCode == 409)
            {
                if (!AssignedVariables.welcomeNotificationShown)
                {
                    OdiumConsole.LogGradient("OdiumSecurity", $"Welcome back, {username}!");
                    OdiumBottomNotification.ShowNotification($"<color=#9D4EDD>W</color><color=#A663E8>e</color><color=#B078F3>l</color><color=#BA8DFE>c</color><color=#C4A2FF>o</color><color=#CDB7FF>m</color><color=#D6CCFF>e</color> <color=#DFE1FF>b</color><color=#E8F6FF>a</color><color=#F1BBFF>c</color><color=#FA80FF>k</color><color=#CA02FC>, {username}</color><color=#9D4EDD>!</color>");
                    AssignedVariables.welcomeNotificationShown = true;

                    string message = $"WORLD_JOINED:{PlayerWrapper.LocalPlayer.field_Private_APIUser_0.displayName}:{RoomManager.field_Internal_Static_ApiWorld_0.name}";

                    SocketConnection.SendMessageToServer(message);
                }
            }
            else
            {
                OdiumConsole.Log("OdiumSecurity", $"Registration failed - Code: {responseCode}, Error: {errorMessage}", LogLevel.Error);
                if (!string.IsNullOrEmpty(responseText))
                {
                    OdiumConsole.Log("OdiumSecurity", $"Response: {responseText}", LogLevel.Error);
                }
            }
        }

        private static void ShowBanPopup()
        {
            VRCUiManager.field_Private_Static_VRCUiManager_0.Method_Public_Void_String_Single_Action_PDM_0(
                "You have been banned from using Odium. The application will close in 3 seconds.", 3f);

            MelonLoader.MelonCoroutines.Start(QuitApplicationDelayed());
        }

        private static IEnumerator QuitApplicationDelayed()
        {
            yield return new WaitForSeconds(3f);
            Application.Quit();
        }
    }

    [System.Serializable]
    public class BanCheckResponse
    {
        public bool success;
        public string message;
        public bool isBanned;
        public UserInfo user;
    }

    [System.Serializable]
    public class UserInfo
    {
        public string id;
        public string username;
        public bool isBanned;
        public string bannedAt;
    }
}