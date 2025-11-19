using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActivityMap : MonoBehaviour
{
    public static ActivityMap Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject tilePrefab;

    [Header("Visuals")]
    [SerializeField] private float lerpSpeed = 3f;
    [SerializeField] private Color minColor = Color.gray;
    [SerializeField] private Color maxColor = Color.red;
    [SerializeField] private float colorDecayPerSecond = 1f;
    [SerializeField] private float colorBoost = 0.5f;
    [SerializeField] private float tileGap = 0.01f;

    private readonly Dictionary<string, double> totalCalls = new();
    private readonly Dictionary<string, GameObject> tiles = new();
    private readonly Dictionary<string, Rect> rects = new();

    private readonly Dictionary<string, float> liveHeat = new();

    private HashSet<string> validMethods;

    private double globalMax;
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

    public void AddExecutionSample(ExecutionSample sample)
    {
        if (paused) return;

        foreach (var f in sample.Frames)
        {
            string key = $"{f.ClassName}.{f.Method}";
            if (!validMethods.Contains(key)) continue;

            totalCalls[key] = totalCalls.GetValueOrDefault(key, 0) + 1;
            if (totalCalls[key] > globalMax) globalMax = totalCalls[key];

            liveHeat[key] = Mathf.Clamp01(liveHeat.GetValueOrDefault(key, 0f) + colorBoost);
        }
    }

    private void Update()
    {
        if (paused || totalCalls.Count == 0) return;

        foreach (var key in liveHeat.Keys.ToList())
        {
            liveHeat[key] -= colorDecayPerSecond * Time.deltaTime;
            if (liveHeat[key] <= 0f)
                liveHeat.Remove(key);
        }

        var normalized = totalCalls.ToDictionary(kv => kv.Key, kv => (float)(kv.Value / globalMax));

        if (normalized.Count != rects.Count)
            SquarifiedTreemap(normalized);

        foreach (var (key, rect) in rects)
        {
            if (!tiles.TryGetValue(key, out var tileGO))
            {
                tileGO = Instantiate(tilePrefab, transform);
                tiles[key] = tileGO;

                tileGO.GetComponent<MethodActivityTile>().Initialize(
                    key,
                    minColor,
                    maxColor,
                    lerpSpeed,
                    tileGap
                );
            }

            float flash = liveHeat.GetValueOrDefault(key, 0f);
            double total = totalCalls[key];

            tileGO.GetComponent<MethodActivityTile>().SetVisual(
                rect,
                flash,
                (float) globalMax,
                total
            );
        }
    }

    // Compute layout with slice & dice method
    private void SliceAndDiceLayout(Dictionary<string, float> weights)
    {
        rects.Clear();
        float x = 0, y = 0, rowHeight = 0;
        foreach (var (key, value) in weights.OrderByDescending(kv => kv.Value))
        {
            float area = Mathf.Clamp(value, 0.0001f, 1f);
            float width = Mathf.Sqrt(area);
            float height = area / width;
            if (x + width > 1f)
            {
                x = 0;
                y += rowHeight;
                rowHeight = 0;
            }
            if (y + height > 1f)
            {
                height = 1f - y;
            }
            rects[key] = new(x, y, width, height);
            x += width;
            rowHeight = Mathf.Max(rowHeight, height);
        }
    }

    // Compute layout with squarified treemap method
    private void SquarifiedTreemap(Dictionary<string, float> weights)
    {
        rects.Clear();

        float sum = weights.Values.Sum();
        var sorted = weights.OrderByDescending(kv => kv.Value).ToList();

        LayoutRow(sorted, Vector2.zero, new(1f, 1f), sum);
    }

    private void LayoutRow(
        IList<KeyValuePair<string, float>> items,
        Vector2 origin,
        Vector2 size,
        float totalArea
    )
    {
        if (items.Count == 0) return;

        float totalWeight = items.Sum(i => i.Value);
        float scale = (size.x * size.y) / totalWeight;
        var areas = items.Select(i => i.Value * scale).ToList();

        List<(int index, float area)> row = new();
        int start = 0;
        Layout(items, areas, start, items.Count, origin, size, row);
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

        Vector2 remainingOrigin = horizontal
            ? new(
                origin.x + rowWidth,
                origin.y
            ) : new(
                origin.x,
                origin.y + rowHeight
            );
        Vector2 remainingSize = horizontal
            ? new(
                size.x - rowWidth,
                size.y
            ) : new(
                size.x,
                size.y - rowHeight
            );

        Layout(items, areas, start + row.Count, end, remainingOrigin, remainingSize, row);
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

    public void ResetAll()
    {
        foreach (var t in tiles.Values) Destroy(t);
        tiles.Clear();
        rects.Clear();
        totalCalls.Clear();
        globalMax = 0;
    }
}
