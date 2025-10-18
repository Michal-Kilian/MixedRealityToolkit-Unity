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

    private ClientWebSocket ws;

    private readonly Queue<string> messageQueue = new();
    private CancellationTokenSource cts;

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
                case "projectStructure":
                    var structure = envelope.Data.ToObject<ProjectStructure>();
                    Debug.Log($"Received project: {structure.ProjectName} with {structure.Packages.Count} packages");
                    ProjectCity.Instance.RebuildCity(structure);
                    break;

                case "executionSample":
                    var sample = envelope.Data.ToObject<ExecutionSample>();
                    ProjectCity.Instance.OnExecutionSample(sample);
                    break;

                case "projectSnapshot":
                    Debug.Log("Received projectSnapshot");
                    break;

                case "openTabs":
                    Debug.Log("Received OpenTabs");
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
}
