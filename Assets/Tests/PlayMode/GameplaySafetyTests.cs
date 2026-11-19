using System.Reflection;
using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GameplaySafetyTests
{
    [UnityTest]
    public IEnumerator EnemyInputDoesNotUpdateWhileGameplayInputIsBlocked()
    {
        GameObject enemyObject = new GameObject("Enemy");
        GameObject uiObject = new GameObject("GameUI");
        GameObject pausePanel = new GameObject("PausePanel");
        GameObject gameOverPanel = new GameObject("GameOverPanel");

        try
        {
            Type enemyInputType = GetProductionType("EnemyInput");
            Component enemyInput = enemyObject.AddComponent(enemyInputType);

            uiObject.SetActive(false);
            Type uiType = GetProductionType("GameUIManager");
            Component ui = uiObject.AddComponent(uiType);
            SetPrivateField(ui, "pausePanel", pausePanel);
            SetPrivateField(ui, "gameOverPanel", gameOverPanel);
            SetPrivateField(ui, "resultText", null);
            LogAssert.Expect(
                LogType.Error,
                "GameUIManager is missing one or more panel/text references.");
            uiObject.SetActive(true);
            uiType.GetMethod("Pause").Invoke(ui, null);

            InvokePrivateMethod(enemyInput, "Update");

            Assert.AreEqual(Vector2.zero, GetProperty(enemyInput, "MoveTarget"));
            Assert.IsFalse((bool)GetProperty(enemyInput, "DashRequested"));

            yield return null;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(enemyObject);
            UnityEngine.Object.DestroyImmediate(uiObject);
            UnityEngine.Object.DestroyImmediate(pausePanel);
            UnityEngine.Object.DestroyImmediate(gameOverPanel);
        }
    }

    [UnityTest]
    public IEnumerator BeyControllerDoesNotThrowWhenRequiredComponentsAreMissing()
    {
        GameObject beyObject = new GameObject("IncompleteBey");
        try
        {
            beyObject.SetActive(false);
            Type controllerType = GetProductionType("BeyController");
            beyObject.AddComponent(controllerType);

            LogAssert.Expect(LogType.Error, "BeyData requires a BeyStats reference.");
            LogAssert.Expect(
                LogType.Error,
                "BeyController requires BeyData, BeyPhysics, BeyCombat, an IBeyInput, and a child BeyVisual.");
            beyObject.SetActive(true);

            yield return null;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(beyObject);
        }

    }

    [UnityTest]
    public IEnumerator BeyVisualDoesNotThrowWithoutData()
    {
        GameObject visualObject = new GameObject("IncompleteVisual");
        try
        {
            visualObject.SetActive(false);
            Type visualType = GetProductionType("BeyVisual");
            visualObject.AddComponent(visualType);

            visualObject.SetActive(true);

            yield return null;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(visualObject);
        }
    }

    [UnityTest]
    public IEnumerator WaveResetIgnoresSerializedToggles()
    {
        GameObject playerObject = new GameObject("Player");
        GameObject managerObject = new GameObject("GameManager");

        try
        {
            playerObject.SetActive(false);
            managerObject.SetActive(false);

            Component data = playerObject.AddComponent(
                GetProductionType("BeyData"));

            object stats = ScriptableObject.CreateInstance(
                GetProductionType("BeyStats"));

            SetField(stats, "maxRPM", 3000f);
            SetBackingField(data, "Base", stats);
            InvokePrivateMethod(data, "Awake");

            Component controller = playerObject.AddComponent(
                GetProductionType("BeyController"));

            SetBackingField(controller, "Data", data);

            Component manager = managerObject.AddComponent(
                GetProductionType("GameManager"));

            SetPrivateField(manager, "player", controller);

            ClearBoolFields(manager);

            InvokeMethod(data, "ReduceRPM", 1200f);

            Assert.AreEqual(
                1800f,
                (float)GetProperty(data, "CurrentRPM"),
                0.01f);

            InvokePrivateMethod(manager, "ResetPlayerForNextWave");

            Assert.AreEqual(
                3000f,
                (float)GetProperty(data, "CurrentRPM"),
                0.01f);

            yield return null;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(playerObject);
            UnityEngine.Object.DestroyImmediate(managerObject);
        }
    }

    private static void SetField(object instance, string fieldName, object value)
    {
        FieldInfo field = instance.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        field.SetValue(instance, value);
    }

    private static void SetBackingField(
        object instance,
        string propertyName,
        object value)
    {
        SetField(instance, "<" + propertyName + ">k__BackingField", value);
    }

    private static void ClearBoolFields(object instance)
    {
        foreach (FieldInfo field in instance.GetType().GetFields(
            BindingFlags.Instance | BindingFlags.NonPublic))
        {
            if (field.FieldType == typeof(bool))
                field.SetValue(instance, false);
        }
    }

    private static object InvokeMethod(
        object instance,
        string methodName,
        params object[] args)
    {
        MethodInfo method = instance.GetType().GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        Assert.IsNotNull(method, methodName + " must exist.");

        return method.Invoke(instance, args);
    }

    private static void SetPrivateField(object instance, string fieldName, object value)
    {
        FieldInfo field = instance.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);

        field.SetValue(instance, value);
    }

    private static object GetProperty(object instance, string propertyName)
    {
        return instance.GetType().GetProperty(propertyName).GetValue(instance);
    }

    private static void InvokePrivateMethod(object instance, string methodName)
    {
        MethodInfo method = instance.GetType().GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.NonPublic);

        method.Invoke(instance, null);
    }

    private static Type GetProductionType(string typeName)
    {
        return Type.GetType(typeName + ", Assembly-CSharp");
    }
}
