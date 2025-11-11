using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class WebSocketManager : MonoBehaviour
{
    [SerializeField] private bool shouldConnect = false;
    [SerializeField] private FlameGraphManager flameGraphManager;
    [SerializeField] private UIManager uiManager;

    private ClientWebSocket ws;

    private readonly Queue<string> messageQueue = new();
    private CancellationTokenSource cts;

    public bool IsConnected => ws != null && ws.State == WebSocketState.Open;

    void Awake()
    {
        Application.runInBackground = true;
    }

    async void Start()
    {
        if (!shouldConnect) return;

        ws = new ClientWebSocket();
        cts = new CancellationTokenSource();

        try
        {
            Uri serverUri = new("ws://localhost:8765");
            await ws.ConnectAsync(serverUri, cts.Token);

            Debug.Log("Connected to server");

            _ = Task.Run(ReceiveLoop);
        }
        catch (Exception e)
        {
            Debug.LogError("WebSocket connect error: " + e.Message);
        }
    }

    void Update()
    {
        if (!shouldConnect) return;

        lock (messageQueue)
        {
            while (messageQueue.Count > 0)
            {
                string raw = messageQueue.Dequeue();
                HandleMessage(raw);
            }
        }
    }

    private void HandleMessage(string raw)
    {
        try
        {
            var envelope = JsonConvert.DeserializeObject<WebSocketMessage>(raw);

            switch(envelope.Type)
            {
                case MessageType.PROJECT_STRUCTURE:
                    var structure = envelope.Data.ToObject<ProjectStructure>();
                    Debug.Log($"Received project: {structure.ProjectName} with {structure.Packages.Count} packages");
                    ProjectCity.Instance.RebuildCity(structure);
                    break;

                case MessageType.EXECUTION_SAMPLE:
                    var sample = envelope.Data.ToObject<ExecutionSample>();
                    ProjectCity.Instance.OnExecutionSample(sample);
                    flameGraphManager.AddSample(sample);
                    break;

                case MessageType.PROJECT_SNAPSHOT:
                    var snapshot = envelope.Data.ToObject<ProjectSnapshot>();
                    Debug.Log($"Received project snapshot: {snapshot}");
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
                            Time.timeScale = 0f;
                            ProjectCity.Instance.Paused = true;
                            break;

                        case CommandType.RESUME:
                            Debug.Log("Resume command received");
                            Time.timeScale = 1f;
                            ProjectCity.Instance.Paused = false;
                            break;
                    }
                    break;

                default:
                    Debug.Log($"Unknown message type:\n\tType: {envelope.Type}\n\tData: {envelope.Data}");
                    break;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("JSON Parse Error: " + e.Message + " | Raw: " + raw);
        }
    }

    private async Task ReceiveLoop()
    {
        byte[] buffer = new byte[8192];

        try
        {
            while (ws.State == WebSocketState.Open)
            {
                var builder = new StringBuilder();
                WebSocketReceiveResult result;

                do
                {
                    result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cts.Token);
                        Debug.Log("WebSocket closed by server");
                        return;
                    }

                    builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                }
                while (!result.EndOfMessage);

                string completeMessage = builder.ToString();

                lock (messageQueue)
                {
                    messageQueue.Enqueue(completeMessage);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("ReceiveLoop error: " + e.Message);
        }
    }

    async void OnApplicationQuit()
    {
        try
        {
            if (ws != null && ws.State == WebSocketState.Open)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Quit", CancellationToken.None);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error closing WebSocket: " + e.Message);
        }

        cts?.Cancel();
    }

    public async Task SendAsync(string json)
    {
        if (!IsConnected) return;
        byte[] data = Encoding.UTF8.GetBytes(json);
        await ws.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}
