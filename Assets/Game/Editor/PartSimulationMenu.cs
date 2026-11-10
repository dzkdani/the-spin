#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class PartSimulationMenu
{
    [MenuItem("Bey/Run Part Simulation")]
    public static void Run()
    {
        ContentManifest manifest = AssetDatabase.LoadAssetAtPath<ContentManifest>(
            "Assets/Game/Datas/ContentManifest.asset");

        if (manifest == null)
        {
            Debug.LogError("ContentManifest.asset was not found.");
            return;
        }

        ContentRegistry.Initialize(manifest);

        List<BalanceBuild> builds = BalanceCatalog.CreateReferenceBuilds();
        BalanceBuild control = builds.Find(build => build.Id == "bal_neutral");

        if (control == null)
        {
            Debug.LogError("Missing bal_neutral reference build.");
            return;
        }

        List<PartDefinition> parts = LoadParts();

        PartSimulation simulation = PartSimulation.Run(
            control,
            parts,
            builds,
            25,
            manifest.matchupTable,
            manifest.combatTuning);

        Directory.CreateDirectory("Logs");
        File.WriteAllText("Logs/PartSimulation.csv", simulation.ToCsv());

        Debug.Log(
            $"Part simulation: {simulation.Rows.Count} parts, 25 seeds, " +
            "control bal_neutral, CSV at Logs/PartSimulation.csv.");

        for (int i = 0; i < simulation.Rows.Count; i++)
        {
            PartSimulation.Row row = simulation.Rows[i];

            Debug.Log(
                $"{row.PartId} ({row.SlotId}, budget {row.BudgetCost}): " +
                $"{row.Delta * 100f:+0.0;-0.0;0.0} points.");
        }

        List<string> outside = simulation.OutsideSlotBand(0.05f);

        Debug.Log($"Outside slot band: {outside.Count}.");

        for (int i = 0; i < outside.Count; i++)
            Debug.Log("Out of band: " + outside[i]);

        ContentRegistry.Clear();
    }

    private static List<PartDefinition> LoadParts()
    {
        string[] guids = AssetDatabase.FindAssets(
            "t:PartDefinition",
            new[] { "Assets/Game/Datas/Parts" });

        List<PartDefinition> parts = new List<PartDefinition>();

        for (int i = 0; i < guids.Length; i++)
        {
            PartDefinition part = AssetDatabase.LoadAssetAtPath<PartDefinition>(
                AssetDatabase.GUIDToAssetPath(guids[i]));

            if (part != null)
                parts.Add(part);
        }

        parts.Sort((a, b) => string.CompareOrdinal(a.id, b.id));

        return parts;
    }
}
#endif
