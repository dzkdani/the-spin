using System.Collections.Generic;
using System.Globalization;
using System.Text;

public sealed class PartSimulation
{
    private const float StartVariance = 0.2f;
    private const float ArenaRadius = 8f;

    public readonly struct Row
    {
        public readonly string PartId;
        public readonly string SlotId;
        public readonly int BudgetCost;
        public readonly float ControlRate;
        public readonly float PartRate;

        public Row(
            string partId,
            string slotId,
            int budgetCost,
            float controlRate,
            float partRate)
        {
            PartId = partId;
            SlotId = slotId;
            BudgetCost = budgetCost;
            ControlRate = controlRate;
            PartRate = partRate;
        }

        public float Delta => PartRate - ControlRate;
    }

    public List<Row> Rows { get; }

    private PartSimulation(List<Row> rows)
    {
        Rows = rows;
    }

    public static PartSimulation Run(
        BalanceBuild control,
        IReadOnlyList<PartDefinition> parts,
        IReadOnlyList<BalanceBuild> opponents,
        int seeds,
        MatchupTable matchups,
        CombatTuning tuning)
    {
        float controlRate = Rate(
            control,
            opponents,
            seeds,
            matchups,
            tuning);

        List<Row> rows = new List<Row>();

        for (int i = 0; i < parts.Count; i++)
        {
            PartDefinition part = parts[i];

            if (part == null)
                continue;

            BalanceBuild build = CreateBuild(control, part);

            rows.Add(new Row(
                part.id,
                part.slot != null ? part.slot.id : string.Empty,
                part.budgetCost,
                controlRate,
                Rate(build, opponents, seeds, matchups, tuning)));
        }

        return new PartSimulation(rows);
    }

    public static BalanceBuild CreateBuild(
        BalanceBuild control,
        PartDefinition part)
    {
        StatBlock stats = control.Stats.Clone();

        string source = "part:" + part.id;

        for (int i = 0; i < part.statModifiers.Count; i++)
        {
            StatModifierSpec spec = part.statModifiers[i];

            stats.Add(
                new StatModifier(
                    spec.stat,
                    spec.op,
                    spec.value,
                    source,
                    spec.scope,
                    float.PositiveInfinity,
                    StackPolicy.Refresh,
                    1),
                0f);
        }

        BalanceBuild build = new BalanceBuild(
            control.Id + "+" + part.id,
            stats,
            control.MaxRpm,
            control.Element,
            control.Innate);

        build.RpmDecayRate = control.RpmDecayRate;

        int elementPriority = 0;

        if (part.elementClaim != null && part.slot != null)
        {
            build.Element = part.elementClaim;
            elementPriority = part.slot.elementPriority;
        }

        for (int i = 0; i < part.grantedAbilities.Count; i++)
        {
            AbilityDefinition ability = part.grantedAbilities[i];

            if (ability == null)
                continue;

            if (ability.kind == AbilityKind.Active &&
                !build.Actives.Exists(active => active.id == ability.id))
            {
                build.Actives.Add(ability);
            }

            string grantSource = $"grant:{part.id}/{ability.id}";

            for (int e = 0; e < ability.effects.Count; e++)
            {
                if (ability.effects[e] is AddModifierEffect modifier)
                {
                    stats.Add(
                        new StatModifier(
                            modifier.stat,
                            modifier.op,
                            modifier.value,
                            grantSource,
                            modifier.scope,
                            float.PositiveInfinity,
                            modifier.stacking,
                            modifier.maxStacks),
                        0f);
                }
                else if (ability.effects[e] is ClaimElementEffect claim &&
                         claim.element != null &&
                         claim.priority >= elementPriority)
                {
                    build.Element = claim.element;
                    elementPriority = claim.priority;
                }
            }
        }

        return build;
    }

    public string ToCsv()
    {
        StringBuilder csv = new StringBuilder();

        csv.AppendLine("part,slot,budget,control,part,delta");

        for (int i = 0; i < Rows.Count; i++)
        {
            Row row = Rows[i];

            csv.Append(row.PartId).Append(',')
                .Append(row.SlotId).Append(',')
                .Append(row.BudgetCost).Append(',')
                .Append(row.ControlRate.ToString("F3", CultureInfo.InvariantCulture)).Append(',')
                .Append(row.PartRate.ToString("F3", CultureInfo.InvariantCulture)).Append(',')
                .Append(row.Delta.ToString("+0.000;-0.000;0.000", CultureInfo.InvariantCulture))
                .AppendLine();
        }

        return csv.ToString();
    }

    public List<string> OutsideSlotBand(float band)
    {
        Dictionary<string, float> averages = new Dictionary<string, float>();
        Dictionary<string, int> counts = new Dictionary<string, int>();

        for (int i = 0; i < Rows.Count; i++)
        {
            Row row = Rows[i];

            averages.TryGetValue(row.SlotId, out float sum);
            counts.TryGetValue(row.SlotId, out int count);

            averages[row.SlotId] = sum + row.Delta;
            counts[row.SlotId] = count + 1;
        }

        List<string> outside = new List<string>();

        for (int i = 0; i < Rows.Count; i++)
        {
            Row row = Rows[i];
            float average = averages[row.SlotId] / counts[row.SlotId];

            if (System.Math.Abs(row.Delta - average) > band)
            {
                outside.Add(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}: {1:+0.0;-0.0;0.0} vs slot {2:+0.0;-0.0;0.0}",
                    row.PartId,
                    row.Delta * 100f,
                    average * 100f));
            }
        }

        return outside;
    }

    private static float Rate(
        BalanceBuild build,
        IReadOnlyList<BalanceBuild> opponents,
        int seeds,
        MatchupTable matchups,
        CombatTuning tuning)
    {
        int wins = 0;
        int games = 0;

        for (int i = 0; i < opponents.Count; i++)
        {
            BalanceSimulator.Result forward = BalanceSimulator.Run(
                build,
                opponents[i],
                seeds,
                matchups,
                tuning,
                StartVariance,
                ArenaRadius);

            BalanceSimulator.Result reverse = BalanceSimulator.Run(
                opponents[i],
                build,
                seeds,
                matchups,
                tuning,
                StartVariance,
                ArenaRadius);

            wins += forward.WinsA + reverse.WinsB;
            games +=
                forward.WinsA +
                forward.WinsB +
                reverse.WinsA +
                reverse.WinsB;
        }

        return games > 0
            ? (float)wins / games
            : 0f;
    }
}
