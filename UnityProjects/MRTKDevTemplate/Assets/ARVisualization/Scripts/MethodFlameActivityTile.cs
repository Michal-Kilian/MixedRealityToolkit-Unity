using MixedReality.Toolkit;
using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(LineRenderer))]
public class MethodFlameActivityTile : MonoBehaviour
{
    [SerializeField] private GameObject tooltipPrefab;
    [SerializeField] private string tooltipOriginTag = "ActivityMapTooltipOrigin";

    private GameObject currentTooltip;
    private GameObject tooltipOrigin;

    private string methodKey;
    private string methodName;
    private MeshRenderer meshRenderer;
    private Color minColor = Color.gray;
    private Color maxColor = Color.red;
    private float lerpSpeed;
    private float tileGap = 0.002f;

    private FlameActivityMap flameActivityMap;
    private FlameNode flameNode;
    private int depth;

    private FlameActivityMap childLayer;

    public string MethodKey => methodKey;
    public string MethodName => methodName;

    private MRTKBaseInteractable interactable;

    public static MethodFlameActivityTile CurrentlyClicked {  get; private set; }
    public void RegisterChildLayer(FlameActivityMap layer)
    {
        childLayer = layer;
    }

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
        float tileG,
        FlameActivityMap map,
        FlameNode node,
        int d
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
        flameActivityMap = map;
        flameNode = node;
        depth = d;

        name = $"Method:{methodName}";

        interactable = gameObject.AddComponent<MRTKBaseInteractable>();
        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);
        interactable.selectEntered.AddListener(OnSelectEntered);
    }

    public void SetVisual(
        Rect rect,
        float flash
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
        var tooltip = currentTooltip.GetComponent<MethodFlameTileTooltip>();
        tooltip.Initialize(this);

        UIManager.RegisterMethodTileTooltip(currentTooltip);
    }

    private void OnSelectEntered(SelectEnterEventArgs arg0)
    {
        NextLayer();
    }

    private void NextLayer()
    {
        CurrentlyClicked = this;
        flameActivityMap.OnTileSelected?.Invoke(flameNode, depth);
        CurrentlyClicked = null;
    }

    private void OnDestroy()
    {
        if (currentTooltip != null)
        {
            UIManager.UnregisterMethodTileTooltip(currentTooltip);
            Destroy(currentTooltip);
        }
    }
}
