using NUnit.Framework;
using UnityEngine;

public class ArenaMathTests
{
    private static readonly Vector2 Radii = new Vector2(8f, 4f);

    [Test]
    public void NormalizedDistanceUsesEllipseRadii()
    {
        Assert.AreEqual(
            1f,
            Distance(new Vector2(8f, 0f)),
            0.0001f);

        Assert.AreEqual(
            1f,
            Distance(new Vector2(0f, 4f)),
            0.0001f);

        Assert.AreEqual(0f, Distance(Vector2.zero), 0.0001f);
    }

    [Test]
    public void ClampKeepsPointInsideEllipse()
    {
        Vector2 clamped = (Vector2)ProductionReflection.CallStatic(
            "ArenaBounds",
            "Clamp",
            new Vector2(20f, 0f),
            Vector2.zero,
            Radii,
            0.1f);

        Assert.AreEqual(7.2f, clamped.x, 0.0001f);
        Assert.AreEqual(0f, clamped.y, 0.0001f);
    }

    [Test]
    public void CameraSizeFitsEllipseOnWideAndNarrowScreens()
    {
        Vector2 demoRadii = new Vector2(8.2f, 4.4f);

        Assert.AreEqual(
            4.9f,
            Size(demoRadii, 16f / 9f),
            0.01f);

        Assert.Greater(Size(demoRadii, 4f / 3f), 4.9f);
    }

    private static float Distance(Vector2 point)
    {
        return (float)ProductionReflection.CallStatic(
            "ArenaBounds",
            "NormalizedDistance",
            point,
            Vector2.zero,
            Radii);
    }

    private static float Size(Vector2 radii, float aspect)
    {
        return (float)ProductionReflection.CallStatic(
            "ArenaFraming",
            "ComputeSize",
            radii,
            aspect,
            0.5f);
    }
}
