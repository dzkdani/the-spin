using System.Collections;
using NUnit.Framework;
using UnityEngine;

public class ElementStackTests
{
    [Test]
    public void HighestPriorityClaimWins()
    {
        object pyre = Element("pyre");
        object crye = Element("crye");
        object stack = ProductionReflection.New("ElementStack");

        Add(stack, pyre, 10, "bit", 0f, 0);
        Add(stack, crye, 30, "blade", 0f, 1);

        Assert.AreSame(crye, Current(stack));
    }

    [Test]
    public void TieGoesToNewestClaim()
    {
        object pyre = Element("pyre");
        object crye = Element("crye");
        object stack = ProductionReflection.New("ElementStack");

        Add(stack, pyre, 10, "old", 0f, 1);
        Add(stack, crye, 10, "new", 0f, 2);

        Assert.AreSame(crye, Current(stack));
    }

    [Test]
    public void RemovingTopClaimRevertsToNextLiveClaim()
    {
        object neutral = Element("neutral");
        object pyre = Element("pyre");
        object stack = ProductionReflection.New("ElementStack");

        Add(stack, neutral, 0, "base", float.PositiveInfinity, 0);
        Add(stack, pyre, 10, "bit", float.PositiveInfinity, 1);

        Assert.AreSame(pyre, Current(stack));

        ProductionReflection.Call(stack, "RemoveBySource", "bit");

        Assert.AreSame(neutral, Current(stack));
    }

    [Test]
    public void ExpiredClaimsAreIgnored()
    {
        object neutral = Element("neutral");
        object pyre = Element("pyre");
        object stack = ProductionReflection.New("ElementStack");

        Add(stack, neutral, 0, "base", float.PositiveInfinity, 0);
        Add(stack, pyre, 10, "active", 4f, 1);

        ProductionReflection.Call(stack, "Tick", 5f);

        Assert.AreSame(neutral, Current(stack));
    }

    [Test]
    public void ExplainReturnsLiveClaimsHighestPriorityFirst()
    {
        object neutral = Element("neutral");
        object pyre = Element("pyre");
        object crye = Element("crye");
        object stack = ProductionReflection.New("ElementStack");

        Add(stack, neutral, 0, "base", float.PositiveInfinity, 0);
        Add(stack, pyre, 10, "bit", float.PositiveInfinity, 1);
        Add(stack, crye, 30, "blade", float.PositiveInfinity, 2);

        IEnumerable claims = (IEnumerable)ProductionReflection.Call(
            stack,
            "Explain");

        int index = 0;

        foreach (object claim in claims)
        {
            int priority = (int)claim
                .GetType()
                .GetField("Priority")
                .GetValue(claim);

            if (index == 0)
                Assert.AreEqual(30, priority);
            else if (index == 1)
                Assert.AreEqual(10, priority);

            index++;
        }

        Assert.AreEqual(3, index);
    }

    private static object Element(string id)
    {
        object element = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("ElementDefinition"));

        element
            .GetType()
            .GetField("id")
            .SetValue(element, id);

        return element;
    }

    private static void Add(
        object stack,
        object element,
        int priority,
        string source,
        float expiresAt,
        long sequence)
    {
        object claim = ProductionReflection.New(
            "ElementClaim",
            element,
            priority,
            source,
            expiresAt,
            sequence);

        ProductionReflection.Call(stack, "Add", claim);
    }

    private static object Current(object stack)
    {
        return ProductionReflection.Property(stack, "Current");
    }
}
