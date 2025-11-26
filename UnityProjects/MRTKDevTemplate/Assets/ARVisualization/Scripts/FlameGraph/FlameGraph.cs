using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlameGraph : MonoBehaviour
{
    public static FlameGraph Instance { get; private set; }

    [Header("Layout Settings")]
    [SerializeField] private GameObject barPrefab;
    [SerializeField] private float graphWidth = 3f;
    [SerializeField] private float maxGraphHeight = 5f;
    [SerializeField] private float layerHeight = 0.05f;
    [SerializeField] private float verticalSpacing = 0.003f;
    [SerializeField] private float horizontalSpacing = 0.002f;

    [Header("Visuals")]
    [SerializeField] private Color userMinColor = Color.yellow;
    [SerializeField] private Color userMaxColor = new(1f, 0.25f, 0f);
    [SerializeField] private Color systemMinColor = new(0.25f, 0.5f, 1f);
    [SerializeField] private Color systemMaxColor = new(0f, 0.75f, 1f);
    [SerializeField] private float lerpSpeed = 8f;

    //[SerializeField] private bool showSystemMethods = false;
    /*public bool ShowSystemMethods
    {
        get => showSystemMethods;
        set
        {
            if (showSystemMethods == value) return;
            showSystemMethods = value;
            LayoutGraph();
        }
    }*/

    private readonly FlameNode root = new("ROOT");
    private HashSet<string> validMethods;

    private int maxDepth = 1;
    private float totalSamples = 1f;

    private bool paused;

    public bool Paused
    {
        get => paused;
        set => paused = value;
    }

    private void Awake() => Instance = this;

    public void SetValidMethods(IEnumerable<string> methods)
    {
        validMethods = new(methods);
    }

    public void OnExecutionSample(ExecutionSample sample)
    {
        if (paused) return;

        FlameNode node = root;

        for (int i = sample.Frames.Count - 1; i >= 0; i--)
        {
            string name = $"{sample.Frames[i].ClassName}.{sample.Frames[i].Method}";
            bool userDefined = validMethods == null || validMethods.Contains(name);

            node = node.GetOrAdd(name, userDefined);
            node.sampleCount++;
        }

        root.sampleCount = Mathf.Max(root.children.Values.Sum(c => c.sampleCount), 1);
        totalSamples = root.sampleCount;
        maxDepth = Mathf.Max(maxDepth, GetMaxDepth(root, 0));

        LayoutGraph();
    }

    private int GetMaxDepth(FlameNode node, int depth)
    {
        int max = depth;
        foreach (var child in node.children.Values)
            max = Mathf.Max(max, GetMaxDepth(child, depth + 1));
        return max;
    }

    private void LayoutGraph()
    {
        if (root.children.Count == 0)
            return;

        float totalWidth = graphWidth;
        float startX = -graphWidth * 0.5f;

        float naturalHeight = maxDepth * (layerHeight + verticalSpacing);
        float verticalScale = 1f;
        if (naturalHeight > maxGraphHeight)
            verticalScale = maxGraphHeight / naturalHeight;

        DrawNode(root, startX, totalWidth, 0, verticalScale);
    }

    private void DrawNode(FlameNode node, float startX, float width, int depth, float vScale)
    {
        if (node.children.Count == 0)
            return;

        float nodeTotal = node.children.Values
            //.Where(c => showSystemMethods || c.userDefined)
            .Sum(c => c.sampleCount);

        float scaledLayerHeight = layerHeight * vScale;
        float scaledSpacing = verticalSpacing * vScale;
        float y = depth * (scaledLayerHeight + scaledSpacing);

        float cursor = startX;

        foreach (
            var child in node.children.Values
                //.Where(c => showSystemMethods || c.userDefined)
                .OrderBy(c => c.name)
        )
        {
            float ratio = nodeTotal > 0 ? (float)child.sampleCount / nodeTotal : 0f;
            float childWidth = width * ratio - horizontalSpacing;
            childWidth = Mathf.Max(childWidth, 0.001f);

            float cx = cursor + childWidth * 0.5f;
            cursor += childWidth + horizontalSpacing;

            if (child.bar == null)
            {
                GameObject go = Instantiate(barPrefab, transform);
                child.bar = go.GetComponent<FlameBar>();
                var minColor = child.userDefined ? userMinColor : systemMinColor;
                var maxColor = child.userDefined ? userMaxColor : systemMaxColor;
                child.bar.Initialize(child.name, minColor, maxColor, lerpSpeed);
            }

            Vector3 targetPos = new(cx, y, 0);
            Vector3 targetScale = new(childWidth, scaledLayerHeight, 0.02f);
            float intensity = Mathf.Clamp01(child.sampleCount / totalSamples);

            /*if (!child.userDefined && !showSystemMethods)
            {
                if (child.bar) child.bar.gameObject.SetActive(false);
                continue;
            }*/

            if (child.bar)
            {
                child.bar.gameObject.SetActive(true);
                child.bar.SetTarget(targetPos, targetScale, intensity);
            }

            DrawNode(child, cx - childWidth * 0.5f, childWidth, depth + 1, vScale);
        }
    }
}
