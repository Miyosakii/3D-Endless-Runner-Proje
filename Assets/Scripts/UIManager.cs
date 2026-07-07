using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject panelUI;
    [SerializeField] private GameObject panelPause;

    private int currentScore = 0;

    private void Awake()
    {
        // Event'lere abone ol
        GameEvents.OnCollectibleCollected += UpdateScore;
        GameEvents.OnPlayerDamaged += UpdateHealth;
        GameEvents.OnPlayerDeath += ShowGameOver;
        GameEvents.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnDestroy()
    {
        // Abonelikleri temizle
        GameEvents.OnCollectibleCollected -= UpdateScore;
        GameEvents.OnPlayerDamaged -= UpdateHealth;
        GameEvents.OnPlayerDeath -= ShowGameOver;
        GameEvents.OnGameStateChanged -= OnGameStateChanged;
    }

    private void UpdateScore(int value)
    {
        currentScore += value;
        if (scoreText != null)
            scoreText.text = "Score: " + currentScore;
    }

    private void UpdateHealth(int currentHealth)
    {
        if (healthText != null)
            healthText.text = "Health: " + currentHealth;
    }


    private void OnGameStateChanged(GameState state)
    {
        // Örneðin oyun baþlangýcýnda paneli kapat
        if (state == GameState.Playing && gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
    public void ShowGameOver()
    {
        if (panelPause != null) panelPause.SetActive(false);
        if (panelUI != null) panelUI.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void ShowGamePausePanel()
    {
        if (panelPause != null) panelPause.SetActive(true);
        if (panelUI != null) panelUI.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void HideGamePausePanel()
    {
        if (panelPause != null) panelPause.SetActive(false);
        if (panelUI != null) panelUI.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void RestartGame()
    {
        // Tek kaynaktan restart: GameManager'a delege et
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(0); // yedek davranýþ
    }
}