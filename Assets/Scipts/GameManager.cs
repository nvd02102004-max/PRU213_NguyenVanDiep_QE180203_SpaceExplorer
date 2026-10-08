using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Quản lý trạng thái màn chơi Gameplay:
/// - Quản lý và hiển thị điểm số (Score) nhỏ gọn, hài hòa với giao diện.
/// - Hệ thống Tạm dừng (Pause / Resume / Retry / Main Menu) bằng Button UI hoặc phím ESC / P.
/// - Xử lý sự kiện Thua (Game Over) và Thắng (Victory).
/// - Lưu điểm số và trạng thái kết quả chuyển sang End Game Scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Giao diện UI Gameplay")]
    [Tooltip("Text hiển thị điểm số trong màn chơi")]
    public TextMeshProUGUI scoreText;

    [Header("Cài đặt Tạm dừng (Pause Game)")]
    [Tooltip("Nút bấm tạm dừng trận đấu trên góc màn hình")]
    public Button pauseButton;

    [Tooltip("Panel bảng tạm dừng trận đấu")]
    public GameObject pausePanel;

    public string mainMenuSceneName = "MainMenuScene";

    [Header("Cài đặt Kết thúc Game")]
    [Tooltip("Tên Scene kết thúc game")]
    public string endGameSceneName = "EndGameScene";

    [Tooltip("Thời gian chờ trước khi chuyển Scene sau khi Game Over / Victory")]
    public float delayBeforeEndScene = 1.5f;

    private int currentScore = 0;
    private bool isGameOver = false;
    public bool isPaused { get; private set; } = false;

    void Awake()
    {
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
        isGameOver = false;
        Time.timeScale = 1f; // Luôn đảm bảo tốc độ thời gian bình thường khi bắt đầu màn
        isPaused = false;

        // 1. Tự động căn chỉnh Text Score nhỏ gọn, nằm kế bên Nút Pause
        if (scoreText != null)
        {
            scoreText.fontSize = 15;
            RectTransform rt = scoreText.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = new Vector2(62, -22);
                rt.sizeDelta = new Vector2(140, 26);
            }
        }

        UpdateScoreUI();

        // 2. Khởi tạo / Tìm kiếm Nút Pause & Bảng Tạm Dừng trên Canvas
        SetupPauseUI();
    }

    void Update()
    {
        // Phím tắt bàn phím: ESC hoặc P để Tạm dừng / Tiếp tục
        if (!isGameOver && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)))
        {
            TogglePause();
        }
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
            scoreText.text = "<size=13><color=#94A3B8>Score: </color></size><size=15><color=#FFFFFF><b>" + currentScore + "</b></color></size>";
        }
    }

    private bool wasWaveTextActive = false;
    private bool wasBossSliderActive = false;

    #region HỆ THỐNG TẠM DỪNG (PAUSE SYSTEM)

    /// <summary>
    /// Đổi trạng thái giữa Tạm dừng và Tiếp tục
    /// </summary>
    public void TogglePause()
    {
        if (isGameOver) return;

        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// <summary>
    /// Tạm dừng trận đấu:
    /// - Đóng băng thời gian
    /// - Ẩn HUD và text thông báo để tránh trùng lặp
    /// - Hiện bảng Pause trên nền mờ trong suốt nhìn xuyên thấu tàu và quái
    /// </summary>
    public void PauseGame()
    {
        if (isGameOver) return;

        isPaused = true;
        Time.timeScale = 0f; // Đóng băng toàn bộ hoạt động trong game

        // 1. Tạm ẩn nút Pause góc trái và chữ Điểm số
        if (pauseButton != null) pauseButton.gameObject.SetActive(false);
        if (scoreText != null) scoreText.gameObject.SetActive(false);

        // 2. Tạm ẩn số mạng (LivesText)
        GameObject livesObj = GameObject.Find("LivesText");
        if (livesObj != null) livesObj.SetActive(false);

        // 3. Tạm ẩn thông báo Wave nếu đang hiển thị
        GameObject waveObj = GameObject.Find("WaveText");
        if (waveObj != null && waveObj.activeSelf)
        {
            wasWaveTextActive = true;
            waveObj.SetActive(false);
        }
        else
        {
            wasWaveTextActive = false;
        }

        // 4. Tạm ẩn thanh máu Boss nếu đang hiển thị
        GameObject bossSlider = GameObject.Find("BossHealthSlider");
        if (bossSlider != null && bossSlider.activeSelf)
        {
            wasBossSliderActive = true;
            bossSlider.SetActive(false);
        }
        else
        {
            wasBossSliderActive = false;
        }

        // 5. Hiển thị bảng Pause lên trên cùng của Canvas
        if (pausePanel != null)
        {
            pausePanel.transform.SetAsLastSibling();
            pausePanel.SetActive(true);
        }
    }

    /// <summary>
    /// Tiếp tục trận đấu:
    /// - Khôi phục thời gian
    /// - Ẩn bảng Pause
    /// - Khôi phục toàn bộ HUD ban đầu
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Khôi phục thời gian bình thường

        // 1. Tắt bảng Pause
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // 2. Khôi phục lại nút Pause và Điểm số
        if (pauseButton != null) pauseButton.gameObject.SetActive(true);
        if (scoreText != null) scoreText.gameObject.SetActive(true);

        // 3. Khôi phục số mạng
        GameObject livesObj = GameObject.Find("LivesText");
        if (livesObj != null) livesObj.SetActive(true);

        // 4. Khôi phục thông báo Wave nếu trước đó đang bật
        if (wasWaveTextActive)
        {
            GameObject waveObj = GameObject.Find("WaveText");
            if (waveObj != null) waveObj.SetActive(true);
        }

        // 5. Khôi phục thanh máu Boss nếu trước đó đang bật
        if (wasBossSliderActive)
        {
            GameObject bossSlider = GameObject.Find("BossHealthSlider");
            if (bossSlider != null) bossSlider.SetActive(true);
        }
    }

    /// <summary>
    /// Chơi lại màn chơi hiện tại
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Thoát về Main Menu
    /// </summary>
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void SetupPauseUI()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        // 1. Tự động tìm hoặc tạo Nút Pause nếu chưa gán
        if (pauseButton == null)
        {
            GameObject pbObj = GameObject.Find("PauseButton");
            if (pbObj != null)
            {
                pauseButton = pbObj.GetComponent<Button>();
            }
            else
            {
                GameObject newBtn = new GameObject("PauseButton", typeof(RectTransform), typeof(Image), typeof(Button));
                newBtn.transform.SetParent(canvas.transform, false);
                RectTransform rt = newBtn.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = new Vector2(20, -18);
                rt.sizeDelta = new Vector2(32, 30);

                Image img = newBtn.GetComponent<Image>();
                img.color = new Color(0.1f, 0.15f, 0.25f, 0.85f);

                pauseButton = newBtn.GetComponent<Button>();
                newBtn.AddComponent<ButtonHoverEffect>();

                GameObject textObj = new GameObject("Icon", typeof(RectTransform));
                textObj.transform.SetParent(newBtn.transform, false);
                RectTransform textRt = textObj.GetComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.sizeDelta = Vector2.zero;

                TextMeshProUGUI iconText = textObj.AddComponent<TextMeshProUGUI>();
                iconText.text = "<b>❚❚</b>";
                iconText.fontSize = 15;
                iconText.color = new Color(0.35f, 0.85f, 1f);
                iconText.alignment = TextAlignmentOptions.Center;
            }
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(TogglePause);
            pauseButton.onClick.AddListener(TogglePause);
        }

        // 2. Tìm hoặc Tạo Bảng Tạm Dừng (PausePanel)
        if (pausePanel == null)
        {
            // Tìm trong Canvas kể cả khi PausePanel đang bị Inactive
            Transform ppTr = canvas.transform.Find("PausePanel");
            if (ppTr != null)
            {
                pausePanel = ppTr.gameObject;
            }
            else
            {
                GameObject ppObj = GameObject.Find("PausePanel");
                if (ppObj != null)
                {
                    pausePanel = ppObj;
                }
            }
        }

        if (pausePanel == null)
        {
            // Nếu hoàn toàn chưa có trong Scene thì khởi tạo mới
            pausePanel = new GameObject("PausePanel", typeof(RectTransform), typeof(Image));
            pausePanel.transform.SetParent(canvas.transform, false);
            RectTransform panelRt = pausePanel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.pivot = new Vector2(0.5f, 0.5f);
            panelRt.anchoredPosition = Vector2.zero;
            panelRt.sizeDelta = new Vector2(220, 210);

            Image panelImg = pausePanel.GetComponent<Image>();
            panelImg.color = new Color(0.06f, 0.09f, 0.16f, 0.7f);
        }

        // Nếu PausePanel đang trống (chưa có nút nào bên trong), tự động tạo Title và 3 nút chuẩn
        if (pausePanel != null && pausePanel.transform.childCount == 0)
        {
            RectTransform panelRt = pausePanel.GetComponent<RectTransform>();
            if (panelRt != null)
            {
                panelRt.anchorMin = new Vector2(0.5f, 0.5f);
                panelRt.anchorMax = new Vector2(0.5f, 0.5f);
                panelRt.pivot = new Vector2(0.5f, 0.5f);
                panelRt.anchoredPosition = Vector2.zero;
                panelRt.sizeDelta = new Vector2(220, 210);
            }

            Image panelImg = pausePanel.GetComponent<Image>();
            if (panelImg != null)
            {
                panelImg.color = new Color(0.06f, 0.09f, 0.16f, 0.7f);
            }

            // Tiêu đề: GAME PAUSED
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform));
            titleObj.transform.SetParent(pausePanel.transform, false);
            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchoredPosition = new Vector2(0, 65);
            titleRt.sizeDelta = new Vector2(200, 32);
            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "<color=#38BDF8><b>GAME PAUSED</b></color>";
            title.fontSize = 22;
            title.alignment = TextAlignmentOptions.Center;

            // 3 Nút chuẩn
            CreatePauseButton(pausePanel.transform, "ResumeButton", "RESUME", new Vector2(0, 18), new Color(0.0627f, 0.7255f, 0.5059f, 0.95f), ResumeGame);
            CreatePauseButton(pausePanel.transform, "RetryButton", "RETRY", new Vector2(0, -26), new Color(0.3098f, 0.2745f, 0.8980f, 0.95f), RestartGame);
            CreatePauseButton(pausePanel.transform, "MenuButton", "MAIN MENU", new Vector2(0, -70), new Color(0.8627f, 0.1490f, 0.1490f, 0.95f), LoadMainMenu);
        }

        // Tự động kết nối sự kiện OnClick cho các nút
        if (pausePanel != null)
        {
            HookButton(pausePanel.transform.Find("ResumeButton"), ResumeGame);
            HookButton(pausePanel.transform.Find("RetryButton"), RestartGame);
            HookButton(pausePanel.transform.Find("MenuButton"), LoadMainMenu);

            Transform center = pausePanel.transform.Find("CenterGroup");
            if (center != null)
            {
                HookButton(center.Find("ResumeButton"), ResumeGame);
                HookButton(center.Find("RetryButton"), RestartGame);
                HookButton(center.Find("MenuButton"), LoadMainMenu);
            }

            Transform card = pausePanel.transform.Find("Card");
            if (card != null)
            {
                HookButton(card.Find("ResumeButton"), ResumeGame);
                HookButton(card.Find("RetryButton"), RestartGame);
                HookButton(card.Find("MenuButton"), LoadMainMenu);
            }

            pausePanel.SetActive(false);
        }
    }

    void HookButton(Transform btnT, UnityEngine.Events.UnityAction action)
    {
        if (btnT == null) return;
        Button btn = btnT.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveListener(action);
            btn.onClick.AddListener(action);
        }
    }

    void CreatePauseButton(Transform parent, string name, string label, Vector2 pos, Color color, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(180, 36);

        Image img = btnObj.GetComponent<Image>();
        img.color = color;

        Button btn = btnObj.GetComponent<Button>();
        btn.onClick.AddListener(onClick);
        btnObj.AddComponent<ButtonHoverEffect>();

        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "<b>" + label + "</b>";
        tmp.fontSize = 14;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    #endregion

    /// <summary>
    /// Kích hoạt khi tàu người chơi bị nổ
    /// </summary>
    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 1f;

        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(false);
        }

        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (currentScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
        }

        PlayerPrefs.SetString("GameResult", "GAMEOVER");
        PlayerPrefs.SetInt("FinalScore", currentScore);
        PlayerPrefs.Save();

        StartCoroutine(TransitionToEndScene());
    }

    /// <summary>
    /// Kích hoạt khi người chơi bắn hạ được Trùm Cuối (Boss)
    /// </summary>
    public void Victory()
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 1f;

        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(false);
        }

        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (currentScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
        }

        PlayerPrefs.SetString("GameResult", "VICTORY");
        PlayerPrefs.SetInt("FinalScore", currentScore);
        PlayerPrefs.Save();

        StartCoroutine(TransitionToEndScene());
    }

    private IEnumerator TransitionToEndScene()
    {
        yield return new WaitForSeconds(delayBeforeEndScene);
        SceneManager.LoadScene(endGameSceneName);
    }
}
