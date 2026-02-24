using System.Threading.Tasks;
using Unity.XR.CompositionLayers.Extensions;
using UnityEngine;

public class TextureUpdate : MonoBehaviour //TODO this is just ugly way to force composition layer update, needs to be fixed
{
    [SerializeField]
    private TexturesExtension sourceTexture;
    [SerializeField]
    private RenderTexture texture;
    [SerializeField]
    private RenderTexture tmpTexture;

    private bool swap = true;
    private bool initialized = false;

    async void Start()
    {
        await Task.Delay(2000);
        initialized = true;
    }

    void Update()
    {
        if (initialized == false)
        {
            return;
        }
        if (swap)
        {
            sourceTexture.LeftTexture = sourceTexture.RightTexture = tmpTexture;
        }
        else
        {
            Graphics.Blit(tmpTexture, texture);
            sourceTexture.LeftTexture = sourceTexture.RightTexture = texture;
        }
        swap = !swap;
    }
}
