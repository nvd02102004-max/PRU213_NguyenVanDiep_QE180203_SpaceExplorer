using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Quản lý trạng thái màn chơi Gameplay:
/// - Quản lý và hiển thị điểm số (Score).
/// - Xử lý sự kiện kết thúc game (Game Over).
/// - Lưu điểm số và chuyển sang End Game Scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Giao diện UI Gameplay")]
    [Tooltip("Text hiển thị điểm số trong màn chơi")]
    public TextMeshProUGUI scoreText;

    [Header("Cài đặt Game Over")]
    [Tooltip("Tên Scene kết thúc game")]
    public string endGameSceneName = "EndGameScene";

    [Tooltip("Thời gian chờ trước khi chuyển Scene sau khi Game Over")]
    public float delayBeforeEndScene = 1.5f;

    private int currentScore = 0;
    private bool isGameOver = false;

    void Awake()
    {
        // Khởi tạo Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentScore = 0;
        UpdateScoreUI();
    }

    /// <summary>
    /// Cộng điểm cho người chơi
    /// </summary>
    public void AddScore(int amount)
    {
        if (isGameOver) return;

        currentScore += amount;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }
    }

    /// <summary>
    /// Kích hoạt khi tàu người chơi bị nổ / đâm phải thiên thạch
    /// </summary>
    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        // Lưu điểm số hiện tại để Scene End Game có thể đọc
        PlayerPrefs.SetInt("FinalScore", currentScore);
        PlayerPrefs.Save();

        // Chờ 1 khoảng thời gian rồi chuyển sang End Game Scene
        StartCoroutine(TransitionToEndScene());
    }

    private IEnumerator TransitionToEndScene()
    {
        yield return new WaitForSeconds(delayBeforeEndScene);
        SceneManager.LoadScene(endGameSceneName);
    }
}
