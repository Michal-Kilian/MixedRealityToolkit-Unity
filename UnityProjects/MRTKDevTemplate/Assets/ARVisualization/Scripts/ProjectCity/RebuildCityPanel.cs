using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

public class RebuildCityPanel : MonoBehaviour
{
    [SerializeField] private BoundsControl boundsControl;

    private Transform followTarget;
    private Vector3 localOffset;

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
}
