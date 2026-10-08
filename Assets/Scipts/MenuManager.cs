using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Quản lý UI và điều hướng giữa các Scene:
/// - Main Menu: Bắt đầu game (Play), Xem hướng dẫn (Instructions).
/// - End Game: Hiển thị trạng thái Thắng/Thua (VICTORY / GAME OVER), Điểm số đạt được, Chơi lại (Retry), Về menu chính, Thoát game (Quit).
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("UI Main Menu")]
    [Tooltip("Panel hướng dẫn chơi (Instructions Panel)")]
    public GameObject instructionsPanel;

    [Header("UI End Game")]
    [Tooltip("Tiêu đề màn kết thúc (Tự đổi thành VICTORY hoặc GAME OVER)")]
    public TextMeshProUGUI titleText;

    [Tooltip("Text hiển thị điểm cuối cùng đạt được")]
    public TextMeshProUGUI finalScoreText;

    [Header("Tên các Scene")]
    public string gameplaySceneName = "SampleScene";
    public string mainMenuSceneName = "MainMenuScene";

    void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        bool isEndGameScene = currentScene == "EndGameScene" || currentScene.Contains("End");

        // 1. Chỉ thực hiện cập nhật giao diện Thắng / Thua khi thực sự ở EndGameScene
        if (isEndGameScene)
        {
            // Tự động tìm kiếm TitleText và FinalScoreText trên Canvas nếu chưa kéo vào Inspector
            if (titleText == null)
            {
                GameObject tObj = GameObject.Find("TitleText");
                if (tObj != null)
                {
                    titleText = tObj.GetComponent<TextMeshProUGUI>();
                }
            }

            if (finalScoreText == null)
            {
                GameObject sObj = GameObject.Find("FinalScoreText");
                if (sObj == null) sObj = GameObject.Find("ScoreText");
                if (sObj != null)
                {
                    finalScoreText = sObj.GetComponent<TextMeshProUGUI>();
                }
            }

            string result = PlayerPrefs.GetString("GameResult", "GAMEOVER");

            // Tự động nhận diện Panel người dùng tạo trên Hierarchy hoặc tự tạo nếu chưa có
            GameObject cardObj = GameObject.Find("ResultCard");
            if (cardObj == null) cardObj = GameObject.Find("ResultPanel");
            if (cardObj == null) cardObj = GameObject.Find("GameOverPanel");

            Color cardColor = (result == "VICTORY")
                ? new Color(0.02f, 0.08f, 0.14f, 0.92f) // Xanh thẫm vũ trụ huyền ảo mừng chiến thắng
                : new Color(0.06f, 0.03f, 0.08f, 0.92f); // Đen thẫm huyền bí khi Game Over

            if (cardObj == null && (titleText != null || finalScoreText != null))
            {
                Canvas canvas = FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    cardObj = new GameObject("ResultCard", typeof(RectTransform), typeof(Image));
                    cardObj.transform.SetParent(canvas.transform, false);
                    cardObj.transform.SetSiblingIndex(1); // Nằm ngay trên Background, dưới toàn bộ Text và Nút

                    RectTransform cardRt = cardObj.GetComponent<RectTransform>();
                    cardRt.anchorMin = new Vector2(0.5f, 0.5f);
                    cardRt.anchorMax = new Vector2(0.5f, 0.5f);
                    cardRt.pivot = new Vector2(0.5f, 0.5f);
                    cardRt.anchoredPosition = new Vector2(0, 0);
                    cardRt.sizeDelta = new Vector2(680, 580);

                    Image cardImg = cardObj.GetComponent<Image>();
                    cardImg.color = cardColor;
                }
            }
            else if (cardObj != null)
            {
                Image cardImg = cardObj.GetComponent<Image>();
                if (cardImg != null)
                {
                    cardImg.color = cardColor;
                }
            }

            // Cập nhật giao diện Thắng / Thua sống động, sắc nét
            if (finalScoreText != null || titleText != null)
            {
                int score = PlayerPrefs.GetInt("FinalScore", 0);
                int highScore = PlayerPrefs.GetInt("HighScore", 0);
                int totalStars = PlayerPrefs.GetInt("TotalStars", 0);

                // Hiển thị Tiêu đề kết quả to rõ, tương phản cao tuyệt đối
                if (titleText != null)
                {
                    titleText.alignment = TextAlignmentOptions.Center;
                    if (result == "VICTORY")
                    {
                        titleText.text = "<size=72><color=#FACC15><b>VICTORY!</b></color></size>\n<size=26><color=#4ADE80><b>GALAXY DEFENDED!</b></color></size>";
                    }
                    else
                    {
                        titleText.text = "<size=72><color=#EF4444><b>GAME OVER</b></color></size>\n<size=26><color=#FCA5A5><b>SPACESHIP DESTROYED!</b></color></size>";
                    }
                }

                // Hiển thị Điểm số, Kỷ lục và Số sao to rõ, sắc nét, không dùng ký tự lạ tránh lỗi font □
                if (finalScoreText != null)
                {
                    finalScoreText.alignment = TextAlignmentOptions.Center;
                    bool isNewRecord = (score > 0 && score >= highScore);
                    string recordTag = isNewRecord
                        ? "<size=24><color=#FDE047><b>-- NEW HIGH SCORE --</b></color></size>\n"
                        : (result == "VICTORY" ? "<size=22><color=#38BDF8><b>-- MISSION ACCOMPLISHED --</b></color></size>\n" : "");

                    finalScoreText.text = $"{recordTag}" +
                        $"<size=38><color=#94A3B8>SCORE: </color><color=#FFFFFF><b>{score}</b></color></size>\n" +
                        $"<size=22><color=#94A3B8>BEST: </color><color=#FBBF24><b>{highScore}</b></color>      <color=#94A3B8>STARS: </color><color=#38BDF8><b>{totalStars}</b></color></size>";
                }
            }
        }

        // 2. Tự động gán hiệu ứng phóng to rê chuột (ButtonHoverEffect) cho các nút bấm trong cả 2 Scene
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var btn in buttons)
        {
            if (btn.GetComponent<ButtonHoverEffect>() == null)
            {
                btn.gameObject.AddComponent<ButtonHoverEffect>();
            }
        }

        // 3. Đảm bảo bảng hướng dẫn bị ẩn lúc ban đầu ở Main Menu
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Chuyển vào màn chơi chính
    /// </summary>
    public void PlayGame()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    /// <summary>
    /// Bật/Tắt bảng hướng dẫn cách chơi
    /// </summary>
    public void ToggleInstructions(bool show)
    {
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(show);
        }
    }

    /// <summary>
    /// Quay về màn hình Main Menu
    /// </summary>
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Thoát khỏi game
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
