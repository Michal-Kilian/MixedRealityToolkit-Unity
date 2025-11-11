using System;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TMP_Text textMessage;

    private WebSocketManager wsManager;

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

        WebSocketMessage message = new()
        {
            Type = MessageType.REQUEST_PROJECT_STRUCTURE,
            Source = "Unity",
            TimeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            Data = new JObject()
        };

        string json = JsonConvert.SerializeObject(message);
        await wsManager.SendAsync(json);
        Debug.Log("[UIManager] Requesting project structure");
    }
}
