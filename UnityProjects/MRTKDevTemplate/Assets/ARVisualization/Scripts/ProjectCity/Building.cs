using UnityEngine;

public class Building : MonoBehaviour
{
    private Vector2 footprint;
    private float totalHeight;
    private string className;

    public Vector2 Footprint => footprint;

    public void BuildFromClass(
        ClassNode cls,
        ProjectCityNew city,
        GameObject floorPrefab
    )
    {
        className = cls.Name;

        float side = Mathf.Max(
            city.MinBuildingSide,
            city.MinBuildingSide + cls.FieldCount * city.SidePerField
        );
        footprint = new(side, side);

        if (cls.MethodCount == 0)
        {
            float h = Mathf.Max(city.MinFloorHeight, city.HeightPerLOC * 1f);
            CreateFloor(
                floorPrefab,
                side,
                h,
                yBottom: 0f
            );
            totalHeight = h;
            return;
        }

        float y = 0f;
        foreach (MethodNode m in cls.Methods)
        {
            var loc = Mathf.Max(0, m.LineCount);
            float h = Mathf.Max(city.MinFloorHeight, city.HeightPerLOC * loc);

            CreateFloor(
                floorPrefab,
                side,
                h,
                yBottom: y
            );

            y += h + city.FloorGap;
        }

        totalHeight = y - city.FloorGap;
        if (totalHeight < 0) totalHeight = 0;
    }

    private void CreateFloor(
        GameObject prefab,
        float side,
        float height,
        float yBottom
    )
    {
        var floor = Instantiate(prefab, transform);
        floor.name = "Floor";
        floor.transform.localScale = new(side, height, side);
        floor.transform.localPosition = new(0f, yBottom + height * 0.5f, 0f);
    }
}
