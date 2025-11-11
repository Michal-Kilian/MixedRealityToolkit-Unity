using System.Linq;
using UnityEngine;

public static class FlameGraphRenderer
{
    private static float layerHeight = 0.2f;
    private static float baseBlockDepth = 1.0f;
    private static float horizontalScale = 0.05f;

    public static void DrawFlameNode(
        FlameNode node,
        Transform parent,
        Vector3 origin,
        int maxSamples,
        GameObject cubePrefab,
        int depth = 0
    )
    {
        float width = Mathf.Max(0.5f, node.SampleCount + horizontalScale);
        float y = depth * layerHeight;
        Vector3 position = origin + new Vector3(width * 0.5f, y, 0f);

        GameObject cube = GameObject.Instantiate(cubePrefab, position, Quaternion.identity, parent);
        cube.transform.localScale = new(width, layerHeight, baseBlockDepth);

        var renderer = cube.GetComponent<Renderer>();
        renderer.material.color = HeatColor(node.SampleCount, maxSamples);

        var label = cube.AddComponent<TextMesh>();
        label.text = node.Method;
        label.characterSize = 0.02f;
        label.anchor = TextAnchor.MiddleCenter;
        label.transform.localPosition = new(0f, 0.15f, 0f);
        label.color = Color.white;

        //cube.AddComponent<Microsoft.MixedReality.Toolkit.UI.NearestInteractionTouchable>();

        float childOffsetX = 0f;
        foreach (var child in node.Children.Values.OrderByDescending(c => c.SampleCount))
        {
            Vector3 childOrigin = origin + new Vector3(childOffsetX, 0f, 0f);
            DrawFlameNode(child, parent, childOrigin, maxSamples, cubePrefab, depth + 1);
            childOffsetX += Mathf.Max(0.5f, child.SampleCount * horizontalScale);
        }
    }

    private static Color HeatColor(float count, float maxCount)
    {
        float t = Mathf.Clamp01(count / maxCount);
        return new Color(1f, 1f - t, 1f - t * 0.7f);
    }
}
