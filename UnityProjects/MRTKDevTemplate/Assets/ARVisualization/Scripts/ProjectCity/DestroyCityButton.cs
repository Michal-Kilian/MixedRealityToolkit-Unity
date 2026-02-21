using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.UX;
using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(PressableButton))]
public class DestroyCityButton : MonoBehaviour
{
    [SerializeField] private BoundsControl boundsControl;
    
    private PressableButton button;

    private Transform followTarget;
    private Vector3 localOffset;

    private void Awake()
    {
        button = GetComponent<PressableButton>();
        button.OnClicked.AddListener(Destroy);
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

    public void Destroy()
    {
        if (ProjectCity.Instance != null)
            ProjectCity.Instance.Destroy();
    }

    private void ShowTooltip(HoverEnterEventArgs arg0)
    {
        CitySettingsTooltip.Instance.Show("Destroy city");
    }

    private void HideTooltip(HoverExitEventArgs arg0)
    {
        CitySettingsTooltip.Instance.Hide();
    }
}
