#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public sealed class BattleDebugWindow : EditorWindow
{
    private const int MaxImpacts = 30;

    private readonly List<ImpactEvent> impacts =
        new List<ImpactEvent>();

    private Vector2 scroll;

    [MenuItem("Bey/Battle Debug")]
    private static void Open()
    {
        GetWindow<BattleDebugWindow>("Battle Debug");
    }

    [MenuItem("Bey/Add Debug Overlay")]
    private static void AddOverlay()
    {
        GameObject overlay = new GameObject("BattleDebugOverlay");

        Undo.RegisterCreatedObjectUndo(
            overlay,
            "Add Battle Debug Overlay");

        overlay.AddComponent<BattleDebugOverlay>();
        Selection.activeGameObject = overlay;
    }

    private void OnEnable()
    {
        BattleEvents.ImpactResolved -= OnImpactResolved;
        BattleEvents.ImpactResolved += OnImpactResolved;
    }

    private void OnDisable()
    {
        BattleEvents.ImpactResolved -= OnImpactResolved;
    }

    private void OnImpactResolved(ImpactEvent impact)
    {
        impacts.Insert(0, impact);

        if (impacts.Count > MaxImpacts)
            impacts.RemoveAt(impacts.Count - 1);

        Repaint();
    }

    private void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        BeyController[] beys = UnityEngine.Object.FindObjectsByType<BeyController>(
            FindObjectsSortMode.None);

        if (beys.Length == 0)
        {
            EditorGUILayout.HelpBox(
                "Open BattleScene and press Play to inspect beys.",
                MessageType.Info);
        }

        for (int i = 0; i < beys.Length; i++)
            DrawBey(beys[i]);

        DrawMatchup(beys);
        DrawImpacts();

        EditorGUILayout.EndScrollView();
    }

    private static void DrawBey(BeyController bey)
    {
        BeyData data = bey.Data;

        if (data == null || data.Base == null)
            return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(data.name, EditorStyles.boldLabel);

        EditorGUILayout.LabelField(
            "Type",
            data.Base.type != null ? data.Base.type.id : "none");

        EditorGUILayout.LabelField(
            "Element",
            data.Elements.Current != null
                ? data.Elements.Current.id
                : "none");

        EditorGUILayout.LabelField(
            "RPM",
            $"{data.CurrentRPM:F0} / {data.Base.maxRPM:F0}");

        EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);

        foreach (StatId stat in Enum.GetValues(typeof(StatId)))
        {
            EditorGUILayout.LabelField(
                stat.ToString(),
                data.Stats.Get(stat).ToString("F2"));

            foreach (StatContribution contribution in data.Stats.Explain(stat))
            {
                EditorGUILayout.LabelField(
                    "  " + contribution.SourceId,
                    $"{contribution.Op} {contribution.Value:F2}");
            }
        }

        EditorGUILayout.LabelField("Element claims", EditorStyles.boldLabel);

        foreach (ElementClaim claim in data.Elements.Explain())
        {
            EditorGUILayout.LabelField(
                claim.SourceId,
                $"{(claim.Element != null ? claim.Element.id : "none")} @ {claim.Priority}");
        }

        AbilityRunner runner = bey.GetComponent<AbilityRunner>();

        if (runner == null)
            return;

        EditorGUILayout.LabelField("Abilities", EditorStyles.boldLabel);

        List<string> lines = runner.DescribeAbilities();

        for (int i = 0; i < lines.Count; i++)
            EditorGUILayout.LabelField(lines[i]);
    }

    private static void DrawMatchup(BeyController[] beys)
    {
        if (beys.Length < 2 ||
            beys[0].Data == null ||
            beys[1].Data == null)
        {
            return;
        }

        ElementDefinition a = beys[0].Data.Elements.Current;
        ElementDefinition b = beys[1].Data.Elements.Current;

        float forward = MatchupResolver.Get(
            ContentRegistry.Matchups,
            a,
            b);

        float reverse = MatchupResolver.Get(
            ContentRegistry.Matchups,
            b,
            a);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Matchup", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            $"{ElementId(a)} -> {ElementId(b)}",
            $"x{forward:F2}");
        EditorGUILayout.LabelField(
            $"{ElementId(b)} -> {ElementId(a)}",
            $"x{reverse:F2}");
    }

    private void DrawImpacts()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Impact trace", EditorStyles.boldLabel);

        if (impacts.Count == 0)
        {
            EditorGUILayout.LabelField("No impacts recorded.");
            return;
        }

        for (int i = 0; i < impacts.Count; i++)
        {
            ImpactEvent impact = impacts[i];

            string attacker = impact.Attacker != null
                ? impact.Attacker.name
                : "?";

            string defender = impact.Defender != null
                ? impact.Defender.name
                : "?";

            EditorGUILayout.LabelField(
                $"{attacker} -> {defender}",
                $"dmgA {impact.Result.DamageToA:F0} " +
                $"dmgB {impact.Result.DamageToB:F0} " +
                $"speed {impact.Speed:F1}");
        }
    }

    private static string ElementId(ElementDefinition element)
    {
        return element != null ? element.id : "none";
    }
}
#endif
