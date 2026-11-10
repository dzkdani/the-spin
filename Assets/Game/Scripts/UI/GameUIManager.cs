using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour, IBattleStateGate
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Game Over")]
    [SerializeField] private TMP_Text resultText;

    private bool isPaused;
    private bool gameEnded;

    // Update still runs while paused or after game over, so input reads must be gated on this.
    public bool CanReceiveGameplayInput => !isPaused && !gameEnded;

    private void Awake()
    {
        if (!ServiceLocator.TryRegister(this))
        {
            Destroy(gameObject);
            return;
        }

        ServiceLocator.Register<IBattleStateGate>(this);

        Time.timeScale = 1f;

        if (pausePanel == null || gameOverPanel == null || resultText == null)
        {
            Debug.LogError("GameUIManager is missing one or more panel/text references.", this);
            return;
        }

        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<IBattleStateGate>(this);
        ServiceLocator.Unregister(this);
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
        ServiceLocator.Get<TitleAudioManager>()?.PlayClick();

        if (gameEnded)
            return;

        isPaused = true;

        pausePanel?.SetActive(true);

        Time.timeScale = 0f;
    }

    public void Resume()
    {
        ServiceLocator.Get<TitleAudioManager>()?.PlayClick();

        isPaused = false;

        pausePanel?.SetActive(false);

        Time.timeScale = 1f;
    }

    public void ShowGameOver(
        GameResult result,
        FinishReason reason = FinishReason.RpmDepleted)
    {
        gameEnded = true;
        Time.timeScale = 0f;

        if (resultText != null)
        {
            resultText.text = reason == FinishReason.RingOut
                ? "RING OUT!"
                : reason == FinishReason.Burst
                    ? "BURST!"
                    : result == GameResult.Win
                        ? "YOU WIN!"
                        : "YOU LOSE!";
        }

        gameOverPanel?.SetActive(true);
    }

    public void Retry()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        ServiceLocator.Get<TitleAudioManager>()?.PlayClick();
        
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
