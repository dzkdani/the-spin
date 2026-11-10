using System.Collections.Generic;
using UnityEngine;

public static class BalanceActives
{
    public readonly struct UseResult
    {
        public readonly bool Used;
        public readonly bool Dashing;
        public readonly float RpmAfter;
        public readonly float CooldownUntil;
        public readonly List<PendingImpactMod> Pending;

        public UseResult(
            bool used,
            bool dashing,
            float rpmAfter,
            float cooldownUntil,
            List<PendingImpactMod> pending)
        {
            Used = used;
            Dashing = dashing;
            RpmAfter = rpmAfter;
            CooldownUntil = cooldownUntil;
            Pending = pending;
        }
    }

    public static UseResult TryUse(
        BalanceBuild build,
        int index,
        float rpm,
        float cooldownUntil,
        float now)
    {
        if (build == null || build.Actives == null ||
            index < 0 || index >= build.Actives.Count)
        {
            return Reject(rpm, cooldownUntil);
        }

        AbilityDefinition ability = build.Actives[index];

        if (ability == null || ability.kind != AbilityKind.Active ||
            now < cooldownUntil)
        {
            return Reject(rpm, cooldownUntil);
        }

        string scope = ability.tags != null && ability.tags.Length > 0
            ? ability.tags[0]
            : null;

        float cost = ability.active.rpmCost *
            build.Stats.Get(StatId.AbilityCost, scope);

        if (build.MaxRpm <= 0f || rpm / build.MaxRpm < cost)
            return Reject(rpm, cooldownUntil);

        float rpmAfter =
            rpm - ability.active.rpmCost * build.MaxRpm;

        float nextCooldown =
            now + ability.active.cooldown *
            build.Stats.Get(StatId.AbilityCooldown, scope);

        float power = build.Stats.Get(StatId.AbilityPower, scope);
        List<PendingImpactMod> pending = new List<PendingImpactMod>();
        bool dashing = false;

        for (int i = 0; i < ability.effects.Count; i++)
        {
            if (ability.effects[i] is ForceDashEffect force)
            {
                dashing = true;

                pending.Add(new PendingImpactMod(
                    "active:" + ability.id + ":dash",
                    force.power * power,
                    1,
                    now + 0.25f));
            }
            else if (ability.effects[i] is ScaleNextImpactEffect scale)
            {
                pending.Add(new PendingImpactMod(
                    "active:" + ability.id + ":impact",
                    1f + (scale.multiplier - 1f) * power,
                    scale.count,
                    scale.duration <= 0f
                        ? float.PositiveInfinity
                        : now + scale.duration));
            }
            else if (ability.effects[i] is DrainRpmEffect drain)
            {
                rpmAfter -= drain.normalizedAmount * build.MaxRpm;
            }
        }

        return new UseResult(
            true,
            dashing,
            Mathf.Max(0f, rpmAfter),
            nextCooldown,
            pending);
    }

    private static UseResult Reject(float rpm, float cooldownUntil)
    {
        return new UseResult(false, false, rpm, cooldownUntil, null);
    }
}
