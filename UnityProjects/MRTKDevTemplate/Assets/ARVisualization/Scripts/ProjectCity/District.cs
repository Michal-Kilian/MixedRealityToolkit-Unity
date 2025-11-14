using UnityEngine;

public class District : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Initialize(PackageNode pkg, float width, float height, float depth)
    {
        name = $"District: {pkg.Name}";
        meshRenderer.transform.localScale = new(width, height, depth);
        meshRenderer.transform.localPosition = new(0, height / 2f, 0);
        meshRenderer.material.color = CityHelpers.Instance.GetColorForPackage(pkg.Name) * 0.8f;
    }
}
