using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Dữ liệu cấu hình cho từng Đợt (Wave) thiên thạch
/// </summary>
[System.Serializable]
public class WaveConfig
{
    public string waveName = "WAVE 1";
    public int asteroidCount = 6;
    public float spawnInterval = 1.5f;
    public float speedMultiplier = 1.0f;
    public int extraHealth = 0;
    public float restDuration = 4.0f; // Khoảng thời gian nghỉ sau đợt (giây)
}

/// <summary>
/// Quản lý tiến trình màn chơi theo từng Đợt (Wave Progression):
/// - Thiên thạch rơi theo từng đợt, càng về sau càng dày đặc và nguy hiểm.
/// - Có khoảng nghỉ giữa các đợt (thả sao và vật phẩm nâng cấp để nạp lại sức).
/// - Hiển thị thông báo Đợt (Wave) trên màn hình.
/// - Sau khi vượt qua hết các đợt -> Báo động và Triệu hồi Trùm Cuối (Boss)!
/// - Tự động kích hoạt và kết nối Thanh máu của Boss (Slider).
/// </summary>
public class Spawner : MonoBehaviour
{
    [Header("Prefabs Cơ Bản")]
    public GameObject asteroidPrefab;
    public GameObject starPrefab;

    [Header("Prefabs Vật Phẩm Bổ Trợ (Power-Up)")]
    public GameObject powerUpWeaponPrefab;
    public GameObject powerUpShieldPrefab;

    [Header("Trùm Cuối (Boss Fight)")]
    public GameObject bossPrefab;
    [Tooltip("Thanh máu UI của Boss trong màn chơi (Slider)")]
    public Slider bossHealthSlider;

    [Header("Giao diện Thông Báo Đợt (Wave UI)")]
    [Tooltip("Text thông báo Đợt / Nghỉ ngơi / Cảnh báo Boss")]
    public TextMeshProUGUI waveNoticeText;

    [Header("Cấu hình Các Đợt Thiên Thạch")]
    public List<WaveConfig> waves = new List<WaveConfig>();

    private Camera mainCamera;
    private float screenWidth;
    private float spawnY;
    private bool bossSpawned = false;
    private int currentWaveIndex = 0;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null && mainCamera.orthographic)
        {
            spawnY = mainCamera.orthographicSize + 1.5f;
            screenWidth = mainCamera.orthographicSize * mainCamera.aspect - 0.5f;
        }
        else
        {
            spawnY = 7f;
            screenWidth = 8f;
        }

        // Tự động tìm hoặc tạo WaveText trên Canvas (hiện ở chính giữa màn hình)
        if (waveNoticeText == null)
        {
            GameObject waveObj = GameObject.Find("WaveText");
            if (waveObj != null)
            {
                waveNoticeText = waveObj.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                Canvas canvas = FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject newWave = new GameObject("WaveText", typeof(RectTransform));
                    newWave.transform.SetParent(canvas.transform, false);
                    RectTransform rt = newWave.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = Vector2.zero; // Chính giữa màn hình
                    rt.sizeDelta = new Vector2(400, 36);
                    waveNoticeText = newWave.AddComponent<TextMeshProUGUI>();
                    waveNoticeText.fontSize = 15;
                    waveNoticeText.alignment = TextAlignmentOptions.Center;
                }
            }
        }

        if (waveNoticeText != null)
        {
            RectTransform rt = waveNoticeText.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero; // Luôn đặt ngay chính giữa màn hình
                rt.sizeDelta = new Vector2(400, 36);
            }
            waveNoticeText.fontSize = 15; // Kích thước nhỏ lại vừa vặn, không che quái vật hay thanh máu
            waveNoticeText.alignment = TextAlignmentOptions.Center;
        }

        // Tự động tìm BossHealthSlider nếu chưa kéo vào Inspector
        if (bossHealthSlider == null)
        {
            GameObject sliderObj = GameObject.Find("BossHealthSlider");
            if (sliderObj != null)
            {
                bossHealthSlider = sliderObj.GetComponent<Slider>();
            }
        }

        if (bossHealthSlider != null)
        {
            bossHealthSlider.gameObject.SetActive(false); // Ẩn thanh máu Boss khi chưa xuất hiện
        }

        // Khởi tạo 3 đợt chuẩn nếu danh sách trống
        if (waves.Count == 0)
        {
            waves.Add(new WaveConfig { waveName = "WAVE 1: WARM UP", asteroidCount = 6, spawnInterval = 1.6f, speedMultiplier = 1.0f, extraHealth = 0, restDuration = 2.5f });
            waves.Add(new WaveConfig { waveName = "WAVE 2: SPEED UP", asteroidCount = 12, spawnInterval = 1.1f, speedMultiplier = 1.25f, extraHealth = 1, restDuration = 2.5f });
            waves.Add(new WaveConfig { waveName = "WAVE 3: METEOR STORM", asteroidCount = 18, spawnInterval = 0.85f, speedMultiplier = 1.45f, extraHealth = 2, restDuration = 2.5f });
        }

        // Sinh sao định kỳ độc lập
        InvokeRepeating(nameof(SpawnStar), 2f, 3.5f);

        // Bắt đầu chuỗi các đợt chơi
        StartCoroutine(WaveRoutine());
    }

    /// <summary>
    /// Tiến trình chạy lần lượt từng đợt thiên thạch
    /// </summary>
    IEnumerator WaveRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < waves.Count; i++)
        {
            currentWaveIndex = i;
            WaveConfig wave = waves[i];

            // 1. Chỉ thông báo Bắt đầu Đợt (Gọn gàng, tinh tế)
            ShowNotice("<color=#FACC15><b>" + wave.waveName + "</b></color>", 2.0f);
            yield return new WaitForSeconds(2.0f);

            // 2. Rơi thiên thạch theo số lượng của đợt
            for (int count = 0; count < wave.asteroidCount; count++)
            {
                SpawnAsteroid(wave.speedMultiplier, wave.extraHealth);
                yield return new WaitForSeconds(wave.spawnInterval);
            }

            // Đợi thêm một chút để các thiên thạch bay qua hết
            yield return new WaitForSeconds(1.5f);

            // 3. Nếu chưa phải đợt cuối cùng -> KHOẢNG NGHỈ 2.5s GIỮA CÁC ĐỢT (Không hiện chữ thông báo nghỉ)
            if (i < waves.Count - 1)
            {
                // Thả 1 hộp tiếp tế nâng cấp (Khiên hoặc Đạn) và sao trong lúc nghỉ
                SpawnPowerUp();
                yield return new WaitForSeconds(0.8f);
                SpawnStar();

                yield return new WaitForSeconds(Mathf.Max(0.5f, wave.restDuration - 0.8f));
            }
        }

        // 4. ĐÃ VƯỢT QUA HẾT CÁC ĐỢT -> BÁO ĐỘNG TRÙM CUỐI
        yield return new WaitForSeconds(1.0f);
        TriggerBossFight();
    }

    void SpawnAsteroid(float speedMultiplier, int extraHealth)
    {
        if (asteroidPrefab == null || bossSpawned) return;

        float randomX = Random.Range(-screenWidth, screenWidth);
        Vector3 spawnPos = new Vector3(randomX, spawnY, 0);

        GameObject asteroidObj = Instantiate(asteroidPrefab, spawnPos, Quaternion.identity);
        Asteroid asteroid = asteroidObj.GetComponent<Asteroid>();
        if (asteroid != null)
        {
            asteroid.SetDifficulty(speedMultiplier, extraHealth);
        }
    }

    void SpawnStar()
    {
        if (starPrefab == null) return;

        float randomX = Random.Range(-screenWidth, screenWidth);
        Vector3 spawnPos = new Vector3(randomX, spawnY, 0);
        Instantiate(starPrefab, spawnPos, Quaternion.identity);
    }

    void SpawnPowerUp()
    {
        GameObject selectedPowerUp = (Random.value > 0.4f) ? powerUpWeaponPrefab : powerUpShieldPrefab;
        if (selectedPowerUp == null)
        {
            selectedPowerUp = powerUpWeaponPrefab != null ? powerUpWeaponPrefab : powerUpShieldPrefab;
        }

        if (selectedPowerUp != null)
        {
            float randomX = Random.Range(-screenWidth * 0.7f, screenWidth * 0.7f);
            Vector3 spawnPos = new Vector3(randomX, spawnY, 0);
            Instantiate(selectedPowerUp, spawnPos, Quaternion.identity);
        }
    }

    private Coroutine noticeCoroutine;

    void ShowNotice(string text, float duration, bool blink = false)
    {
        if (waveNoticeText != null)
        {
            if (noticeCoroutine != null)
            {
                StopCoroutine(noticeCoroutine);
                noticeCoroutine = null;
            }
            noticeCoroutine = StartCoroutine(NoticeRoutine(text, duration, blink));
        }
    }

    IEnumerator NoticeRoutine(string text, float duration, bool blink)
    {
        waveNoticeText.gameObject.SetActive(true);
        waveNoticeText.enabled = true;
        waveNoticeText.text = text;

        if (blink)
        {
            float elapsed = 0f;
            float blinkInterval = 0.18f; // Nhịp nhấp nháy dồn dập, hồi hộp
            bool visible = true;

            while (elapsed < duration)
            {
                visible = !visible;
                waveNoticeText.enabled = visible;
                yield return new WaitForSeconds(blinkInterval);
                elapsed += blinkInterval;
            }
        }
        else
        {
            yield return new WaitForSeconds(duration);
        }

        waveNoticeText.enabled = true;
        waveNoticeText.gameObject.SetActive(false);
        noticeCoroutine = null;
    }

    /// <summary>
    /// Kích hoạt Trận Đấu Trùm Cuối (Boss Fight)
    /// </summary>
    public void TriggerBossFight()
    {
        if (bossSpawned || bossPrefab == null) return;

        bossSpawned = true;

        // Báo động đỏ nhấp nháy ở chính giữa màn hình, chữ nhỏ gọn vừa vặn
        ShowNotice("<color=#EF4444><b>WARNING: BOSS COMING!</b></color>", 3.0f, true);

        // Xuất hiện Boss ở giữa trên đỉnh màn hình
        Vector3 bossSpawnPos = new Vector3(0, spawnY + 1.2f, 0);
        GameObject bossObj = Instantiate(bossPrefab, bossSpawnPos, Quaternion.identity);
        Boss boss = bossObj.GetComponent<Boss>();
        if (boss != null)
        {
            if (asteroidPrefab != null)
            {
                boss.summonedAsteroidPrefab = asteroidPrefab;
            }

            // Kết nối thanh máu của Boss trên UI
            if (bossHealthSlider != null)
            {
                bossHealthSlider.gameObject.SetActive(true);
                boss.healthSlider = bossHealthSlider;
                bossHealthSlider.maxValue = boss.maxHealth;
                bossHealthSlider.value = boss.currentHealth;
            }
        }
    }
}
