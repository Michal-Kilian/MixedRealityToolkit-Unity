using MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class DistrictContextMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text packageLabel;
    [SerializeField] private PressableButton hideDistrictButton;
    [SerializeField] private PressableButton closeButton;
    [SerializeField] private float clickThreshold = 0.3f;
    [SerializeField] private RawImage hideDistrictImage;
    [SerializeField] private Color enabledColor;
    [SerializeField] private Color disabledColor;

    private District targetDistrict;
    private Camera mainCamera;
    private float hideDistrictSelectStartTime;
    private float closeSelectStartTime;

    private bool hideDistrictEnabled = false;

    private void Awake()
    {
        mainCamera = Camera.main;

        hideDistrictButton.selectEntered.AddListener(HideDistrictSelectEntered);
        hideDistrictButton.selectExited.AddListener(HideDistrictSelectExited);
        closeButton.selectEntered.AddListener(OnCloseSelectEntered);
        closeButton.selectExited.AddListener(OnCloseSelectExited);
    }

    public void Initialize(District district)
    {
        targetDistrict = district;

        packageLabel.text = district.packageName;
        
        gameObject.transform.localScale = new(
            gameObject.transform.localScale.x / 150f,
            gameObject.transform.localScale.y / 150f,
            gameObject.transform.localScale.z / 150f
        );
    }

    private void Update()
    {
        EnableHideDistrict(!ProjectCity.Instance.HasOneOrNonePackageVisible());
    }

    private void EnableHideDistrict(bool enable)
    {
        hideDistrictEnabled = enable;
        hideDistrictImage.color = enable ? enabledColor : disabledColor;
    }

    private void LateUpdate()
    {
        if (mainCamera == null) return;

        Vector3 lookDirection = transform.position - mainCamera.transform.position;
        transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    private void HideDistrictSelectEntered(SelectEnterEventArgs arg0)
    {
        hideDistrictSelectStartTime = Time.time;
    }

    private void HideDistrictSelectExited(SelectExitEventArgs arg0)
    {
        float heldTime = Time.time - hideDistrictSelectStartTime;
        if (heldTime < clickThreshold)
        {
            if (!hideDistrictEnabled) return;
            HideDistrict();
            Close();
        }
    }

    private void OnCloseSelectEntered(SelectEnterEventArgs arg0)
    {
        closeSelectStartTime = Time.time;
    }

    private void OnCloseSelectExited(SelectExitEventArgs arg0)
    {
        float heldTime = Time.time - closeSelectStartTime;
        if (heldTime < clickThreshold)
        {
            Close();
        }
    }

    public void HideDistrict()
    {
        Debug.Log($"Hiding package: {targetDistrict.packageName}");

        ExperimentManager.Instance.LogInteraction(
            InteractionType.CityDistrictHide,
            targetDistrict.packageName
        );

        ProjectCity.Instance.HidePackage(targetDistrict.packageName);
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
