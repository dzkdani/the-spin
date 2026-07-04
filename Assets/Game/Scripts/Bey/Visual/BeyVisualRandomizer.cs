using UnityEngine;

public class BeyVisualRandomizer : MonoBehaviour
{
    [Header("Sprite Renderers")]
    [SerializeField] private SpriteRenderer outerLayer;
    [SerializeField] private SpriteRenderer middleLayer;
    [SerializeField] private SpriteRenderer innerLayer;

    [Space]

    [Header("Possible Outer Sprites")]
    [SerializeField] private Sprite[] outerSprites;

    [Header("Possible Middle Sprites")]
    [SerializeField] private Sprite[] middleSprites;

    [Header("Possible Inner Sprites")]
    [SerializeField] private Sprite[] innerSprites;

    [Space]

    [Header("Color Palettes")]
    [SerializeField] private BeyColorPalette[] palettes;

    [Space]

    [SerializeField] private bool randomizeOnAwake = true;

    private void Awake()
    {
        if (randomizeOnAwake)
            Randomize();
    }

    public void Randomize()
    {
        RandomizeSprites();

        if (palettes == null || palettes.Length == 0)
        {
            Debug.LogWarning($"{name} has no color palettes assigned.");
            return;
        }

        BeyColorPalette palette =
            palettes[Random.Range(0, palettes.Length)];

        ApplyPalette(palette);
    }

    public void RandomizeSprites()
    {
        RandomizeSprite(outerLayer, outerSprites);
        RandomizeSprite(middleLayer, middleSprites);
        RandomizeSprite(innerLayer, innerSprites);
    }

    private void RandomizeSprite(SpriteRenderer renderer, Sprite[] sprites)
    {
        if (renderer == null)
            return;

        if (sprites == null || sprites.Length == 0)
            return;

        renderer.sprite = sprites[Random.Range(0, sprites.Length)];
    }

    public void ApplyPalette(BeyColorPalette palette)
    {
        if (outerLayer != null)
            outerLayer.color = palette.outer;

        if (middleLayer != null)
            middleLayer.color = palette.middle;

        if (innerLayer != null)
            innerLayer.color = palette.inner;
    }
}

[System.Serializable]
public class BeyColorPalette
{
    public string name;

    [Header("Layer Colors")]
    public Color outer = Color.white;
    public Color middle = Color.white;
    public Color inner = Color.white;
}