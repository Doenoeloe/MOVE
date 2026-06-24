    using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class RadialGradient : MonoBehaviour
{
    [SerializeField] private Color innerColor = new Color(1f, 0.4f, 0f, 0.6f);
    [SerializeField] private Color outerColor = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private int resolution = 256;

    void Start()
    {
        GetComponent<RawImage>().texture = GenerateTexture();
    }

    Texture2D GenerateTexture()
    {
        Texture2D tex = new Texture2D(resolution, resolution);
        Vector2 center = new Vector2(0.5f, 0.5f);

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                Vector2 uv = new Vector2((float)x / resolution, (float)y / resolution);
                float dist = Vector2.Distance(uv, center);

                // Remap 0-0.5 to 0-1, then invert so center = 1
                float t = Mathf.Clamp01(dist * 2f);
                t = Mathf.Pow(t, 1.5f); // adjust falloff

                tex.SetPixel(x, y, Color.Lerp(innerColor, outerColor, t));
            }
        }

        tex.Apply();
        return tex;
    }
}
