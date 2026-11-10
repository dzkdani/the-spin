using System.Collections.Generic;
using System.Globalization;
using System.Text;

public sealed class BalanceMatrix
{
    private readonly int[,] wins;
    private readonly int[,] losses;
    private readonly int[,] draws;

    public IReadOnlyList<BalanceBuild> Builds { get; }

    public BalanceMatrix(
        IReadOnlyList<BalanceBuild> builds,
        int[,] wins,
        int[,] losses,
        int[,] draws)
    {
        Builds = builds;
        this.wins = wins;
        this.losses = losses;
        this.draws = draws;
    }

    public static BalanceMatrix Run(
        IReadOnlyList<BalanceBuild> builds,
        int seeds,
        MatchupTable matchups,
        CombatTuning tuning)
    {
        int count = builds.Count;
        int[,] wins = new int[count, count];
        int[,] losses = new int[count, count];
        int[,] draws = new int[count, count];

        for (int a = 0; a < count; a++)
        {
            for (int b = a + 1; b < count; b++)
            {
                BalanceSimulator.Result forward = BalanceSimulator.Run(
                    builds[a],
                    builds[b],
                    seeds,
                    matchups,
                    tuning);

                BalanceSimulator.Result reverse = BalanceSimulator.Run(
                    builds[b],
                    builds[a],
                    seeds,
                    matchups,
                    tuning);

                wins[a, b] = forward.WinsA + reverse.WinsB;
                losses[a, b] = forward.WinsB + reverse.WinsA;
                draws[a, b] = forward.Draws + reverse.Draws;

                wins[b, a] = losses[a, b];
                losses[b, a] = wins[a, b];
                draws[b, a] = draws[a, b];
            }
        }

        return new BalanceMatrix(builds, wins, losses, draws);
    }

    public float WinRate(int buildIndex)
    {
        int won = 0;
        int played = 0;

        for (int b = 0; b < Builds.Count; b++)
        {
            won += wins[buildIndex, b];
            played += wins[buildIndex, b] + losses[buildIndex, b];
        }

        return played > 0
            ? (float)won / played
            : 0f;
    }

    public List<string> BuildsOutsideBand(float min, float max)
    {
        List<string> outside = new List<string>();

        for (int i = 0; i < Builds.Count; i++)
        {
            float rate = WinRate(i);

            if (rate < min || rate > max)
            {
                outside.Add(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}: {1:P0}",
                    Builds[i].Id,
                    rate));
            }
        }

        return outside;
    }

    public string ToCsv()
    {
        StringBuilder csv = new StringBuilder();

        csv.Append("build");

        for (int i = 0; i < Builds.Count; i++)
            csv.Append(',').Append(Builds[i].Id);

        csv.AppendLine();

        for (int a = 0; a < Builds.Count; a++)
        {
            csv.Append(Builds[a].Id);

            for (int b = 0; b < Builds.Count; b++)
            {
                csv.Append(',');

                if (a == b)
                {
                    csv.Append('-');
                    continue;
                }

                float rate = wins[a, b] + losses[a, b] > 0
                    ? (float)wins[a, b] / (wins[a, b] + losses[a, b])
                    : 0f;

                csv.Append(rate.ToString("F3", CultureInfo.InvariantCulture));
            }

            csv.AppendLine();
        }

        return csv.ToString();
    }
}
