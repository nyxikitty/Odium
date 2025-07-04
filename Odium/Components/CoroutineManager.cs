using MelonLoader;
using Odium.UI;


namespace Odium.Components
{
    public class CoroutineManager
    {
        public static void Init()
        {
            System.Console.WriteLine("CoroutineManager: " + "Starting Coroutines...");
            MelonCoroutines.Start(CustomVRLoadingOverlay.Init());
        }
    }
}