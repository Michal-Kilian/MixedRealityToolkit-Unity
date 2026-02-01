using System.Collections.Generic;
using UnityEngine;

public class FlameActivityMapManager : MonoBehaviour
{
    public static FlameActivityMapManager Instance { get; private set; }

    [SerializeField] private GameObject activityMapPrefab;
    [SerializeField] private float layerZOffset = 0.15f;

    private readonly List<FlameActivityMap> layers = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        var root = FlameGraph.Instance != null ? FlameGraph.Instance.GetRootNode() : null;
        if (root == null)
        {
            Debug.LogWarning("FlameActivityMapManager: No root node found in FlameGraph.");
            return;
        }

        SpawnNewLayer(
            root,
            0,
            transform,
            Vector3.zero,
            Vector3.one
        );
    }

    public FlameActivityMap SpawnNewLayer(
    FlameNode node, int depth,
    Transform parent, Vector3 localPos, Vector3 localScale)
    {
        for (int i = layers.Count - 1; i >= depth; i--)
        {
            Destroy(layers[i].gameObject);
            layers.RemoveAt(i);
        }

        GameObject newLayerGO = Instantiate(activityMapPrefab, parent);
        newLayerGO.name = $"FlameActivityLayer_{depth}";

        float zForward = layerZOffset;
        newLayerGO.transform.localPosition = new Vector3(
            localPos.x,
            localPos.y,
            localPos.z + zForward
        );
        newLayerGO.transform.localRotation = Quaternion.identity;
        newLayerGO.transform.localScale = localScale;

        var map = newLayerGO.GetComponent<FlameActivityMap>();
        map.Setup(node, depth);
        map.OnTileSelected += HandleTileSelected;

        layers.Add(map);
        return map;
    }

    private void HandleTileSelected(FlameNode node, int currentDepth)
    {
        if (node == null || node.children.Count == 0) return;

        MethodFlameActivityTile source = MethodFlameActivityTile.CurrentlyClicked;
        if (source == null)
        {
            Debug.LogWarning("No source tile found for click!");
            return;
        }

        FlameActivityMap childLayer = SpawnNewLayer(
            node,
            currentDepth + 1,
            source.transform,
            Vector3.zero,
            Vector3.one
        );
        source.RegisterChildLayer(childLayer);
    }
}
