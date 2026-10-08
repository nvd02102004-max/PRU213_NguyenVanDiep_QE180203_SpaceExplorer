using System.Collections;
using UnityEngine;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Script điều khiển Tàu vũ trụ (Player):
/// - Di chuyển đa hướng bằng các phím mũi tên / WASD, giới hạn trong Camera.
/// - Hệ thống 3 Mạng (Lives) kèm hiệu ứng chớp bất tử sau khi mất mạng.
/// - Màn chắn năng lượng (Shield Bubble) hiện vòng hào quang xung quanh tàu cực kỳ trực quan khi ăn khiên.
/// - Bắn Laser 3 cấp độ (1 tia, 2 tia song song, 3 tia chùm).
/// - Tự động đồng bộ skin phi thuyền đã chọn từ Shop.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Cài đặt Di chuyển")]
    public float moveSpeed = 8f;
    public float padding = 0.5f;

    [Header("Hệ thống Mạng (Lives)")]
    [Tooltip("Số mạng tối đa của tàu (Mặc định 3 mạng)")]
    public int maxLives = 3;
    public int currentLives = 3;
    [Tooltip("UI hiển thị số mạng (Tùy chọn)")]
    public TextMeshProUGUI livesText;

    [Header("Hệ thống Khiên & Màn Chắn (Shield)")]
    [Tooltip("Tàu có đang kích hoạt khiên chắn không")]
    public bool hasShield = false;
    [Tooltip("Vật thể màn chắn hiển thị quanh tàu")]
    public GameObject shieldVisual;
    public Color shieldColor = new Color(0.2f, 0.9f, 1f, 1f);

    [Header("Cài đặt bắn đạn Laser")]
    public GameObject laserPrefab;
    public Transform firePoint;
    public float fireRate = 0.25f;

    [Header("Hệ thống Nâng cấp Vũ khí")]
    [Range(1, 3)]
    public int weaponLevel = 1;

    [Header("Skin Tàu Từ Cửa Hàng (Shop)")]
    public Sprite[] availableShipSprites;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float nextFireTime = 0f;
    private Camera mainCamera;
    private Vector2 minBounds;
    private Vector2 maxBounds;

    private bool isInvulnerable = false; // Trạng thái bất tử tạm thời sau khi mất mạng
    private GameObject autoShieldObj;

    void Start()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentLives = maxLives;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;

            // Đổi hình dáng phi thuyền theo tàu đã trang bị từ Shop
            int selectedShip = PlayerPrefs.GetInt("SelectedShip", 0);
            if (availableShipSprites != null && selectedShip >= 0 && selectedShip < availableShipSprites.Length)
            {
                if (availableShipSprites[selectedShip] != null)
                {
                    spriteRenderer.sprite = availableShipSprites[selectedShip];
                }
            }
        }

        // Tự động tìm hoặc tạo LivesText trên Canvas
        if (livesText == null)
        {
            GameObject livesObj = GameObject.Find("LivesText");
            if (livesObj != null)
            {
                livesText = livesObj.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                Canvas canvas = FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject newLives = new GameObject("LivesText", typeof(RectTransform));
                    newLives.transform.SetParent(canvas.transform, false);
                    RectTransform rt = newLives.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(1, 1);
                    rt.anchorMax = new Vector2(1, 1);
                    rt.pivot = new Vector2(1, 1);
                    rt.anchoredPosition = new Vector2(-20, -22);
                    rt.sizeDelta = new Vector2(140, 26);
                    livesText = newLives.AddComponent<TextMeshProUGUI>();
                    livesText.fontSize = 15;
                    livesText.alignment = TextAlignmentOptions.Right;
                }
            }
        }

        if (livesText != null)
        {
            livesText.fontSize = 15;
        }

        // Khởi tạo màn chắn hào quang xung quanh tàu nếu chưa có
        SetupShieldVisual();

        UpdateLivesUI();
        CalculateScreenBounds();
    }

    void SetupShieldVisual()
    {
        if (shieldVisual == null)
        {
            // Tự động tạo 1 vòng tròn năng lượng hào quang quanh tàu bằng LineRenderer
            autoShieldObj = new GameObject("ShieldBarrier_Auto");
            autoShieldObj.transform.SetParent(transform);
            autoShieldObj.transform.localPosition = Vector3.zero;

            LineRenderer line = autoShieldObj.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.startWidth = 0.08f;
            line.endWidth = 0.08f;
            line.positionCount = 36;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = shieldColor;
            line.endColor = shieldColor;

            float radius = 1.15f;
            for (int i = 0; i < 36; i++)
            {
                float angle = i * 10f * Mathf.Deg2Rad;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0));
            }

            shieldVisual = autoShieldObj;
        }

        // Mặc định ban đầu chưa có khiên thì ẩn đi
        shieldVisual.SetActive(hasShield);
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();

        // Xoay nhẹ màn chắn khiên tạo hiệu ứng năng lượng công nghệ cao
        if (hasShield && shieldVisual != null)
        {
            shieldVisual.transform.Rotate(0, 0, 90f * Time.deltaTime);
        }
    }

    void CalculateScreenBounds()
    {
        if (mainCamera == null) return;

        Vector2 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector2(0, 0));
        Vector2 topRight = mainCamera.ViewportToWorldPoint(new Vector2(1, 1));

        minBounds = new Vector2(bottomLeft.x + padding, bottomLeft.y + padding);
        maxBounds = new Vector2(topRight.x - padding, topRight.y - padding);
    }

    void HandleMovement()
    {
        Vector2 moveDirection = GetPlayerInput();
        Vector3 newPos = transform.position + (Vector3)(moveDirection * moveSpeed * Time.deltaTime);

        newPos.x = Mathf.Clamp(newPos.x, minBounds.x, maxBounds.x);
        newPos.y = Mathf.Clamp(newPos.y, minBounds.y, maxBounds.y);

        transform.position = newPos;
    }

    Vector2 GetPlayerInput()
    {
        float moveX = 0f;
        float moveY = 0f;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX += 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveY += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveY -= 1f;
        }
#endif

        if (moveX == 0f && moveY == 0f)
        {
            moveX = Input.GetAxisRaw("Horizontal");
            moveY = Input.GetAxisRaw("Vertical");
        }

        return new Vector2(moveX, moveY).normalized;
    }

    void HandleShooting()
    {
        bool shootPressed = false;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
        {
            shootPressed = true;
        }
#endif

        if (!shootPressed && Input.GetKey(KeyCode.Space))
        {
            shootPressed = true;
        }

        if (shootPressed && Time.time >= nextFireTime)
        {
            ShootLaser();
            nextFireTime = Time.time + fireRate;
        }
    }

    void ShootLaser()
    {
        if (laserPrefab == null) return;
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        switch (weaponLevel)
        {
            case 1:
                Instantiate(laserPrefab, spawnPos, Quaternion.identity);
                break;

            case 2:
                Instantiate(laserPrefab, spawnPos + new Vector3(-0.3f, 0, 0), Quaternion.identity);
                Instantiate(laserPrefab, spawnPos + new Vector3(0.3f, 0, 0), Quaternion.identity);
                break;

            case 3:
                Instantiate(laserPrefab, spawnPos, Quaternion.identity);
                Instantiate(laserPrefab, spawnPos + new Vector3(-0.35f, -0.1f, 0), Quaternion.Euler(0, 0, 15f));
                Instantiate(laserPrefab, spawnPos + new Vector3(0.35f, -0.1f, 0), Quaternion.Euler(0, 0, -15f));
                break;
        }
    }

    /// <summary>
    /// Nâng cấp vũ khí khi ăn vật phẩm
    /// </summary>
    public void UpgradeWeapon()
    {
        if (weaponLevel < 3)
        {
            weaponLevel++;
            fireRate = Mathf.Max(0.18f, fireRate - 0.03f);
        }
    }

    /// <summary>
    /// Kích hoạt khiên bảo vệ (Hiện màn chắn hào quang xung quanh)
    /// </summary>
    public void ActivateShield()
    {
        hasShield = true;
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(true);
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.cyan;
        }
    }

    /// <summary>
    /// Nhận sát thương khi bị trúng đạn hoặc va chạm
    /// </summary>
    public bool TakeDamage()
    {
        if (isInvulnerable) return true; // Đang trong thời gian chớp bất tử thì bỏ qua

        // 1. Nếu có khiên chắn -> Khiên vỡ đỡ đòn
        if (hasShield)
        {
            hasShield = false;
            if (shieldVisual != null)
            {
                shieldVisual.SetActive(false);
            }
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
            StartCoroutine(FlashColor(Color.cyan, 0.15f));
            return true;
        }

        // 2. Không có khiên -> Trừ 1 mạng
        currentLives--;
        UpdateLivesUI();

        if (currentLives > 0)
        {
            // Vẫn còn mạng: Bắt đầu chớp nhấp nháy bất tử tạm thời 1.5 giây
            StartCoroutine(InvulnerabilityRoutine());
            return true;
        }
        else
        {
            // Hết sạch mạng -> Game Over
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }

            Destroy(gameObject);
            return false;
        }
    }

    IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;

        // Chớp nhấp nháy tàu 6 lần
        for (int i = 0; i < 6; i++)
        {
            if (spriteRenderer != null) spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.25f);
            yield return new WaitForSeconds(0.12f);
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.12f);
        }

        isInvulnerable = false;
    }

    IEnumerator FlashColor(Color color, float duration)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
            yield return new WaitForSeconds(duration);
            spriteRenderer.color = originalColor;
        }
    }

    public void UpdateLivesUI()
    {
        if (livesText != null)
        {
            string hearts = "";
            for (int i = 0; i < currentLives; i++)
            {
                hearts += "♥  ";
            }
            if (string.IsNullOrEmpty(hearts))
            {
                hearts = "-";
            }
            livesText.text = "<size=16><color=#EF4444><b>" + hearts.Trim() + "</b></color></size>";
        }
    }
}
