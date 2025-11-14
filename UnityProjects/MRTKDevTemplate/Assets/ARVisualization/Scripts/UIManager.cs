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

    private static readonly List<GameObject> activeContextMenus = new();
    private static readonly List<GameObject> activeTooltips = new();

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

    public static void RegisterTooltip(GameObject tooltip)
    {
        foreach (var existing in activeTooltips)
        {
            if (existing != null) Destroy(existing);
        }

        activeTooltips.Clear();
        activeTooltips.Add(tooltip);
    }

    public static void UnregisterTooltip(GameObject tooltip)
    {
        if (activeTooltips.Contains(tooltip))
            activeTooltips.Remove(tooltip);
    }
}
