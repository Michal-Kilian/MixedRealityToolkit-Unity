using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProjectCity : MonoBehaviour
{
    public static ProjectCity Instance { get; private set; }

    [SerializeField] private bool useDummyProjectStructure = false;

    public float MaxSize = 1.0f;
    public float BaseBuildingHeight = 0.01f;
    public float DistrictPadding = 0.02f;
    public float BaseDistrictHeight = 0.01f;

    private ProjectStructure _project;
    private readonly Dictionary<string, GameObject> _classBuildings = new();

    private void Awake()
    {
       Instance = this;
    }

    private void Start()
    {
        if (useDummyProjectStructure)
        {
            _project = ProjectStructure.CreateDummy();
            BuildCity(_project);
        }
    }

    public void RebuildCity(ProjectStructure structure)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        _classBuildings.Clear();
        _project = structure;
        BuildCity(structure);
    }

    private void BuildCity(ProjectStructure project)
    {
        var packages = project.Packages
            .Where(p => p.Files.Any(f => f.Classes.Any()))
            .ToList();

        if (packages.Count == 0)
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

                BuildDistrict(pkg, pos, cellW, cellD);
                index++;
            }
        }
    }

    private void BuildDistrict(PackageNode package, Vector3 center, float maxWidth, float maxDepth)
    {
        var districtGO = new GameObject($"District_{(string.IsNullOrEmpty(package.Name) ? "Unnamed" : package.Name)}");
        districtGO.transform.SetParent(transform, false);
        districtGO.transform.localPosition = center;

        List<ClassNode> allClasses = package.Files.SelectMany(f => f.Classes).ToList();
        List<PackageNode> subPackages = package.SubPackages ?? new List<PackageNode>();

        if (allClasses.Count == 0 && subPackages.Count == 0) return;

        float buildingFootprint = 0.1f;
        int cols = Mathf.CeilToInt(Mathf.Sqrt(Mathf.Max(1, allClasses.Count + subPackages.Count)));
        int rows = Mathf.CeilToInt((float)(allClasses.Count + subPackages.Count) / cols);

        float neededWidth = Mathf.Min(maxWidth, cols * buildingFootprint + DistrictPadding * (cols + 1));
        float neededDepth = Mathf.Min(maxDepth, rows * buildingFootprint + DistrictPadding * (rows + 1));

        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.transform.SetParent(districtGO.transform, false);
        block.transform.localScale = new Vector3(neededWidth, BaseDistrictHeight, neededDepth);
        block.transform.localPosition = new Vector3(0, BaseDistrictHeight / 2f, 0);

        var rmat = block.GetComponent<Renderer>();
        rmat.material.color = GetColorForPackage(package.Name) * 0.8f;

        float halfW = neededWidth / 2f;
        float halfD = neededDepth / 2f;

        float cellW = (neededWidth - DistrictPadding) / cols;
        float cellD = (neededDepth - DistrictPadding) / rows;

        for (int i = 0; i < allClasses.Count; i++)
        {
            var cls = allClasses[i];
            int row = i / cols;
            int col = i % cols;

            float height = BaseBuildingHeight + cls.MethodCount * 0.05f;
            float rawFootprint = 0.02f + cls.FieldCount * 0.005f;
            float maxFootprint = Mathf.Min(cellW, cellD) - DistrictPadding * 0.5f;
            float footprint = Mathf.Clamp(rawFootprint, 0.02f, maxFootprint);

            float x = -halfW + (col + 0.5f) * cellW;
            float z = -halfD + (row + 0.5f) * cellD;

            var building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.transform.SetParent(districtGO.transform, false);
            building.transform.localScale = new Vector3(footprint, height, footprint);
            building.transform.localPosition = new Vector3(x, BaseDistrictHeight + height / 2f, z);
            building.name = cls.Name;

            _classBuildings[$"{package.Name}.{cls.Name}"] = building;

            var rend = building.GetComponent<Renderer>();
            rend.material.color = GetColorForPackage(package.Name);
        }

        int offset = allClasses.Count;
        for (int i = 0; i < subPackages.Count; i++)
        {
            var sub = subPackages[i];
            int row = (i + offset) / cols;
            int col = (i + offset) % cols;

            float x = -halfW + (col + 0.5f) * cellW;
            float z = -halfD + (row + 0.5f) * cellD;

            Vector3 subPos = new Vector3(x, BaseDistrictHeight, z);

            float subWidth = cellW * 0.9f;
            float subDepth = cellD * 0.9f;

            BuildDistrictRecursive(sub, districtGO.transform, subPos, subWidth, subDepth);
        }
    }

    private void BuildDistrictRecursive(PackageNode package, Transform parent, Vector3 localPos, float width, float depth)
    {
        var districtGO = new GameObject($"SubDistrict_{(string.IsNullOrEmpty(package.Name) ? "Unnamed" : package.Name)}");
        districtGO.transform.SetParent(parent, false);
        districtGO.transform.localPosition = localPos;

        List<ClassNode> allClasses = package.Files.SelectMany(f => f.Classes).ToList();
        List<PackageNode> subPackages = package.SubPackages ?? new List<PackageNode>();

        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.transform.SetParent(districtGO.transform, false);
        block.transform.localScale = new Vector3(width, BaseDistrictHeight, depth);
        block.transform.localPosition = new Vector3(0, BaseDistrictHeight / 2f, 0);

        var rend = block.GetComponent<Renderer>();
        rend.material.color = GetColorForPackage(package.Name) * 0.8f;

        int elementCount = allClasses.Count + subPackages.Count;
        if (elementCount == 0) return;

        int cols = Mathf.CeilToInt(Mathf.Sqrt(elementCount));
        int rows = Mathf.CeilToInt((float)elementCount / cols);

        float cellW = (width - DistrictPadding * (cols + 1)) / cols;
        float cellD = (depth - DistrictPadding * (rows + 1)) / rows;

        float startX = -width / 2f + DistrictPadding + cellW / 2f;
        float startZ = -depth / 2f + DistrictPadding + cellD / 2f;

        for (int i = 0; i < allClasses.Count; i++)
        {
            var cls = allClasses[i];
            int row = i / cols;
            int col = i % cols;

            float height = BaseBuildingHeight + cls.MethodCount * 0.05f;
            float footprint = Mathf.Max(0.05f, 0.02f + cls.FieldCount * 0.005f);

            float x = startX + col * (cellW + DistrictPadding);
            float z = startZ + row * (cellD + DistrictPadding);

            var building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.transform.SetParent(districtGO.transform, false);
            building.transform.localScale = new Vector3(footprint, height, footprint);
            building.transform.localPosition = new Vector3(x, BaseDistrictHeight + height / 2f, z);
            building.name = cls.Name;

            var bmat = building.GetComponent<Renderer>();
            bmat.material.color = GetColorForPackage(package.Name);
        }

        int offset = allClasses.Count;
        for (int i = 0; i < subPackages.Count; i++)
        {
            var sub = subPackages[i];
            int row = (i + offset) / cols;
            int col = (i + offset) % cols;

            float x = startX + col * (cellW + DistrictPadding);
            float z = startZ + row * (cellD + DistrictPadding);

            float subWidth = cellW * 0.9f;
            float subDepth = cellD * 0.9f;
            Vector3 subPos = new Vector3(x, BaseDistrictHeight, z);

            BuildDistrictRecursive(sub, districtGO.transform, subPos, subWidth, subDepth);
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
        foreach (var frame in sample.Frames)
        {
            if (string.IsNullOrEmpty(frame.ClassName)) continue;

            if (!_classBuildings.ContainsKey(frame.ClassName))
            {
                Debug.Log($"[City] No match for class: {frame.ClassName}");
            }
            else
            {
                Debug.Log($"[City] Found match for class: {frame.ClassName}");
                StartCoroutine(FlashBuilding(_classBuildings[frame.ClassName]));
            }
        }
    }

    private IEnumerator FlashBuilding(GameObject building)
    {
        var renderer = building.GetComponent<Renderer>();
        Color original = renderer.material.color;
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", Color.yellow);
        renderer.material.color = Color.yellow;

        yield return new WaitForSeconds(0.001f);

        renderer.material.color = original;
    }
}
