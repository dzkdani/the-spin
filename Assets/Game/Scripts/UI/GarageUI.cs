using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class GarageUI : MonoBehaviour
{
    private GarageModel model;
    private BeyController player;
    private Action onDone;
    private bool built;
    private TMP_Text[] rowValues;
    private TMP_Text statsText;
    private TMP_Text previewText;
    private TMP_Text budgetText;
    private TMP_Text errorText;
    private Button confirmButton;

    public static GarageUI EnsureInScene()
    {
        GarageUI existing = FindFirstObjectByType<GarageUI>(
            FindObjectsInactive.Include);

        if (existing != null)
            return existing;

        GameObject host = new GameObject("Garage", typeof(RectTransform));
        Canvas canvas = FindFirstObjectByType<Canvas>();

        if (canvas != null)
            host.transform.SetParent(canvas.transform, false);

        RectTransform rect = host.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        GarageUI ui = host.AddComponent<GarageUI>();
        host.SetActive(false);

        return ui;
    }

    public void Show(
        BeyController target,
        BeyBlueprint nextEnemy,
        int wave,
        int totalWaves,
        Action done)
    {
        player = target;
        onDone = done;

        if (player == null || player.Data == null)
        {
            Close();
            return;
        }

        model = GarageModel.FromContent(player.Data.Blueprint);

        if (!built)
            BuildUI();

        gameObject.SetActive(true);
        Refresh();
        ShowPreview(nextEnemy, wave, totalWaves);

        Time.timeScale = 0f;
        ServiceLocator.Get<GameUIManager>()?.SetGarageOpen(true);
    }

    private void BuildUI()
    {
        RectTransform overlay = MakeRect(
            "Overlay", transform, Vector2.zero, Vector2.zero);
        overlay.anchorMin = Vector2.zero;
        overlay.anchorMax = Vector2.one;
        overlay.offsetMin = Vector2.zero;
        overlay.offsetMax = Vector2.zero;

        Image dim = overlay.gameObject.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.82f);

        RectTransform card = MakeRect(
            "Card", overlay, new Vector2(1040f, 660f), Vector2.zero);

        Image cardImage = card.gameObject.AddComponent<Image>();
        cardImage.color = new Color(0.09f, 0.11f, 0.16f, 1f);

        AddText(
            "Title", card, "GARAGE",
            new Vector2(0f, 300f), new Vector2(600f, 60f),
            42, TextAlignmentOptions.Center);

        statsText = AddText(
            "Stats", card, "",
            new Vector2(-280f, 140f), new Vector2(420f, 300f),
            20, TextAlignmentOptions.TopLeft);

        previewText = AddText(
            "Preview", card, "",
            new Vector2(-280f, -95f), new Vector2(420f, 110f),
            20, TextAlignmentOptions.TopLeft);
        previewText.color = new Color(0.55f, 0.78f, 1f, 1f);

        IReadOnlyList<GarageModel.Row> rows = model.Rows;
        rowValues = new TMP_Text[rows.Count];

        for (int i = 0; i < rows.Count; i++)
        {
            float y = 250f - i * 52f;

            AddText(
                "Label" + i, card, rows[i].Label,
                new Vector2(120f, y), new Vector2(160f, 40f),
                24, TextAlignmentOptions.MidlineLeft);

            Button previous = AddButton(
                "Previous" + i, card, "<",
                new Vector2(250f, y), new Vector2(44f, 40f));

            rowValues[i] = AddText(
                "Value" + i, card, rows[i].Value,
                new Vector2(380f, y), new Vector2(180f, 40f),
                26, TextAlignmentOptions.Center);

            Button next = AddButton(
                "Next" + i, card, ">",
                new Vector2(500f, y), new Vector2(44f, 40f));

            int index = i;
            previous.onClick.AddListener(() => Cycle(index, -1));
            next.onClick.AddListener(() => Cycle(index, 1));
        }

        budgetText = AddText(
            "Budget", card, "",
            new Vector2(-280f, -205f), new Vector2(420f, 40f),
            24, TextAlignmentOptions.MidlineLeft);

        errorText = AddText(
            "Error", card, "",
            new Vector2(0f, -245f), new Vector2(900f, 40f),
            22, TextAlignmentOptions.Center);
        errorText.color = new Color(1f, 0.45f, 0.45f, 1f);

        Button skip = AddButton(
            "Skip", card, "SKIP",
            new Vector2(-300f, -295f), new Vector2(220f, 60f));
        skip.onClick.AddListener(Skip);

        confirmButton = AddButton(
            "Confirm", card, "SPIN!",
            new Vector2(300f, -295f), new Vector2(220f, 60f));
        confirmButton.onClick.AddListener(Confirm);

        built = true;
    }

    private void ShowPreview(BeyBlueprint nextEnemy, int wave, int totalWaves)
    {
        if (previewText == null)
            return;

        if (nextEnemy == null)
        {
            previewText.text = "NEXT WAVE  " + wave + " / " + totalWaves +
                "\nTYPE  -\nELEMENT  -\nAI  -";
            return;
        }

        previewText.text =
            "NEXT WAVE  " + wave + " / " + totalWaves + "\n" +
            "TYPE  " + NameOf(nextEnemy.type) + "\n" +
            "ELEMENT  " + NameOf(nextEnemy.baseElement) + "\n" +
            "AI  " + NameOf(nextEnemy.aiProfile);
    }

    private void Refresh()
    {
        IReadOnlyList<GarageModel.Row> rows = model.Rows;

        for (int i = 0; i < rows.Count; i++)
            rowValues[i].text = rows[i].Value;

        BeyBlueprint candidate = model.Candidate;
        string type = NameOf(candidate.type);
        string element = NameOf(candidate.baseElement);

        string parts = "";

        for (int i = 0; i < candidate.parts.Count; i++)
        {
            if (i > 0)
                parts += "\n";

            parts += NameOf(candidate.parts[i]);
        }

        if (string.IsNullOrEmpty(parts))
            parts = "(none)";

        string active = NameOf(
            candidate.equippedActives.Count > 0
                ? candidate.equippedActives[0]
                : null);

        string passives = "";

        for (int i = 0; i < candidate.equippedPassives.Count; i++)
        {
            if (i > 0)
                passives += ", ";

            passives += NameOf(candidate.equippedPassives[i]);
        }

        if (string.IsNullOrEmpty(passives))
            passives = "-";

        statsText.text =
            "TYPE   " + type + "\n" +
            "ELEMENT   " + element + "\n\n" +
            "ACTIVE   " + active + "\n" +
            "PASSIVES   " + passives + "\n\n" +
            "PARTS\n" + parts;

        budgetText.text =
            "BUDGET   " + model.Budget + " / " + model.BudgetCap;

        errorText.text = model.IsValid
            ? "READY"
            : model.Errors[0];

        confirmButton.interactable = model.IsValid;
    }

    private void Cycle(int row, int delta)
    {
        model.Cycle(row, delta);
        Refresh();
    }

    private void Confirm()
    {
        if (!model.IsValid)
            return;

        if (!player.ApplyBlueprint(model.Candidate))
        {
            errorText.text = "LOADOUT REJECTED";
            return;
        }

        Close();
    }

    private void Skip()
    {
        Close();
    }

    private void Close()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        ServiceLocator.Get<GameUIManager>()?.SetGarageOpen(false);

        Action done = onDone;
        onDone = null;
        done?.Invoke();
    }

    private static string NameOf(ContentDefinition definition)
    {
        if (definition == null)
            return "-";

        return string.IsNullOrEmpty(definition.displayName)
            ? definition.id
            : definition.displayName;
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
        TextAlignmentOptions alignment)
    {
        RectTransform rect = MakeRect(name, parent, size, position);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;

        return text;
    }

    private static Button AddButton(
        string name,
        Transform parent,
        string label,
        Vector2 position,
        Vector2 size)
    {
        RectTransform rect = MakeRect(name, parent, size, position);

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0.16f, 0.19f, 0.26f, 1f);

        Button button = rect.gameObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.28f, 0.34f, 0.44f, 1f);
        colors.pressedColor = new Color(0.4f, 0.5f, 0.65f, 1f);
        colors.disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        button.colors = colors;

        AddText(
            "Label", rect, label,
            Vector2.zero, size, 24, TextAlignmentOptions.Center);

        return button;
    }
}
