using System.Collections.Generic;
using UnityEngine;

public static class BalanceSimulator
{
    private const float DashChance = 0.15f;
    private const float ContactMin = 0.85f;
    private const float ContactMax = 1.15f;
    private const float StepSeconds = 0.25f;

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
        return Run(a, b, seeds, matchups, tuning, 0f);
    }

    public static Result Run(
        BalanceBuild a,
        BalanceBuild b,
        int seeds,
        MatchupTable matchups,
        CombatTuning tuning,
        float startVariance)
    {
        int winsA = 0;
        int winsB = 0;
        int draws = 0;

        for (int seed = 0; seed < seeds; seed++)
        {
            SeededRandom random = new SeededRandom(seed + 1);

            float rpmA = a.MaxRpm;
            float rpmB = b.MaxRpm;

            if (startVariance > 0f)
            {
                rpmA *= random.Range(1f - startVariance, 1f);
                rpmB *= random.Range(1f - startVariance, 1f);
            }

            float[] cooldownsA = new float[a.Actives.Count];
            float[] cooldownsB = new float[b.Actives.Count];

            float now = 0f;
            int steps = 0;

            bool oppositeSpin = random.Value() < 0.5f;

            while (rpmA > 0f && rpmB > 0f && steps < 200)
            {
                bool rolledA = random.Value() < DashChance;
                bool rolledB = random.Value() < DashChance;

                List<PendingImpactMod> activePendingA =
                    new List<PendingImpactMod>();
                List<PendingImpactMod> activePendingB =
                    new List<PendingImpactMod>();

                bool activeDashA = UseActives(
                    a, cooldownsA, ref rpmA, now, activePendingA);

                bool activeDashB = UseActives(
                    b, cooldownsB, ref rpmB, now, activePendingB);

                bool dashingA = activeDashA || rolledA;
                bool dashingB = activeDashB || rolledB;

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
                    OppositeSpin = oppositeSpin,
                    DashingA = dashingA,
                    DashingB = dashingB,
                    Normal = Vector2.right,
                    Now = now,
                    Tuning = tuning
                };

                input.PendingA.AddRange(activePendingA);
                input.PendingB.AddRange(activePendingB);

                input.PendingA.Add(new PendingImpactMod(
                    "contact",
                    random.Range(ContactMin, ContactMax),
                    1,
                    now + StepSeconds));

                input.PendingB.Add(new PendingImpactMod(
                    "contact",
                    random.Range(ContactMin, ContactMax),
                    1,
                    now + StepSeconds));

                if (dashingA)
                {
                    BalanceInnates.AddDashPending(
                        a,
                        input.PendingA,
                        a.Stats.Get(StatId.AbilityPower),
                        now);
                }

                if (dashingB)
                {
                    BalanceInnates.AddDashPending(
                        b,
                        input.PendingB,
                        b.Stats.Get(StatId.AbilityPower),
                        now);
                }

                ImpactResult result = ImpactSolver.Resolve(input);

                rpmA = Mathf.Clamp(
                    rpmA - result.DamageToA + result.RpmGainA,
                    0f,
                    a.MaxRpm);

                rpmB = Mathf.Clamp(
                    rpmB - result.DamageToB + result.RpmGainB,
                    0f,
                    b.MaxRpm);

                now += StepSeconds;
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

    private static bool UseActives(
        BalanceBuild build,
        float[] cooldowns,
        ref float rpm,
        float now,
        List<PendingImpactMod> pending)
    {
        bool dashing = false;

        for (int i = 0; i < build.Actives.Count; i++)
        {
            BalanceActives.UseResult use = BalanceActives.TryUse(
                build,
                i,
                rpm,
                cooldowns[i],
                now);

            if (!use.Used)
                continue;

            rpm = use.RpmAfter;
            cooldowns[i] = use.CooldownUntil;
            dashing |= use.Dashing;

            if (use.Pending != null)
                pending.AddRange(use.Pending);
        }

        return dashing;
    }
}
