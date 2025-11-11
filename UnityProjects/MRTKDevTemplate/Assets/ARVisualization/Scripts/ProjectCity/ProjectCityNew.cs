using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProjectCityNew : MonoBehaviour
{
    public static ProjectCityNew Instance { get; private set; }

    [Header("Data Source")]
    [SerializeField] private bool useDummyProjectStructure = false;

    [SerializeField] private ProjectStructure project;

    [Header("Prefabs")]
    [SerializeField] private GameObject districtPrefab;
    [SerializeField] private GameObject buildingPrefab;

    [Header("Scales and Spacing")]
    [Tooltip("Minimum side length (XZ) for any building footprint")]
    [SerializeField] private float minBuildingSide = 1.0f;

    [Tooltip("Extra side length per field in a class")]
    [SerializeField] private float sidePerField = 0.15f;

    [Tooltip("Minimum per-floor height (Y)")]
    [SerializeField] private float minFloorHeight = 0.15f;

    [Tooltip("Height scale per line of code for methods")]
    [SerializeField] private float heightPerLOC = 0.02f;

    [Tooltip("Vertical gap between method floors")]
    [SerializeField] private float floorGap = 0.02f;

    [Tooltip("Empty space between siblings inside a district")]
    [SerializeField] private float buildingSpacing = 0.4f;

    [Tooltip("Padding between content and district pedestal edges on X and Z")]
    [SerializeField] private float districtPadding = 0.5f;

    [Tooltip("Space between top-level districts")]
    [SerializeField] private float districtSpacing = 1.0f;

    [Tooltip("Pedestal thickness (Y) for districts")]
    [SerializeField] private float districtPedestalHeight = 0.2f;

    [Tooltip("Minimum district pedestal side (XZ)")]
    [SerializeField] private float minDistrictSide = 1.0f;

    private bool builtOnce;
    private readonly List<Building> classBuildings = new();

    public float MinBuildingSide => minBuildingSide;
    public float SidePerField => sidePerField;
    public float MinFloorHeight => minFloorHeight;
    public float HeightPerLOC => heightPerLOC;
    public float FloorGap => floorGap;
    public float BuildingSpacing => buildingSpacing;
    public float DistrictPadding => districtPadding;
    public float DistrictSpacing => districtSpacing;
    public float DistrictPedestalHeight => districtPedestalHeight;
    public float MinDistrictSide => minDistrictSide;
    public GameObject DistrictPrefab => districtPrefab;
    public GameObject BuildingPrefab => buildingPrefab;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (useDummyProjectStructure)
        {
            project = ProjectStructure.CreateDummy();
            BuildCity(project);
        }
    }

    public void RebuildCity(ProjectStructure structure)
    {
        if (project == structure && builtOnce)
            return;

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        classBuildings.Clear();
        project = structure;
        builtOnce = true;

        BuildCity(structure);

        Debug.Log($"[City] City built for {project.ProjectName}");
        Debug.Log($"[City] Built {classBuildings.Count} class buildings across all {structure.Packages.Count} packages");
    }

    private void BuildCity(ProjectStructure structure)
    {
        var topDistricts = new List<District>();
        foreach (var pkg in structure.Packages)
        {
            var district = District.Create(pkg, this, transform);
            classBuildings.AddRange(district.EnumerateAllBuildings());
            topDistricts.Add(district);
        }

        var items = topDistricts.Select(d => new LayoutItem
        {
            transform = d.transform,
            size = d.Footprint,
            pedestalHeight = d.PedestalHeight
        }).ToList();

        var totalArea = items.Aggregate(
            0f,
            (a, it) => a + it.size.x * it.size.y
        );
        var targetRowWidth = Mathf.Sqrt(totalArea);
        if (targetRowWidth < minDistrictSide)
            targetRowWidth = minDistrictSide;

        var packed = ShelfPack(items, targetRowWidth, districtSpacing);

        var cityWidth = packed.bounds.x;
        var cityDepth = packed.bounds.y;

        foreach (var placed in packed.placed)
        {
            var centerX = -cityWidth * 0.5f + placed.position.x + placed.size.x * 0.5f;
            var centerZ = -cityDepth * 0.5f + placed.position.y + placed.size.y * 0.5f;
            var y = placed.pedestalHeight * 0.5f;

            placed.transform.localPosition = new(centerX, y, centerZ);
        }
    }

    private static PackedResult ShelfPack(
        List<LayoutItem> items,
        float targetRowWidth,
        float spacing
    )
    {
        var placed = new List<PlacedItem>();
        float rowX = 0, rowZ = 0, rowHeight = 0, maxRowWidth = 0;

        foreach (var it in items)
        {
            var nextX = rowX == 0 ? it.size.x : rowX + spacing + it.size.x;
            if (nextX > targetRowWidth && rowX > 0)
            {
                maxRowWidth = MathF.Max(maxRowWidth, rowX);
                rowZ += rowHeight + spacing;
                rowX = 0;
                rowHeight = 0;
            }

            var placedX = rowX == 0 ? 0 : rowX + spacing;
            placed.Add(new PlacedItem
                {
                    transform = it.transform,
                    size = it.size,
                    position = new Vector2(placedX, rowZ),
                    pedestalHeight = it.pedestalHeight,
                }
            );

            rowX = placedX + it.size.x;
            rowHeight = Mathf.Max(rowHeight, it.size.y);
        }

        maxRowWidth = Mathf.Max(maxRowWidth, rowX);
        var totalDepth = rowZ + rowHeight;
        return new PackedResult
        {
            placed = placed,
            bounds = new Vector2(maxRowWidth, totalDepth),
        };
    }

    public void OnExecutionSample(ExecutionSample sample)
    {

    }

    private struct LayoutItem
    {
        public Transform transform;
        public Vector2 size;
        public float pedestalHeight;
    }

    private struct PlacedItem
    {
        public Transform transform;
        public Vector2 size;
        public Vector2 position;
        public float pedestalHeight;
    }

    private struct PackedResult
    {
        public List<PlacedItem> placed;
        public Vector2 bounds;
    }
}
