using TMPro;
using UnityEngine;

public class FloorTooltip : MonoBehaviour
{
    [SerializeField] private TMP_Text methodLabel;

    private Camera mainCamera;
    
    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Initialize(Floor floor)
    {
        methodLabel.text = floor.methodName;

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

    private void OnDestroy()
    {
        UIManager.UnregisterTooltip(gameObject);
    }
}
