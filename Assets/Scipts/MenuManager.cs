using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Quản lý UI và điều hướng giữa các Scene:
/// - Main Menu: Bắt đầu game (Play), Xem hướng dẫn (Instructions).
/// - End Game: Hiển thị điểm số đạt được, Chơi lại (Retry), Về menu chính, Thoát game (Quit).
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("UI Main Menu")]
    [Tooltip("Panel hướng dẫn chơi (Instructions Panel)")]
    public GameObject instructionsPanel;

    [Header("UI End Game")]
    [Tooltip("Text hiển thị điểm cuối cùng đạt được")]
    public TextMeshProUGUI finalScoreText;

    [Header("Tên các Scene")]
    public string gameplaySceneName = "SampleScene"; // hoặc GameplayScene
    public string mainMenuSceneName = "MainMenuScene";

    void Start()
    {
        // Nếu ở màn hình End Game và có gán finalScoreText
        if (finalScoreText != null)
        {
            int score = PlayerPrefs.GetInt("FinalScore", 0);
            finalScoreText.text = "Final Score: " + score;
        }

        // Đảm bảo bảng hướng dẫn bị ẩn lúc ban đầu
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
