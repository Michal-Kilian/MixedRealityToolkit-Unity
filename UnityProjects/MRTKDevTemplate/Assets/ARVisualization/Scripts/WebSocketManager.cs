using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
}

public class WebSocketManager : MonoBehaviour
{
    [Header("Connection")]
    [SerializeField] private bool shouldConnect = false;
    [SerializeField] private string serverHost = "localhost";  //192.168.88.60
    [SerializeField] private int serverPort = 8765;

    [SerializeField] private UIManager uiManager;

    [Header("Heartbeat")]
    [SerializeField] private float heartbeatInterval = 3f;
    [SerializeField] private float heartbeatTimeout = 10f;

    private ClientWebSocket ws;
    private CancellationTokenSource cts;

    private readonly Queue<string> messageQueue = new();

    public ConnectionState State { get; private set; } = ConnectionState.Disconnected;

    public event Action<ConnectionState> OnConnectionStateChanged;
    public event Action<ProjectSnapshot> OnProjectSnapshotReceived;

    public bool IsConnected => ws != null && ws.State == WebSocketState.Open;

    private float heartbeatTimer;
    private float lastPongTime;

    private volatile bool pendingDisconnect;

    void Awake()
    {
        Application.runInBackground = true;
    }

    async void Start()
    {
        if (shouldConnect)
        {
            await ConnectAsync();
        }
    }

    private void SetState(ConnectionState newState)
    {
        if (State == newState) return;
        State = newState;
        OnConnectionStateChanged?.Invoke(State);
    }

    private async Task ConnectAsync()
    {
        if (State == ConnectionState.Connecting) return;

        Disconnect();

        SetState(ConnectionState.Connecting);

        ws = new ClientWebSocket();
        cts = new CancellationTokenSource();

        try
        {
            Uri serverUri = new($"ws://{serverHost}:{serverPort}");

            await ws.ConnectAsync(serverUri, cts.Token);

            Debug.Log("WebSocket connected");

            heartbeatTimer = 0f;
            lastPongTime = Time.time;

            SetState(ConnectionState.Connected);

            _ = Task.Run(ReceiveLoop);
        }
        catch (Exception e)
        {
            Debug.LogError("WebSocket connect error: " + e.Message);
            Disconnect();
        }
    }

    public async void Reconnect()
    {
        if (State == ConnectionState.Connecting) return;

        Disconnect();
        await ConnectAsync();
    }

    private void Disconnect()
    {
        if (State == ConnectionState.Disconnected) return;

        Debug.Log("Disconnecting WebSocket");

        try
        {
            cts?.Cancel();
        }
        catch { }

        try
        {
            if (ws != null)
            {
                if (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseReceived)
                {
                    ws.Abort();
                }

                ws.Dispose();
            }
        }
        catch { }

        ws = null;
        cts = null;

        SetState(ConnectionState.Disconnected);
    }

    void Update()
    {
        if (pendingDisconnect)
        {
            pendingDisconnect = false;
            Disconnect();
            return;
        }

        if (State != ConnectionState.Connected)
            return;

        heartbeatTimer += Time.deltaTime;
        if (heartbeatTimer >= heartbeatInterval)
        {
            heartbeatTimer = 0f;
            _ = SendPing();
        }

        if (Time.time - lastPongTime > heartbeatTimeout)
        {
            Debug.LogWarning("Heartbeat timeout");
            Disconnect();
            return;
        }

        lock (messageQueue)
        {
            while (messageQueue.Count > 0)
            {
                HandleMessage(messageQueue.Dequeue());
            }
        }
    }

    private async Task ReceiveLoop()
    {
        byte[] buffer = new byte[65536];

        try
        {
            while (ws != null && ws.State == WebSocketState.Open)
            {
                var builder = new StringBuilder();
                WebSocketReceiveResult result;

                do
                {
                    result = await ws.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        cts.Token
                    );

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        pendingDisconnect = true;
                        return;
                    }

                    builder.Append(
                        Encoding.UTF8.GetString(
                            buffer,
                            0,
                            result.Count
                        )
                    );
                }
                while (!result.EndOfMessage);

                lock (messageQueue)
                {
                    messageQueue.Enqueue(builder.ToString());
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("ReceiveLoop error: " + e.Message);
            pendingDisconnect = true;
        }
    }

    private async Task SendPing()
    {
        if (!IsConnected) return;

        try
        {
            await SendMessage(MessageType.PING, new JObject());
        }
        catch
        {
            pendingDisconnect = true;
        }
    }

    public async Task SendMessage(MessageType type, JObject data)
    {
        if (!IsConnected) return;

        WebSocketMessage message = new()
        {
            Type = type,
            Source = "Unity",
            TimeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            Data = data
        };

        string json = JsonConvert.SerializeObject(message);
        byte[] bytes = Encoding.UTF8.GetBytes(json);

        await ws.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );
    }

    private void HandleMessage(string raw)
    {
        try
        {
            var envelope = JsonConvert.DeserializeObject<WebSocketMessage>(raw);

            switch (envelope.Type)
            {
                case MessageType.PONG:
                    lastPongTime = Time.time;
                    break;

                case MessageType.PROJECT_STRUCTURE:
                    var structure = envelope.Data.ToObject<ProjectStructure>();
                    Debug.Log($"Received project: {structure.ProjectName} with {structure.Packages.Count} root packages");
                    if (ProjectCity.Instance != null)
                        ProjectCity.Instance.RebuildCity(structure);
                    break;

                case MessageType.EXECUTION_SAMPLE:
                    var sample = envelope.Data.ToObject<ExecutionSample>();
                    if (ProjectCity.Instance != null)
                        ProjectCity.Instance.OnExecutionSample(sample);
                    if (ActivityMap.Instance != null)
                        ActivityMap.Instance.OnExecutionSample(sample);
                    if (FlameGraph.Instance != null)
                        FlameGraph.Instance.OnExecutionSample(sample);
                    break;

                case MessageType.PROJECT_SNAPSHOT:
                    var snapshot = envelope.Data.ToObject<ProjectSnapshot>();
                    Debug.Log($"Received project snapshot: {snapshot}");
                    OnProjectSnapshotReceived?.Invoke(snapshot);
                    break;

                case MessageType.OPEN_TABS:
                    var openTabs = envelope.Data.ToObject<OpenTabs>();
                    Debug.Log($"Received open tabs: {openTabs}");
                    break;

                case MessageType.PROJECT_OUTDATED:
                    uiManager.OnProjectOutdatedReceived();
                    break;

                case MessageType.COMMAND:
                    var command = envelope.Data.ToObject<CommandMessage>();
                    switch (command.Command)
                    {
                        case CommandType.PAUSE:
                            Debug.Log("Pause command received");
                            if (ProjectCity.Instance != null)
                                ProjectCity.Instance.Paused = true;
                            if (ActivityMap.Instance != null)
                                ActivityMap.Instance.Paused = true;
                            if (FlameGraph.Instance != null)
                                FlameGraph.Instance.Paused = true;
                            break;

                        case CommandType.RESUME:
                            Debug.Log("Resume command received");
                            if (ProjectCity.Instance != null)
                                ProjectCity.Instance.Paused = false;
                            if (ActivityMap.Instance != null)
                                ActivityMap.Instance.Paused = false;
                            if (FlameGraph.Instance != null)
                                FlameGraph.Instance.Paused = false;
                            break;
                    }
                    break;

                default:
                    Debug.Log($"Unknown message type:\n\tType: {envelope.Type}\n\tData: {envelope.Data}");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(
                "JSON parse error: " + e.Message
            );
        }
    }

    async void OnApplicationQuit()
    {
        try
        {
            if (ws != null &&
                ws.State == WebSocketState.Open)
            {
                await ws.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Quit",
                    CancellationToken.None
                );
            }
        }
        catch { }

        Disconnect();
    }
}
