using MixedReality.Toolkit;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(MeshRenderer))]
public class District : MonoBehaviour
{
    [SerializeField] private float clickThreshold = 0.3f;

    private GameObject currentContextMenu;
    private GameObject currentTooltip;

    private Vector3 tooltipPosition;

    public string packageName;

    private MRTKBaseInteractable interactable;

    private float selectStartTime;

    private GameObject tooltipPrefab;
    private GameObject contextMenuPrefab;

    public void Initialize(
        string packageName,
        Vector3 tooltipLocalPosition,
        GameObject tooltipPrefab,
        GameObject contextMenuPrefab
    )
    {
        this.packageName = packageName;
        this.tooltipPosition = tooltipLocalPosition;
        this.tooltipPrefab = tooltipPrefab;
        this.contextMenuPrefab = contextMenuPrefab;

        interactable = gameObject.AddComponent<MRTKBaseInteractable>();
        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);
        interactable.selectEntered.AddListener(OnSelectEntered);
        interactable.selectExited.AddListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs arg0)
    {
        selectStartTime = Time.time;
    }

    private void OnSelectExited(SelectExitEventArgs arg0)
    {
        float heldTime = Time.time - selectStartTime;
        if (heldTime < clickThreshold)
        {
            ShowContextMenu();
        }
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        ShowTooltip();
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        HideTooltip();
    }

    private void ShowTooltip()
    {
        if (currentTooltip != null) return;

        currentTooltip = Instantiate(
            tooltipPrefab,
            transform.parent
        );

        currentTooltip.transform.SetLocalPositionAndRotation(
            tooltipPosition,
            Quaternion.identity
        );

        var tooltip = currentTooltip.GetComponent<FloorTooltip>();
        tooltip.InitializeDistrict(packageName);
    }

    private void HideTooltip()
    {
        if (currentTooltip == null) return;

        Destroy(currentTooltip);
    }

    private void ShowContextMenu()
    {
        if (currentContextMenu != null) Destroy(currentContextMenu);

        Vector3 position = new(
            transform.position.x,
            ProjectCity.Instance.CityTopHeight + 0.2f,
            transform.position.z
        );
        currentContextMenu = Instantiate(contextMenuPrefab, position, Quaternion.identity);
        var ctx = currentContextMenu.GetComponent<DistrictContextMenu>();
        ctx.Initialize(this);

        UIManager.RegisterContextMenu(currentContextMenu);
    }
}
