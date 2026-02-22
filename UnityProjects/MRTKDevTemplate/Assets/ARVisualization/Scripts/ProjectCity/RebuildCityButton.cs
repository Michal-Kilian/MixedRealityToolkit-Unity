using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.UX;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(PressableButton))]
public class RebuildCityButton : MonoBehaviour
{
    [SerializeField] private Renderer buttonRenderer;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material outdatedMaterial;
    [SerializeField] private GameObject outdatedPanel;
    [SerializeField] private BoundsControl boundsControl;

    private PressableButton button;

    private Transform followTarget;
    private Vector3 localOffset;

    private bool isProjectOutdated = false;

    private void Awake()
    {
        button = GetComponent<PressableButton>();
        button.OnClicked.AddListener(Rebuild);
        button.hoverEntered.AddListener(ShowTooltip);
        button.hoverExited.AddListener(HideTooltip);
    }

    private void Start()
    {
        followTarget = boundsControl.Target != null
            ? boundsControl.Target.transform
            : boundsControl.transform;
        localOffset = followTarget.InverseTransformPoint(transform.position);
    }

    private void LateUpdate()
    {
        if (followTarget == null) return;

        transform.SetPositionAndRotation(
            followTarget.TransformPoint(localOffset),
            followTarget.rotation
        );
    }

    public void Rebuild()
    {
        if (ProjectCity.Instance == null) return;

        ProjectCity.Instance.ResetFilters();

        if (isProjectOutdated)
        {
            ProjectCity.Instance.RequestProjectStructure();
            SetProjectOutdated(false);
        }
    }

    private void ShowTooltip(HoverEnterEventArgs arg0)
    {
        CitySettingsTooltip.Instance.Show("Reset & Rebuild City");
    }

    private void HideTooltip(HoverExitEventArgs arg0)
    {
        CitySettingsTooltip.Instance.Hide();
    }

    public void SetProjectOutdated(bool outdated)
    {
        isProjectOutdated = outdated;

        buttonRenderer.material = outdated
            ? outdatedMaterial
            : normalMaterial;

        if (outdatedPanel != null)
            outdatedPanel.SetActive(outdated);
    }
}
