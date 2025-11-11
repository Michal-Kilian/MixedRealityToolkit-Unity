using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class District : MonoBehaviour
{
    private Vector2 footprint;
    private float pedestalHeight;
    private string packageName;

    private ProjectCityNew city;
    private PackageNode package;

    private readonly List<District> childDistricts = new();
    private readonly List<Building> buildings = new();

    public Vector2 Footprint => footprint;
    public float PedestalHeight => pedestalHeight;

    public static District Create(
        PackageNode pkg,
        ProjectCityNew city,
        Transform parent
    )
    {
        GameObject districtGO = Instantiate(city.DistrictPrefab, parent);
        districtGO.name = $"District_{pkg.Name}";
        var district = districtGO.AddComponent<District>();
        district.Initialize(pkg, city);
        district.BuildRecursive();
        return district;
    }

    public IEnumerable<Building> EnumerateAllBuildings()
    {
        foreach (var b in buildings)
            yield return b;

        foreach (var d in childDistricts)
            foreach (var b in d.EnumerateAllBuildings())
                yield return b;
    }

    private void Initialize(PackageNode pkg, ProjectCityNew projectCity)
    {
        package = pkg;
        city = projectCity;
        packageName = pkg.Name;
        pedestalHeight = city.DistrictPedestalHeight;
    }

    private void BuildRecursive()
    {
        if (package.SubPackages != null)
        {
            foreach (var sub in package.SubPackages)
            {
                var child = Create(sub, city, transform);
                childDistricts.Add(child);
            }
        }

        foreach (var cls in CollectClasses(package))
        {
            var buildingGO = new GameObject($"Building_{cls.Name}");
            buildingGO.transform.SetParent(transform, false);
            var building = buildingGO.AddComponent<Building>();
            building.BuildFromClass(cls, city, city.BuildingPrefab);
            buildings.Add(building);
        }

        var items = new List<ChildItem>();
        items.AddRange(
            childDistricts.Select(d => new ChildItem
            {
                transform = d.transform,
                size = d.footprint,
                type = ChildType.District,
                pedestalHeight = d.pedestalHeight,
            })
        );
        items.AddRange(
            buildings.Select(b => new ChildItem
            {
                transform = b.transform,
                size = b.Footprint,
                type = ChildType.Building,
                pedestalHeight = 0f
            })
        );

        var contentSize = Vector2.zero;
        var positions = new List<Vector2>();

        if (items.Count > 0)
        {
            var totalArea = items.Aggregate(
                0f,
                (a, it) => a + it.size.x * it.size.y
            );
            var targetRowWidth = Mathf.Sqrt(totalArea);
            targetRowWidth = Mathf.Max(
                targetRowWidth,
                city.MinDistrictSide - 2 * city.DistrictPadding
            );

            var packed = ShelfPack(
                items,
                targetRowWidth,
                city.BuildingSpacing
            );
            contentSize = packed.bounds;
            positions = packed.positions;
        }

        var contentX = Mathf.Max(contentSize.x, 0f);
        var contentZ = Mathf.Max(contentSize.y, 0f);
        var pad = city.DistrictPadding;
        var width = Mathf.Max(
            contentX + 2 * pad,
            city.MinDistrictSide
        );
        var depth = Mathf.Max(
            contentZ + 2 * pad,
            city.MinDistrictSide
        );
        footprint = new(width, depth);

        transform.localScale = new(
            footprint.x,
            pedestalHeight,
            footprint.y
        );

        var origin = new Vector2(-width * 0.5f + pad, -depth * 0.5f + pad);

        for (int i  = 0; i < items.Count; i++)
        {
            var it = items[i];
            var posBL = positions[i];
            var centerX = origin.x + posBL.x + it.size.x * 0.5f;
            var centerZ = origin.y + posBL.y + it.size.y * 0.5f;

            float centerY;
            if (it.type == ChildType.Building)
            {
                centerY = pedestalHeight * 0.5f;
            }
            else
            {
                centerY = pedestalHeight * 0.5f + it.pedestalHeight * 0.5f;
            }

            it.transform.localPosition = new(centerX, centerY, centerZ);
        }
    }

    private static List<ClassNode> CollectClasses(PackageNode pkg)
    {
        var result = new List<ClassNode>();
        if (pkg.Files != null)
        {
            foreach (var f in pkg.Files)
            {
                if (f.Classes != null)
                {
                    result.AddRange(f.Classes);
                }
            }
        }
        return result;
    }

    private static Packed ShelfPack(
        List<ChildItem> items,
        float targetRowWidth,
        float spacing
    )
    {
        var positions = new List<Vector2>();
        float rowX = 0, rowZ = 0, rowHeight = 0, maxRowWidth = 0;

        foreach (var it in items)
        {
            var nextX = rowX == 0 ? it.size.x : rowX + spacing + it.size.x;
            if (nextX > targetRowWidth && rowX > 0)
            {
                maxRowWidth = Mathf.Max(maxRowWidth, rowX);
                rowZ += rowHeight + spacing;
                rowX = 0;
                rowHeight = 0;
            }

            var placedX = rowX == 0 ? 0 : rowX + spacing;
            positions.Add(new(placedX, rowZ));

            rowX = placedX + it.size.x;
            rowHeight = Mathf.Max(rowHeight, it.size.y);
        }

        maxRowWidth = Mathf.Max(maxRowWidth, rowX);
        var totalDepth = rowZ + rowHeight;

        return new Packed
        {
            positions = positions,
            bounds = new(maxRowWidth, totalDepth),
        };
    }

    private enum ChildType
    {
        Building,
        District,
    }

    private struct ChildItem
    {
        public Transform transform;
        public Vector2 size;
        public ChildType type;
        public float pedestalHeight;
    }

    private struct Packed
    {
        public List<Vector2> positions;
        public Vector2 bounds;
    }
}
