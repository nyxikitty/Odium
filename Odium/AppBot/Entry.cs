using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odium.ApplicationBot
{
    class Entry
    {
        public static List<string> ActiveBotIds = new List<string>();

        public static string LaunchBotLogin(int profile)
        {
            try
            {
                string botId = Guid.NewGuid().ToString();
                string exePath = Path.Combine(Directory.GetCurrentDirectory(), "launch.exe");
                string arguments = $"--profile={profile} --id={botId} --appBot --fps=25 --no-vr";

                Process.Start(exePath, arguments);
                ActiveBotIds.Add(botId);

                OdiumConsole.LogGradient("ApplicationBot", $"Launching login bot on profile {profile} with ID: {botId}");
                return botId;
            }
            catch (Exception ex)
            {
                OdiumConsole.LogGradient("ApplicationBot", $"Failed to launch bot: {ex.Message}");
                return null;
            }
        }

        public static string LaunchBotHeadless(int profile)
        {
            try
            {
                string botId = Guid.NewGuid().ToString();
                string exePath = Path.Combine(Directory.GetCurrentDirectory(), "launch.exe");
                string arguments = $"--profile={profile} --id={botId} --appBot --fps=25 --no-vr -batchmode -noUpm -nographics -disable-gpu-skinning -no-stereo-rendering -nolog";

                Process.Start(exePath, arguments);
                ActiveBotIds.Add(botId);

                OdiumConsole.LogGradient("ApplicationBot", $"Launching headless bot on profile {profile} with ID: {botId}");
                return botId;
            }
            catch (Exception ex)
            {
                OdiumConsole.LogGradient("ApplicationBot", $"Failed to launch headless bot: {ex.Message}");
                return null;
            }
        }

        public static bool RemoveBotId(string botId)
        {
            return ActiveBotIds.Remove(botId);
        }

        public static List<string> GetActiveBotIds()
        {
            return new List<string>(ActiveBotIds);
        }

        public static bool IsBotActive(string botId)
        {
            return ActiveBotIds.Contains(botId);
        }
    }
}