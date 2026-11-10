using NUnit.Framework;

public class GrindSolverTests
{
    [Test]
    public void SelfRpmModeScalesWithOwnNormalizedRpm()
    {
        object result = Resolve(
            mode: "SelfRpm",
            rpmA: 3000f,
            rpmB: 3000f,
            normalizedA: 1f,
            normalizedB: 0.5f);

        float damageA = ReadFloat(result, "DamageA");
        float damageB = ReadFloat(result, "DamageB");

        Assert.AreEqual(0.06f, damageA, 0.0001f);
        Assert.AreEqual(0.03f, damageB, 0.0001f);
    }

    [Test]
    public void SymmetricModeUsesRpmDifference()
    {
        object result = Resolve(
            mode: "Symmetric",
            rpmA: 3000f,
            rpmB: 2000f,
            normalizedA: 1f,
            normalizedB: 0.666f);

        Assert.AreEqual(0.02f, ReadFloat(result, "DamageA"), 0.0001f);
        Assert.AreEqual(0.02f, ReadFloat(result, "DamageB"), 0.0001f);
    }

    [Test]
    public void EqualRpmDoesNothingInSymmetricMode()
    {
        object result = Resolve(
            mode: "Symmetric",
            rpmA: 2000f,
            rpmB: 2000f,
            normalizedA: 0.666f,
            normalizedB: 0.666f);

        Assert.AreEqual(0f, ReadFloat(result, "DamageA"), 0.0001f);
        Assert.AreEqual(0f, ReadFloat(result, "DamageB"), 0.0001f);
    }

    private static object Resolve(
        string mode,
        float rpmA,
        float rpmB,
        float normalizedA,
        float normalizedB)
    {
        return ProductionReflection.CallStatic(
            "GrindSolver",
            "Resolve",
            rpmA,
            rpmB,
            3000f,
            3000f,
            normalizedA,
            normalizedB,
            ProductionReflection.EnumValue("GrindMode", mode),
            0.001f,
            0.02f);
    }

    private static float ReadFloat(object instance, string fieldName)
    {
        return (float)ProductionReflection.Field(instance, fieldName);
    }
}
