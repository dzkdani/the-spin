using UnityEngine;

public static class GrindSolver
{
    public readonly struct Result
    {
        public readonly float DamageA;
        public readonly float DamageB;

        public Result(float damageA, float damageB)
        {
            DamageA = damageA;
            DamageB = damageB;
        }
    }

    public static Result Resolve(
        float rpmA,
        float rpmB,
        float maxRpmA,
        float maxRpmB,
        float normalizedA,
        float normalizedB,
        GrindMode mode,
        float rate,
        float deltaTime)
    {
        if (mode == GrindMode.SelfRpm)
        {
            return new Result(
                normalizedA * maxRpmA * rate * deltaTime,
                normalizedB * maxRpmB * rate * deltaTime);
        }

        float damage =
            Mathf.Abs(rpmA - rpmB) *
            rate *
            deltaTime;

        return new Result(damage, damage);
    }
}
