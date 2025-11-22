using MixedReality.Toolkit.SpatialManipulation;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProjectCity : MonoBehaviour
{
    public static ProjectCity Instance { get; private set; }

    [SerializeField] private bool UseDummyProjectStructure = false;

    [Header("City Settings")]
    [SerializeField] private float MaxSize = 1.0f;
    [SerializeField] private float MaxBuildingHeight = 0.5f;
    [SerializeField] private float BaseBuildingHeight = 0.01f;
    [SerializeField] private float DistrictPadding = 0.02f;
    [SerializeField] private float BaseDistrictHeight = 0.01f;
    [SerializeField] private float FloorGap = 0.002f;
    [SerializeField] private VisualizationModes VisualizationMode = VisualizationModes.Heatmap;

    [SerializeField] private GameObject floorPrefab;

    [SerializeField] private UIManager UIManager;

    public float CityTopHeight => MaxBuildingHeight + 1f;

    private ProjectStructure _project;
    private readonly Dictionary<string, GameObject> _classBuildings = new();
    private readonly Dictionary<string, GameObject> _methodFloors = new();
    private readonly Dictionary<GameObject, Color> _baseColors = new();
    private bool _builtOnce = false;

    private HashSet<string> userMethods;

    private FlashMode _flashMode;
    private HeatmapMode _heatmapMode;
    private IVisualizationMode _activeMode;

    private bool paused;

    public bool Paused
    {
        get => paused;
        set => paused = value;
    }

    private void Awake()
    {
        Instance = this;
        _flashMode = new(this);
        _heatmapMode = new();
    }

    private void Start()
    {
        if (UseDummyProjectStructure)
        {
            _project = ProjectStructure.CreateDummy();
            BuildCity(_project);
        }

        SetMode(VisualizationMode);
    }

    public void RebuildCity(ProjectStructure structure)
    {
        if (_project == structure && _builtOnce) return;

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        _classBuildings.Clear();
        _project = structure;
        _builtOnce = true;

        BuildCity(structure);

        Debug.Log($"[City] City built for {_project.ProjectName} with {_classBuildings.Values.Count} buildings");
    }

    private void BuildCity(ProjectStructure project)
    {
        userMethods = new();

        float maxRawHeight = CityHelpers.Instance.FindMaxRawBuildingHeight(project);

        var packages = project.Packages
            .Where(p => CityHelpers.Instance.HasAnyClassesRecursive(p))
            .ToList();

        if (packages == null || packages.Count == 0)
        {
            Debug.LogWarning("No packages/classes found in project.");
            return;
        }

        int cols = Mathf.CeilToInt(Mathf.Sqrt(packages.Count));
        int rows = Mathf.CeilToInt((float)packages.Count / cols);

        float availableWidth = MaxSize - DistrictPadding * (cols + 1);
        float availableDepth = MaxSize - DistrictPadding * (rows + 1);

        float cellW = availableWidth / cols;
        float cellD = availableDepth / rows;

        int index = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols && index < packages.Count; c++)
            {
                var pkg = packages[index];
                Vector3 pos = new(
                    -MaxSize / 2f + DistrictPadding + c * (cellW + DistrictPadding) + cellW / 2f,
                    0,
                    -MaxSize / 2f + DistrictPadding + r * (cellD + DistrictPadding) + cellD / 2f
                );

                BuildPackageRecursive(pkg, transform, pos, cellW, cellD, maxRawHeight);
                index++;
            }
        }

        _flashMode.Initialize(_methodFloors, _baseColors);
        _heatmapMode.Initialize(_methodFloors, _baseColors);

        ActivityMap.Instance.SetValidMethods(userMethods);
        FlameGraph.Instance.SetValidMethods(userMethods);
    }

    private void BuildPackageRecursive(PackageNode pkg, Transform parent, Vector3 localPos, float width, float depth, float maxRawHeight)
    {
        if (pkg == null) return;

        GameObject districtGO = new($"District_{pkg.Name}");
        districtGO.transform.SetParent(parent, false);
        districtGO.transform.localPosition = localPos;

        List<ClassNode> classes = pkg.Files?.SelectMany(f => f.Classes).ToList() ?? new List<ClassNode>();
        List<PackageNode> subs = pkg.SubPackages ?? new List<PackageNode>();

        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.transform.SetParent(districtGO.transform, false);
        block.transform.localScale = new(width, BaseDistrictHeight, depth);
        block.transform.localPosition = new(0, BaseDistrictHeight / 2f, 0);
        block.GetComponent<Renderer>().material.color = GetColorForPackage(pkg.Name) * 0.8f;

        int total = classes.Count + subs.Count;
        if (total == 0) return;

        int cols = Mathf.CeilToInt(Mathf.Sqrt(total));
        int rows = Mathf.CeilToInt((float)total / cols);

        float cellW = (width - DistrictPadding * (cols + 1)) / cols;
        float cellD = (depth - DistrictPadding * (rows + 1)) / rows;
        float startX = -width / 2f + DistrictPadding + cellW / 2f;
        float startZ = -depth / 2f + DistrictPadding + cellD / 2f;

        float heightScaleFactor = (maxRawHeight > 0.001f) ? MaxBuildingHeight / maxRawHeight : 0f;

        for (int i = 0; i < classes.Count; i++)
        {
            var cls = classes[i];
            int row = i / cols;
            int col = i % cols;

            float x = startX + col * (cellW + DistrictPadding);
            float z = startZ + row * (cellD + DistrictPadding);

            float footprint = Mathf.Max(0.05f, 0.02f + cls.FieldCount * 0.005f);
            float baseY = BaseDistrictHeight;

            GameObject classGO = new($"Class_{cls.Name}");
            classGO.transform.SetParent(districtGO.transform, false);
            classGO.transform.localPosition = new(x, 0, z);

            var methods = cls.Methods?.OrderByDescending(m => m.LineCount).ToList() ?? new List<MethodNode>();
            float currentHeight = baseY;

            float totalRawHeight = Mathf.Max(
                0.001f,
                cls.Methods.Sum(m => Mathf.Max(0.01f, m.LineCount * 0.001f))
            );
            
            float normalized = totalRawHeight / maxRawHeight;
            
            float buildingScale = Mathf.Sqrt(normalized);
            
            float totalScaledHeight = buildingScale * MaxBuildingHeight;
            
            float perUnitScale = totalScaledHeight / totalRawHeight;
            
            for (int j = 0; j < methods.Count; j++)
            {
                MethodNode method = methods[j];

                float rawFloorHeight = Mathf.Max(0.01f, method.LineCount * 0.001f);
                float scaledFloorHeight = rawFloorHeight * perUnitScale;

                Vector3 tooltipPosition = new(x, baseY + totalScaledHeight + 0.05f, z);

                GameObject floorGO = Instantiate(floorPrefab, classGO.transform);
                Floor floor = floorGO.GetComponent<Floor>();
                floor.Initialize(
                    path: method.Path,
                    line: method.LineStart,
                    packageName: pkg.Name,
                    className: cls.Name,
                    methodName: method.Name,
                    lineCount: method.LineCount,
                    footprint: footprint,
                    scaledHeight: scaledFloorHeight,
                    currentHeight: currentHeight,
                    floorGap: FloorGap,
                    tooltipPosition: tooltipPosition
                );

                Color baseColor = GetColorForPackage(pkg.Name);
                float brightness = Mathf.Lerp(0.6f, 1.2f, (float)j / methods.Count);
                Color floorColor = baseColor * brightness;
                
                var floorRenderer = floor.GetComponent<Renderer>();
                floorRenderer.material.color = floorColor;
                _baseColors[floorGO] = floorColor;

                string key = $"{pkg.Name}.{cls.Name}.{method.Name}";
                _methodFloors[key] = floorGO;
                userMethods.Add(key);

                currentHeight += scaledFloorHeight + FloorGap;
            }

            if (!string.IsNullOrEmpty(cls.ID))
                _classBuildings[cls.ID] = classGO;

            string keyName = $"{pkg.Name}.{cls.Name}";
            if (!_classBuildings.ContainsKey(keyName))
                _classBuildings[keyName] = classGO;
        }

        int offset = classes.Count;
        for (int i = 0; i < subs.Count; i++)
        {
            int row = (i + offset) / cols;
            int col = (i + offset) % cols;
            Vector3 subPos = new(
                startX + col * (cellW + DistrictPadding),
                BaseDistrictHeight,
                startZ + row * (cellD + DistrictPadding)
            );

            float subW = cellW * 0.9f;
            float subD = cellD * 0.9f;
            BuildPackageRecursive(subs[i], districtGO.transform, subPos, subW, subD, maxRawHeight);
        }
    }

    private Color GetColorForPackage(string packageName)
    {
        int hash = packageName.GetHashCode();
        Random.InitState(hash);
        return new Color(
            0.3f + Random.value * 0.5f,
            0.3f + Random.value * 0.5f,
            0.3f + Random.value * 0.5f
        );
    }

    public void OnExecutionSample(ExecutionSample sample)
    {
        if (paused) return;
        _activeMode?.OnExecutionSample(sample);
    }

    private void Update()
    {
        if (paused) return;
        _activeMode?.Update();
    }

    public void SetMode(VisualizationModes mode)
    {
        _activeMode?.ResetAll();

        _activeMode = mode switch
        {
            VisualizationModes.Flash => _flashMode,
            VisualizationModes.Heatmap => _heatmapMode,
            _ => null,
        };

        VisualizationMode = mode;
    }

    public GameObject GetMethodFloor(string methodKey)
    {
        if (_methodFloors.TryGetValue(methodKey, out GameObject floor))
        {
            return floor;
        }
        return null;
    }
}
