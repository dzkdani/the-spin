using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class MenuHoverBuilder
{
    private const string BattleScenePath =
        "Assets/Game/Scenes/BattleScene.unity";

    [MenuItem("Tools/Wire Menu Hover")]
    public static void Wire()
    {
        UnityEngine.SceneManagement.Scene scene =
            EditorSceneManager.OpenScene(
                BattleScenePath,
                OpenSceneMode.Single);

        Button[] buttons = Object.FindObjectsByType<Button>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        int added = 0;

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];

            if (button.GetComponent<UIButtonSFX>() != null)
                continue;

            button.gameObject.AddComponent<UIButtonSFX>();
            added++;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("Added hover feedback to " + added + " battle buttons");
    }
}
