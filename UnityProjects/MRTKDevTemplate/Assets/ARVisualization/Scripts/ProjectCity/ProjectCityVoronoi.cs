using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProjectCityVoronoiCircular : MonoBehaviour
{
    [Header("City Layout")]
    public float CityRadius = 4f;
    public int VoronoiResolution = 160;
    public int RelaxationSteps = 2;

    [Header("District Settings")]
    public float DistrictHeight = 0.25f;
    public float SubdistrictElevation = 0.02f;
    public Material DefaultMaterial;

    [Header("Buildings")]
    public float BaseBuildingHeight = 0.05f;
    public float BuildingScale = 0.1f;

    private ProjectStructure _project;

    void Start()
    {
        _project = new ProjectStructure();
        BuildCity(_project);
    }

    private void BuildCity(ProjectStructure project)
    {
        var seeds = CreateSeeds(project.Packages, CityRadius);
        var cells = ComputeAndRelax(seeds, CityRadius, null);

        foreach (var cell in cells)
            SpawnDistrict(cell, transform, 0);
    }

    private void SpawnDistrict(Cell cell, Transform parent, float elevation)
    {
        var poly = cell.Vertices;
        if (poly.Count < 3) return;

        var go = new GameObject($"District_{cell.Seed.Data.Name}");
        go.transform.SetParent(parent, false);

        var prism = CreateExtrudedMesh(poly, DistrictHeight);
        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = new Material(DefaultMaterial);
        var color = GetColor(cell.Seed.Data.Name);
        mr.sharedMaterial.color = color;
        mf.sharedMesh = prism;

        go.transform.localPosition = Vector3.up * (elevation + SubdistrictElevation);

        float roofY = elevation + DistrictHeight + SubdistrictElevation;
        PlaceBuildings(go.transform, poly, roofY, color);

        var subs = cell.Seed.Data.SubPackages ?? new List<PackageNode>();
        if (subs.Count > 0)
        {
            float localRadius = Mathf.Sqrt(GetPolygonArea(poly) / Mathf.PI) * 0.8f;
            var subSeeds = CreateSeeds(subs, localRadius);
            var subCells = ComputeAndRelax(subSeeds, localRadius, poly);

            foreach (var sub in subCells)
            {
                SpawnDistrict(sub, go.transform, elevation + DistrictHeight);
            }
        }
    }

    private void PlaceBuildings(Transform parent, List<Vector2> poly, float surfaceY, Color color)
    {
        Random.InitState(parent.name.GetHashCode());
        var bounds = GetBounds(poly);

        // Increase number of buildings relative to the area
        int numBuildings = Mathf.Clamp(Random.Range(6, 15), 3, 20);

        for (int i = 0; i < numBuildings; i++)
        {
            Vector2 p;
            int tries = 0;
            do
            {
                p = new Vector2(Random.Range(bounds.min.x, bounds.max.x),
                                Random.Range(bounds.min.y, bounds.max.y));
                tries++;
            } while (!PointInPolygon(p, poly) && tries < 25);

            // Taller buildings
            float height = BaseBuildingHeight * Random.Range(3f, 6f);
            float footprint = BuildingScale * Random.Range(0.7f, 1.4f);

            var b = GameObject.CreatePrimitive(PrimitiveType.Cube);
            b.transform.SetParent(parent, false);
            b.transform.localScale = new Vector3(footprint, height, footprint);
            // sits right on top of roof surface
            b.transform.localPosition = new Vector3(p.x, surfaceY + height / 2f, p.y);
            b.name = "Building_" + i;

            var r = b.GetComponent<Renderer>();
            r.sharedMaterial = new Material(DefaultMaterial);
            r.sharedMaterial.color = color * Random.Range(0.9f, 1.2f);
        }
    }

    private List<Seed> CreateSeeds(List<PackageNode> packages, float radius)
    {
        var list = new List<Seed>();
        foreach (var pkg in packages)
        {
            list.Add(new Seed()
            {
                Data = pkg,
                Position = Random.insideUnitCircle * radius * 0.8f
            });
        }
        return list;
    }

    private List<Cell> ComputeAndRelax(List<Seed> seeds, float radius, List<Vector2> clipPolygon)
    {
        List<Cell> cells = new();
        for (int i = 0; i < RelaxationSteps; i++)
        {
            cells = ComputeDiagram(seeds, radius, clipPolygon);
            foreach (var c in cells)
            {
                c.ComputeCentroid();
                c.Seed.Position = Vector2.Lerp(c.Seed.Position, c.Centroid, 0.75f);
            }
        }
        return ComputeDiagram(seeds, radius, clipPolygon);
    }

    private List<Cell> ComputeDiagram(List<Seed> seedList, float radius, List<Vector2> clipPolygon)
    {
        int steps = VoronoiResolution;
        float min = -radius, max = radius, step = (max - min) / steps;

        var ownership = new Dictionary<Seed, HashSet<Vector2>>();
        foreach (var s in seedList)
            ownership[s] = new();

        for (int ix = 0; ix <= steps; ix++)
        {
            for (int iz = 0; iz <= steps; iz++)
            {
                Vector2 p = new(min + ix * step, min + iz * step);
                if (p.sqrMagnitude > radius * radius) continue;
                if (clipPolygon != null && !PointInPolygon(p, clipPolygon)) continue;

                var nearest = seedList.OrderBy(s => (s.Position - p).sqrMagnitude).First();
                ownership[nearest].Add(p);
            }
        }

        var res = new List<Cell>();
        foreach (var s in seedList)
        {
            if (ownership[s].Count < 3) continue;
            var hull = ConvexHull(ownership[s].ToList());
            res.Add(new Cell() { Seed = s, Vertices = hull });
        }
        return res;
    }

    private static Mesh CreateExtrudedMesh(List<Vector2> verts, float height)
    {
        var mesh = new Mesh();
        if (verts.Count < 3) return mesh;

        // --- Geometry setup ---
        var lower = verts.Select(v => new Vector3(v.x, 0, v.y)).ToList();
        var upper = verts.Select(v => new Vector3(v.x, height, v.y)).ToList();

        List<Vector3> allVerts = new();
        allVerts.AddRange(lower);
        allVerts.AddRange(upper);

        List<int> tris = new();

        int count = verts.Count;

        // --- Side walls (correct outward winding) ---
        for (int i = 0; i < count; i++)
        {
            int next = (i + 1) % count;
            int a = i;
            int b = next;
            int c = i + count;
            int d = next + count;

            // Two triangles per quad
            tris.Add(a); tris.Add(c); tris.Add(b);       // bottom-left to top-left to bottom-right
            tris.Add(b); tris.Add(c); tris.Add(d);       // bottom-right to top-left to top-right
        }

        // --- Top cap (facing upward) ---
        for (int i = 1; i < count - 1; i++)
        {
            // order reversed compared to previous function — ensures normal faces up
            tris.Add(count);
            tris.Add(count + i + 1);
            tris.Add(count + i);
        }

        // --- Bottom cap (optional, facing downward) ---
        for (int i = 1; i < count - 1; i++)
        {
            tris.Add(0);
            tris.Add(i);
            tris.Add(i + 1);
        }

        mesh.SetVertices(allVerts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private static List<Vector2> ConvexHull(List<Vector2> pts)
    {
        if (pts.Count < 3) return pts.Distinct().ToList();
        pts = pts.OrderBy(p => p.x).ThenBy(p => p.y).ToList();

        List<Vector2> lower = new(), upper = new();
        foreach (var p in pts)
        {
            while (lower.Count >= 2 && Cross(lower[^1] - lower[^2], p - lower[^1]) <= 0)
                lower.RemoveAt(lower.Count - 1);
            lower.Add(p);
        }
        for (int i = pts.Count - 1; i >= 0; i--)
        {
            var p = pts[i];
            while (upper.Count >= 2 && Cross(upper[^1] - upper[^2], p - upper[^1]) <= 0)
                upper.RemoveAt(upper.Count - 1);
            upper.Add(p);
        }
        lower.RemoveAt(lower.Count - 1);
        upper.RemoveAt(upper.Count - 1);
        return lower.Concat(upper).ToList();
    }

    private static float GetPolygonArea(List<Vector2> verts)
    {
        float a = 0;
        for (int i = 0; i < verts.Count; i++)
        {
            Vector2 p1 = verts[i], p2 = verts[(i + 1) % verts.Count];
            a += (p1.x * p2.y - p2.x * p1.y);
        }
        return Mathf.Abs(a) * 0.5f;
    }

    private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

    private static bool PointInPolygon(Vector2 p, List<Vector2> poly)
    {
        bool c = false;
        for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
        {
            if (((poly[i].y > p.y) != (poly[j].y > p.y)) &&
                (p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) /
                (poly[j].y - poly[i].y) + poly[i].x))
                c = !c;
        }
        return c;
    }

    private static Bounds GetBounds(List<Vector2> poly)
    {
        Vector2 min = new(float.MaxValue, float.MaxValue);
        Vector2 max = new(float.MinValue, float.MinValue);
        foreach (var p in poly)
        {
            if (p.x < min.x) min.x = p.x;
            if (p.y < min.y) min.y = p.y;
            if (p.x > max.x) max.x = p.x;
            if (p.y > max.y) max.y = p.y;
        }
        return new Bounds((min + max) / 2, max - min);
    }

    private static Color GetColor(string name)
    {
        int h = name.GetHashCode();
        Random.InitState(h);
        return new Color(
            0.3f + Random.value * 0.5f,
            0.3f + Random.value * 0.5f,
            0.3f + Random.value * 0.5f
        );
    }
    private class Seed
    {
        public PackageNode Data;
        public Vector2 Position;
    }
    private class Cell
    {
        public Seed Seed;
        public List<Vector2> Vertices = new();
        public Vector2 Centroid;
        public void ComputeCentroid()
        {
            if (Vertices.Count == 0)
            {
                Centroid = Seed.Position;
                return;
            }
            Vector2 acc = Vector2.zero;
            foreach (var v in Vertices) acc += v;
            Centroid = acc / Vertices.Count;
        }
    }
}
