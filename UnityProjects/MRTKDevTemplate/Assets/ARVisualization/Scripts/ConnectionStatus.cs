using MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;

public class ConnectionStatus : MonoBehaviour
{
    [SerializeField] private WebSocketManager webSocketManager;

    [SerializeField] private TMP_Text statusText;
    [SerializeField] private SpriteRenderer statusIcon;
    [SerializeField] private Sprite connectedIcon;
    [SerializeField] private Sprite connectingIcon;
    [SerializeField] private Sprite disconnectedIcon;

    [SerializeField] private float connectingSpinSpeed = 180f;

    [SerializeField] private PressableButton connectButton;

    [SerializeField] private GameObject projectPanel;
    [SerializeField] private TMP_Text projectText;

    private bool isSpinning;
    private Transform iconTransform;

    private Camera mainCamera;

    private string currentProjectName;

    private void Awake()
    {
        iconTransform = statusIcon.transform;
        connectButton.OnClicked.AddListener(OnConnectClicked);
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        webSocketManager.OnConnectionStateChanged += UpdateUI;
        webSocketManager.OnProjectSnapshotReceived += OnProjectSnapshot;
        UpdateUI(webSocketManager.State);
    }

    private void OnDisable()
    {
        webSocketManager.OnConnectionStateChanged -= UpdateUI;        webSocketManager.OnProjectSnapshotReceived += OnProjectSnapshot;
        webSocketManager.OnProjectSnapshotReceived -= OnProjectSnapshot;
    }

    private void UpdateUI(ConnectionState state)
    {
        StopSpinning();

        switch (state)
        {
            case ConnectionState.Connected:
                statusText.text = "Connected";
                statusIcon.sprite = connectedIcon;
                connectButton.gameObject.SetActive(false);
                break;

            case ConnectionState.Connecting:
                statusText.text = "Connecting...";
                statusIcon.sprite = connectingIcon;
                connectButton.gameObject.SetActive(false);
                HideProjectPanel();
                StartSpinning();
                break;

            case ConnectionState.Disconnected:
                statusText.text = "Disconnected";
                statusIcon.sprite = disconnectedIcon;
                connectButton.gameObject.SetActive(true);
                HideProjectPanel();
                break;
        }
    }

    private void Update()
    {
        if (!isSpinning) return;

        iconTransform.Rotate(0f, 0f, -connectingSpinSpeed * Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (mainCamera == null) return;

        Vector3 direction = transform.position - mainCamera.transform.position;

        if (direction.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void StartSpinning()
    {
        isSpinning = true;
    }

    private void StopSpinning()
    {
        isSpinning = false;
        iconTransform.localRotation = Quaternion.identity;
    }

    private void OnConnectClicked()
    {
        webSocketManager.Reconnect();
    }

    private void OnProjectSnapshot(ProjectSnapshot snapshot)
    {
        currentProjectName = snapshot.ProjectName;

        projectText.text = snapshot.ProjectName;
        projectPanel.SetActive(true);
    }

    private void HideProjectPanel()
    {
        projectPanel.SetActive(false);
        projectText.text = "";
    }
}
