using UnityEngine;

[CreateAssetMenu(fileName = "CityConstants", menuName = "Scriptable Objects/City Constants")]
public class CityConstants : ScriptableObject
{
    public float MaxSize = 1.0f;
    public float MaxBuildingHeight = 0.5f;
    public float BaseBuildingHeight = 0.01f;
    public float DistrictPadding = 0.02f;
    public float BaseDistrictHeight = 0.01f;
    public float FloorGap = 0.002f;
}
