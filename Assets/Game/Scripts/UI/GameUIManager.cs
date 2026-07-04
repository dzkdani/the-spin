using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Game Over")]
    [SerializeField] private TMP_Text resultText;

    private bool isPaused;
    private bool gameEnded;

    public bool CanReceiveGameplayInput => !isPaused && !gameEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Time.timeScale = 1f;

        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        if (gameEnded)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        TitleAudioManager.Instance?.PlayClick();

        if (gameEnded)
            return;

        isPaused = true;

        pausePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void Resume()
    {
        TitleAudioManager.Instance?.PlayClick();

        isPaused = false;

        pausePanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void ShowGameOver(GameResult result)
    {
        gameEnded = true;
        Time.timeScale = 0f;

        resultText.text = result == GameResult.Win
            ? "YOU WIN!"
            : "YOU LOSE!";

        gameOverPanel.SetActive(true);
    }

    public void Retry()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        TitleAudioManager.Instance?.PlayClick();
        
        Time.timeScale = 1f;

        SceneManager.LoadScene("TitleScene");
    }

    public void Quit()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}