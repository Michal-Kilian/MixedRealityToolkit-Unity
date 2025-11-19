using MixedReality.Toolkit;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(LineRenderer))]
public class MethodActivityTile : MonoBehaviour
{
    [SerializeField] private float clickThreshold = 0.3f;
    [SerializeField] private GameObject tooltipPrefab;
    [SerializeField] private string tooltipOriginTag = "TooltipOrigin";
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.01f;

    private GameObject currentTooltip;
    private GameObject tooltipOrigin;
    private Transform targetFloorTransform;
    private bool isLineActive;

    private string methodKey;
    private string methodName;
    private MeshRenderer meshRenderer;
    private Color minColor = Color.gray;
    private Color maxColor = Color.red;
    private float lerpSpeed;
    private float tileGap = 0.002f;

    public string MethodKey => methodKey;
    public string MethodName => methodName;

    private MRTKBaseInteractable interactable;

    private float selectStartTime;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        tooltipOrigin = GameObject.FindGameObjectWithTag(tooltipOriginTag);
    }

    public void Initialize(
        string key,
        Color min,
        Color max,
        float lerpS,
        float tileG
    )
    {
        methodKey = key;
        methodName = key.Contains('.')
            ? key[(key.LastIndexOf('.') + 1)..]
            : key;
        minColor = min;
        maxColor = max;
        lerpSpeed = lerpS;
        tileGap = tileG;

        name = $"Method:{methodName}";

        interactable = gameObject.AddComponent<MRTKBaseInteractable>();
        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);
        interactable.selectEntered.AddListener(OnSelectEntered);
        interactable.selectExited.AddListener(OnSelectExited);

        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;
    }

    public void SetVisual(
        Rect rect,
        float flash,
        float globalMax,
        double totalCalls
    )
    {
        float gapWidth = Mathf.Max(rect.width - tileGap, 0f);
        float gapHeight = Mathf.Max(rect.height - tileGap, 0f);

        Vector3 targetPosition = new(
            rect.x + rect.width / 2f - 0.5f,
            0f,
            rect.y + rect.height / 2f - 0.5f
        );

        Vector3 targetScale = new(gapWidth, 0.01f, gapHeight);

        gameObject.transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            Time.deltaTime * lerpSpeed
        );
        gameObject.transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * lerpSpeed
        );

        Color current = meshRenderer.material.color;
        Color targetColor = Color.Lerp(minColor, maxColor, flash);
        meshRenderer.material.color = Color.Lerp(
            current,
            targetColor,
            Time.deltaTime * lerpSpeed
        );
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
        if (!show)
        {
            Destroy(currentTooltip);
            UIManager.UnregisterMethodTileTooltip(currentTooltip);
            return;
        }

        if (currentTooltip != null) Destroy(currentTooltip);

        currentTooltip = Instantiate(tooltipPrefab, tooltipOrigin.transform.position, Quaternion.identity);
        var tooltip = currentTooltip.GetComponent<MethodTileTooltip>();
        tooltip.Initialize(this);

        UIManager.RegisterMethodTileTooltip(currentTooltip);
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
            ViewInCity();
        }
    }

    public void ViewInCity()
    {
        if (ProjectCity.Instance == null) return;

        if (isLineActive)
        {
            lineRenderer.enabled = false;
            isLineActive = false;

            UIManager.UnregisterLineConnection(lineRenderer);
            return;
        }

        GameObject floor = ProjectCity.Instance.GetMethodFloor(MethodKey);
        if (floor == null) return;

        targetFloorTransform = floor.transform;
        isLineActive = true;

        lineRenderer.enabled = true;
        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 2;

        UpdateLineConnection();

        UIManager.RegisterLineConnection(lineRenderer);
    }

    private void LateUpdate()
    {
        if (isLineActive && targetFloorTransform != null)
        {
            UpdateLineConnection();
        }
    }

    private void UpdateLineConnection()
    {
        if (!lineRenderer.enabled) return;

        Vector3 start = transform.position;
        Vector3 end = targetFloorTransform.position;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    private void OnDestroy()
    {
        if (currentTooltip != null)
        {
            UIManager.UnregisterMethodTileTooltip(currentTooltip);
            Destroy(currentTooltip);
        }

        if (lineRenderer != null)
            UIManager.UnregisterLineConnection(lineRenderer);

        isLineActive = false;
        targetFloorTransform = null;
    }
}
