using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class BattleHUD : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (SceneManager.GetActiveScene().name != "BattleScene")
            return;

        if (FindFirstObjectByType<BattleHUD>() != null)
            return;

        GameObject host = new GameObject("BattleHUD");
        host.AddComponent<BattleHUD>();
    }

    private GameManager game;

    private void Awake()
    {
        game = FindFirstObjectByType<GameManager>();
    }

    private void OnGUI()
    {
        if (game == null)
            return;

        GUILayout.BeginArea(new Rect(10f, 10f, 440f, 230f), GUI.skin.box);

        GUILayout.Label(
            $"WAVE {game.Wave}/{game.TotalEnemies}");

        DrawMatchup();
        DrawStatuses();
        DrawAbilities();

        if (game.LastFinishReason.HasValue)
        {
            GUILayout.Label(
                $"RESULT: {game.LastFinishReason.Value}  " +
                $"RPM {game.PlayerRpm:F0}");
        }
        else if (Time.time - game.WaveResetAt < 2f)
        {
            GUILayout.Label("WAVE CLEARED · RPM RESTORED");
        }
        else if (game.Wave == 1)
        {
            GUILayout.Label("MOUSE move  ·  CLICK dash");
        }

        GUILayout.EndArea();
    }

    private void DrawMatchup()
    {
        BeyData player = game.Player != null
            ? game.Player.Data
            : null;

        BeyData enemy = game.CurrentEnemy;

        if (player == null || enemy == null)
            return;

        BeyBlueprint blueprint = enemy.Blueprint;

        string enemyName =
            blueprint != null && !string.IsNullOrEmpty(blueprint.displayName)
                ? blueprint.displayName
                : "Enemy";

        string enemyType =
            blueprint != null && blueprint.type != null
                ? blueprint.type.id.ToUpperInvariant()
                : string.Empty;

        GUILayout.Label($"{enemyName}  [{enemyType}]");

        ElementDefinition from = player.Elements.Current;
        ElementDefinition to = enemy.Elements.Current;

        float multiplier = MatchupResolver.Get(
            ContentRegistry.Matchups,
            from,
            to);

        GUILayout.Label(
            $"{Name(from)} -> {Name(to)}  x{multiplier:F2}");
    }

    private void DrawStatuses()
    {
        HashSet<string> statuses = new HashSet<string>();

        if (game.Player != null && game.Player.Data != null)
            AddStatuses(statuses, game.Player.Data);

        if (game.CurrentEnemy != null)
            AddStatuses(statuses, game.CurrentEnemy);

        if (statuses.Count == 0)
            return;

        GUILayout.Label("STATUS  " + string.Join(", ", statuses));
    }

    private void DrawAbilities()
    {
        if (game.Player == null)
            return;

        AbilityRunner runner =
            game.Player.GetComponent<AbilityRunner>();

        if (runner == null)
            return;

        List<string> lines = runner.DescribeAbilities();

        for (int i = 0; i < lines.Count; i++)
            GUILayout.Label(lines[i]);
    }

    private static void AddStatuses(
        HashSet<string> statuses,
        BeyData data)
    {
        foreach (StatId stat in Enum.GetValues(typeof(StatId)))
        {
            foreach (StatContribution contribution in data.Stats.Explain(stat))
            {
                string source = contribution.SourceId;

                if (source == null || !source.StartsWith("element:"))
                    continue;

                statuses.Add(source.Substring("element:".Length));
            }
        }
    }

    private static string Name(ElementDefinition element)
    {
        return element !=null ? element.id : "none";
    }
}
