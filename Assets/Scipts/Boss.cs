using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script điều khiển Trùm Cuối (Boss Fight):
/// - Máu trâu (80 HP), có 2 Giai đoạn (Phase 1 & Phase 2 Cuồng nộ).
/// - Bộ kỹ năng đa dạng:
///   1. Bắn tỏa hình quạt (3 tia).
///   2. Bắn ngắm bắn chuẩn xác đuổi theo tàu người chơi (Sniper Burst).
///   3. Bão đạn vòng cung (5 tia).
///   4. Bão đạn xoay tròn 8 hướng (Spiral Nova - Phase 2).
///   5. Triệu hồi thiên thạch hộ vệ / Cú lao áp sát (Dive Attack).
/// - Thanh máu UI cập nhật thời gian thực.
/// - Tiêu diệt Boss sẽ kích hoạt Chiến Thắng (Victory).
/// </summary>
public class Boss : MonoBehaviour
{
    [Header("Cài đặt Máu Boss (Trâu hơn)")]
    [Tooltip("Lượng máu tối đa của Boss (Mặc định 80 HP)")]
    public int maxHealth = 80;
    public int currentHealth;
    public int bossScore = 1500;

    [Header("Thanh Máu UI (Nhỏ gọn ở góc)")]
    [Tooltip("Thanh máu UI của Boss (Slider)")]
    public Slider healthSlider;
    public TMPro.TextMeshProUGUI healthText;

    [Header("Cài đặt Di chuyển")]
    public float entrySpeed = 2f;
    public float targetY = 3.2f;
    public float moveSpeed = 1.3f;
    public float moveRange = 4.5f;

    [Header("Cài đặt Tấn công")]
    public GameObject bossBulletPrefab;
    [Tooltip("Prefab thiên thạch triệu hồi (Tùy chọn)")]
    public GameObject summonedAsteroidPrefab;
    public Transform firePoint;
    public float baseFireRate = 2.2f;

    private bool hasEntered = false;
    private float nextFireTime = 0f;
    private float initialX;
    private SpriteRenderer spriteRenderer;
    private Color defaultColor;
    private Transform playerTransform;

    // Quản lý giai đoạn chiến đấu
    private bool isEnraged = false; // Phase 2 khi máu dưới 50%
    private int skillCounter = 0;
    private bool isDiving = false;

    void Start()
    {
        currentHealth = maxHealth;
        initialX = transform.position.x;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            defaultColor = spriteRenderer.color;
        }

        // Tìm vị trí tàu người chơi
        FindPlayer();

        // Cập nhật thanh máu nếu có, hoặc tự động tạo trên Canvas nếu chưa gán
        SetupBossHealthBar();
    }

    void SetupBossHealthBar()
    {
        if (healthSlider == null)
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                // Thanh máu đặt nhỏ gọn ở góc trên bên phải (ngay dưới Live của tàu)
                GameObject barObj = new GameObject("BossHealthBar", typeof(RectTransform));
                barObj.transform.SetParent(canvas.transform, false);
                RectTransform rt = barObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                rt.anchoredPosition = new Vector2(-20, -56);
                rt.sizeDelta = new Vector2(140, 12);

                healthSlider = barObj.AddComponent<Slider>();

                // Nền đen bán trong suốt
                GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
                bgObj.transform.SetParent(barObj.transform, false);
                RectTransform bgRt = bgObj.GetComponent<RectTransform>();
                bgRt.anchorMin = Vector2.zero;
                bgRt.anchorMax = Vector2.one;
                bgRt.sizeDelta = Vector2.zero;
                bgObj.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.85f);

                // Khu vực Fill
                GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
                fillArea.transform.SetParent(barObj.transform, false);
                RectTransform faRt = fillArea.GetComponent<RectTransform>();
                faRt.anchorMin = Vector2.zero;
                faRt.anchorMax = Vector2.one;
                faRt.sizeDelta = Vector2.zero;

                // Thanh máu đỏ neon
                GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
                fillObj.transform.SetParent(fillArea.transform, false);
                RectTransform fillRt = fillObj.GetComponent<RectTransform>();
                fillRt.anchorMin = Vector2.zero;
                fillRt.anchorMax = Vector2.one;
                fillRt.sizeDelta = Vector2.zero;
                Image fillImg = fillObj.GetComponent<Image>();
                fillImg.color = new Color(0.95f, 0.2f, 0.2f, 1f);

                healthSlider.fillRect = fillRt;
                healthSlider.targetGraphic = fillImg;
                healthSlider.direction = Slider.Direction.LeftToRight;

                // Tiêu đề nhỏ gọn phía trên thanh máu: "BOSS: 80/80"
                GameObject textObj = new GameObject("BossTitle", typeof(RectTransform));
                textObj.transform.SetParent(barObj.transform, false);
                RectTransform textRt = textObj.GetComponent<RectTransform>();
                textRt.anchorMin = new Vector2(0, 1);
                textRt.anchorMax = new Vector2(1, 1);
                textRt.pivot = new Vector2(1f, 0);
                textRt.anchoredPosition = new Vector2(0, 2);
                textRt.sizeDelta = new Vector2(0, 16);
                healthText = textObj.AddComponent<TMPro.TextMeshProUGUI>();
                healthText.fontSize = 11;
                healthText.alignment = TMPro.TextAlignmentOptions.Right;
            }
        }

        if (healthSlider != null)
        {
            healthSlider.gameObject.SetActive(true);
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        UpdateHealthUI();
    }

    public void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"<color=#EF4444><b>BOSS</b></color> <size=10><color=#94A3B8>{Mathf.Max(0, currentHealth)}/{maxHealth}</color></size>";
        }
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void Update()
    {
        if (playerTransform == null)
        {
            FindPlayer();
        }

        if (!hasEntered)
        {
            // Giai đoạn xuất hiện: Boss bay từ từ từ đỉnh màn hình xuống targetY
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(initialX, targetY, 0), entrySpeed * Time.deltaTime);
            if (Mathf.Abs(transform.position.y - targetY) < 0.05f)
            {
                hasEntered = true;
                nextFireTime = Time.time + 1f;
            }
            return;
        }

        if (isDiving) return; // Nếu đang trong đòn lao thì tạm dừng lượn sóng

        // Giai đoạn chiến đấu: Lượn sóng qua lại theo phương ngang
        float currentSpeed = isEnraged ? moveSpeed * 1.5f : moveSpeed;
        float newX = initialX + Mathf.Sin(Time.time * currentSpeed) * moveRange;
        transform.position = new Vector3(newX, targetY, 0);

        // Tung chiêu theo chu kỳ
        if (Time.time >= nextFireTime)
        {
            ExecuteRandomSkill();
            float currentRate = isEnraged ? baseFireRate * 0.65f : baseFireRate;
            nextFireTime = Time.time + currentRate;
        }
    }

    /// <summary>
    /// Hệ thống luân phiên các kỹ năng của Boss
    /// </summary>
    void ExecuteRandomSkill()
    {
        skillCounter++;

        if (!isEnraged)
        {
            // === PHASE 1: MÁU TRÊN 50% ===
            int skill = skillCounter % 3;
            switch (skill)
            {
                case 0:
                    SkillSpreadAttack(3); // Bắn chùm 3 tia
                    break;
                case 1:
                    StartCoroutine(SkillSniperBurst(2)); // Ngắm bắn chính xác 2 phát
                    break;
                case 2:
                    SkillArcNovaAttack(5); // Bão đạn vòng cung 5 tia
                    break;
            }
        }
        else
        {
            // === PHASE 2: CUỒNG NỘ (ENRAGED - MÁU DƯỚI 50%) ===
            int skill = skillCounter % 5;
            switch (skill)
            {
                case 0:
                    SkillArcNovaAttack(7); // Vòng cung 7 tia cực dày
                    break;
                case 1:
                    StartCoroutine(SkillSniperBurst(3)); // Ngắm bắn 3 phát dồn dập
                    break;
                case 2:
                    SkillSpiralBarrage(8); // Bão đạn xoáy 8 hướng
                    break;
                case 3:
                    SkillSummonMeteors(); // Triệu hồi thiên thạch rơi
                    break;
                case 4:
                    StartCoroutine(SkillDiveRush()); // Lao áp sát người chơi
                    break;
            }
        }
    }

    // --- KỸ NĂNG 1: Bắn tỏa hình quạt ---
    void SkillSpreadAttack(int bulletCount)
    {
        if (bossBulletPrefab == null) return;
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        CreateBullet(spawnPos, Vector2.down);
        CreateBullet(spawnPos, new Vector2(-0.35f, -1f));
        CreateBullet(spawnPos, new Vector2(0.35f, -1f));
    }

    // --- KỸ NĂNG 2: Ngắm bắn chuẩn xác theo người chơi (Sniper Burst) ---
    IEnumerator SkillSniperBurst(int burstCount)
    {
        if (bossBulletPrefab == null) yield break;

        for (int i = 0; i < burstCount; i++)
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            Vector2 aimDir = Vector2.down;

            if (playerTransform != null)
            {
                aimDir = ((Vector2)playerTransform.position - (Vector2)spawnPos).normalized;
            }

            CreateBullet(spawnPos, aimDir);
            yield return new WaitForSeconds(0.2f);
        }
    }

    // --- KỸ NĂNG 3: Bão đạn vòng cung (Arc Nova Blast) ---
    void SkillArcNovaAttack(int count)
    {
        if (bossBulletPrefab == null) return;
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        float angleSpread = 80f; // Tổng góc tỏa ra
        float startAngle = -angleSpread / 2f;
        float angleStep = angleSpread / (count - 1);

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + (angleStep * i);
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            Vector2 dir = rot * Vector2.down;
            CreateBullet(spawnPos, dir);
        }
    }

    // --- KỸ NĂNG 4: Bão đạn xoay tròn 8 hướng (Spiral Nova - Phase 2) ---
    void SkillSpiralBarrage(int directions)
    {
        if (bossBulletPrefab == null) return;
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        float angleStep = 360f / directions;
        for (int i = 0; i < directions; i++)
        {
            float angle = angleStep * i;
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            Vector2 dir = rot * Vector2.down;
            CreateBullet(spawnPos, dir);
        }
    }

    // --- KỸ NĂNG 5: Triệu hồi thiên thạch rơi đuổi theo tàu ---
    void SkillSummonMeteors()
    {
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        // Nếu có prefab thiên thạch thì gọi thiên thạch, nếu không thì bắn 3 đạn siêu nhanh
        if (summonedAsteroidPrefab != null)
        {
            Instantiate(summonedAsteroidPrefab, new Vector3(transform.position.x - 2f, 6.5f, 0), Quaternion.identity);
            Instantiate(summonedAsteroidPrefab, new Vector3(transform.position.x + 2f, 6.5f, 0), Quaternion.identity);
        }
        else
        {
            // Bắn 3 viên đạn kép tốc độ cao
            CreateBullet(spawnPos + new Vector3(-1f, 0, 0), Vector2.down);
            CreateBullet(spawnPos, Vector2.down);
            CreateBullet(spawnPos + new Vector3(1f, 0, 0), Vector2.down);
        }
    }

    // --- KỸ NĂNG 6: Lao áp sát bất ngờ (Dive Rush - Phase 2) ---
    IEnumerator SkillDiveRush()
    {
        isDiving = true;
        Vector3 originPos = transform.position;
        Vector3 diveTarget = new Vector3(originPos.x, 0.5f, 0); // Lao xuống gần giữa màn hình

        // Lao xuống nhanh
        float t = 0;
        while (t < 0.6f)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(originPos, diveTarget, t / 0.6f);
            yield return null;
        }

        // Bắn 1 đợt đạn tỏa ở vị trí thấp
        SkillSpreadAttack(3);
        yield return new WaitForSeconds(0.3f);

        // Lùi về vị trí cũ
        t = 0;
        while (t < 0.8f)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(diveTarget, originPos, t / 0.8f);
            yield return null;
        }

        transform.position = originPos;
        isDiving = false;
    }

    void CreateBullet(Vector3 position, Vector2 direction)
    {
        GameObject bulletObj = Instantiate(bossBulletPrefab, position, Quaternion.identity);
        BossBullet bullet = bulletObj.GetComponent<BossBullet>();
        if (bullet != null)
        {
            bullet.SetDirection(direction);
        }
    }

    /// <summary>
    /// Nhận sát thương khi bị đạn người chơi bắn trúng
    /// </summary>
    public void TakeDamage(int damage = 1)
    {
        currentHealth -= damage;
        UpdateHealthUI();

        // Kích hoạt Phase 2 (Cuồng nộ) khi máu giảm dưới 50%
        if (!isEnraged && currentHealth <= maxHealth / 2)
        {
            TriggerEnragePhase();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(FlashRed());
        }
    }

    void TriggerEnragePhase()
    {
        isEnraged = true;
        // Đổi màu thân tàu sang tông màu lửa đỏ cuồng nộ
        if (spriteRenderer != null)
        {
            defaultColor = new Color(1f, 0.4f, 0.4f, 1f);
            spriteRenderer.color = defaultColor;
        }
    }

    private IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.06f);
            spriteRenderer.color = defaultColor;
        }
    }

    private void Die()
    {
        // Ẩn thanh máu và chữ
        if (healthSlider != null)
        {
            healthSlider.gameObject.SetActive(false);
        }
        if (healthText != null)
        {
            healthText.gameObject.SetActive(false);
        }

        // Cộng điểm thưởng lớn
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(bossScore);
            // Kích hoạt Chiến Thắng
            GameManager.Instance.Victory();
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Nếu Boss va chạm trực tiếp với Tàu người chơi
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage();
            }
        }
    }
}
