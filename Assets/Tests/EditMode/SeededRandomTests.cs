using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class SeededRandomTests
{
    [Test]
    public void SeededRandomTypeExists()
    {
        Assert.IsNotNull(
            GetRandomType(),
            "SeededRandom must exist in Assembly-CSharp.");
    }

    [Test]
    public void SameSeedProducesSameSequence()
    {
        object a = Create(12345);
        object b = Create(12345);

        for (int i = 0; i < 32; i++)
        {
            Assert.AreEqual(
                InvokeFloat(a, "Value"),
                InvokeFloat(b, "Value"),
                $"Sequence diverged at draw {i}.");
        }
    }

    [Test]
    public void DifferentSeedsProduceDifferentSequences()
    {
        object a = Create(11);
        object b = Create(22);

        bool differs = false;

        for (int i = 0; i < 32; i++)
        {
            if (!Mathf.Approximately(InvokeFloat(a, "Value"), InvokeFloat(b, "Value")))
            {
                differs = true;
                break;
            }
        }

        Assert.IsTrue(differs, "Different seeds produced identical sequences.");
    }

    [Test]
    public void ValueStaysInUnitInterval()
    {
        object random = Create(7);

        for (int i = 0; i < 256; i++)
        {
            float value = InvokeFloat(random, "Value");

            Assert.GreaterOrEqual(value, 0f, $"Draw {i} was negative.");
            Assert.Less(value, 1f, $"Draw {i} was not below 1.");
        }
    }

    [Test]
    public void FloatRangeStaysWithinBounds()
    {
        object random = Create(99);

        for (int i = 0; i < 256; i++)
        {
            float value = InvokeFloat(random, "Range", 2f, 5f);

            Assert.GreaterOrEqual(value, 2f);
            Assert.Less(value, 5f);
        }
    }

    [Test]
    public void IntRangeStaysWithinBounds()
    {
        object random = Create(1234);

        for (int i = 0; i < 256; i++)
        {
            int value = (int)Invoke(random, "Range", 3, 7);

            Assert.GreaterOrEqual(value, 3);
            Assert.Less(value, 7);
        }
    }

    [Test]
    public void InsideUnitCircleStaysWithinUnitRadius()
    {
        object random = Create(2024);

        for (int i = 0; i < 256; i++)
        {
            Vector2 point = (Vector2)Invoke(random, "InsideUnitCircle");

            Assert.LessOrEqual(point.magnitude, 1.0001f);
        }
    }

    private static Type GetRandomType()
    {
        return Type.GetType("SeededRandom, Assembly-CSharp");
    }

    private static object Create(int seed)
    {
        Type type = GetRandomType();

        Assert.IsNotNull(type, "SeededRandom must exist in Assembly-CSharp.");

        return Activator.CreateInstance(type, seed);
    }

    private static float InvokeFloat(object instance, string methodName, params object[] args)
    {
        return (float)Invoke(instance, methodName, args);
    }

    private static object Invoke(object instance, string methodName, params object[] args)
    {
        MethodInfo method = FindMethod(instance.GetType(), methodName, args);

        Assert.IsNotNull(method, $"SeededRandom.{methodName} must exist.");

        return method.Invoke(instance, args);
    }

    private static MethodInfo FindMethod(Type type, string methodName, object[] args)
    {
        foreach (MethodInfo candidate in type.GetMethods())
        {
            if (candidate.Name != methodName)
                continue;

            ParameterInfo[] parameters = candidate.GetParameters();

            if (parameters.Length != args.Length)
                continue;

            bool matches = true;

            for (int i = 0; i < parameters.Length; i++)
            {
                if (!parameters[i].ParameterType.IsInstanceOfType(args[i]))
                {
                    matches = false;
                    break;
                }
            }

            if (matches)
                return candidate;
        }

        return null;
    }
}
