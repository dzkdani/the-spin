using System.Collections.Generic;

public sealed class PendingImpactRule : IImpactRule
{
    public int Order => 400;

    public void Apply(ImpactState state, ImpactInput input)
    {
        state.PowerA = ApplyMods(
            input.PendingA,
            state.PowerA,
            state.PendingUsedA,
            input.Now);

        state.PowerB = ApplyMods(
            input.PendingB,
            state.PowerB,
            state.PendingUsedB,
            input.Now);
    }

    private static float ApplyMods(
        List<PendingImpactMod> mods,
        float power,
        List<string> used,
        float now)
    {
        if (mods == null)
            return power;

        for (int i = 0; i < mods.Count; i++)
        {
            PendingImpactMod mod = mods[i];

            if (mod.Remaining <= 0)
                continue;

            if (now >= mod.ExpiresAt)
                continue;

            power *= mod.Multiplier;
            used.Add(mod.SourceId);
        }

        return power;
    }
}
