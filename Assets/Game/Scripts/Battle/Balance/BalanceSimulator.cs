using UnityEngine;

public static class BalanceSimulator
{
    public readonly struct Result
    {
        public readonly int WinsA;
        public readonly int WinsB;
        public readonly int Draws;

        public Result(int winsA, int winsB, int draws)
        {
            WinsA = winsA;
            WinsB = winsB;
            Draws = draws;
        }
    }

    public static Result Run(
        BalanceBuild a,
        BalanceBuild b,
        int seeds,
        MatchupTable matchups,
        CombatTuning tuning)
    {
        int winsA = 0;
        int winsB = 0;
        int draws = 0;

        for (int seed = 0; seed < seeds; seed++)
        {
            SeededRandom random = new SeededRandom(seed + 1);

            float rpmA = a.MaxRpm;
            float rpmB = b.MaxRpm;
            int steps = 0;

            while (rpmA > 0f && rpmB > 0f && steps < 200)
            {
                ImpactInput input = new ImpactInput
                {
                    Speed = random.Range(8f, 16f),
                    RpmA = rpmA / a.MaxRpm,
                    RpmB = rpmB / b.MaxRpm,
                    StatsA = a.Stats,
                    StatsB = b.Stats,
                    ElementA = a.Element,
                    ElementB = b.Element,
                    Matchups = matchups,
                    OppositeSpin = random.Value() < 0.5f,
                    Normal = Vector2.right,
                    Now = 0f,
                    Tuning = tuning
                };

                ImpactResult result = ImpactSolver.Resolve(input);

                rpmA -= result.DamageToA;
                rpmB -= result.DamageToB;
                steps++;
            }

            if (rpmA <= 0f && rpmB <= 0f)
                draws++;
            else if (rpmB <= 0f)
                winsA++;
            else if (rpmA <= 0f)
                winsB++;
            else
                draws++;
        }

        return new Result(winsA, winsB, draws);
    }
}
