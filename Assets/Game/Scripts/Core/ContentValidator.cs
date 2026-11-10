using System.Collections.Generic;

public static class ContentValidator
{
    public static List<string> Validate(ContentManifest manifest)
    {
        List<string> errors = new List<string>();

        if (manifest == null)
        {
            errors.Add("Content manifest is missing.");
            return errors;
        }

        HashSet<string> ids = new HashSet<string>();

        for (int i = 0; i < manifest.assets.Count; i++)
        {
            ContentDefinition asset = manifest.assets[i];

            if (asset == null)
            {
                errors.Add($"Content entry {i} is null.");
                continue;
            }

            if (string.IsNullOrEmpty(asset.id))
            {
                errors.Add($"Content asset '{asset.name}' has no id.");
                continue;
            }

            if (!ids.Add(asset.id))
                errors.Add($"Duplicate content id '{asset.id}'.");
        }

        if (manifest.matchupTable != null)
            ValidateMatchups(manifest.matchupTable, errors);

        return errors;
    }

    private static void ValidateMatchups(
        MatchupTable table,
        List<string> errors)
    {
        HashSet<string> pairs = new HashSet<string>();

        for (int i = 0; i < table.entries.Count; i++)
        {
            MatchupTable.Entry entry = table.entries[i];

            if (entry.attacker == null || entry.defender == null)
            {
                errors.Add($"Matchup entry {i} has a missing element.");
                continue;
            }

            if (entry.attacker == entry.defender)
            {
                errors.Add($"Matchup entry {i} is a self-pair.");
                continue;
            }

            if (entry.attacker.isNeutral || entry.defender.isNeutral)
            {
                errors.Add($"Matchup entry {i} involves Neutral.");
                continue;
            }

            string key = entry.attacker.id + ">" + entry.defender.id;

            if (!pairs.Add(key))
                errors.Add($"Duplicate matchup '{key}'.");
        }
    }
}
