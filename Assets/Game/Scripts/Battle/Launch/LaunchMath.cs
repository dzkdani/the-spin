using UnityEngine;

public static class LaunchMath
{
    public static float Power(float pullDistance, float pullMax)
    {
        if (pullMax <= 0f)
            return 0f;

        return Mathf.Clamp01(pullDistance / pullMax);
    }

    public static float StartRpm(
        float maxRpm,
        float power,
        float minStartFactor)
    {
        float floor = Mathf.Clamp01(minStartFactor);

        return maxRpm * Mathf.Lerp(floor, 1f, Mathf.Clamp01(power));
    }

    public static float EnemyPower(float basePower, float variance)
    {
        float offset = variance > 0f
            ? Random.Range(-variance, variance)
            : 0f;

        return Mathf.Clamp01(basePower + offset);
    }
}
