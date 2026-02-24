using MixedReality.Toolkit;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(MeshRenderer))]
public class District : MonoBehaviour
{
    [SerializeField] private GameObject contextMenuPrefab;
    [SerializeField] private GameObject tooltipPrefab;

    private GameObject currentContextMenu;
    private GameObject currentTooltip;

    private Vector3 tooltipPosition;

    public string packageName;

    private StatefulInteractable interactable;

    public void Initialize(
        string packageName,
        Vector3 tooltipPosition
    )
    {
        this.packageName = packageName;
        this.tooltipPosition = tooltipPosition;

        interactable = gameObject.GetComponent<StatefulInteractable>();
        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);
        interactable.selectEntered.AddListener(OnSelect);
    }

    private void OnSelect(SelectEnterEventArgs arg0)
    {
        ExperimentManager.Instance.LogInteraction(
            InteractionType.CityDistrictSelect,
            packageName
        );

        ShowContextMenu();
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        ExperimentManager.Instance.LogInteraction(
            InteractionType.CityDistrictHover,
            packageName
        );

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
