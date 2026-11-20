using UnityEngine;

public static class LaunchMath
{
    public static float RotationPower(
        float degrees,
        float fullRotationDegrees)
    {
        if (fullRotationDegrees <= 0f)
            return 0f;

        return Mathf.Clamp01(
            Mathf.Abs(degrees) / fullRotationDegrees);
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
