using Il2CppSystem;
using Odium;
using static Odium.OdiumConsole;
using static Odium.Components.HttpServer;
using static Odium.Components.LoudMic;
using Odium.Components;

public class ExternalMenu
{
    
    private HttpServer _httpServer;
    public void StartServer()
    {
        Console.WriteLine("External Menu: HTTP Server is ON");
        // Create and start the HTTP server
        _httpServer = new HttpServer();
        _httpServer.OnCommandReceived += HandleCommand;
        _httpServer.Start();

        Log("HTTP SERVER","HTTP server started on port 8080", LogLevel.Info);
    }
    
    //Remote functions that can be called externally (via http post)
    private void HandleCommand(string command)
    {
        Log("HTTP SERVER","Recived command: " + command, LogLevel.Info);
        
        switch (command)
        {
            case "LoudMicON":
                Log("HTTP SERVER", "Loud Mic Enabled", LogLevel.Info);
                Activated(true);
                break;
            
            
            case "LoudMicOFF":
                Log("HTTP SERVER", "Loud Mic Disabled", LogLevel.Info);
                Activated(false);
                break;
            
            
            case "ping" :
                Log("HTTP SERVER", "Pong", LogLevel.Info);
                Console.WriteLine("HTTP SERVER: Pong");
                break;
        }
    }
}