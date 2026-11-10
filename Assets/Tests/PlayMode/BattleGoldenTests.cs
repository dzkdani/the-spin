using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BattleGoldenTests
{
    // Captured from the pre-rework combat code at battle-rework-baseline.
    // Phase 1 must change only the documented entries in battle system.md §23.
    private const string Expected =
        "equal dmgA=320.00 dmgB=320.00 velA=-12.00,0.00 velB=12.00,0.00 | " +
        "opposite dmgA=456.00 dmgB=456.00 velA=-12.00,0.00 velB=12.00,0.00 | " +
        "charged dmgA=560.00 dmgB=560.00 velA=-12.00,0.00 velB=12.00,0.00";

    [UnityTest]
    public IEnumerator ScriptedCollisionsMatchGolden()
    {
        yield return RunScenario(false, false, 0, 0);
        string equal = lastResult;

        yield return RunScenario(false, false, 0, 1);
        string opposite = lastResult;

        yield return RunScenario(true, true, 0, 0);
        string charged = lastResult;

        string actual =
            "equal " + equal + " | " +
            "opposite " + opposite + " | " +
            "charged " + charged;

        Debug.Log("GOLDEN: " + actual);

        Assert.AreEqual(Expected, actual);
    }

    private string lastResult;

    private IEnumerator RunScenario(bool chargeA, bool chargeB, int spinA, int spinB)
    {
        GoldenBey a = CreateBey("GoldenA", -2f, 8f, spinA, chargeA);
        GoldenBey b = CreateBey("GoldenB", 2f, -8f, spinB, chargeB);

        yield return null;

        int steps = 0;

        while (steps < 180 &&
               Mathf.Approximately(ReadRpm(a), 3000f) &&
               Mathf.Approximately(ReadRpm(b), 3000f))
        {
            yield return new WaitForFixedUpdate();
            steps++;
        }

        float damageA = 3000f - ReadRpm(a);
        float damageB = 3000f - ReadRpm(b);

        Vector2 velocityA = a.Rigidbody.linearVelocity;
        Vector2 velocityB = b.Rigidbody.linearVelocity;

        UnityEngine.Object.Destroy(a.GameObject);
        UnityEngine.Object.Destroy(b.GameObject);

        yield return null;

        lastResult = string.Format(
            CultureInfo.InvariantCulture,
            "dmgA={0:F2} dmgB={1:F2} velA={2:F2},{3:F2} velB={4:F2},{5:F2}",
            damageA,
            damageB,
            velocityA.x,
            velocityA.y,
            velocityB.x,
            velocityB.y);

        yield break;
    }

    private static float ReadRpm(GoldenBey bey)
    {
        return (float)bey.Data
            .GetType()
            .GetProperty("CurrentRPM")
            .GetValue(bey.Data);
    }

    private static GoldenBey CreateBey(
        string name,
        float x,
        float velocityX,
        int spinDirection,
        bool charged)
    {
        Type beyData = ProductionType("BeyData");
        Type beyPhysics = ProductionType("BeyPhysics");
        Type beyCombat = ProductionType("BeyCombat");
        Type beyController = ProductionType("BeyController");

        GameObject gameObject = new GameObject(name);
        gameObject.SetActive(false);

        Rigidbody2D rigidbody = gameObject.AddComponent<Rigidbody2D>();
        rigidbody.gravityScale = 0f;
        rigidbody.linearDamping = 0f;
        rigidbody.angularDamping = 0f;
        rigidbody.freezeRotation = true;

        CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.5f;

        Component data = gameObject.AddComponent(beyData);
        Component physics = gameObject.AddComponent(beyPhysics);
        Component combat = gameObject.AddComponent(beyCombat);
        gameObject.AddComponent(beyController);

        ScriptableObject tuning = CreateTuning(spinDirection);

        data.GetType()
            .GetField("<Base>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(data, tuning);

        LogAssert.Expect(
            LogType.Error,
            "BeyController requires BeyData, BeyPhysics, BeyCombat, an IBeyInput, and a child BeyVisual.");

        gameObject.SetActive(true);

        Invoke(physics, "Initialize", data, null);
        Invoke(combat, "Initialize", data, physics);

        ((Behaviour)physics).enabled = true;
        ((Behaviour)combat).enabled = true;

        if (charged)
        {
            combat.GetType()
                .GetField("chargedHits", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(combat, 1);
        }

        gameObject.transform.position = new Vector3(x, 0f, 0f);
        rigidbody.linearVelocity = new Vector2(velocityX, 0f);

        return new GoldenBey(gameObject, rigidbody, data);
    }

    private static ScriptableObject CreateTuning(int spinDirection)
    {
        Type tuningType = ProductionType("BeyStats");

        ScriptableObject tuning =
            ScriptableObject.CreateInstance(tuningType);

        SetField(tuning, "maxVelocity", 15f);
        SetField(tuning, "maxRPM", 3000f);
        SetField(tuning, "acceleration", 25f);
        SetField(tuning, "dashRPMCost", 50f);
        SetField(tuning, "dashImpulse", 10f);
        SetField(tuning, "attack", 10f);
        SetField(tuning, "weight", 5f);
        SetField(tuning, "minMoveAccelerationMultiplier", 0.6f);

        FieldInfo spin = tuningType.GetField("spinDirection");
        spin.SetValue(tuning, Enum.ToObject(spin.FieldType, spinDirection));

        return tuning;
    }

    private static void SetField(object instance, string fieldName, object value)
    {
        instance.GetType()
            .GetField(fieldName)
            .SetValue(instance, value);
    }

    private static object Invoke(Component component, string methodName, params object[] args)
    {
        MethodInfo method = component.GetType().GetMethod(methodName);

        Assert.IsNotNull(method, $"{component.GetType().Name}.{methodName} must exist.");

        return method.Invoke(component, args);
    }

    private static Type ProductionType(string name)
    {
        Type type = Type.GetType(name + ", Assembly-CSharp");

        Assert.IsNotNull(type, $"{name} must exist in Assembly-CSharp.");

        return type;
    }

    private sealed class GoldenBey
    {
        public readonly GameObject GameObject;
        public readonly Rigidbody2D Rigidbody;
        public readonly Component Data;

        public GoldenBey(GameObject gameObject, Rigidbody2D rigidbody, Component data)
        {
            GameObject = gameObject;
            Rigidbody = rigidbody;
            Data = data;
        }
    }
}
