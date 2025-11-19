using TMPro;
using UnityEngine;

public class MethodTileTooltip : MonoBehaviour
{
    [SerializeField] private TMP_Text methodLabel;

    private Camera mainCamera;
    private MethodActivityTile associatedMethodTile;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Initialize(MethodActivityTile methodTile)
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
