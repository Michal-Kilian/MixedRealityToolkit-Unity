using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlameGraph : MonoBehaviour
{
    public static FlameGraph Instance { get; private set; }

    [Header("Layout Settings")]
    [SerializeField] private GameObject barPrefab;
    [SerializeField] private float graphWidth = 3f;
    [SerializeField] private float layerHeight = 0.05f;
    [SerializeField] private float verticalSpacing = 0.003f;
    [SerializeField] private float horizontalSpacing = 0.002f;

    [Header("Visuals")]
    [SerializeField] private Color minColor = Color.yellow;
    [SerializeField] private Color maxColor = new(1f, 0.25f, 0f);
    [SerializeField] private float lerpSpeed = 8f;

    private readonly FlameNode root = new("ROOT");
    private HashSet<string> validMethods;

    private int maxDepth = 1;
    private float totalSamples = 1f;

    private void Awake() => Instance = this;

    public void SetValidMethods(IEnumerable<string> methods)
    {
        validMethods = new(methods);
    }

    public void OnExecutionSample(ExecutionSample sample)
    {
        FlameNode node = root;

        for (int i = sample.Frames.Count - 1; i >= 0; i--)
        {
            string name = $"{sample.Frames[i].ClassName}.{sample.Frames[i].Method}";
            if (validMethods != null && !validMethods.Contains(name))
                continue;

            node = node.GetOrAdd(name);
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

        DrawNode(root, startX, totalWidth, depth: 0);
    }

    private void DrawNode(FlameNode node, float startX, float width, int depth)
    {
        if (node.children.Count == 0)
            return;

        float nodeTotal = node.children.Values.Sum(c => c.sampleCount);
        float y = depth * (layerHeight + verticalSpacing);
        float cursor = startX;

        foreach (var child in node.children.Values.OrderBy(c => c.name))
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
                child.bar.Initialize(child.name, minColor, maxColor, lerpSpeed);
            }

            Vector3 targetPos = new(cx, y, 0);
            Vector3 targetScale = new(childWidth, layerHeight, 0.02f);
            float intensity = Mathf.Clamp01(child.sampleCount / totalSamples);

            child.bar.SetTarget(targetPos, targetScale, intensity);

            DrawNode(child, cx - childWidth * 0.5f, childWidth, depth + 1);
        }
    }
}
