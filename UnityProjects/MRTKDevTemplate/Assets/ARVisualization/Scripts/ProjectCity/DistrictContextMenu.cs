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
    [SerializeField] private RawImage hideDistrictImage;
    [SerializeField] private Color enabledColor;
    [SerializeField] private Color disabledColor;

    private District targetDistrict;
    private Camera mainCamera;

    private bool hideDistrictEnabled = false;

    private void Awake()
    {
        mainCamera = Camera.main;

        hideDistrictButton.selectEntered.AddListener(HideDistrictSelectEntered);
        closeButton.selectEntered.AddListener(OnCloseSelectEntered);
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
        if (!hideDistrictEnabled) return;
        HideDistrict();
        Close();
    }

    private void OnCloseSelectEntered(SelectEnterEventArgs arg0)
    {
        Close();
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
