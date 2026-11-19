using UnityEngine;

public sealed class BeyElementTint : MonoBehaviour
{
    [SerializeField] private SpriteRenderer outerLayer;
    [SerializeField] private SpriteRenderer middleLayer;
    [SerializeField] private SpriteRenderer innerLayer;

    private BeyData data;
    private ElementDefinition applied;

    private void Awake()
    {
        data = GetComponent<BeyData>();
    }

    private void Start()
    {
        ApplyCurrent();
    }

    private void Update()
    {
        if (data == null || data.Elements.Current == applied)
            return;

        ApplyCurrent();
    }

    private void ApplyCurrent()
    {
        if (data != null)
            Apply(data.Elements.Current);
    }

    public void Apply(ElementDefinition element)
    {
        if (element == null)
            return;

        applied = element;

        Color baseColor = element.color;

        if (outerLayer != null)
            outerLayer.color = Color.Lerp(baseColor, Color.black, 0.2f);

        if (middleLayer != null)
            middleLayer.color = baseColor;

        if (innerLayer != null)
            innerLayer.color = Color.Lerp(baseColor, Color.white, 0.25f);
    }
}
