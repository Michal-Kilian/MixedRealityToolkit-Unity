using MixedReality.Toolkit;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Floor : MonoBehaviour
{
    [SerializeField] private float clickThreshold = 0.001f;
    [SerializeField] private GameObject contextMenuPrefab;
    [SerializeField] private GameObject tooltipPrefab;

    private GameObject currentContextMenu;
    private GameObject currentTooltip;

    public string path;
    public int line;
    public string packageName;
    public string className;
    public string methodName;
    public int lineCount;

    private MRTKBaseInteractable interactable;

    private float selectStartTime;

    public void Initialize(
        string path,
        int line,
        string packageName,
        string className,
        string methodName,
        int lineCount,
        float footprint,
        float scaledHeight,
        float currentHeight,
        float floorGap
    )
    {
        this.path = path;
        this.line = line;
        this.packageName = packageName;
        this.className = className;
        this.methodName = methodName;
        this.lineCount = lineCount;

        name = $"{className}.{methodName}";
        ResizeToMethod(footprint, scaledHeight, currentHeight, floorGap);

        interactable = gameObject.AddComponent<MRTKBaseInteractable>();
        interactable.selectEntered.AddListener(OnSelectEntered);
        interactable.selectExited.AddListener(OnSelectExited);
        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);
    }

    private void ResizeToMethod(float footprint, float scaledHeight, float currentHeight, float floorGap)
    {
        gameObject.transform.localScale = new(footprint, scaledHeight, footprint);
        gameObject.transform.localPosition = new(0f, currentHeight + scaledHeight / 2f + floorGap, 0f);
    }

    private void OnSelectEntered(SelectEnterEventArgs arg0)
    {
        selectStartTime = Time.time;
    }

    private void OnSelectExited(SelectExitEventArgs arg0)
    {
        float heldTime = Time.time - selectStartTime;
        if (heldTime < clickThreshold)
        {
            ShowContextMenu();
        }
    }

    private void OnHoverEntered(HoverEnterEventArgs arg0)
    {
        ToggleTooltip(true);
    }

    private void OnHoverExited(HoverExitEventArgs arg0)
    {
        ToggleTooltip(false);
    }

    private void ToggleTooltip(bool show)
    {
        if (currentTooltip != null) Destroy(currentTooltip);

        Vector3 position = transform.position;
        position += Vector3.right * (transform.localScale.x + 0.1f);
        position += Vector3.back * (transform.localScale.z + 0.1f);
        currentTooltip = Instantiate(tooltipPrefab, position, Quaternion.identity);
        var tooltip = currentTooltip.GetComponent<FloorTooltip>();
        tooltip.Initialize(this);

        UIManager.RegisterTooltip(currentTooltip);
    }

    private void ShowContextMenu()
    {
        if (currentContextMenu != null) Destroy(currentContextMenu);

        Vector3 position = transform.position;
        position += Vector3.up * (transform.localScale.y + 0.1f);
        position += Vector3.back * (transform.localScale.z + 0.1f);
        currentContextMenu = Instantiate(contextMenuPrefab, position, Quaternion.identity);
        var ctx = currentContextMenu.GetComponent<FloorContextMenu>();
        ctx.Initialize(this);

        UIManager.RegisterContextMenu(currentContextMenu);
    }
}
