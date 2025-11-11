using System.Collections.Generic;
using UnityEngine;

public class FlameGraphManager : MonoBehaviour
{
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private Transform flameGraphRoot;
    [SerializeField] private float maxWidth = 10f;
    [SerializeField] private float maxDepth = 10f;
    [SerializeField] private float layerHeight = 0.15f;
    [SerializeField] private float fadeSeconds = 5f;

    private readonly Dictionary<string, FlameNode> threadRoots = new();

    private void Awake() => Application.runInBackground = true;

    public void AddSample(ExecutionSample sample)
    {
        if (sample.Frames == null || sample.Frames.Count == 0)
            return;

        float threadZ = threadRoots.Count * (maxDepth / 4f);

        if (!threadRoots.TryGetValue(sample.ThreadName, out var root))
        {
            root = new FlameNode
            {
                Key = sample.ThreadName,
                Method = sample.ThreadName,
                Depth = 0
            };
            threadRoots[sample.ThreadName] = root;

            root.Cube = Instantiate(cubePrefab, flameGraphRoot);
            root.Cube.name = $"Thread:{sample.ThreadName}";
            root.Cube.transform.localScale = new(maxWidth, layerHeight, maxDepth);
            root.Cube.GetComponent<Renderer>().material.color = Color.gray;
        }

        UpdateNodeHierarchy(root, sample.Frames, sample.ThreadName, threadZ);
    }

    private void UpdateNodeHierarchy(FlameNode root, List<ExecutionSample.Frame> frames, string thread, float threadZ)
    {
        FlameNode parent = root;
        float yOffset = layerHeight;
        float currentWidth = maxWidth;

        for (int i = 0; i < frames.Count; i++)
        {
            var f = frames[i];
            string key = $"{f.ClassName}.{f.Method}:{f.Line}";

            if (!parent.Children.TryGetValue(key, out var node))
            {
                node = new FlameNode
                {
                    Key = key,
                    Method = f.Method,
                    Depth = parent.Depth + 1,
                    NormalizedWidth = 1f,
                    Cube = Instantiate(cubePrefab, flameGraphRoot)
                };
                parent.Children[key] = node;

                float frac = 1f / (parent.Children.Count + 1);
                float usedX = parent.Children.Count * frac * currentWidth - currentWidth / 2f;
                Vector3 pos = new(usedX, yOffset * node.Depth, threadZ);
                node.Cube.transform.localScale = new Vector3(currentWidth * frac, layerHeight, maxDepth / 4f);
                node.Cube.transform.localPosition = pos;
                node.Cube.name = node.Method;
                node.Cube.GetComponent<Renderer>().material.color = Color.black;
            }

            node.SampleCount++;
            node.LastUpdatedTime = Time.time;

            parent = node;
        }
    }

    private void Update()
    {
        foreach (var t in threadRoots.Values)
            UpdateHeatRecursive(t);
    }

    private void UpdateHeatRecursive(FlameNode node)
    {
        if (node.Cube != null)
        {
            float since = Time.time - node.LastUpdatedTime;
            float heat = Mathf.Clamp01(1f - since / fadeSeconds);
            float brightness = Mathf.Lerp(0.1f, 1f, heat);
            var r = node.Cube.GetComponent<Renderer>();
            r.material.color = HeatColor(node.SampleCount, brightness);
        }

        foreach (var c in node.Children.Values)
            UpdateHeatRecursive(c);
    }

    private Color HeatColor(int samples, float intensity)
    {
        float t = Mathf.Clamp01(Mathf.Log10(1 + samples) / 3f);
        Color baseColor = Color.Lerp(new Color(0.2f, 0.3f, 0.9f), new Color(1f, 0.2f, 0.1f), t);
        return baseColor * intensity;
    }
}
