namespace Odium.Components
{
    using System;
    using System.Net;
    using System.Text;
    using System.Threading;
    using static OdiumConsole;
        
    public class HttpServer
    {
        private readonly HttpListener _listener;
        private readonly int _port;
        private Thread _serverThread;
        private bool _isRunning;
        
        
        public event Action<string> OnCommandReceived;

        public HttpServer(int port = 8080)
        {
            _port = port;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{_port}/");
        }

        public void Start()
        {
            if (_isRunning) return;
            
            _isRunning = true;
            _serverThread = new Thread(Listen);
            _serverThread.IsBackground = true;
            _serverThread.Start();
        }

        public void Stop()
        {
            if (!_isRunning) return;
            
            _isRunning = false;
            _listener.Stop();
            _serverThread?.Join();
            _serverThread = null;
        }

        private void Listen()
        {
            _listener.Start();
            
            while (_isRunning)
            {
                try
                {
                    // Wait for a request
                    var context = _listener.GetContext();
                    ProcessRequest(context);
                }
                catch (HttpListenerException)
                {
                    break;
                }
                catch (Exception e)
                {
                    // Log any other exceptions
                    LogException(e);
                }
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                if (context.Request.HttpMethod == "GET" && context.Request.Url.AbsolutePath == "/command")
                {
                    var query = context.Request.QueryString;
                    var action = query["action"];
                    
                    if (!string.IsNullOrEmpty(action))
                    {
                        OnCommandReceived?.Invoke(action);
                        Log("HTTP SERVER","Command " + action + " received", LogLevel.Info);
                        return;
                    }
                }
                
                context.Response.StatusCode = 404;
                context.Response.Close();
            }
            catch (Exception e)
            {
                context.Response.StatusCode = 500;
                LogException(e);
                context.Response.Close();
            }
        }
    }
}
