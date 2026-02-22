using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.UX;
using System.Collections;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MethodOrderingPanelController : MonoBehaviour
{
    [Header("Floating")]
    [SerializeField] private BoundsControl boundsControl;

    [Header("UI")]
    [SerializeField] private PressableButton toggleButton;
    [SerializeField] private Transform panel;
    [SerializeField] private MethodOrderingRadioGroup radioGroup;

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.25f;

    private bool open = false;

    private Transform followTarget;
    private Vector3 localOffset;

    private Coroutine animationRoutine;

    private Vector3 originalLocalScale;

    private void Awake()
    {
        toggleButton.OnClicked.AddListener(TogglePanel);
        toggleButton.hoverEntered.AddListener(ShowTooltip);
        toggleButton.hoverExited.AddListener(HideTooltip);
        originalLocalScale = panel.localScale;
        panel.localScale = Vector3.zero;
        panel.gameObject.SetActive(true);

        radioGroup.OnSelect += TogglePanel;
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

    private void TogglePanel()
    {
        open = !open;

        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(AnimatePanel(open));
    }

    private IEnumerator AnimatePanel(bool show)
    {
        float time = 0f;

        Vector3 start = panel.localScale;

        Vector3 target = show ? originalLocalScale : Vector3.zero;

        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = time / animationDuration;
            float eased = Mathf.SmoothStep(0f, 1f, t);

            panel.localScale = Vector3.Lerp(start, target, eased);

            yield return null;
        }

        panel.localScale = target;
    }

    private void ShowTooltip(HoverEnterEventArgs arg0)
    {
        CitySettingsTooltip.Instance.Show("Change Method Ordering");
    }

    private void HideTooltip(HoverExitEventArgs arg0)
    {
        CitySettingsTooltip.Instance.Hide();
    }
}
