using System.Collections.Generic;
using UnityEngine;

public class HeatmapMode : IVisualizationMode
{
    private readonly float decayDuration = 10f;
    private float decayRate;
    private Color heatColor = Color.white;

    private Dictionary<string, GameObject> _methodFloors;
    private Dictionary<GameObject, Color> _baseColors;
    private readonly Dictionary<string, float> _heat = new();
    private readonly Dictionary<GameObject, Material> _materials = new();

    public void Initialize(Dictionary<string, GameObject> methodFloors, Dictionary<GameObject, Color> baseColors)
    {
        _methodFloors = methodFloors;
        _baseColors = baseColors;

        decayRate = 1f / decayDuration;
    }

    public void OnExecutionSample(ExecutionSample sample)
    {
        foreach (var frame in sample.Frames)
        {
            string key = $"{frame.ClassName}.{frame.Method}";
            if (_methodFloors.ContainsKey(key))
            {
                if (!_heat.ContainsKey(key))
                    _heat[key] = 0f;

                _heat[key] = Mathf.Min(1f, _heat[key] + 0.5f);
            }
        }
    }

    public void Update()
    {
        var keys = new List<string>(_heat.Keys);
        foreach (var key in keys)
        {
            _heat[key] = Mathf.Max(0, _heat[key] - decayRate * Time.deltaTime);

            if (_methodFloors.TryGetValue(key, out var go))
            {
                if (!go.TryGetComponent<Renderer>(out var renderer)) continue;
                if (!_baseColors.TryGetValue(go, out var baseColor)) continue;

                if (!_materials.TryGetValue(go, out var mat))
                {
                    mat = renderer.material;
                    _materials[go] = mat;
                }

                Color blended = Color.Lerp(baseColor, heatColor, _heat[key]);
                mat.color = blended;
            }

            if (_heat[key] <= 0.0001f)
                _heat.Remove(key);
        }
    }

    public void ResetAll()
    {
        foreach (var (go, mat) in _materials)
        {
            if (_baseColors.TryGetValue(go, out var baseColor))
            {
                mat.color = baseColor;
            }
        }
        _heat.Clear();
    }
}
