using UnityEngine;

public sealed class SeededRandom : IRandom
{
    private const uint FallbackSeed = 0x9E3779B9u;

    private uint state;

    public SeededRandom(int seed)
    {
        state = seed == 0
            ? FallbackSeed
            : (uint)seed;
    }

    public float Value()
    {
        return (NextUInt() & 0xFFFFFFu) / 16777216f;
    }

    public float Range(float min, float max)
    {
        return min + (max - min) * Value();
    }

    public int Range(int min, int max)
    {
        if (max <= min)
            return min;

        return min + (int)(NextUInt() % (uint)(max - min));
    }

    public Vector2 InsideUnitCircle()
    {
        float angle =
            Value() * 2f * Mathf.PI;

        float radius =
            Mathf.Sqrt(Value());

        return new Vector2(
            Mathf.Cos(angle) * radius,
            Mathf.Sin(angle) * radius);
    }

    private uint NextUInt()
    {
        uint x = state;

        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;

        state = x;

        return x;
    }
}
