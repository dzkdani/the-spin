using System.Collections.Generic;

public static class BalanceInnates
{
    public static void Apply(BalanceBuild build, float now)
    {
        if (build == null || build.Innate == null)
            return;

        string source = "innate:" + build.Innate.id;

        for (int i = 0; i < build.Innate.effects.Count; i++)
        {
            if (!(build.Innate.effects[i] is AddModifierEffect effect))
                continue;

            float expiresAt = effect.duration <= 0f
                ? float.PositiveInfinity
                : now + effect.duration;

            build.Stats.Add(
                new StatModifier(
                    effect.stat,
                    effect.op,
                    effect.value,
                    source,
                    effect.scope,
                    expiresAt,
                    effect.stacking,
                    effect.maxStacks),
                now);
        }
    }

    public static void AddDashPending(
        BalanceBuild build,
        List<PendingImpactMod> target,
        float abilityPower,
        float now)
    {
        if (build == null || build.Innate == null || target == null)
            return;

        string source = "innate:" + build.Innate.id;

        for (int i = 0; i < build.Innate.effects.Count; i++)
        {
            if (!(build.Innate.effects[i] is ScaleNextImpactEffect effect))
                continue;

            target.Add(new PendingImpactMod(
                source,
                1f + (effect.multiplier - 1f) * abilityPower,
                effect.count,
                effect.duration <= 0f
                    ? float.PositiveInfinity
                    : now + effect.duration));
        }
    }
}
