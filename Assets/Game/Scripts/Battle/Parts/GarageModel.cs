using System.Collections.Generic;
using UnityEngine;

public readonly struct GarageOption
{
    public readonly string Label;
    public readonly BeyTypeDefinition Type;
    public readonly ElementDefinition Element;
    public readonly PartDefinition Part;
    public readonly AbilityDefinition Ability;

    private GarageOption(
        string label,
        BeyTypeDefinition type,
        ElementDefinition element,
        PartDefinition part,
        AbilityDefinition ability)
    {
        Label = label;
        Type = type;
        Element = element;
        Part = part;
        Ability = ability;
    }

    public static GarageOption None =>
        new GarageOption("NONE", null, null, null, null);

    public static GarageOption ForType(BeyTypeDefinition type)
    {
        return new GarageOption(LabelOf(type), type, null, null, null);
    }

    public static GarageOption ForElement(ElementDefinition element)
    {
        return new GarageOption(LabelOf(element), null, element, null, null);
    }

    public static GarageOption ForPart(PartDefinition part)
    {
        return new GarageOption(LabelOf(part), null, null, part, null);
    }

    public static GarageOption ForAbility(AbilityDefinition ability)
    {
        return new GarageOption(LabelOf(ability), null, null, null, ability);
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
        Slot,
        Active,
        Passive
    }

    private sealed class OptionSet
    {
        public string Label;
        public Kind Kind;
        public List<GarageOption> Options = new List<GarageOption>();
        public int Index;
        public AbilityDefinition CurrentAbility;
    }

    private const float MaxActiveRpmCost = 0.5f;

    private readonly BeyBlueprint template;
    private readonly CombatTuning tuning;
    private readonly IReadOnlyList<AbilityDefinition> allAbilities;
    private readonly List<OptionSet> sets = new List<OptionSet>();
    private readonly List<string> errors = new List<string>();
    private readonly BeyBlueprint candidate;

    public GarageModel(
        BeyBlueprint template,
        CombatTuning tuning,
        IReadOnlyList<BeyTypeDefinition> types,
        IReadOnlyList<ElementDefinition> elements,
        IReadOnlyList<PartDefinition> parts,
        IReadOnlyList<AbilityDefinition> abilities)
    {
        this.template = template;
        this.tuning = tuning;
        allAbilities = abilities ?? new List<AbilityDefinition>();

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

        AddAbilitySet(
            "ACTIVE",
            Kind.Active,
            EquippedAt(template, true, 0));

        AddAbilitySet(
            "PASSIVE 1",
            Kind.Passive,
            EquippedAt(template, false, 0));

        AddAbilitySet(
            "PASSIVE 2",
            Kind.Passive,
            EquippedAt(template, false, 1));

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

        List<AbilityDefinition> abilities =
            new List<AbilityDefinition>(
                ContentRegistry.All<AbilityDefinition>());

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

        abilities.Sort((a, b) =>
        {
            int byKind = a.kind.CompareTo(b.kind);

            return byKind != 0
                ? byKind
                : string.CompareOrdinal(a.id, b.id);
        });

        return new GarageModel(
            template,
            ContentRegistry.Tuning,
            types,
            elements,
            parts,
            abilities);
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

        if (set.Kind == Kind.Active || set.Kind == Kind.Passive)
            set.CurrentAbility = set.Options[set.Index].Ability;

        Apply();
    }

    private void AddAbilitySet(string label, Kind kind, AbilityDefinition selected)
    {
        sets.Add(new OptionSet
        {
            Label = label,
            Kind = kind,
            CurrentAbility = selected
        });
    }

    private static AbilityDefinition EquippedAt(
        BeyBlueprint blueprint,
        bool active,
        int index)
    {
        if (blueprint == null)
            return null;

        IReadOnlyList<AbilityDefinition> list = active
            ? blueprint.equippedActives
            : blueprint.equippedPassives;

        return list.Count > index ? list[index] : null;
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

        BeyTypeDefinition type = null;
        ElementDefinition element = null;
        List<PartDefinition> selectedParts = new List<PartDefinition>();

        for (int i = 0; i < sets.Count; i++)
        {
            OptionSet set = sets[i];

            if (set.Options.Count == 0)
                continue;

            GarageOption option = set.Options[
                Mathf.Clamp(set.Index, 0, set.Options.Count - 1)];

            if (set.Kind == Kind.Type)
                type = option.Type;
            else if (set.Kind == Kind.Element)
                element = option.Element;
            else if (set.Kind == Kind.Slot && option.Part != null)
                selectedParts.Add(option.Part);
        }

        candidate.type = type;
        candidate.baseElement = element;
        candidate.chassis = type != null && type.chassis != null
            ? type.chassis
            : template != null ? template.chassis : null;

        for (int i = 0; i < selectedParts.Count; i++)
            candidate.parts.Add(selectedParts[i]);

        HashSet<string> forbidden = new HashSet<string>();

        if (type != null && !string.IsNullOrEmpty(type.innatePassiveId))
            forbidden.Add(type.innatePassiveId);

        for (int i = 0; i < selectedParts.Count; i++)
        {
            PartDefinition part = selectedParts[i];

            for (int j = 0; j < part.grantedAbilities.Count; j++)
            {
                AbilityDefinition granted = part.grantedAbilities[j];

                if (granted != null && !string.IsNullOrEmpty(granted.id))
                    forbidden.Add(granted.id);
            }
        }

        HashSet<string> chosen = new HashSet<string>();

        for (int i = 0; i < sets.Count; i++)
        {
            OptionSet set = sets[i];

            if (set.Kind != Kind.Active && set.Kind != Kind.Passive)
                continue;

            List<GarageOption> options = new List<GarageOption>
            {
                GarageOption.None
            };

            for (int j = 0; j < allAbilities.Count; j++)
            {
                AbilityDefinition ability = allAbilities[j];

                if (string.IsNullOrEmpty(ability.id) ||
                    forbidden.Contains(ability.id) ||
                    chosen.Contains(ability.id))
                    continue;

                if (set.Kind == Kind.Active)
                {
                    if (ability.kind != AbilityKind.Active ||
                        ability.active.rpmCost > MaxActiveRpmCost)
                        continue;
                }
                else if (ability.kind != AbilityKind.Passive)
                {
                    continue;
                }

                options.Add(GarageOption.ForAbility(ability));
            }

            set.Options = options;

            int index = -1;

            for (int j = 0; j < options.Count; j++)
            {
                if (options[j].Ability == set.CurrentAbility)
                {
                    index = j;
                    break;
                }
            }

            if (index < 0)
                set.CurrentAbility = null;

            set.Index = index >= 0 ? index : 0;

            if (set.Index > 0)
                chosen.Add(options[set.Index].Ability.id);
        }

        candidate.equippedActives.Clear();
        candidate.equippedPassives.Clear();

        for (int i = 0; i < sets.Count; i++)
        {
            OptionSet set = sets[i];

            if (set.Options.Count == 0)
                continue;

            AbilityDefinition ability =
                set.Options[Mathf.Clamp(set.Index, 0, set.Options.Count - 1)].Ability;

            if (ability == null)
                continue;

            if (set.Kind == Kind.Active)
                candidate.equippedActives.Add(ability);
            else if (set.Kind == Kind.Passive)
                candidate.equippedPassives.Add(ability);
        }

        errors.Clear();
        errors.AddRange(BeyBlueprintValidator.Validate(candidate, tuning));
    }
}
