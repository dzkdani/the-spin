using UnityEngine;

public static class AiSimulation
{
    public readonly struct Result
    {
        public readonly int Dashes;
        public readonly int AbilityRequests;
        public readonly int Retreats;

        public Result(int dashes, int abilityRequests, int retreats)
        {
            Dashes = dashes;
            AbilityRequests = abilityRequests;
            Retreats = retreats;
        }
    }

    public static Result Run(AiProfile profile, int seed, int ticks)
    {
        EnemyBrain brain = new EnemyBrain(
            profile,
            new SeededRandom(seed));

        int dashes = 0;
        int abilityRequests = 0;
        int retreats = 0;

        for (int i = 0; i < ticks; i++)
        {
            float phase = i * 0.25f;
            float distance = 2f + 3f * Mathf.Abs(Mathf.Sin(phase));

            BattleSnapshot snapshot = new BattleSnapshot(
                Mathf.Clamp01(0.65f + 0.25f * Mathf.Sin(phase * 0.5f)),
                Mathf.Clamp01(0.35f + 0.35f * Mathf.Cos(phase * 0.7f)),
                Vector2.zero,
                new Vector2(distance, 0f),
                distance,
                null,
                null,
                false,
                1f,
                i * 0.15f,
                new[] { true },
                new[] { 0f });

            AiCommand command = brain.Tick(snapshot, 0.15f);

            if (command.RequestDash)
                dashes++;

            if (command.AbilitySlot >= 0)
                abilityRequests++;

            if (brain.CurrentBehavior == "retreat")
                retreats++;
        }

        return new Result(dashes, abilityRequests, retreats);
    }
}
