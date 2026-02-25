using MixedReality.Toolkit;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(StatefulInteractable))]
public class Floor : MonoBehaviour
{
    [SerializeField] private GameObject contextMenuPrefab;
    [SerializeField] private GameObject tooltipPrefab;

    private GameObject currentContextMenu;
    private GameObject currentTooltip;

    private Vector3 tooltipPosition;

    public string path;
    public int line;
    public string packageName;
    public string className;
    public string methodName;
    public int lineCount;

    private StatefulInteractable interactable;

    [SerializeField] private float spawnAnimationDuration = 0.6f;
    [SerializeField] private AnimationCurve spawnEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public void Initialize(
        string path,
        int line,
        string packageName,
        string className,
        string methodName,
        int lineCount,
        Vector3 targetLocalPosition,
        Vector3 targetLocalScale,
        Vector3 tooltipPosition,
        float spawnDelay = 0f
    )
    {
        this.path = path;
        this.line = line;
        this.packageName = packageName;
        this.className = className;
        this.methodName = methodName;
        this.lineCount = lineCount;
        this.tooltipPosition = tooltipPosition;

        name = $"{className}.{methodName}";

        PlaySpawnAnimation(targetLocalPosition, targetLocalScale, spawnDelay);

        interactable = gameObject.GetComponent<StatefulInteractable>();
        interactable.selectEntered.AddListener(OnSelect);
        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);
    }

    private void PlaySpawnAnimation(
        Vector3 targetLocalPosition,
        Vector3 targetLocalScale,
        float delay
    )
    {
        transform.localPosition = targetLocalPosition;
        transform.localScale = Vector3.zero;

        StartCoroutine(SpawnRoutine(
            targetLocalScale,
            delay
        ));
    }

    private IEnumerator SpawnRoutine(
        Vector3 targetLocalScale,
        float delay
    )
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        float time = 0f;

        while (time < spawnAnimationDuration)
        {
            time += Time.deltaTime;
            float t = time / spawnAnimationDuration;

            float eased = spawnEase.Evaluate(t);

            transform.localScale = Vector3.Lerp(Vector3.zero, targetLocalScale, eased);

            yield return null;
        }

        transform.localScale = targetLocalScale;
    }

    private void OnSelect(SelectEnterEventArgs arg0)
    {
        ExperimentManager.Instance.LogInteraction(
            InteractionType.CityFloorSelect,
            packageName,
            className,
            methodName
        );
        ShowContextMenu();
    }

    private void OnHoverEntered(HoverEnterEventArgs arg0)
    {
        ExperimentManager.Instance.LogInteraction(
            InteractionType.CityFloorHover,
            packageName,
            className,
            methodName
        );
        ToggleTooltip(true);
    }

    private void OnHoverExited(HoverExitEventArgs arg0)
    {
        ToggleTooltip(false);
    }

    private void ToggleTooltip(bool show)
    {
        if (!show)
        {
            if (currentTooltip != null)
            {
                UIManager.UnregisterFloorTooltip(currentTooltip);
                Destroy(currentTooltip);
            }
            return;
        }

        if (currentTooltip != null) Destroy(currentTooltip);

        currentTooltip = Instantiate(tooltipPrefab, transform.parent);

        currentTooltip.transform.SetLocalPositionAndRotation(tooltipPosition, Quaternion.identity);
        var tooltip = currentTooltip.GetComponent<FloorTooltip>();
        tooltip.Initialize(this);

        UIManager.RegisterFloorTooltip(currentTooltip);
    }

    private void ShowContextMenu()
    {
        if (currentContextMenu != null) Destroy(currentContextMenu);

        Vector3 position = new(
            transform.position.x,
            ProjectCity.Instance.CityTopHeight + 0.2f,
            transform.position.z
        );
        currentContextMenu = Instantiate(contextMenuPrefab, position, Quaternion.identity);
        var ctx = currentContextMenu.GetComponent<FloorContextMenu>();
        ctx.Initialize(this);

        UIManager.RegisterContextMenu(currentContextMenu);
    }
}
