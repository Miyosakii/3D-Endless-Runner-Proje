using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameState initialState = GameState.WaitingToStart;
    public GameState CurrentState { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        SetState(initialState);
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        GameEvents.TriggerGameStateChanged(newState);

        switch (newState)
        {
            case GameState.WaitingToStart:
                // Baþlangýç bekleme ekraný gösterilebilir
                break;
            case GameState.Playing:
                // Oyun baþlat
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                break;
        }
    }

    // GameOver çaðrýsý
    public void GameOver()
    {
        SetState(GameState.GameOver);
        GameEvents.TriggerPlayerDeath(); // UI ve diðerleri dinleyecek (UI zaten event'e abone)
        // UIManager'e doðrudan çaðrý kaldýrýldý
    }

    public void PauseGame()
    {
        SetState(GameState.Paused);

        UIManager uiManager = FindAnyObjectByType<UIManager>();
        if (uiManager != null)
            uiManager.ShowGamePausePanel(); // Pause için doðru panel çaðrýsý
    }

    public void ResumeGame()
    {
        SetState(GameState.Playing);

        UIManager uiManager = FindAnyObjectByType<UIManager>();
        if (uiManager != null)
            uiManager.HideGamePausePanel();
    }

    public void RestartGame()
    {
        // Tek bir sorumluluk: sahneyi yeniden yüklemek
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}