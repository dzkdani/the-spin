using UnityEngine;

public sealed class UnityRandom : IRandom
{
    public float Value()
    {
        return Random.value;
    }

    public float Range(float min, float max)
    {
        return Random.Range(min, max);
    }

    public int Range(int min, int max)
    {
        return Random.Range(min, max);
    }

    public Vector2 InsideUnitCircle()
    {
        return Random.insideUnitCircle;
    }
}
