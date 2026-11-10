using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class AbilitySystemTests
{
    [TearDown]
    public void TearDown()
    {
        ProductionReflection.CallStatic("BattleEvents", "Reset");
    }

    [Test]
    public void AlwaysPassiveAppliesModifier()
    {
        object ability = CreateAbility("hard_shell", "Passive", "Triggered", "Always");
        AddEffect(ability, "AddModifierEffect", ("stat", ProductionReflection.EnumValue("StatId", "Defense")), ("op", ProductionReflection.EnumValue("ModOp", "Add")), ("value", 10f));

        float defense = (float)WithBey(ability, (data, runner) =>
        {
            return (float)ProductionReflection.Call(
                ProductionReflection.Property(data, "Stats"),
                "Get",
                ProductionReflection.EnumValue("StatId", "Defense"),
                null);
        });

        Assert.AreEqual(10f, defense, 0.0001f);
    }

    [Test]
    public void DashEndedPassiveAddsPendingImpact()
    {
        object ability = CreateAbility("surge", "Passive", "Triggered", "DashEnded");
        AddEffect(ability, "ScaleNextImpactEffect", ("multiplier", 1.15f), ("count", 1), ("duration", 2f));

        int count = (int)WithBey(ability, (data, runner) =>
        {
            IList installed = (IList)ProductionReflection.Field(runner, "abilities");
            Assert.AreEqual(1, installed.Count, "Ability must be installed on the runner.");

            ProductionReflection.CallStatic("BattleEvents", "RaiseDashEnded", data);

            object combat = ProductionReflection.Field(runner, "combat");
            IList pending = (IList)ProductionReflection.Field(combat, "pendingMods");

            Assert.AreEqual(1, pending.Count);

            object mod = pending[0];
            Assert.AreEqual(1.15f, (float)ProductionReflection.Field(mod, "Multiplier"), 0.0001f);

            return pending.Count;
        });

        Assert.AreEqual(1, count);
    }

    [Test]
    public void AuraAppliesAndCleansUpElementClaim()
    {
        object ability = CreateAbility("ember_aura", "Passive", "Aura", "Always");
        AddCondition(ability, "RpmAboveCondition", ("normalizedRpm", 0.5f));
        AddEffect(
            ability,
            "ClaimElementEffect",
            ("element", CreateElement("pyre")),
            ("priority", 10),
            ("duration", 0f));

        WithBey(ability, (data, runner) =>
        {
            ProductionReflection.CallPrivate(runner, "TickAuras", 0.2f);

            object elements = ProductionReflection.Property(data, "Elements");
            object current = ProductionReflection.Property(elements, "Current");

            Assert.AreEqual("pyre", (string)ProductionReflection.Field(current, "id"));

            ProductionReflection.Call(data, "ReduceRPM", 2500f);
            ProductionReflection.CallPrivate(runner, "TickAuras", 0.2f);

            current = ProductionReflection.Property(elements, "Current");
            Assert.AreEqual("neutral", (string)ProductionReflection.Field(current, "id"));

            return null;
        });
    }

    [Test]
    public void ActivePaysCostAndEntersCooldown()
    {
        object ability = CreateAbility("guard", "Active", "Triggered", "CastComplete");
        AddEffect(ability, "AddModifierEffect", ("stat", ProductionReflection.EnumValue("StatId", "Defense")), ("op", ProductionReflection.EnumValue("ModOp", "Add")), ("value", 30f), ("duration", 3f));

        object active = ProductionReflection.Field(ability, "active");
        ProductionReflection.SetField(active, "rpmCost", 0.2f);
        ProductionReflection.SetField(active, "cooldown", 6f);
        ProductionReflection.SetField(active, "castTime", 0f);

        bool first = (bool)WithBey(ability, (data, runner) =>
        {
            bool activated = (bool)ProductionReflection.Call(runner, "TryActivate", 0);

            float rpm = (float)ProductionReflection.Property(data, "CurrentRPM");
            float defense = (float)ProductionReflection.Call(
                ProductionReflection.Property(data, "Stats"),
                "Get",
                ProductionReflection.EnumValue("StatId", "Defense"),
                null);

            Assert.IsTrue(activated);
            Assert.AreEqual(2400f, rpm, 0.5f);
            Assert.AreEqual(30f, defense, 0.0001f);

            return activated;
        });

        Assert.IsTrue(first);
    }

    private static object WithBey(object ability, System.Func<object, object, object> action)
    {
        object neutral = CreateElement("neutral");
        object type = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("BeyTypeDefinition"));

        ProductionReflection.SetField(type, "id", "test_type");
        ProductionReflection.SetField(type, "innatePassiveId", string.Empty);

        object stats = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("BeyStats"));

        ProductionReflection.SetField(stats, "maxRPM", 3000f);
        ProductionReflection.SetField(stats, "attack", 10f);
        ProductionReflection.SetField(stats, "weight", 5f);
        ProductionReflection.SetField(stats, "maxVelocity", 15f);
        ProductionReflection.SetField(stats, "acceleration", 25f);
        ProductionReflection.SetField(stats, "dashRPMCost", 50f);
        ProductionReflection.SetField(stats, "dashImpulse", 10f);
        ProductionReflection.SetField(stats, "minMoveAccelerationMultiplier", 0.6f);
        ProductionReflection.SetField(stats, "type", type);
        ProductionReflection.SetField(stats, "baseElement", neutral);

        GameObject gameObject = new GameObject("AbilityBey");
        gameObject.SetActive(false);

        Rigidbody2D rigidbody = gameObject.AddComponent<Rigidbody2D>();
        rigidbody.gravityScale = 0f;

        gameObject.AddComponent<CircleCollider2D>();

        Component data = gameObject.AddComponent(
            ProductionReflection.TypeOf("BeyData"));

        Component physics = gameObject.AddComponent(
            ProductionReflection.TypeOf("BeyPhysics"));

        Component combat = gameObject.AddComponent(
            ProductionReflection.TypeOf("BeyCombat"));

        data.GetType()
            .GetField("<Base>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(data, stats);

        gameObject.SetActive(true);

        ProductionReflection.Call(physics, "Initialize", data, null);
        ProductionReflection.Call(combat, "Initialize", data, physics);

        Component runner = gameObject.AddComponent(
            ProductionReflection.TypeOf("AbilityRunner"));

        object[] initArgs = { data, physics, combat, null };
        ProductionReflection.Call(runner, "Initialize", initArgs);

        object abilityDefinition = CreateAbilityInstance(ability);
        object[] installArgs = { abilityDefinition, "equipped:test" };
        bool installed = (bool)ProductionReflection.Call(runner, "Install", installArgs);

        Assert.IsTrue(installed, "Ability must install.");

        try
        {
            return action(data, runner);
        }
        finally
        {
            Object.DestroyImmediate(gameObject);
        }
    }

    private static object CreateAbilityInstance(object ability)
    {
        return ability;
    }

    private static object CreateAbility(
        string id,
        string kind,
        string mode,
        string trigger)
    {
        object ability = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("AbilityDefinition"));

        ProductionReflection.SetField(ability, "id", id);
        ProductionReflection.SetField(ability, "kind", ProductionReflection.EnumValue("AbilityKind", kind));
        ProductionReflection.SetField(ability, "mode", ProductionReflection.EnumValue("PassiveMode", mode));
        ProductionReflection.SetField(ability, "trigger", ProductionReflection.EnumValue("TriggerId", trigger));

        return ability;
    }

    private static void AddCondition(object ability, string typeName, params (string Field, object Value)[] fields)
    {
        object condition = ProductionReflection.New(typeName);

        foreach (var field in fields)
            ProductionReflection.SetField(condition, field.Field, field.Value);

        IList conditions = (IList)ProductionReflection.Field(ability, "conditions");
        conditions.Add(condition);
    }

    private static void AddEffect(object ability, string typeName, params (string Field, object Value)[] fields)
    {
        object effect = ProductionReflection.New(typeName);

        foreach (var field in fields)
            ProductionReflection.SetField(effect, field.Field, field.Value);

        IList effects = (IList)ProductionReflection.Field(ability, "effects");
        effects.Add(effect);
    }

    private static object CreateElement(string id)
    {
        object element = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("ElementDefinition"));

        ProductionReflection.SetField(element, "id", id);

        return element;
    }
}
