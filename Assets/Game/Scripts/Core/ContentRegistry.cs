using System.Collections.Generic;
using UnityEngine;

public static class ContentRegistry
{
    private static readonly Dictionary<string, ContentDefinition> byId =
        new Dictionary<string, ContentDefinition>();

    private static ContentManifest manifest;
    private static CombatTuning defaultTuning;

    public static CombatTuning Tuning
    {
        get
        {
            if (manifest != null && manifest.combatTuning != null)
                return manifest.combatTuning;

            if (defaultTuning == null)
                defaultTuning = ScriptableObject.CreateInstance<CombatTuning>();

            return defaultTuning;
        }
    }

    public static MatchupTable Matchups
    {
        get
        {
            if (manifest != null && manifest.matchupTable != null)
                return manifest.matchupTable;

            return null;
        }
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Clear();
        defaultTuning = null;
    }

    public static void Initialize(ContentManifest contentManifest)
    {
        Clear();

        manifest = contentManifest;

        if (manifest == null)
            return;

        for (int i = 0; i < manifest.assets.Count; i++)
        {
            ContentDefinition asset = manifest.assets[i];

            if (asset == null || string.IsNullOrEmpty(asset.id))
                continue;

            byId[asset.id] = asset;
        }
    }

    public static void Clear()
    {
        byId.Clear();
        manifest = null;
    }

    public static T Get<T>(string id) where T : ContentDefinition
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return byId.TryGetValue(id, out ContentDefinition asset)
            ? asset as T
            : null;
    }

    public static IEnumerable<T> All<T>() where T : ContentDefinition
    {
        foreach (ContentDefinition asset in byId.Values)
        {
            if (asset is T typed)
                yield return typed;
        }
    }
}
