using TMPro;
using UnityEngine;

public class FlameBarTooltip : MonoBehaviour
{
    [SerializeField] private TMP_Text methodLabel;

    private Camera mainCamera;
    private FlameBar associatedFlameBar;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Initialize(FlameBar flameBar)
    {
        associatedFlameBar = flameBar;
        methodLabel.text = flameBar.MethodKey;

        gameObject.transform.localScale = new(
            gameObject.transform.localScale.x / 150f,
            gameObject.transform.localScale.y / 150f,
            gameObject.transform.localScale.z / 150f
        );
    }

    private void LateUpdate()
    {
        if (mainCamera == null) return;

        Vector3 lookDirection = transform.position - mainCamera.transform.position;
        transform.rotation = Quaternion.LookRotation(lookDirection);
    }
}
