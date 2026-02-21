using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.UX;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(PressableButton))]
public class LockUnlockButton : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite unlockedSprite;
    [SerializeField] private BoundsControl boundsControl;
    [SerializeField] private BoxCollider boxCollider;

    private PressableButton button;

    private bool locked = true;
    public bool Locked => locked;

    private Coroutine animateRoutine;

    private Transform followTarget;
    private Vector3 localOffset;

    private void Awake()
    {
        button = GetComponent<PressableButton>();
        spriteRenderer.sprite = lockedSprite;
        button.OnClicked.AddListener(Toggle);
        button.hoverEntered.AddListener(ShowTooltip);
        button.hoverExited.AddListener(HideTooltip);
        UpdateLockState();
        UpdateVisuals();
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

    public void Toggle()
    {
        locked = !locked;
        UpdateLockState();
        UpdateVisuals();
    }

    private void UpdateLockState()
    {
        if (boundsControl == null) return;

        if (locked)
        {
            boundsControl.HandlesActive = false;
            boxCollider.enabled = false;
        }
        else
        {
            boundsControl.HandlesActive = true;
            boxCollider.enabled = true;
        }
    }

    private void UpdateVisuals()
    {
        spriteRenderer.sprite = locked ? lockedSprite : unlockedSprite;
        spriteRenderer.material.color = locked ? Color.red : Color.green;

        if (animateRoutine != null) StopCoroutine(animateRoutine);
        animateRoutine = StartCoroutine(AnimateRoutine());
    }

    private IEnumerator AnimateRoutine()
    {
        float duration = 0.25f;
        float time = 0f;

        Quaternion startRotation = spriteRenderer.transform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(
            0f, 0f, locked ? 0f : -45f
        );

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            spriteRenderer.transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        spriteRenderer.transform.localRotation = targetRotation;
    }

    private void ShowTooltip(HoverEnterEventArgs arg0)
    {
        CitySettingsTooltip.Instance.Show("Lock/Unlock City");
    }

    private void HideTooltip(HoverExitEventArgs arg0)
    {
        CitySettingsTooltip.Instance.Hide();
    }
}
