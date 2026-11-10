using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BattleDebugOverlay : MonoBehaviour
{
    private void OnGUI()
    {
        BeyController[] beys =
            UnityEngine.Object.FindObjectsByType<BeyController>(
                FindObjectsSortMode.None);

        if (beys.Length == 0)
            return;

        GUILayout.BeginArea(
            new Rect(10f, 10f, 520f, 24f + beys.Length * 82f));

        GUILayout.BeginVertical(GUI.skin.box);

        for (int i = 0; i < beys.Length; i++)
            DrawBey(beys[i]);

        if (beys.Length >= 2)
            DrawMatchup(beys[0], beys[1]);

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private static void DrawBey(BeyController bey)
    {
        BeyData data = bey.Data;

        if (data == null || data.Base == null)
            return;

        ElementDefinition element = data.Elements.Current;

        GUILayout.Label(
            $"{data.name}  RPM {data.CurrentRPM:F0}/{data.Base.maxRPM:F0}  " +
            $"element {ElementId(element)}");

        AbilityRunner runner = bey.GetComponent<AbilityRunner>();

        if (runner == null)
            return;

        List<string> lines = runner.DescribeAbilities();

        for (int i = 0; i < lines.Count; i++)
            GUILayout.Label("  " + lines[i]);
    }

    private static void DrawMatchup(
        BeyController a,
        BeyController b)
    {
        if (a.Data == null || b.Data == null)
            return;

        ElementDefinition elementA = a.Data.Elements.Current;
        ElementDefinition elementB = b.Data.Elements.Current;

        float forward = MatchupResolver.Get(
            ContentRegistry.Matchups,
            elementA,
            elementB);

        float reverse = MatchupResolver.Get(
            ContentRegistry.Matchups,
            elementB,
            elementA);

        GUILayout.Label(
            $"matchup {ElementId(elementA)}->{ElementId(elementB)} x{forward:F2}  " +
            $"{ElementId(elementB)}->{ElementId(elementA)} x{reverse:F2}");
    }

    private static string ElementId(ElementDefinition element)
    {
        return element != null ? element.id : "none";
    }
}
