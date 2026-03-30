using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI coinText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;

    [Header("Тохиргоо")]
    public float scorePerSecond = 5f;
    public int scorePerCoin = 10;
    public string highScoreKey = "EndlessCarHighScore";

    private int currentScore = 0;
    private int currentCoins = 0;
    private float scoreAccumulator = 0f;
    private bool isGameRunning = false;
    private bool isGameOver = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        StartGame();
    }

    void Update()
    {
        if (!isGameRunning) return;

        scoreAccumulator += scorePerSecond * Time.deltaTime;
        if (scoreAccumulator >= 1f)
        {
            int toAdd = Mathf.FloorToInt(scoreAccumulator);
            currentScore += toAdd;
            scoreAccumulator -= toAdd;
            UpdateScoreUI();
        }
    }

    public void StartGame()
    {
        currentScore = 0;
        currentCoins = 0;
        isGameRunning = true;
        isGameOver = false;
        Time.timeScale = 1f;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        UpdateScoreUI();
        UpdateCoinUI();
    }

    public void AddCoin()
    {
        currentCoins++;
        currentScore += scorePerCoin;
        UpdateCoinUI();
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "Score:" + currentScore;
    }

    void UpdateCoinUI()
    {
        if (coinText != null) coinText.text = "Coins:" + currentCoins;
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        isGameRunning = false;

        // Тоглоомыг зогсоох
        Time.timeScale = 0f;

        int highScore = PlayerPrefs.GetInt(highScoreKey, 0);
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt(highScoreKey, highScore);
            PlayerPrefs.Save();
        }

        StartCoroutine(ShowGameOver(1f, highScore));
    }

    IEnumerator ShowGameOver(float delay, int highScore)
    {
        // Time.timeScale=0 үед WaitForSecondsRealtime ашиглах
        yield return new WaitForSecondsRealtime(delay);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = "Score: " + currentScore;
        if (highScoreText != null) highScoreText.text = "Best: " + highScore;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public int GetScore() => currentScore;
    public int GetCoins() => currentCoins;
    public bool IsGameRunning() => isGameRunning;
}