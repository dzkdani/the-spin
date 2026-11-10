using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class PartSystemTests
{
    [Test]
    public void PartAppliesAndRemovesStatAndElement()
    {
        object neutral = CreateElement("neutral");
        object pyre = CreateElement("pyre");
        object slot = CreateSlot("blade", 30);

        object part = CreatePart(
            "pyre_blade",
            slot,
            pyre,
            3,
            ("Attack", "Multiply", 1.1f));

        WithBey(neutral, (data, runner) =>
        {
            object blueprint = CreateBlueprint(part);
            ProductionReflection.SetField(blueprint, "chassis", ProductionReflection.Property(data, "Base"));

            bool assembled = (bool)ProductionReflection.CallStatic(
                "BeyBuilder",
                "Assemble",
                blueprint,
                data,
                runner);

            Assert.IsTrue(assembled);

            Assert.AreEqual(11f, Stat(data, "Attack"), 0.0001f);
            Assert.AreEqual("pyre", Element(data));

            ProductionReflection.CallStatic(
                "BeyBuilder",
                "RemovePart",
                part,
                data,
                runner);

            Assert.AreEqual(10f, Stat(data, "Attack"), 0.0001f);
            Assert.AreEqual("neutral", Element(data));

            return null;
        });
    }

    [Test]
    public void ValidatorRejectsDuplicateSlotAndOverBudget()
    {
        object slot = CreateSlot("blade", 30);
        object partA = CreatePart("a", slot, null, 6, ("Attack", "Multiply", 1.1f));
        object partB = CreatePart("b", slot, null, 6, ("Attack", "Multiply", 1.1f));

        object blueprint = CreateBlueprint(partA, partB);

        object errors = ProductionReflection.CallStatic(
            "BeyBlueprintValidator",
            "Validate",
            blueprint,
            ContentRegistryDefaultTuning());

        IList list = (IList)errors;

        Assert.Greater(list.Count, 0);
    }

    [Test]
    public void PartGrantInstallsAbilityAndRemoveCleansIt()
    {
        object neutral = CreateElement("neutral");
        object slot = CreateSlot("bit", 10);
        object ability = CreateAbility("ember_aura");
        object part = CreatePart("ember_bit", slot, null, 3, null);
        ((IList)ProductionReflection.Field(part, "grantedAbilities")).Add(ability);

        WithBey(neutral, (data, runner) =>
        {
            object blueprint = CreateBlueprint(part);
            ProductionReflection.SetField(blueprint, "chassis", ProductionReflection.Property(data, "Base"));

            Assert.IsTrue((bool)ProductionReflection.CallStatic(
                "BeyBuilder", "Assemble", blueprint, data, runner));

            IList installed = (IList)ProductionReflection.Field(runner, "abilities");
            Assert.AreEqual(1, installed.Count);

            object runtime = installed[0];
            Assert.AreEqual(
                "grant:ember_bit/ember_aura",
                (string)ProductionReflection.Field(runtime, "SourceId"));

            ProductionReflection.CallStatic(
                "BeyBuilder", "RemovePart", part, data, runner);

            installed = (IList)ProductionReflection.Field(runner, "abilities");
            Assert.AreEqual(0, installed.Count);

            return null;
        });
    }

    private static object ContentRegistryDefaultTuning()
    {
        return ProductionReflection.StaticProperty("ContentRegistry", "Tuning");
    }

    private static float Stat(object data, string stat)
    {
        return (float)ProductionReflection.Call(
            ProductionReflection.Property(data, "Stats"),
            "Get",
            ProductionReflection.EnumValue("StatId", stat),
            null);
    }

    private static string Element(object data)
    {
        object elements = ProductionReflection.Property(data, "Elements");
        object current = ProductionReflection.Property(elements, "Current");

        return (string)ProductionReflection.Field(current, "id");
    }

    private static object CreateElement(string id)
    {
        object element = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("ElementDefinition"));

        ProductionReflection.SetField(element, "id", id);

        return element;
    }

    private static object CreateSlot(string id, int priority)
    {
        object slot = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("PartSlotDefinition"));

        ProductionReflection.SetField(slot, "id", id);
        ProductionReflection.SetField(slot, "elementPriority", priority);

        return slot;
    }

    private static object CreatePart(
        string id,
        object slot,
        object element,
        int budget,
        (string Stat, string Op, float Value)? modifier)
    {
        object part = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("PartDefinition"));

        ProductionReflection.SetField(part, "id", id);
        ProductionReflection.SetField(part, "slot", slot);
        ProductionReflection.SetField(part, "elementClaim", element);
        ProductionReflection.SetField(part, "budgetCost", budget);

        if (modifier.HasValue)
        {
            object spec = ProductionReflection.New("StatModifierSpec");
            ProductionReflection.SetField(spec, "stat", ProductionReflection.EnumValue("StatId", modifier.Value.Stat));
            ProductionReflection.SetField(spec, "op", ProductionReflection.EnumValue("ModOp", modifier.Value.Op));
            ProductionReflection.SetField(spec, "value", modifier.Value.Value);

            ((IList)ProductionReflection.Field(part, "statModifiers")).Add(spec);
        }

        return part;
    }

    private static object CreateAbility(string id)
    {
        object ability = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("AbilityDefinition"));

        ProductionReflection.SetField(ability, "id", id);
        ProductionReflection.SetField(ability, "kind", ProductionReflection.EnumValue("AbilityKind", "Passive"));
        ProductionReflection.SetField(ability, "mode", ProductionReflection.EnumValue("PassiveMode", "Triggered"));
        ProductionReflection.SetField(ability, "trigger", ProductionReflection.EnumValue("TriggerId", "Always"));

        return ability;
    }

    private static object CreateBlueprint(params object[] parts)
    {
        object blueprint = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("BeyBlueprint"));

        ProductionReflection.SetField(blueprint, "id", "test_blueprint");

        IList list = (IList)ProductionReflection.Field(blueprint, "parts");

        for (int i = 0; i < parts.Length; i++)
            list.Add(parts[i]);

        return blueprint;
    }

    private static object WithBey(object baseElement, System.Func<object, object, object> action)
    {
        object stats = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("BeyStats"));

        ProductionReflection.SetField(stats, "id", "test_chassis");
        ProductionReflection.SetField(stats, "maxRPM", 3000f);
        ProductionReflection.SetField(stats, "attack", 10f);
        ProductionReflection.SetField(stats, "weight", 5f);
        ProductionReflection.SetField(stats, "maxVelocity", 15f);
        ProductionReflection.SetField(stats, "acceleration", 25f);
        ProductionReflection.SetField(stats, "dashRPMCost", 50f);
        ProductionReflection.SetField(stats, "dashImpulse", 10f);
        ProductionReflection.SetField(stats, "minMoveAccelerationMultiplier", 0.6f);
        ProductionReflection.SetField(stats, "baseElement", baseElement);

        GameObject gameObject = new GameObject("PartBey");
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

        try
        {
            return action(data, runner);
        }
        finally
        {
            Object.DestroyImmediate(gameObject);
        }
    }
}
