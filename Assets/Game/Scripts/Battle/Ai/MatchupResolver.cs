public static class MatchupResolver
{
    public static float Get(
        MatchupTable table,
        ElementDefinition attacker,
        ElementDefinition defender)
    {
        if (table == null || attacker == null || defender == null)
            return 1f;

        if (attacker.isNeutral || defender.isNeutral)
            return 1f;

        for (int i = 0; i < table.entries.Count; i++)
        {
            MatchupTable.Entry entry = table.entries[i];

            if (entry.attacker == attacker && entry.defender == defender)
                return entry.multiplier;
        }

        return 1f;
    }
}
