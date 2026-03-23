using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro; // TextMeshPro ашиглахгүй бол энийг "using UnityEngine.UI;" болгоно

/// <summary>
/// GameManager - Тоглоомын төлөв, оноо, coin удирдах
/// 
/// UNITY ТОХИРГОО:
/// 1. Empty GameObject "GameManager" нэрлэж энэ скрипт нэмнэ
/// 2. Inspector-т UI элементүүдийг холбоно:
///    - scoreText: Score харуулах TextMeshProUGUI
///    - coinText: Coin харуулах TextMeshProUGUI
///    - gameOverPanel: Game over UI panel
///    - finalScoreText: Game over оноо
///    - highScoreText: Хамгийн өндөр оноо
/// 3. Canvas дотор Score, Coins UI текст үүсгэнэ
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton

    [Header("UI Элементүүд")]
    [Tooltip("Score текст (зургийн Score:39 гэсэн хэсэг)")]
    public TextMeshProUGUI scoreText;

    [Tooltip("Coin тоо текст (зургийн Coins:33 гэсэн хэсэг)")]
    public TextMeshProUGUI coinText;

    [Tooltip("Game Over цонх")]
    public GameObject gameOverPanel;

    [Tooltip("Game Over дахь эцсийн оноо")]
    public TextMeshProUGUI finalScoreText;

    [Tooltip("Хамгийн өндөр оноо")]
    public TextMeshProUGUI highScoreText;

    [Header("Оноо тохиргоо")]
    [Tooltip("Секундэд хэдэн оноо нэмэгдэх")]
    public float scorePerSecond = 5f;

    [Tooltip("Coin бүр хэдэн оноо нэмэх")]
    public int scorePerCoin = 10;

    [Tooltip("PlayerPrefs-т хадгалах оноо key")]
    public string highScoreKey = "EndlessCarHighScore";

    // Дотоод хувьсагчид
    private int currentScore = 0;
    private int currentCoins = 0;
    private float scoreAccumulator = 0f;
    private bool isGameRunning = false;
    private bool isGameOver = false;

    void Awake()
    {
        // Singleton тохиргоо
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        StartGame();
    }

    void Update()
    {
        if (!isGameRunning) return;

        // Цаг өнгөрөх тусам оноо нэмэх
        scoreAccumulator += scorePerSecond * Time.deltaTime;

        if (scoreAccumulator >= 1f)
        {
            int toAdd = Mathf.FloorToInt(scoreAccumulator);
            currentScore += toAdd;
            scoreAccumulator -= toAdd;
            UpdateScoreUI();
        }
    }

    /// <summary>
    /// Тоглоом эхлүүлэх
    /// </summary>
    public void StartGame()
    {
        currentScore = 0;
        currentCoins = 0;
        isGameRunning = true;
        isGameOver = false;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        UpdateScoreUI();
        UpdateCoinUI();
    }

    /// <summary>
    /// Coin цуглуулсан үед дуудагдана (PlayerCarController-аас)
    /// </summary>
    public void AddCoin()
    {
        currentCoins++;
        currentScore += scorePerCoin;
        UpdateCoinUI();
        UpdateScoreUI();
    }

    /// <summary>
    /// Score UI шинэчлэх
    /// </summary>
    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score:" + currentScore.ToString();
    }

    /// <summary>
    /// Coin UI шинэчлэх
    /// </summary>
    void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = "Coins:" + currentCoins.ToString();
    }

    /// <summary>
    /// Тоглоом дуусах (PlayerCarController-аас дуудагдана)
    /// </summary>
    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        isGameRunning = false;

        // High score шалгах ба хадгалах
        int highScore = PlayerPrefs.GetInt(highScoreKey, 0);
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt(highScoreKey, highScore);
            PlayerPrefs.Save();
        }

        // Game Over UI харуулах
        StartCoroutine(ShowGameOverWithDelay(1.5f, highScore));
    }

    IEnumerator ShowGameOverWithDelay(float delay, int highScore)
    {
        yield return new WaitForSeconds(delay);

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = "Score: " + currentScore;
        if (highScoreText != null) highScoreText.text = "Best: " + highScore;
    }

    /// <summary>
    /// Дахин тоглох (Game Over буюу UI товч)
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Getter функцүүд
    public int GetScore() => currentScore;
    public int GetCoins() => currentCoins;
    public bool IsGameRunning() => isGameRunning;
}
