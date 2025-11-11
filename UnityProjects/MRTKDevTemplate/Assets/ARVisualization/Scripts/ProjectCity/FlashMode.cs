using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashMode : IVisualizationMode
{
    private readonly float flashDuration = 0.05f;
    private readonly Color emissionColor = Color.white;
    private readonly float emissionStrength = 2f;

    private readonly MonoBehaviour _owner;
    private Dictionary<string, GameObject> _methodFloors;
    private Dictionary<GameObject, Color> _baseColors;
    private readonly Dictionary<GameObject, Coroutine> _activeFlashes = new();
    private readonly Dictionary<GameObject, Material> _materials = new();

    public FlashMode(MonoBehaviour owner)
    {
        _owner = owner;
    }

    public void Initialize(Dictionary<string, GameObject> methodFloors, Dictionary<GameObject, Color> baseColors)
    {
        _methodFloors = methodFloors;
        _baseColors = baseColors;
    }

    public void OnExecutionSample(ExecutionSample sample)
    {
        foreach (var frame in sample.Frames)
        {
            string key = $"{frame.ClassName}.{frame.Method}";
            if (_methodFloors.TryGetValue(key, out var go))
                _owner.StartCoroutine(Flash(go));
        }
    }

    public void Update() { }

    public void ResetAll()
    {
        foreach (var (go, mat) in _materials)
        {
            if (_baseColors.TryGetValue(go, out var baseColor))
                mat.color = baseColor;
            mat.SetColor("_EmissionColor", Color.black);
        }
        _activeFlashes.Clear();
    }

    private IEnumerator Flash(GameObject go)
    {
        if (!go || !_baseColors.TryGetValue(go, out var baseColor)) yield break;

        if (!go.TryGetComponent<Renderer>(out var renderer)) yield break;

        if (!_materials.TryGetValue(go, out var mat))
        {
            mat = renderer.material;
            _materials[go] = mat;
        }

        mat.EnableKeyword("_EMISSION");
        mat.color = emissionColor;
        mat.SetColor("_EmissionColor", emissionColor * emissionStrength);

        yield return new WaitForSeconds(flashDuration);

        mat.DisableKeyword("_EMISSION");
        mat.color = baseColor;
        mat.SetColor("_EmissionColor", Color.black);
    }
}
