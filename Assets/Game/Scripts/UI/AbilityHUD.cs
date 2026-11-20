using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class AbilityHUD : MonoBehaviour
{
    private const int SlotCount = 3;

    private BeyController player;
    private bool built;
    private readonly GameObject[] panels = new GameObject[SlotCount];
    private readonly TMP_Text[] names = new TMP_Text[SlotCount];
    private readonly TMP_Text[] costs = new TMP_Text[SlotCount];
    private readonly Image[] backgrounds = new Image[SlotCount];
    private readonly Image[] fills = new Image[SlotCount];

    public static AbilityHUD EnsureInScene()
    {
        AbilityHUD existing = FindFirstObjectByType<AbilityHUD>(
            FindObjectsInactive.Include);

        if (existing != null)
            return existing;

        GameObject host = new GameObject("AbilityHUD", typeof(RectTransform));
        Canvas canvas = FindFirstObjectByType<Canvas>();

        if (canvas != null)
            host.transform.SetParent(canvas.transform, false);

        RectTransform rect = host.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.sizeDelta = new Vector2(630f, 90f);
        rect.anchoredPosition = new Vector2(0f, 70f);

        return host.AddComponent<AbilityHUD>();
    }

    public void Bind(BeyController target)
    {
        player = target;

        if (!built)
            BuildUI();
    }

    private void BuildUI()
    {
        for (int i = 0; i < SlotCount; i++)
        {
            RectTransform panel = MakeRect(
                "Slot" + (i + 1), transform,
                new Vector2(200f, 84f),
                new Vector2((i - 1) * 210f, 0f));

            Image background = panel.gameObject.AddComponent<Image>();
            background.color = new Color(0.1f, 0.12f, 0.17f, 0.9f);
            backgrounds[i] = background;

            AddText(
                "Key", panel, (i + 1).ToString(),
                new Vector2(-82f, 22f), new Vector2(30f, 28f),
                22, TextAlignmentOptions.Center,
                new Color(0.7f, 0.8f, 1f, 1f));

            names[i] = AddText(
                "Name", panel, "-",
                new Vector2(8f, 20f), new Vector2(160f, 30f),
                22, TextAlignmentOptions.Center,
                Color.white);

            costs[i] = AddText(
                "Cost", panel, "",
                new Vector2(78f, -14f), new Vector2(50f, 22f),
                16, TextAlignmentOptions.Right,
                new Color(0.8f, 0.8f, 0.8f, 1f));

            RectTransform bar = MakeRect(
                "Cooldown", panel,
                new Vector2(180f, 8f),
                new Vector2(0f, -33f));

            Image fill = bar.gameObject.AddComponent<Image>();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 1f;
            fill.color = Color.white;
            fills[i] = fill;

            panels[i] = panel.gameObject;
        }

        built = true;
    }

    private void Update()
    {
        if (!built || player == null || player.Abilities == null)
        {
            HideAll();
            return;
        }

        AbilityRunner runner = player.Abilities;
        Color accent = player.Data != null &&
            player.Data.Elements.Current != null
                ? player.Data.Elements.Current.color
                : new Color(0.4f, 0.8f, 1f, 1f);

        for (int i = 0; i < SlotCount; i++)
        {
            AbilityDefinition definition = runner.GetActiveDefinition(i);

            if (definition == null)
            {
                panels[i].SetActive(false);
                continue;
            }

            panels[i].SetActive(true);

            names[i].text = string.IsNullOrEmpty(definition.displayName)
                ? definition.id
                : definition.displayName;

            costs[i].text =
                Mathf.RoundToInt(definition.active.rpmCost * 100f) + "% RPM";

            float duration = definition.active.cooldown > 0f
                ? definition.active.cooldown
                : 1f;

            float remaining = runner.GetActiveCooldown(i);
            fills[i].fillAmount = runner.IsActiveReady(i)
                ? 1f
                : 1f - Mathf.Clamp01(remaining / duration);

            bool affordable = player.Data != null &&
                player.Data.RPMNormalized >= definition.active.rpmCost;

            bool ready = runner.IsActiveReady(i) && affordable;

            backgrounds[i].color = ready
                ? Color.Lerp(new Color(0.12f, 0.14f, 0.2f, 0.95f), accent, 0.35f)
                : new Color(0.1f, 0.12f, 0.17f, 0.9f);

            fills[i].color = ready ? accent : new Color(0.4f, 0.4f, 0.45f, 1f);
        }
    }

    private void HideAll()
    {
        for (int i = 0; i < SlotCount; i++)
        {
            if (panels[i] != null)
                panels[i].SetActive(false);
        }
    }

    private static RectTransform MakeRect(
        string name,
        Transform parent,
        Vector2 size,
        Vector2 position)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        return rect;
    }

    private static TMP_Text AddText(
        string name,
        Transform parent,
        string content,
        Vector2 position,
        Vector2 size,
        int fontSize,
        TextAlignmentOptions alignment,
        Color color)
    {
        RectTransform rect = MakeRect(name, parent, size, position);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;

        return text;
    }
}
