using MixedReality.Toolkit;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Renderer), typeof(LineRenderer))]
public class FlameBar : MonoBehaviour
{
    [SerializeField] private float clickThreshold = 0.3f;
    [SerializeField] private GameObject tooltipPrefab;
    [SerializeField] private string tooltipOriginTag = "FlameGraphTooltipOrigin";
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.01f;

    private GameObject currentTooltip;
    private GameObject tooltipOrigin;
    private Transform targetMethodCallTransform;
    private bool isLineActive;

    private string methodKey;
    private Renderer meshRenderer;
    private Vector3 targetScale;
    private Vector3 targetPosition;
    private Color targetColor;

    public string MethodKey => methodKey;

    private float lerpSpeed;
    private Color minColor, maxColor;

    private MRTKBaseInteractable interactable;

    private float selectStartTime;

    private void Awake()
    {
        meshRenderer = GetComponent<Renderer>();
        tooltipOrigin = GameObject.FindGameObjectWithTag(tooltipOriginTag);
    }

    public void Initialize(
        string key,
        Color min,
        Color max,
        float speed
    )
    {
        methodKey = key;
        minColor = min;
        maxColor = max;
        lerpSpeed = speed;

        name = $"Call:{methodKey}";

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

    public void SetTarget(Vector3 position, Vector3 scale, float intensity)
    {
        targetPosition = position;
        targetScale = scale;

        /*int hash = name.GetHashCode();
        float hue = (hash & 0xFFFFFF) / (float)0xFFFFFF;
        Color baseColor = Color.HSVToRGB(hue, 0.7f, 1f);
        targetColor = Color.Lerp(baseColor * 0.5f, baseColor, Mathf.Clamp01(intensity));
        */
        targetColor = Color.Lerp(minColor, maxColor, intensity);
    }

    private void Update()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * lerpSpeed);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * lerpSpeed);
        meshRenderer.material.color = Color.Lerp(meshRenderer.material.color, targetColor, Time.deltaTime * lerpSpeed);
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
            UIManager.UnregisterFlameBarTooltip(currentTooltip);
            return;
        }

        if (currentTooltip != null) Destroy(currentTooltip);

        currentTooltip = Instantiate(tooltipPrefab, tooltipOrigin.transform.position, Quaternion.identity);
        var tooltip = currentTooltip.GetComponent<FlameBarTooltip>();
        tooltip.Initialize(this);

        UIManager.RegisterFlameBarTooltip(currentTooltip);
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

            UIManager.UnregisterFlameLineConnection(lineRenderer);
            return;
        }

        GameObject floor = ProjectCity.Instance.GetMethodFloor(methodKey);
        if (floor == null) return;

        targetMethodCallTransform = floor.transform;
        isLineActive = true;

        lineRenderer.enabled = true;
        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 2;

        UpdateLineConnection();

        UIManager.RegisterFlameLineConnection(lineRenderer);
    }

    private void LateUpdate()
    {
        if (isLineActive && targetMethodCallTransform != null)
        {
            UpdateLineConnection();
        }
    }

    private void UpdateLineConnection()
    {
        if (!lineRenderer.enabled) return;

        Vector3 start = transform.position;
        Vector3 end = targetMethodCallTransform.position;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    private void OnDestroy()
    {
        if (currentTooltip != null)
        {
            UIManager.UnregisterFlameBarTooltip(currentTooltip);
            Destroy(currentTooltip);
        }

        if (lineRenderer != null)
            UIManager.UnregisterFlameLineConnection(lineRenderer);

        isLineActive = false;
        targetMethodCallTransform = null;
    }
}
