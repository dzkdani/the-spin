using System.Collections.Generic;

public static class BalanceCatalog
{
    public static List<BalanceBuild> CreateReferenceBuilds()
    {
        List<BeyTypeDefinition> types =
            new List<BeyTypeDefinition>(ContentRegistry.All<BeyTypeDefinition>());

        List<ElementDefinition> elements =
            new List<ElementDefinition>(ContentRegistry.All<ElementDefinition>());

        types.Sort((a, b) => string.CompareOrdinal(a.id, b.id));
        elements.Sort((a, b) => string.CompareOrdinal(a.id, b.id));

        List<BalanceBuild> builds = new List<BalanceBuild>();

        for (int t = 0; t < types.Count; t++)
        {
            for (int e = 0; e < elements.Count; e++)
            {
                StatBlock stats = new StatBlock();

                stats.SetBase(StatId.Attack, 10f);
                stats.SetBase(StatId.Weight, 5f);
                stats.SetBase(StatId.Defense, 0f);
                stats.SetBase(StatId.RpmDecay, 1f);
                stats.SetBase(StatId.MatchupScale, 1f);
                stats.SetBase(StatId.TipFriction, 1f);
                stats.SetBase(StatId.AbilityPower, 1f);

                types[t].ApplyTo(stats, 0f);

                AbilityDefinition innate =
                    ContentRegistry.Get<AbilityDefinition>(
                        types[t].innatePassiveId);

                BalanceBuild build = new BalanceBuild(
                    types[t].id + "_" + elements[e].id,
                    stats,
                    3000f,
                    elements[e],
                    innate);

                BalanceInnates.Apply(build, 0f);

                builds.Add(build);
            }
        }

        return builds;
    }
}
