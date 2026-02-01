using TMPro;
using UnityEngine;

public class MethodFlameTileTooltip : MonoBehaviour
{
    [SerializeField] private TMP_Text methodLabel;

    private Camera mainCamera;
    private MethodFlameActivityTile associatedMethodTile;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Initialize(MethodFlameActivityTile methodTile)
    {
        associatedMethodTile = methodTile;
        methodLabel.text = methodTile.MethodName;

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
