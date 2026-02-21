using MixedReality.Toolkit.SpatialManipulation;
using TMPro;
using UnityEngine;

public class CitySettingsTooltip : MonoBehaviour
{
    public static CitySettingsTooltip Instance { get; private set; }

    [SerializeField] private TMP_Text label;
    [SerializeField] private GameObject plate;
    [SerializeField] private BoundsControl boundsControl;

    private Transform followTarget;
    private Vector3 localOffset;

    private void Awake()
    {
        Instance = this;
        plate.SetActive(false);
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

    public void Show(string text)
    {
        plate.SetActive(true);
        label.text = text;
    }

    public void Hide()
    {
        plate.SetActive(false);
        label.text = "";
    }
}
