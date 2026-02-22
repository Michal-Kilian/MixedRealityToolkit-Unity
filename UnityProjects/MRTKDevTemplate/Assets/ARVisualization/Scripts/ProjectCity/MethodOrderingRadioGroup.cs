using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.UX;
using System;
using UnityEngine;

public class MethodOrderingRadioGroup : MonoBehaviour
{
    [SerializeField] private PressableButton locButton;
    [SerializeField] private PressableButton parButton;
    [SerializeField] private PressableButton annButton;

    [SerializeField] private Renderer locRenderer;
    [SerializeField] private Renderer parRenderer;
    [SerializeField] private Renderer annRenderer;

    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material unselectedMaterial;

    [SerializeField] private BoundsControl boundsControl;

    private Transform followTarget;
    private Vector3 localOffset;

    public Action OnSelect;

    private void Awake()
    {
        locButton.OnClicked.AddListener(() => Select(MethodOrdering.LOC));
        parButton.OnClicked.AddListener(() => Select(MethodOrdering.ParameterCount));
        annButton.OnClicked.AddListener(() => Select(MethodOrdering.AnnotationsCount));

        Select(MethodOrdering.LOC);
    }

    private void Start()
    {
        followTarget = boundsControl.Target != null
            ? boundsControl.Target.transform
            : boundsControl.transform;

        localOffset = followTarget.InverseTransformPoint(transform.position);
    }

    private void Select(MethodOrdering ordering)
    {
        if (ProjectCity.Instance != null)
            ProjectCity.Instance.SetMethodOrdering(ordering);

        SetVisual(locRenderer, ordering == MethodOrdering.LOC);
        SetVisual(parRenderer, ordering == MethodOrdering.ParameterCount);
        SetVisual(annRenderer, ordering == MethodOrdering.AnnotationsCount);

        OnSelect?.Invoke();
    }

    private void LateUpdate()
    {
        if (followTarget == null) return;

        transform.SetPositionAndRotation(
            followTarget.TransformPoint(localOffset),
            followTarget.rotation
        );
    }

    private void SetVisual(Renderer r, bool active)
    {
        r.material = active ? selectedMaterial : unselectedMaterial;
    }
}
