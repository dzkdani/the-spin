using System.Collections.Generic;
using UnityEngine;

public readonly struct GarageOption
{
    public readonly string Label;
    public readonly BeyTypeDefinition Type;
    public readonly ElementDefinition Element;
    public readonly PartDefinition Part;

    private GarageOption(
        string label,
        BeyTypeDefinition type,
        ElementDefinition element,
        PartDefinition part)
    {
        Label = label;
        Type = type;
        Element = element;
        Part = part;
    }

    public static GarageOption None =>
        new GarageOption("NONE", null, null, null);

    public static GarageOption ForType(BeyTypeDefinition type)
    {
        return new GarageOption(LabelOf(type), type, null, null);
    }

    public static GarageOption ForElement(ElementDefinition element)
    {
        return new GarageOption(LabelOf(element), null, element, null);
    }

    public static GarageOption ForPart(PartDefinition part)
    {
        return new GarageOption(LabelOf(part), null, null, part);
    }

    private static string LabelOf(ContentDefinition definition)
    {
        if (definition == null)
            return "NONE";

        return string.IsNullOrEmpty(definition.displayName)
            ? definition.id.ToUpperInvariant()
            : definition.displayName.ToUpperInvariant();
    }
}

public sealed class GarageModel
{
    public readonly struct Row
    {
        public readonly string Label;
        public readonly string Value;
        public readonly int Index;
        public readonly int Count;

        public Row(string label, string value, int index, int count)
        {
            Label = label;
            Value = value;
            Index = index;
            Count = count;
        }
    }

    private enum Kind
    {
        Type,
        Element,
        Slot
    }

    private sealed class OptionSet
    {
        public string Label;
        public Kind Kind;
        public List<GarageOption> Options;
        public int Index;
    }

    private readonly BeyBlueprint template;
    private readonly CombatTuning tuning;
    private readonly List<OptionSet> sets = new List<OptionSet>();
    private readonly List<string> errors = new List<string>();
    private readonly BeyBlueprint candidate;

    public GarageModel(
        BeyBlueprint template,
        CombatTuning tuning,
        IReadOnlyList<BeyTypeDefinition> types,
        IReadOnlyList<ElementDefinition> elements,
        IReadOnlyList<PartDefinition> parts)
    {
        this.template = template;
        this.tuning = tuning;

        candidate = ScriptableObject.CreateInstance<BeyBlueprint>();
        candidate.id = "player_loadout";
        candidate.displayName = "Player Loadout";
        candidate.chassis = template != null ? template.chassis : null;
        candidate.aiProfile = template != null ? template.aiProfile : null;

        AddSet(
            "TYPE",
            Kind.Type,
            types,
            GarageOption.ForType,
            template != null ? template.type : null);

        AddSet(
            "ELEMENT",
            Kind.Element,
            elements,
            GarageOption.ForElement,
            template != null ? template.baseElement : null);

        List<PartSlotDefinition> slots = CollectSlots(parts);

        for (int i = 0; i < slots.Count; i++)
        {
            PartSlotDefinition slot = slots[i];
            List<GarageOption> options = new List<GarageOption>
            {
                GarageOption.None
            };

            for (int j = 0; j < parts.Count; j++)
            {
                if (parts[j].slot == slot)
                    options.Add(GarageOption.ForPart(parts[j]));
            }

            PartDefinition selected = FindPart(template, slot);
            int index = options.FindIndex(option => option.Part == selected);

            sets.Add(new OptionSet
            {
                Label = SlotLabel(slot),
                Kind = Kind.Slot,
                Options = options,
                Index = index >= 0 ? index : 0
            });
        }

        Apply();
    }

    public static GarageModel FromContent(BeyBlueprint template)
    {
        List<BeyTypeDefinition> types =
            new List<BeyTypeDefinition>(
                ContentRegistry.All<BeyTypeDefinition>());

        List<ElementDefinition> elements =
            new List<ElementDefinition>(
                ContentRegistry.All<ElementDefinition>());

        List<PartDefinition> parts =
            new List<PartDefinition>(
                ContentRegistry.All<PartDefinition>());

        types.Sort((a, b) => string.CompareOrdinal(a.id, b.id));
        elements.Sort((a, b) => string.CompareOrdinal(a.id, b.id));

        parts.Sort((a, b) =>
        {
            int ap = a.slot != null ? a.slot.elementPriority : 0;
            int bp = b.slot != null ? b.slot.elementPriority : 0;

            return ap != bp
                ? bp.CompareTo(ap)
                : string.CompareOrdinal(a.id, b.id);
        });

        return new GarageModel(
            template,
            ContentRegistry.Tuning,
            types,
            elements,
            parts);
    }

    public BeyBlueprint Candidate => candidate;

    public IReadOnlyList<string> Errors => errors;

    public bool IsValid => errors.Count == 0;

    public int Budget
    {
        get
        {
            int total = 0;

            for (int i = 0; i < candidate.parts.Count; i++)
                total += candidate.parts[i].budgetCost;

            return total;
        }
    }

    public int BudgetCap => tuning != null ? tuning.partBudgetCap : 0;

    public IReadOnlyList<Row> Rows
    {
        get
        {
            List<Row> rows = new List<Row>(sets.Count);

            for (int i = 0; i < sets.Count; i++)
            {
                OptionSet set = sets[i];

                rows.Add(new Row(
                    set.Label,
                    set.Options[set.Index].Label,
                    set.Index,
                    set.Options.Count));
            }

            return rows;
        }
    }

    public void Cycle(int rowIndex, int delta)
    {
        if (rowIndex < 0 || rowIndex >= sets.Count)
            return;

        OptionSet set = sets[rowIndex];

        if (set.Options.Count <= 1)
            return;

        int next = set.Index + delta;
        set.Index = (next % set.Options.Count + set.Options.Count)
            % set.Options.Count;

        Apply();
    }

    private void AddSet<T>(
        string label,
        Kind kind,
        IReadOnlyList<T> options,
        System.Func<T, GarageOption> wrap,
        ContentDefinition selected) where T : ContentDefinition
    {
        List<GarageOption> wrapped = new List<GarageOption>();

        for (int i = 0; i < options.Count; i++)
            wrapped.Add(wrap(options[i]));

        int index = -1;

        for (int i = 0; i < options.Count; i++)
        {
            if (options[i] == selected)
            {
                index = i;
                break;
            }
        }

        sets.Add(new OptionSet
        {
            Label = label,
            Kind = kind,
            Options = wrapped,
            Index = index >= 0 ? index : 0
        });
    }

    private static List<PartSlotDefinition> CollectSlots(
        IReadOnlyList<PartDefinition> parts)
    {
        List<PartSlotDefinition> slots = new List<PartSlotDefinition>();

        for (int i = 0; i < parts.Count; i++)
        {
            PartSlotDefinition slot = parts[i].slot;

            if (slot == null || slots.Contains(slot))
                continue;

            slots.Add(slot);
        }

        slots.Sort((a, b) =>
            b.elementPriority.CompareTo(a.elementPriority));

        return slots;
    }

    private static PartDefinition FindPart(
        BeyBlueprint blueprint,
        PartSlotDefinition slot)
    {
        if (blueprint == null)
            return null;

        for (int i = 0; i < blueprint.parts.Count; i++)
        {
            PartDefinition part = blueprint.parts[i];

            if (part != null && part.slot == slot)
                return part;
        }

        return null;
    }

    private static string SlotLabel(PartSlotDefinition slot)
    {
        if (slot == null)
            return "PART";

        return string.IsNullOrEmpty(slot.displayName)
            ? slot.id.ToUpperInvariant()
            : slot.displayName.ToUpperInvariant();
    }

    private void Apply()
    {
        candidate.parts.Clear();
        candidate.equippedPassives.Clear();
        candidate.equippedActives.Clear();

        for (int i = 0; i < sets.Count; i++)
        {
            OptionSet set = sets[i];
            GarageOption option = set.Options[set.Index];

            if (set.Kind == Kind.Type)
                candidate.type = option.Type;
            else if (set.Kind == Kind.Element)
                candidate.baseElement = option.Element;
            else if (option.Part != null)
                candidate.parts.Add(option.Part);
        }

        if (template != null)
        {
            for (int i = 0; i < template.equippedPassives.Count; i++)
                candidate.equippedPassives.Add(template.equippedPassives[i]);

            for (int i = 0; i < template.equippedActives.Count; i++)
                candidate.equippedActives.Add(template.equippedActives[i]);
        }

        errors.Clear();
        errors.AddRange(BeyBlueprintValidator.Validate(candidate, tuning));
    }
}
