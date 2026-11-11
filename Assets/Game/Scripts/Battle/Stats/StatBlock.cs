using System;
using System.Collections.Generic;

public sealed class StatBlock : IStatView
{
    private readonly Dictionary<StatId, float> baseValues =
        new Dictionary<StatId, float>();

    private readonly List<StatModifier> modifiers =
        new List<StatModifier>();

    public static bool IsScopedStat(StatId stat)
    {
        return stat == StatId.AbilityCost ||
            stat == StatId.AbilityCooldown ||
            stat == StatId.AbilityPower;
    }

    public void SetBase(StatId stat, float value)
    {
        baseValues[stat] = value;
    }

    public StatBlock Clone()
    {
        StatBlock clone = new StatBlock();

        foreach (KeyValuePair<StatId, float> pair in baseValues)
            clone.baseValues[pair.Key] = pair.Value;

        clone.modifiers.AddRange(modifiers);

        return clone;
    }

    public float GetBase(StatId stat)
    {
        return baseValues.TryGetValue(stat, out float value)
            ? value
            : 0f;
    }

    public void Add(StatModifier modifier, float now)
    {
        if (!IsScopedStat(modifier.Stat))
        {
            if (!string.IsNullOrEmpty(modifier.Scope))
            {
                throw new ArgumentException(
                    $"{modifier.Stat} does not accept a scope.",
                    nameof(modifier));
            }

            modifier = new StatModifier(
                modifier.Stat,
                modifier.Op,
                modifier.Value,
                modifier.SourceId,
                null,
                modifier.ExpiresAt,
                modifier.Stacking,
                modifier.MaxStacks);
        }

        switch (modifier.Stacking)
        {
            case StackPolicy.Ignore:
                if (HasMatchingSource(modifier))
                    return;
                break;

            case StackPolicy.Refresh:
                int refreshIndex = FindMatchingSource(modifier);

                if (refreshIndex >= 0)
                {
                    modifiers[refreshIndex] =
                        modifiers[refreshIndex].WithExpiry(modifier.ExpiresAt);

                    return;
                }
                break;

            case StackPolicy.Stack:
                if (CountMatchingSource(modifier) >= modifier.MaxStacks)
                    return;
                break;
        }

        modifiers.Add(modifier);
    }

    public void Tick(float now)
    {
        modifiers.RemoveAll(modifier => modifier.IsExpired(now));
    }

    public void RemoveBySource(string sourceId)
    {
        modifiers.RemoveAll(modifier => modifier.SourceId == sourceId);
    }

    public void RemoveBySourcePrefix(string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
            return;

        modifiers.RemoveAll(
            modifier =>
                modifier.SourceId != null &&
                modifier.SourceId.StartsWith(prefix));
    }

    public float Get(StatId stat, string scope = null)
    {
        if (scope != null && !IsScopedStat(stat))
        {
            throw new ArgumentException(
                $"{stat} does not accept a scope.",
                nameof(scope));
        }

        float add = 0f;
        float percentAdd = 0f;
        float multiply = 1f;

        for (int i = 0; i < modifiers.Count; i++)
        {
            StatModifier modifier = modifiers[i];

            if (modifier.Stat != stat || !ScopeMatches(modifier, scope))
                continue;

            switch (modifier.Op)
            {
                case ModOp.Add:
                    add += modifier.Value;
                    break;

                case ModOp.PercentAdd:
                    percentAdd += modifier.Value;
                    break;

                case ModOp.Multiply:
                    multiply *= modifier.Value;
                    break;
            }
        }

        return (GetBase(stat) + add) * (1f + percentAdd) * multiply;
    }

    public IEnumerable<StatContribution> Explain(StatId stat, string scope = null)
    {
        if (scope != null && !IsScopedStat(stat))
        {
            throw new ArgumentException(
                $"{stat} does not accept a scope.",
                nameof(scope));
        }

        List<StatContribution> contributions = new List<StatContribution>();

        for (int i = 0; i < modifiers.Count; i++)
        {
            StatModifier modifier = modifiers[i];

            if (modifier.Stat != stat || !ScopeMatches(modifier, scope))
                continue;

            contributions.Add(new StatContribution(
                modifier.SourceId,
                modifier.Op,
                modifier.Value,
                modifier.Scope,
                modifier.ExpiresAt));
        }

        return contributions;
    }

    private bool HasMatchingSource(StatModifier modifier)
    {
        return FindMatchingSource(modifier) >= 0;
    }

    private int FindMatchingSource(StatModifier modifier)
    {
        for (int i = 0; i < modifiers.Count; i++)
        {
            if (SameSource(modifiers[i], modifier))
                return i;
        }

        return -1;
    }

    private int CountMatchingSource(StatModifier modifier)
    {
        int count = 0;

        for (int i = 0; i < modifiers.Count; i++)
        {
            if (SameSource(modifiers[i], modifier))
                count++;
        }

        return count;
    }

    private static bool SameSource(StatModifier a, StatModifier b)
    {
        return a.Stat == b.Stat &&
            a.SourceId == b.SourceId &&
            a.Scope == b.Scope;
    }

    private bool ScopeMatches(StatModifier modifier, string scope)
    {
        if (!IsScopedStat(modifier.Stat))
            return modifier.Scope == null;

        return modifier.Scope == null || modifier.Scope == scope;
    }
}
