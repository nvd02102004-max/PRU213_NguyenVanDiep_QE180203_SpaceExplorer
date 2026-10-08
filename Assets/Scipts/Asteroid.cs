using System.Collections;
using UnityEngine;

/// <summary>
/// Script điều khiển Thiên thạch / Quái vật:
/// - Tự động định vị và lao về phía phi thuyền của người chơi.
/// - Uốn lượn bám đuổi nhẹ (Homing) trong lúc rơi, tạo cảm giác gay cấn.
/// - Có thanh máu (HP) chịu được nhiều phát đạn khi độ khó tăng.
/// - Chớp đỏ khi bị bắn trúng.
/// </summary>
public class Asteroid : MonoBehaviour
{
    [Header("Cài đặt di chuyển")]
    public float minSpeed = 1.5f;
    public float maxSpeed = 2.8f;
    public float rotationSpeed = 40f;

    [Header("Tìm kiếm mục tiêu (Tracking Tàu)")]
    [Tooltip("Bật/tắt tính năng tìm và rơi đến chỗ tàu người chơi")]
    public bool enableTracking = true;

    [Tooltip("Độ nhạy bám đuổi người chơi khi rơi (0.5 - 1.5)")]
    public float trackingStrength = 0.8f;

    [Header("Kích thước Thiên thạch (Nhỏ gọn, dễ quan sát)")]
    [Tooltip("Kích thước nhỏ nhất")]
    public float minScale = 0.32f;
    [Tooltip("Kích thước lớn nhất")]
    public float maxScale = 0.42f;

    [Header("Cài đặt Máu & Điểm")]
    public int maxHealth = 1;
    public int currentHealth = 1;
    public int scoreValue = 50;

    private Vector2 moveDirection;
    private float speed;
    private SpriteRenderer spriteRenderer;
    private Color defaultColor;
    private Transform playerTransform;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            defaultColor = spriteRenderer.color;
        }

        // Thu nhỏ kích thước thiên thạch lại cho vừa vặn, thoáng màn hình
        float randomScale = Random.Range(minScale, maxScale);
        transform.localScale = new Vector3(randomScale, randomScale, 1f);

        // Tốc độ di chuyển ban đầu chậm rãi, dễ né
        speed = Random.Range(minSpeed, maxSpeed);
        rotationSpeed = Random.Range(-60f, 60f);

        // Tìm vị trí tàu người chơi để nhắm hướng rơi
        FindPlayer();

        if (enableTracking && playerTransform != null)
        {
            // Tính hướng bay nhắm thẳng về phía tàu
            Vector2 toPlayer = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            // Đảm bảo thiên thạch luôn có xu hướng rơi xuống dưới
            if (toPlayer.y > -0.2f)
            {
                toPlayer.y = -0.5f;
                toPlayer = toPlayer.normalized;
            }
            moveDirection = toPlayer;
        }
        else
        {
            // Dự phòng nếu không tìm thấy tàu: rơi chếch xuống dưới ngẫu nhiên
            float randomX = Random.Range(-0.35f, 0.35f);
            moveDirection = new Vector2(randomX, -1f).normalized;
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
        // Nếu bật bám đuổi và tàu còn sống
        if (enableTracking)
        {
            if (playerTransform == null)
            {
                FindPlayer();
            }

            // Chỉ rẽ hướng bám đuổi khi thiên thạch còn đang ở phía trên tàu
            if (playerTransform != null && transform.position.y > playerTransform.position.y)
            {
                Vector2 targetDir = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
                moveDirection = Vector2.Lerp(moveDirection, targetDir, trackingStrength * Time.deltaTime).normalized;
            }
        }

        // Di chuyển theo hướng đã tính toán
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);

        // Tự hủy nếu rơi ra ngoài màn hình
        if (transform.position.y < -10f || Mathf.Abs(transform.position.x) > 15f)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Điều chỉnh tốc độ và máu khi độ khó của màn chơi tăng dần
    /// </summary>
    public void SetDifficulty(float speedMultiplier, int extraHealth)
    {
        speed *= speedMultiplier;
        maxHealth += extraHealth;
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Nhận sát thương khi bị đạn Laser bắn trúng
    /// </summary>
    public void TakeDamage(int damage = 1)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Chớp đỏ khi chưa chết
            StartCoroutine(FlashDamage());
        }
    }

    private IEnumerator FlashDamage()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.08f);
            spriteRenderer.color = defaultColor;
        }
    }

    private void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Va chạm với tàu người chơi
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(); // Nếu có khiên sẽ đỡ được, không có khiên thì nổ
            }

            Destroy(gameObject);
        }
        // Va chạm với đạn Laser
        else if (collision.CompareTag("Laser") || collision.GetComponent<Laser>() != null)
        {
            TakeDamage(1);
            Destroy(collision.gameObject);
        }
    }
}
