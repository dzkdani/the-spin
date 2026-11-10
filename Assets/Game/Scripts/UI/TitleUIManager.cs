using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject optionPanel;

    [Header("Scene")]
    [SerializeField] private string gameplayScene = "Game";

    private void Awake()
    {
        optionPanel.SetActive(false);
    }

    public void NewGame()
    {
        SceneManager.LoadScene(gameplayScene);
    }

    public void Continue()
    {
        // Intentionally left blank for game jam.
    }

    public void ToggleOption()
    {
        optionPanel.SetActive(!optionPanel.activeSelf);
    }

    public void CloseOption()
    {
        ServiceLocator.Get<TitleAudioManager>()?.PlayClick();
        optionPanel.SetActive(false);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}