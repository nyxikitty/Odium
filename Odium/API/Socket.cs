using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Enterprise WebSocket Client for real-time communication
/// Compatible with Enterprise WebSocket Server v2.0.0
/// 
/// Features:
/// - Automatic reconnection with exponential backoff
/// - Room management with auto-rejoin
/// - Audio streaming (uspeak)
/// - Movement synchronization
/// - Portal data transmission
/// - Health monitoring and metrics
/// - Comprehensive error handling
/// </summary>
public class EnterpriseWebSocketClient : IDisposable
{
    private ClientWebSocket _webSocket;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly string _serverUrl;
    private bool _isConnected;
    private bool _shouldReconnect = true;
    private bool _isReconnecting;

    // Auto-reconnect configuration
    public int MaxReconnectAttempts { get; set; } = 10; // -1 for infinite
    public TimeSpan InitialReconnectDelay { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan MaxReconnectDelay { get; set; } = TimeSpan.FromSeconds(30);
    public bool UseExponentialBackoff { get; set; } = true;

    private int _reconnectAttempts;
    private string _lastRoomId; // Store for auto-rejoin

    // Client information
    public string ClientId { get; private set; }
    public string ServerVersion { get; private set; }

    // Connection lifecycle events
    public event Action<string, string, string> OnConnected; // message, clientId, serverVersion
    public event Action<string> OnJoined;
    public event Action<string> OnLeft;
    public event Action<string> OnUserJoined;
    public event Action<string> OnUserLeft;
    public event Action OnDisconnected;
    public event Action<int> OnReconnecting; // attempt number
    public event Action OnReconnected;
    public event Action OnReconnectFailed;

    // Communication events
    public event Action<string, object, DateTime> OnUSpeakReceived; // base64Data, sender, timestamp
    public event Action<string, object, DateTime> OnMovementReceived; // base64Data, sender, timestamp
    public event Action<string, object, DateTime> OnPortalDropped; // serializedData, sender, timestamp

    // System events
    public event Action<string, string> OnError; // message, errorCode
    public event Action<string> OnRateLimited; // message with retryAfter
    public event Action<int, int, string> OnActiveCountReceived; // activeConnections, activeRooms, timestamp
    public event Action<int, int, string, object> OnServerInfoReceived; // activeConnections, activeRooms, timestamp, metrics
    public event Action<long> OnPongReceived; // roundTripTimeMs

    // NEW: Admin announcement event
    public event Action<string, string, string, DateTime, string, DateTime?> OnAnnouncementReceived; // message, priority, category, timestamp, broadcastId, expiresAt

    // Properties
    public bool IsConnected => _isConnected && _webSocket?.State == WebSocketState.Open;
    public string CurrentRoom { get; private set; }
    public bool IsReconnecting => _isReconnecting;
    public int ReconnectAttempts => _reconnectAttempts;

    public EnterpriseWebSocketClient(string serverUrl = "ws://localhost:3000")
    {
        _serverUrl = serverUrl;
        _webSocket = new ClientWebSocket();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Connect to the WebSocket server
    /// </summary>
    public async Task<bool> ConnectAsync()
    {
        _shouldReconnect = true;
        return await InternalConnectAsync();
    }

    private async Task<bool> InternalConnectAsync()
    {
        try
        {
            if (_webSocket.State != WebSocketState.None)
            {
                _webSocket?.Dispose();
                _webSocket = new ClientWebSocket();
            }

            await _webSocket.ConnectAsync(new Uri(_serverUrl), _cancellationTokenSource.Token);
            _isConnected = true;
            _reconnectAttempts = 0;

            // Start listening for messages
            _ = Task.Run(ListenForMessages, _cancellationTokenSource.Token);

            // Auto-rejoin room if we were in one
            if (!string.IsNullOrEmpty(_lastRoomId))
            {
                await JoinRoomAsync(_lastRoomId);
            }

            return true;
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Connection failed: {ex.Message}", "CONNECTION_FAILED");
            return false;
        }
    }

    /// <summary>
    /// Disconnect from the WebSocket server
    /// </summary>
    public async Task DisconnectAsync()
    {
        try
        {
            _shouldReconnect = false; // Disable auto-reconnect for manual disconnect
            _isConnected = false;
            _cancellationTokenSource?.Cancel();

            if (_webSocket?.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnect", CancellationToken.None);
            }

            OnDisconnected?.Invoke();
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Disconnect error: {ex.Message}", "DISCONNECT_ERROR");
        }
    }

    /// <summary>
    /// Stop automatic reconnection attempts
    /// </summary>
    public void StopReconnecting()
    {
        _shouldReconnect = false;
    }

    /// <summary>
    /// Enable automatic reconnection
    /// </summary>
    public void StartReconnecting()
    {
        _shouldReconnect = true;
    }

    /// <summary>
    /// Join a room on the server
    /// </summary>
    public async Task<bool> JoinRoomAsync(string roomId)
    {
        if (string.IsNullOrWhiteSpace(roomId))
        {
            OnError?.Invoke("Room ID cannot be empty", "INVALID_ROOM_ID");
            return false;
        }

        var json = SimpleJson.CreateObject()
            .Add("type", "join")
            .Add("roomId", roomId)
            .ToString();

        var result = await SendRawMessageAsync(json);
        if (result)
        {
            _lastRoomId = roomId; // Store for auto-rejoin
        }
        return result;
    }

    /// <summary>
    /// Leave the current room
    /// </summary>
    public async Task<bool> LeaveRoomAsync()
    {
        var json = SimpleJson.CreateObject()
            .Add("type", "leave")
            .ToString();

        var result = await SendRawMessageAsync(json);
        if (result)
        {
            CurrentRoom = null;
            _lastRoomId = null; // Clear stored room
        }
        return result;
    }

    /// <summary>
    /// Leave current room and join a new one atomically
    /// </summary>
    public async Task<bool> LeaveAndJoinRoomAsync(string newRoomId)
    {
        if (string.IsNullOrWhiteSpace(newRoomId))
        {
            OnError?.Invoke("Room ID cannot be empty", "INVALID_ROOM_ID");
            return false;
        }

        var json = SimpleJson.CreateObject()
            .Add("type", "leaveAndJoinRoom")
            .Add("roomId", newRoomId)
            .ToString();

        var result = await SendRawMessageAsync(json);
        if (result)
        {
            _lastRoomId = newRoomId; // Store for auto-rejoin
        }
        return result;
    }

    /// <summary>
    /// Send audio data to other clients in the room
    /// </summary>
    public async Task<bool> SendUSpeakAsync(string base64AudioData, object sender)
    {
        if (string.IsNullOrEmpty(base64AudioData))
        {
            OnError?.Invoke("Audio data cannot be empty", "INVALID_DATA");
            return false;
        }

        var json = SimpleJson.CreateObject()
            .Add("type", "uspeak")
            .Add("data", base64AudioData)
            .Add("sender", sender?.ToString() ?? "")
            .ToString();
        return await SendRawMessageAsync(json);
    }

    /// <summary>
    /// Send audio data to other clients in the room
    /// </summary>
    public async Task<bool> SendUSpeakAsync(byte[] audioData, object sender)
    {
        if (audioData == null || audioData.Length == 0)
        {
            OnError?.Invoke("Audio data cannot be empty", "INVALID_DATA");
            return false;
        }

        string base64Data = Convert.ToBase64String(audioData);
        return await SendUSpeakAsync(base64Data, sender);
    }

    /// <summary>
    /// Send movement data to other clients in the room
    /// </summary>
    public async Task<bool> SendMovementAsync(string base64MovementData, object sender)
    {
        if (string.IsNullOrEmpty(base64MovementData))
        {
            OnError?.Invoke("Movement data cannot be empty", "INVALID_DATA");
            return false;
        }

        var json = SimpleJson.CreateObject()
            .Add("type", "movement")
            .Add("data", base64MovementData)
            .Add("sender", sender?.ToString() ?? "")
            .ToString();
        return await SendRawMessageAsync(json);
    }

    /// <summary>
    /// Send movement data to other clients in the room
    /// </summary>
    public async Task<bool> SendMovementAsync(byte[] movementData, object sender)
    {
        if (movementData == null || movementData.Length == 0)
        {
            OnError?.Invoke("Movement data cannot be empty", "INVALID_DATA");
            return false;
        }

        string base64Data = Convert.ToBase64String(movementData);
        return await SendMovementAsync(base64Data, sender);
    }

    /// <summary>
    /// Drop a portal with serialized data
    /// </summary>
    public async Task<bool> DropPortalAsync(string serializedPortalData, object sender)
    {
        if (string.IsNullOrEmpty(serializedPortalData))
        {
            OnError?.Invoke("Portal data cannot be empty", "INVALID_DATA");
            return false;
        }

        // Validate data size (1MB limit as per server)
        if (serializedPortalData.Length > 1024 * 1024)
        {
            OnError?.Invoke("Portal data too large (max 1MB)", "DATA_TOO_LARGE");
            return false;
        }

        var json = SimpleJson.CreateObject()
            .Add("type", "drop_portal")
            .Add("data", serializedPortalData)
            .Add("sender", sender?.ToString() ?? "")
            .ToString();
        return await SendRawMessageAsync(json);
    }

    /// <summary>
    /// Drop a portal with binary data
    /// </summary>
    public async Task<bool> DropPortalAsync(byte[] portalData, object sender)
    {
        if (portalData == null || portalData.Length == 0)
        {
            OnError?.Invoke("Portal data cannot be empty", "INVALID_DATA");
            return false;
        }

        string serializedData = Convert.ToBase64String(portalData);
        return await DropPortalAsync(serializedData, sender);
    }

    /// <summary>
    /// Request active connection count from server
    /// </summary>
    public async Task<bool> RequestActiveCountAsync()
    {
        var json = SimpleJson.CreateObject()
            .Add("type", "get_active")
            .ToString();
        return await SendRawMessageAsync(json);
    }

    /// <summary>
    /// Send ping to measure round-trip time
    /// </summary>
    public async Task<bool> SendPingAsync()
    {
        var json = SimpleJson.CreateObject()
            .Add("type", "ping")
            .Add("timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            .ToString();
        return await SendRawMessageAsync(json);
    }

    private async Task<bool> SendRawMessageAsync(string json)
    {
        try
        {
            if (!IsConnected)
            {
                OnError?.Invoke("Cannot send message: Not connected", "NOT_CONNECTED");
                return false;
            }

            byte[] buffer = Encoding.UTF8.GetBytes(json);

            await _webSocket.SendAsync(
                new ArraySegment<byte>(buffer),
                WebSocketMessageType.Text,
                true,
                _cancellationTokenSource.Token
            );

            return true;
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Send error: {ex.Message}", "SEND_ERROR");
            return false;
        }
    }

    private async Task ListenForMessages()
    {
        var buffer = new byte[8192];

        try
        {
            while (_isConnected && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (_webSocket.State != WebSocketState.Open)
                {
                    break;
                }

                var result = await _webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    _cancellationTokenSource.Token
                );

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await ProcessReceivedMessage(message);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await HandleDisconnection();
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Listen error: {ex.Message}", "LISTEN_ERROR");
            await HandleDisconnection();
        }
    }

    private async Task HandleDisconnection()
    {
        _isConnected = false;
        OnDisconnected?.Invoke();

        if (_shouldReconnect && !_cancellationTokenSource.Token.IsCancellationRequested)
        {
            await AttemptReconnect();
        }
    }

    private async Task AttemptReconnect()
    {
        if (_isReconnecting)
            return;

        _isReconnecting = true;

        while (_shouldReconnect && !_cancellationTokenSource.Token.IsCancellationRequested)
        {
            if (MaxReconnectAttempts > 0 && _reconnectAttempts >= MaxReconnectAttempts)
            {
                OnReconnectFailed?.Invoke();
                break;
            }

            _reconnectAttempts++;
            OnReconnecting?.Invoke(_reconnectAttempts);

            try
            {
                var delay = CalculateReconnectDelay();
                await Task.Delay(delay, _cancellationTokenSource.Token);

                if (await InternalConnectAsync())
                {
                    _isReconnecting = false;
                    OnReconnected?.Invoke();
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Reconnect attempt {_reconnectAttempts} failed: {ex.Message}", "RECONNECT_FAILED");
            }
        }

        _isReconnecting = false;
    }

    private TimeSpan CalculateReconnectDelay()
    {
        if (!UseExponentialBackoff)
            return InitialReconnectDelay;

        // Exponential backoff: delay = initial * 2^(attempts-1)
        var delay = TimeSpan.FromMilliseconds(
            InitialReconnectDelay.TotalMilliseconds * Math.Pow(2, _reconnectAttempts - 1)
        );

        // Cap at maximum delay
        return delay > MaxReconnectDelay ? MaxReconnectDelay : delay;
    }

    private async Task ProcessReceivedMessage(string messageJson)
    {
        try
        {
            var parsed = SimpleJson.Parse(messageJson);

            if (!parsed.HasProperty("type"))
            {
                return;
            }

            string type = parsed.GetString("type");
            string message = parsed.GetString("message", "");

            switch (type)
            {
                case "connected":
                    ClientId = parsed.GetString("clientId", "");
                    ServerVersion = parsed.GetString("serverVersion", "");
                    OnConnected?.Invoke(message, ClientId, ServerVersion);
                    break;

                case "joined":
                    string roomId = parsed.GetString("roomId", "");
                    if (!string.IsNullOrEmpty(roomId))
                    {
                        CurrentRoom = roomId;
                    }
                    OnJoined?.Invoke(message);
                    break;

                case "left":
                    CurrentRoom = null;
                    OnLeft?.Invoke(message);
                    break;

                case "user_joined":
                    OnUserJoined?.Invoke(message);
                    break;

                case "user_left":
                    OnUserLeft?.Invoke(message);
                    break;

                case "uspeak":
                    string uspeakData = parsed.GetString("data", "");
                    object uspeakSender = ParseSender(parsed);
                    DateTime uspeakTimestamp = ParseTimestamp(parsed);
                    if (!string.IsNullOrEmpty(uspeakData))
                    {
                        OnUSpeakReceived?.Invoke(uspeakData, uspeakSender, uspeakTimestamp);
                    }
                    break;

                case "movement":
                    string movementData = parsed.GetString("data", "");
                    object movementSender = ParseSender(parsed);
                    DateTime movementTimestamp = ParseTimestamp(parsed);
                    if (!string.IsNullOrEmpty(movementData))
                    {
                        OnMovementReceived?.Invoke(movementData, movementSender, movementTimestamp);
                    }
                    break;

                case "drop_portal":
                    string portalData = parsed.GetString("serialized_data", "");
                    object portalSender = ParseSender(parsed);
                    DateTime portalTimestamp = ParseTimestamp(parsed);
                    if (!string.IsNullOrEmpty(portalData))
                    {
                        OnPortalDropped?.Invoke(portalData, portalSender, portalTimestamp);
                    }
                    break;

                case "active_count":
                    int activeConnections = parsed.GetInt("active_connections", 0);
                    int activeRooms = parsed.GetInt("active_rooms", 0);
                    string timestamp = parsed.GetString("timestamp", "");
                    OnActiveCountReceived?.Invoke(activeConnections, activeRooms, timestamp);
                    break;

                case "server_info":
                    int serverConnections = parsed.GetInt("active_connections", 0);
                    int serverRooms = parsed.GetInt("active_rooms", 0);
                    string serverTimestamp = parsed.GetString("timestamp", "");
                    object metrics = parsed.GetRaw("metrics"); // Get raw metrics object
                    OnServerInfoReceived?.Invoke(serverConnections, serverRooms, serverTimestamp, metrics);
                    break;

                case "pong":
                    long clientTimestamp = parsed.GetLong("client_timestamp", 0);
                    long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    long roundTripTime = currentTime - clientTimestamp;
                    OnPongReceived?.Invoke(roundTripTime);
                    break;

                case "error":
                    string errorCode = parsed.GetString("code", "UNKNOWN_ERROR");
                    OnError?.Invoke(message, errorCode);
                    break;

                case "rate_limit":
                    string rateLimitCode = parsed.GetString("code", "RATE_LIMITED");
                    int retryAfter = parsed.GetInt("retryAfter", 60);
                    OnRateLimited?.Invoke($"{message} (retry after {retryAfter}s)");
                    break;

                default:
                    OnError?.Invoke($"Unknown message type: {type}", "UNKNOWN_MESSAGE_TYPE");
                    break;
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Message processing error: {ex.Message}", "MESSAGE_PROCESSING_ERROR");
        }
    }

    private object ParseSender(SimpleJson parsed)
    {
        // Try to get sender as string first, then as number for backward compatibility
        string senderStr = parsed.GetString("sender", "");
        if (!string.IsNullOrEmpty(senderStr))
        {
            // Try to parse as number for backward compatibility
            if (int.TryParse(senderStr, out int senderInt))
                return senderInt;
            return senderStr;
        }

        // Try as number directly
        int senderNumber = parsed.GetInt("sender", -1);
        return senderNumber != -1 ? (object)senderNumber : null;
    }

    private DateTime ParseTimestamp(SimpleJson parsed)
    {
        long timestamp = parsed.GetLong("timestamp", 0);
        if (timestamp > 0)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
        }
        return DateTime.UtcNow;
    }

    public void Dispose()
    {
        try
        {
            _shouldReconnect = false;
            _cancellationTokenSource?.Cancel();
            _webSocket?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Dispose error: {ex.Message}", "DISPOSE_ERROR");
        }
    }
}

// Enhanced lightweight JSON parser/builder
public class SimpleJson
{
    private readonly Dictionary<string, object> _data;

    private SimpleJson()
    {
        _data = new Dictionary<string, object>();
    }

    public static SimpleJson CreateObject()
    {
        return new SimpleJson();
    }

    public SimpleJson Add(string key, string value)
    {
        _data[key] = value;
        return this;
    }

    public SimpleJson Add(string key, int value)
    {
        _data[key] = value;
        return this;
    }

    public SimpleJson Add(string key, long value)
    {
        _data[key] = value;
        return this;
    }

    public SimpleJson Add(string key, bool value)
    {
        _data[key] = value;
        return this;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("{");

        bool first = true;
        foreach (var kvp in _data)
        {
            if (!first) sb.Append(",");
            first = false;

            sb.Append("\"").Append(EscapeString(kvp.Key)).Append("\":");

            if (kvp.Value is string str)
            {
                sb.Append("\"").Append(EscapeString(str)).Append("\"");
            }
            else if (kvp.Value is bool b)
            {
                sb.Append(b ? "true" : "false");
            }
            else if (kvp.Value is int i)
            {
                sb.Append(i.ToString(CultureInfo.InvariantCulture));
            }
            else if (kvp.Value is long l)
            {
                sb.Append(l.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                sb.Append("null");
            }
        }

        sb.Append("}");
        return sb.ToString();
    }

    public static SimpleJson Parse(string json)
    {
        var parser = new SimpleJsonParser(json);
        return parser.ParseObject();
    }

    public bool HasProperty(string key)
    {
        return _data.ContainsKey(key);
    }

    public string GetString(string key, string defaultValue = "")
    {
        if (_data.TryGetValue(key, out var value) && value is string str)
        {
            return str;
        }
        return defaultValue;
    }

    public int GetInt(string key, int defaultValue = 0)
    {
        if (_data.TryGetValue(key, out var value) && value is int i)
        {
            return i;
        }
        return defaultValue;
    }

    public long GetLong(string key, long defaultValue = 0)
    {
        if (_data.TryGetValue(key, out var value))
        {
            if (value is long l)
                return l;
            if (value is int i)
                return i; // Auto-convert int to long
        }
        return defaultValue;
    }

    public bool GetBool(string key, bool defaultValue = false)
    {
        if (_data.TryGetValue(key, out var value) && value is bool b)
        {
            return b;
        }
        return defaultValue;
    }

    public object GetRaw(string key)
    {
        _data.TryGetValue(key, out var value);
        return value;
    }

    private static string EscapeString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var sb = new StringBuilder();
        foreach (char c in input)
        {
            switch (c)
            {
                case '"':
                    sb.Append("\\\"");
                    break;
                case '\\':
                    sb.Append("\\\\");
                    break;
                case '\n':
                    sb.Append("\\n");
                    break;
                case '\r':
                    sb.Append("\\r");
                    break;
                case '\t':
                    sb.Append("\\t");
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }
        return sb.ToString();
    }

    internal void SetValue(string key, object value)
    {
        _data[key] = value;
    }
}

// Simple JSON parser for incoming messages (same as before but with GetRaw support)
internal class SimpleJsonParser
{
    private readonly string _json;
    private int _position;

    public SimpleJsonParser(string json)
    {
        _json = json ?? throw new ArgumentNullException(nameof(json));
        _position = 0;
    }

    public SimpleJson ParseObject()
    {
        var result = SimpleJson.CreateObject();

        SkipWhitespace();
        if (!ConsumeChar('{'))
            throw new FormatException("Expected '{'");

        SkipWhitespace();
        if (PeekChar() == '}')
        {
            ConsumeChar('}');
            return result;
        }

        while (true)
        {
            SkipWhitespace();

            string key = ParseString();

            SkipWhitespace();
            if (!ConsumeChar(':'))
                throw new FormatException("Expected ':'");

            SkipWhitespace();
            object value = ParseValue();

            result.SetValue(key, value);

            SkipWhitespace();
            if (ConsumeChar('}'))
                break;

            if (!ConsumeChar(','))
                throw new FormatException("Expected ',' or '}'");
        }

        return result;
    }

    private object ParseValue()
    {
        SkipWhitespace();
        char c = PeekChar();

        switch (c)
        {
            case '"':
                return ParseString();
            case 't':
            case 'f':
                return ParseBoolean();
            case 'n':
                return ParseNull();
            case '{':
                return ParseObject();
            case '[':
                return ParseArray();
            default:
                if (char.IsDigit(c) || c == '-')
                    return ParseNumber();
                break;
        }

        throw new FormatException($"Unexpected character: {c}");
    }

    private object ParseArray()
    {
        var result = new List<object>();

        if (!ConsumeChar('['))
            throw new FormatException("Expected '['");

        SkipWhitespace();
        if (ConsumeChar(']'))
            return result;

        while (true)
        {
            SkipWhitespace();
            result.Add(ParseValue());

            SkipWhitespace();
            if (ConsumeChar(']'))
                break;

            if (!ConsumeChar(','))
                throw new FormatException("Expected ',' or ']'");
        }

        return result;
    }

    private string ParseString()
    {
        if (!ConsumeChar('"'))
            throw new FormatException("Expected '\"'");

        var sb = new StringBuilder();
        while (_position < _json.Length)
        {
            char c = _json[_position++];

            if (c == '"')
                return sb.ToString();

            if (c == '\\' && _position < _json.Length)
            {
                char next = _json[_position++];
                switch (next)
                {
                    case '"':
                        sb.Append('"');
                        break;
                    case '\\':
                        sb.Append('\\');
                        break;
                    case 'n':
                        sb.Append('\n');
                        break;
                    case 'r':
                        sb.Append('\r');
                        break;
                    case 't':
                        sb.Append('\t');
                        break;
                    default:
                        sb.Append(next);
                        break;
                }
            }
            else
            {
                sb.Append(c);
            }
        }

        throw new FormatException("Unterminated string");
    }

    private bool ParseBoolean()
    {
        if (ConsumeString("true"))
            return true;
        if (ConsumeString("false"))
            return false;

        throw new FormatException("Expected 'true' or 'false'");
    }

    private object ParseNull()
    {
        if (ConsumeString("null"))
            return null;

        throw new FormatException("Expected 'null'");
    }

    private object ParseNumber()
    {
        var sb = new StringBuilder();

        if (PeekChar() == '-')
        {
            sb.Append(ConsumeChar());
        }

        while (_position < _json.Length && char.IsDigit(PeekChar()))
        {
            sb.Append(ConsumeChar());
        }

        string numberStr = sb.ToString();

        // Try parsing as long first, then int
        if (long.TryParse(numberStr, out long longResult))
        {
            // If it fits in an int, return as int for compatibility
            if (longResult >= int.MinValue && longResult <= int.MaxValue)
                return (int)longResult;
            else
                return longResult; // This will be boxed as long
        }

        throw new FormatException("Invalid number format");
    }

    private void SkipWhitespace()
    {
        while (_position < _json.Length && char.IsWhiteSpace(_json[_position]))
        {
            _position++;
        }
    }

    private char PeekChar()
    {
        return _position < _json.Length ? _json[_position] : '\0';
    }

    private char ConsumeChar()
    {
        return _position < _json.Length ? _json[_position++] : '\0';
    }

    private bool ConsumeChar(char expected)
    {
        if (PeekChar() == expected)
        {
            _position++;
            return true;
        }
        return false;
    }

    private bool ConsumeString(string expected)
    {
        if (_position + expected.Length <= _json.Length)
        {
            string actual = _json.Substring(_position, expected.Length);
            if (actual == expected)
            {
                _position += expected.Length;
                return true;
            }
        }
        return false;
    }
}