using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.UX;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PressableButton))]
public class LockUnlockButton : MonoBehaviour
{
    [SerializeField] private PressableButton button;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite unlockedSprite;
    [SerializeField] private BoundsControl boundsControl;
    [SerializeField] private BoxCollider boxCollider;

    private bool locked = true;
    public bool Locked => locked;

    private Coroutine animateRoutine;

    private void Awake()
    {
        button = GetComponent<PressableButton>();
        spriteRenderer.sprite = lockedSprite;
        button.OnClicked.AddListener(Toggle);
        UpdateLockState();
        UpdateVisuals();
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
            boundsControl.EnabledHandles &= ~HandleType.Translation;
            boxCollider.enabled = false;
        } else
        {
            boundsControl.EnabledHandles |= HandleType.Translation;
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
}
