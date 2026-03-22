using MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.IO;

public class FloorContextMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text methodLabel;
    [SerializeField] private TMP_Text packageLabel;
    [SerializeField] private TMP_Text fileLabel;
    [SerializeField] private TMP_Text classLabel;
    [SerializeField] private PressableButton openInIDEButton;
    [SerializeField] private PressableButton closeButton;

    private Floor targetFloor;
    private Camera mainCamera;
    private UIManager UIManager;
    
    private void Awake()
    {
        mainCamera = Camera.main;

        UIManager = FindFirstObjectByType<UIManager>();

        openInIDEButton.selectEntered.AddListener(OnOpenInIDESelectEntered);
        closeButton.selectEntered.AddListener(OnCloseSelectEntered);
    }

    public void Initialize(Floor floor)
    {
        targetFloor = floor;

        methodLabel.text = floor.methodName;
        packageLabel.text = floor.packageName;
        fileLabel.text = Path.GetFileName(floor.path);
        classLabel.text = floor.className;

        gameObject.transform.localScale = new(
            gameObject.transform.localScale.x / 150f,
            gameObject.transform.localScale.y / 150f,
            gameObject.transform.localScale.z / 150f
        );
    }

    private void LateUpdate()
    {
        if (mainCamera == null) return;

        Vector3 lookDirection = transform.position - mainCamera.transform.position;
        transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    private void OnOpenInIDESelectEntered(SelectEnterEventArgs arg0)
    {
        ExperimentManager.Instance.LogInteraction(
                InteractionType.CityFloorOpenInIDE,
                targetFloor.packageName,
                targetFloor.className,
                targetFloor.methodName
            );
        OpenInIde();
        Close();
    }

    private void OnCloseSelectEntered(SelectEnterEventArgs arg0)
    {
        Close();
    }

    public void OpenInIde()
    {
        Debug.Log($"Requested opening in IDE: {targetFloor.path}");
        UIManager.OnOpenInIDEClicked(targetFloor.path, targetFloor.line);
    }

    public void Close()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        UIManager.UnregisterContextMenu(gameObject);
    }
}
