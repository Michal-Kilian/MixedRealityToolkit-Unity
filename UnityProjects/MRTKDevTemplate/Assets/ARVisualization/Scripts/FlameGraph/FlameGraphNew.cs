using System.Collections.Generic;
using UnityEngine;

public class FlameGraphNew : MonoBehaviour
{
    public static FlameGraphNew Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private float boxHeight = 0.05f;
    [SerializeField] private float boxDepth = 0.05f;
    [SerializeField] private float scale = 0.01f;

    private readonly FlameNodeNew root = new("Root");
    private readonly List<GameObject> nodes = new();
    private int poolIndex = 0;

    private HashSet<string> validMethods;

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
        if (paused || sample.Frames == null || sample.Frames.Count == 0) return;

        FlameNodeNew node = root;

        for (int i = sample.Frames.Count - 1; i >= 0; i--)
        {
            var frame = sample.Frames[i];
            string frameKey = $"{frame.ClassName}.{frame.Method}";
            if (!validMethods.Contains(frameKey)) continue;

            node = node.GetOrAddChild(frameKey);
            node.Count++;
        }

        UpdateGraph();
    }

    private void UpdateGraph()
    {
        poolIndex = 0;

        float totalWidth = ComputeWidth(root);
        Vector3 start = transform.position - new Vector3(totalWidth * 0.5f, 0, 0);
        DrawNode(root, start, 0, totalWidth);

        HideUnused();
    }

    private float DrawNode(FlameNodeNew node, Vector3 position, int depth, float parentWidth)
    {
        if (node == null || node.Children.Count == 0)
            return node.Count * scale;

        float xOffset = 0;
        foreach (var child in node.Children.Values)
        {
            float w = child.Count * scale;
            GameObject cube = GetCube();

            cube.transform.position = position + new Vector3(xOffset + w / 2f, depth * boxHeight, 0f);
            cube.transform.localScale = new(w, boxHeight, boxDepth);
            cube.GetComponent<Renderer>().material.color = ColorFromName(child.Name);
            cube.name = child.Name;

            DrawNode(child, position + new Vector3(xOffset, 0, 0), depth + 1, w);

            xOffset += w;
        }

        return parentWidth;
    }

    private float ComputeWidth(FlameNodeNew node)
    {
        float total = 0;
        foreach (var child in node.Children.Values)
            total += child.Count * scale;
        return Mathf.Max(total, 1f);
    }

    private GameObject GetCube()
    {
        if (poolIndex < nodes.Count)
        {
            var obj = nodes[poolIndex++];
            obj.SetActive(true);
            return obj;
        }

        var newObj = Instantiate(cubePrefab, transform);
        nodes.Add(newObj);
        poolIndex++;
        return newObj;
    }

    private void HideUnused()
    {
        for (int i = poolIndex; i < nodes.Count; i++)
            nodes[i].SetActive(false);
    }

    private Color ColorFromName(string name)
    {
        int hash = name.GetHashCode();
        Random.InitState(hash);
        return new Color(Random.value * 0.6f + 0.4f, Random.value * 0.4f + 0.4f, Random.value * 0.3f + 0.3f);
    }

    public void Clear()
    {
        root.Children.Clear();
        poolIndex = 0;
        HideUnused();
    }
}
