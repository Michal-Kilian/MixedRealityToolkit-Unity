using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlameActivityMap : MonoBehaviour
{
    public static FlameActivityMap Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject tilePrefab;

    [Header("Visuals")]
    [SerializeField] private Color minColor = Color.gray;
    [SerializeField] private Color maxColor = Color.red;
    [SerializeField] private float lerpSpeed = 3f;
    [SerializeField] private float colorDecayPerSecond = 1f;
    [SerializeField] private float colorBoost = 0.5f;
    [SerializeField] private float tileGap = 0.01f;

    private readonly Dictionary<string, FlameNode> nodes = new();
    private readonly Dictionary<string, GameObject> tiles = new();
    private readonly Dictionary<string, Rect> rects = new();
    private readonly Dictionary<string, float> liveHeat = new();

    private FlameNode parentNode;
    private int depthIndex = 0;

    public Action<FlameNode, int> OnTileSelected;

    public void Setup(FlameNode node, int depth)
    {
        parentNode = node;
        depthIndex = depth;
    }

    private bool paused;

    public bool Paused
    {
        get => paused;
        set => paused = value;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (paused || FlameGraph.Instance == null)
            return;

        FlameNode root = parentNode ?? FlameGraph.Instance.GetRootNode();
        if (root == null || root.children.Count == 0)
            return;

        var weights = new Dictionary<string, float>();
        float total = root.children.Values.Sum(c => (float)c.sampleCount);
        foreach (var kv in root.children)
        {
            string key = kv.Key;
            FlameNode node = kv.Value;
            float ratio = total > 0f ? (float)node.sampleCount / total : 0f;
            weights[key] = ratio;
            nodes[key] = node;
        }

        foreach (var key in liveHeat.Keys.ToList())
        {
            liveHeat[key] -= colorDecayPerSecond * Time.deltaTime;
            if (liveHeat[key] <= 0f) liveHeat.Remove(key);
        }

        foreach (var kv in nodes)
        {
            string key = kv.Key;
            var node = kv.Value;
            liveHeat[key] = Mathf.Clamp01(liveHeat.GetValueOrDefault(key, 0f) + colorBoost);
        }

        if (weights.Count != rects.Count)
            rects.Clear();

        SquarifiedTreemap(weights);

        foreach (var (key, rect) in rects)
        {
            if (!tiles.TryGetValue(key, out var tileGO))
            {
                tileGO = Instantiate(tilePrefab, transform);
                tileGO.name = $"FlameTile:{key}";
                tiles[key] = tileGO;

                tileGO.GetComponent<MethodFlameActivityTile>().Initialize(
                    key,
                    minColor,
                    maxColor,
                    lerpSpeed,
                    tileGap,
                    this,
                    nodes[key],
                    depthIndex
                );
            }

            float flash = liveHeat.GetValueOrDefault(key, 0f);
            tileGO.GetComponent<MethodFlameActivityTile>().SetVisual(rect, flash);
        }
    }

    private void SquarifiedTreemap(Dictionary<string, float> weights)
    {
        rects.Clear();

        float sum = weights.Values.Sum();
        var sorted = weights.OrderByDescending(kv => kv.Value).ToList();
        LayoutRow(sorted, Vector2.zero, new(1f, 1f));
    }

    private void LayoutRow(
        IList<KeyValuePair<string, float>> items,
        Vector2 origin,
        Vector2 size
    )
    {
        if (items.Count == 0) return;

        float totalWeight = items.Sum(i => i.Value);
        float scale = (size.x * size.y) / totalWeight;
        var areas = items.Select(i => i.Value * scale).ToList();

        List<(int index, float area)> row = new();
        Layout(items, areas, 0, items.Count, origin, size, row);
    }

    private void Layout(
        IList<KeyValuePair<string, float>> items,
        IList<float> areas,
        int start,
        int end,
        Vector2 origin,
        Vector2 size,
        List<(int index, float area)> row
    )
    {
        if (start >= end) return;

        float width = Mathf.Min(size.x, size.y);
        row.Clear();
        float x = origin.x, y = origin.y;
        float rowArea = 0f;

        int i = start;
        while (i < end)
        {
            row.Add((i, areas[i]));
            rowArea += areas[i];

            float aspectBefore = Worst(row, rowArea, width);
            float aspectAfter = Worst(row, rowArea, width);

            if (i + 1 < end)
            {
                float nextRowArea = rowArea + areas[i + 1];
                if (Worst(row.Append((i + 1, areas[i + 1])).ToList(), nextRowArea, width) > aspectBefore)
                    break;
            }
            i++;
        }

        float rowSum = row.Sum(r => r.area);
        bool horizontal = size.x >= size.y;

        float rowWidth = horizontal ? rowSum / size.y : size.x;
        float rowHeight = horizontal ? size.y : rowSum / size.x;

        foreach (var (index, area) in row)
        {
            float tW = horizontal ? rowWidth : area / rowHeight;
            float tH = horizontal ? area / rowWidth : rowHeight;
            rects[items[index].Key] = new(x, y, tW, tH);
            if (horizontal)
                y += tH;
            else
                x += tW;
        }

        Vector2 remainOrigin = horizontal ? new(origin.x + rowWidth, origin.y) : new(origin.x, origin.y + rowHeight);
        Vector2 remainSize = horizontal ? new(size.x - rowWidth, size.y) : new(size.x, size.y - rowHeight);

        Layout(items, areas, start + row.Count, end, remainOrigin, remainSize, row);
    }

    private float Worst(List<(int index, float area)> row, float rowArea, float side)
    {
        float s2 = side * side;
        float rMin = float.MaxValue;
        float rMax = float.MinValue;
        foreach (var (_, a) in row)
        {
            float r = s2 * a / (rowArea * rowArea);
            rMin = Mathf.Min(rMin, r);
            rMax = Mathf.Max(rMax, r);
        }
        return Mathf.Max(rMax / rMin, rMin / rMax);
    }
}
