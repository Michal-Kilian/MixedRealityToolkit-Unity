using System;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TMP_Text textMessage;

    private WebSocketManager wsManager;
    private static readonly int maxLines = 3;

    private static readonly List<GameObject> activeContextMenus = new();
    private static readonly List<GameObject> activeFloorTooltips = new();
    private static readonly List<GameObject> activeMethodTileTooltips = new();
    private static readonly List<LineRenderer> activeLineConnections = new();

    private void Awake()
    {
        wsManager = FindObjectOfType<WebSocketManager>();

        HideNotification();
    }

    public void OnProjectOutdatedReceived()
    {
        ShowProjectOutdated("Project outdated");
    }

    public void ShowProjectOutdated(string message)
    {
        notificationPanel.SetActive(true);
        textMessage.text = message;
    }

    public void HideNotification()
    {
        notificationPanel.SetActive(false);
        textMessage.text = "";
    }

    public async void OnRegenerateClicked()
    {
        HideNotification();

        await wsManager.SendMessage(
            type: MessageType.REQUEST_PROJECT_STRUCTURE,
            data: new JObject()
        );
    }

    public async void OnOpenInIDEClicked(string methodPath, int line)
    {
        CommandMessage message = new()
        {
            Command = CommandType.OPEN_IN_IDE,
            Path = methodPath,
            Line = line,
            Reason = "User clicked on Open in IDE button"
        };

        var json = JsonConvert.SerializeObject(message);
        var jObject = JObject.Parse(json);

        await wsManager.SendMessage(
            type: MessageType.COMMAND,
            data: jObject
        );
    }

    public static void RegisterContextMenu(GameObject menu)
    {
        foreach (var existing in activeContextMenus)
        {
            if (existing != null) Destroy(existing);
        }

        activeContextMenus.Clear();
        activeContextMenus.Add(menu);
    }

    public static void UnregisterContextMenu(GameObject menu)
    {
        if (activeContextMenus.Contains(menu))
            activeContextMenus.Remove(menu);
    }

    public static void RegisterFloorTooltip(GameObject tooltip)
    {
        foreach (var existing in activeFloorTooltips)
        {
            if (existing != null) Destroy(existing);
        }

        activeFloorTooltips.Clear();
        activeFloorTooltips.Add(tooltip);
    }

    public static void UnregisterFloorTooltip(GameObject tooltip)
    {
        if (activeFloorTooltips.Contains(tooltip))
            activeFloorTooltips.Remove(tooltip);
    }

    public static void RegisterMethodTileTooltip(GameObject tooltip)
    {
        foreach (var existing in activeMethodTileTooltips)
        {
            if (existing != null) Destroy(existing);
        }

        activeMethodTileTooltips.Clear();
        activeMethodTileTooltips.Add(tooltip);
    }

    public static void UnregisterMethodTileTooltip(GameObject tooltip)
    {
        if (activeMethodTileTooltips.Contains(tooltip))
            activeMethodTileTooltips.Remove(tooltip);
    }

    public static void RegisterLineConnection(LineRenderer line)
    {
        activeLineConnections.RemoveAll(l => l == null);
        activeLineConnections.Add(line);

        if (activeLineConnections.Count > maxLines)
        {
            LineRenderer oldest = activeLineConnections[0];
            if (oldest != null)
            {
                oldest.enabled = false;
            }
            activeLineConnections.RemoveAt(0);
        }
    }

    public static void UnregisterLineConnection(LineRenderer line)
    {
        activeLineConnections.Remove(line);
    }
}
