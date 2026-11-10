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
        GameObject targetObject = new GameObject("Player");
        GameObject uiObject = new GameObject("GameUI");
        GameObject pausePanel = new GameObject("PausePanel");
        GameObject gameOverPanel = new GameObject("GameOverPanel");

        try
        {
            Type enemyInputType = GetProductionType("EnemyInput");
            Component enemyInput = enemyObject.AddComponent(enemyInputType);
            targetObject.transform.position = new Vector3(10f, 0f, 0f);
            SetPrivateField(enemyInput, "target", targetObject.transform);

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
            UnityEngine.Object.DestroyImmediate(targetObject);
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

            LogAssert.Expect(LogType.Error, "BeyData requires a BeyBaseSO reference.");
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
