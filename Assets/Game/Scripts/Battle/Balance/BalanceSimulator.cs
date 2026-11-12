using System.Collections.Generic;
using UnityEngine;

public static class BalanceSimulator
{
    private const float DashChance = 0.15f;
    private const float ContactMin = 0.85f;
    private const float ContactMax = 1.15f;
    private const float StepSeconds = 0.25f;
    private const float RpmDecayRate = 0.0015f;

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
        return Run(
            a, b, seeds, matchups, tuning, startVariance, 0f);
    }

    public static Result Run(
        BalanceBuild a,
        BalanceBuild b,
        int seeds,
        MatchupTable matchups,
        CombatTuning tuning,
        float startVariance,
        float arenaRadius)
    {
        int winsA = 0;
        int winsB = 0;
        int draws = 0;

        float radius = Mathf.Max(0f, arenaRadius);

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

            Vector2 posA = new Vector2(-radius * 0.25f, 0f);
            Vector2 posB = new Vector2(radius * 0.25f, 0f);
            bool ringOut = false;

            StatBlock statsA = a.Stats.Clone();
            StatBlock statsB = b.Stats.Clone();

            float[] cooldownsA = new float[a.Actives.Count];
            float[] cooldownsB = new float[b.Actives.Count];

            float now = 0f;
            int steps = 0;

            bool oppositeSpin = random.Value() < 0.5f;

            while (rpmA > 0f && rpmB > 0f && steps < 200)
            {
                rpmA = Mathf.Max(
                    0f,
                    rpmA - a.MaxRpm * RpmDecayRate *
                    statsA.Get(StatId.RpmDecay) * StepSeconds);

                rpmB = Mathf.Max(
                    0f,
                    rpmB - b.MaxRpm * RpmDecayRate *
                    statsB.Get(StatId.RpmDecay) * StepSeconds);

                if (rpmA <= 0f || rpmB <= 0f)
                    break;

                statsA.Tick(now);
                statsB.Tick(now);

                bool rolledA = random.Value() < DashChance;
                bool rolledB = random.Value() < DashChance;

                List<PendingImpactMod> activePendingA =
                    new List<PendingImpactMod>();
                List<PendingImpactMod> activePendingB =
                    new List<PendingImpactMod>();

                Motion motionA = UseActives(
                    a, cooldownsA, ref rpmA, now, activePendingA);

                Motion motionB = UseActives(
                    b, cooldownsB, ref rpmB, now, activePendingB);

                Vector2 normal = posB - posA;

                normal = normal.sqrMagnitude > 0.0001f
                    ? normal.normalized
                    : Vector2.right;

                if (radius > 0f)
                {
                    posA += normal * motionA.Approach;
                    posB += normal * motionA.Push;

                    posB -= normal * motionB.Approach;
                    posA -= normal * motionB.Push;
                }

                bool dashingA = motionA.Dashing || rolledA;
                bool dashingB = motionB.Dashing || rolledB;

                ImpactInput input = new ImpactInput
                {
                    Speed = random.Range(8f, 16f),
                    RpmA = rpmA / a.MaxRpm,
                    RpmB = rpmB / b.MaxRpm,
                    StatsA = statsA,
                    StatsB = statsB,
                    ElementA = a.Element,
                    ElementB = b.Element,
                    Matchups = matchups,
                    OppositeSpin = oppositeSpin,
                    DashingA = dashingA,
                    DashingB = dashingB,
                    Normal = normal,
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

                ElementStatusResolver.Apply(
                    a.Element,
                    statsA,
                    statsB,
                    now);

                ElementStatusResolver.Apply(
                    b.Element,
                    statsB,
                    statsA,
                    now);

                if (radius > 0f)
                {
                    posA += result.ImpulseOnA *
                        (StepSeconds * 0.15f);

                    posB += result.ImpulseOnB *
                        (StepSeconds * 0.15f);

                    bool outA = posA.magnitude > radius;
                    bool outB = posB.magnitude > radius;

                    if (outA || outB)
                    {
                        if (outA && outB)
                            draws++;
                        else if (outB)
                            winsA++;
                        else
                            winsB++;

                        ringOut = true;
                        break;
                    }
                }

                now += StepSeconds;
                steps++;
            }

            if (ringOut)
                continue;

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

    private struct Motion
    {
        public bool Dashing;
        public float Approach;
        public float Push;
    }

    private static Motion UseActives(
        BalanceBuild build,
        float[] cooldowns,
        ref float rpm,
        float now,
        List<PendingImpactMod> pending)
    {
        Motion motion = new Motion();

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
            motion.Dashing |= use.Dashing;
            motion.Approach += use.Approach;
            motion.Push += use.Push;

            if (use.Pending != null)
                pending.AddRange(use.Pending);
        }

        return motion;
    }
}
