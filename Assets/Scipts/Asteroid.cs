using UnityEngine;

/// <summary>
/// Script điều khiển Thiên thạch (Asteroid):
/// - Chuyển động ngẫu nhiên hoặc trôi từ trên xuống.
/// - Tự xoay tròn trong không gian.
/// - Xử lý va chạm với Tàu (Player) hoặc Đạn (Laser).
/// </summary>
public class Asteroid : MonoBehaviour
{
    [Header("Cài đặt di chuyển")]
    public float minSpeed = 2f;
    public float maxSpeed = 5f;
    public float rotationSpeed = 50f;

    [Header("Điểm số khi bắn hạ")]
    public int scoreValue = 50;

    private Vector2 moveDirection;
    private float speed;

    void Start()
    {
        // Tốc độ ngẫu nhiên
        speed = Random.Range(minSpeed, maxSpeed);

        // Hướng bay ngẫu nhiên hướng về phía dưới màn hình
        float randomX = Random.Range(-0.5f, 0.5f);
        moveDirection = new Vector2(randomX, -1f).normalized;

        // Tốc độ xoay ngẫu nhiên (xoay trái hoặc xoay phải)
        rotationSpeed = Random.Range(-100f, 100f);
    }

    void Update()
    {
        // Di chuyển thiên thạch
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        // Xoay thiên thạch
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);

        // Tự hủy nếu bay quá sâu ra khỏi mép dưới màn hình (-10)
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Được gọi khi đạn Laser bắn trúng
    /// </summary>
    public void TakeDamage()
    {
        // Cộng điểm thông qua GameManager nếu có
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        // Hủy thiên thạch
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Va chạm với tàu người chơi
        if (collision.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }

            // Hủy tàu và hủy thiên thạch
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
    }
}
