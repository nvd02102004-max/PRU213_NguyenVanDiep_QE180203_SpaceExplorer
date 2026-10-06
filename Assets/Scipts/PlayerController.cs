using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Script điều khiển Tàu vũ trụ (Player):
/// - Di chuyển đa hướng bằng các phím mũi tên / WASD.
/// - Giới hạn vị trí di chuyển trong khung nhìn Camera.
/// - Bắn đạn laser khi nhấn phím Space.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Cài đặt di chuyển")]
    [Tooltip("Tốc độ di chuyển của tàu")]
    public float moveSpeed = 8f;

    [Tooltip("Khoảng cách đệm giữa tàu và viền camera")]
    public float padding = 0.5f;

    [Header("Cài đặt bắn đạn Laser")]
    [Tooltip("Prefab của viên đạn Laser")]
    public GameObject laserPrefab;

    [Tooltip("Vị trí nòng súng để bắn đạn")]
    public Transform firePoint;

    [Tooltip("Khoảng cách tối thiểu giữa 2 lần bắn (Fire Rate)")]
    public float fireRate = 0.25f;

    private float nextFireTime = 0f;
    private Camera mainCamera;
    private Vector2 minBounds;
    private Vector2 maxBounds;

    void Start()
    {
        mainCamera = Camera.main;
        CalculateScreenBounds();
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();
    }

    /// <summary>
    /// Tính toán giới hạn màn hình dựa trên kích thước Orthographic của Camera
    /// </summary>
    void CalculateScreenBounds()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null && mainCamera.orthographic)
        {
            float vertExtent = mainCamera.orthographicSize;
            float horzExtent = vertExtent * mainCamera.aspect;

            Vector3 camPos = mainCamera.transform.position;

            minBounds = new Vector2(camPos.x - horzExtent + padding, camPos.y - vertExtent + padding);
            maxBounds = new Vector2(camPos.x + horzExtent - padding, camPos.y + vertExtent - padding);
        }
    }

    /// <summary>
    /// Xử lý nhận tín hiệu bàn phím và di chuyển tàu
    /// Tương thích cả Input System Mới (Unity 6) và Input Cũ
    /// </summary>
    void HandleMovement()
    {
        float inputX = 0f;
        float inputY = 0f;

#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) inputX -= 1f;
            if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) inputX += 1f;
            if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed) inputY += 1f;
            if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed) inputY -= 1f;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (inputX == 0f && inputY == 0f)
        {
            inputX = Input.GetAxisRaw("Horizontal");
            inputY = Input.GetAxisRaw("Vertical");
        }
#endif

        // Chuẩn hóa vector di chuyển để không bị đi chéo nhanh hơn
        Vector2 movement = new Vector2(inputX, inputY).normalized;

        // Vị trí mới dự kiến
        Vector3 newPos = transform.position + (Vector3)(movement * moveSpeed * Time.deltaTime);

        // Giới hạn tàu luôn nằm gọn trong khung nhìn Camera
        newPos.x = Mathf.Clamp(newPos.x, minBounds.x, maxBounds.x);
        newPos.y = Mathf.Clamp(newPos.y, minBounds.y, maxBounds.y);

        transform.position = newPos;
    }

    /// <summary>
    /// Xử lý bắn đạn Laser khi nhấn phím Space
    /// </summary>
    void HandleShooting()
    {
        bool shootPressed = false;

#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.spaceKey.isPressed)
        {
            shootPressed = true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKey(KeyCode.Space))
        {
            shootPressed = true;
        }
#endif

        if (shootPressed && Time.time >= nextFireTime)
        {
            ShootLaser();
            nextFireTime = Time.time + fireRate;
        }
    }

    void ShootLaser()
    {
        if (laserPrefab != null)
        {
            // Vị trí bắn: nếu chưa gán firePoint thì bắn tại vị trí của tàu
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            Instantiate(laserPrefab, spawnPos, Quaternion.identity);
        }
    }
}
