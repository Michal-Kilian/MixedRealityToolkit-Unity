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

    [SerializeField] private GameObject tipPanel;

    [SerializeField] private PressableButton enableLoggingButton;
    [SerializeField] private SpriteRenderer enableLoggingRenderer;

    private bool isSpinning;
    private Transform iconTransform;

    private Camera mainCamera;

    private string currentProjectName;

    private void Awake()
    {
        iconTransform = statusIcon.transform;
        connectButton.OnClicked.AddListener(OnConnectClicked);
        enableLoggingButton.OnClicked.AddListener(OnEnableLoggingClicked);
    }

    private void Start()
    {
        mainCamera = Camera.main;
        if (ExperimentManager.Instance != null)
            UpdateEnableLoggingUI(ExperimentManager.Instance.DefaultLoggingEnabled);
    }

    private void OnEnable()
    {
        webSocketManager.OnConnectionStateChanged += UpdateUI;
        webSocketManager.OnProjectSnapshotReceived += OnProjectSnapshot;
        UpdateUI(webSocketManager.State);
        if (ExperimentManager.Instance != null)
            UpdateEnableLoggingUI(ExperimentManager.Instance.DefaultLoggingEnabled);
    }

    private void OnDisable()
    {
        webSocketManager.OnConnectionStateChanged -= UpdateUI;
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
                statusIcon.material.color = Color.green;
                connectButton.gameObject.SetActive(false);
                break;

            case ConnectionState.Connecting:
                statusText.text = "Connecting...";
                statusIcon.sprite = connectingIcon;
                statusIcon.material.color = Color.blue;
                connectButton.gameObject.SetActive(false);
                HideProjectPanel();
                StartSpinning();
                break;

            case ConnectionState.Disconnected:
                statusText.text = "Disconnected";
                statusIcon.sprite = disconnectedIcon;
                statusIcon.material.color = Color.red;
                connectButton.gameObject.SetActive(true);
                HideProjectPanel();
                break;
        }
    }

    private void UpdateTipPanel()
    {
        if (!tipPanel.activeInHierarchy &&
            (!ProjectCity.Instance.IsDisplayed() ||
            webSocketManager.State == ConnectionState.Disconnected))
        {
            tipPanel.SetActive(true);
        }

        if (tipPanel.activeInHierarchy &&
            ProjectCity.Instance.IsDisplayed() &&
            webSocketManager.State == ConnectionState.Connected)
        {
            tipPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isSpinning) return;

        iconTransform.Rotate(0f, 0f, -connectingSpinSpeed * Time.deltaTime);
    }

    private void LateUpdate()
    {
        UpdateTipPanel();

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
        ExperimentManager.Instance.LogInteraction(InteractionType.Reconnect);

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

    private void OnEnableLoggingClicked()
    {
        bool newEnable = !ExperimentManager.Instance.EnableLogging;
        ExperimentManager.Instance.SetEnableLogging(newEnable);

        UpdateEnableLoggingUI(newEnable);
    }

    private void UpdateEnableLoggingUI(bool enable)
    {
        enableLoggingRenderer.sprite = enable ? connectedIcon : disconnectedIcon;
        enableLoggingRenderer.material.color = enable ? Color.green : Color.red;
    }
}
