using UnityEngine;

/// <summary>
/// Script điều khiển Đạn Laser:
/// - Bay thẳng lên trên theo trục Y.
/// - Tự hủy sau thời gian tồn tại (lifetime).
/// - Xử lý va chạm với thiên thạch (Asteroid).
/// </summary>
public class Laser : MonoBehaviour
{
    [Header("Cài đặt đạn")]
    [Tooltip("Tốc độ bay của đạn")]
    public float speed = 14f;

    [Tooltip("Thời gian tự hủy nếu không bắn trúng gì")]
    public float lifeTime = 3f;

    void Start()
    {
        // Tự hủy sau một khoảng thời gian để tránh lãng phí RAM
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Đạn luôn bay hướng lên trên
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra nếu bắn trúng Thiên thạch
        if (collision.CompareTag("Asteroid"))
        {
            // Lấy script Asteroid của đối tượng va chạm để gọi logic nổ/cộng điểm
            Asteroid asteroid = collision.GetComponent<Asteroid>();
            if (asteroid != null)
            {
                asteroid.TakeDamage();
            }
            else
            {
                Destroy(collision.gameObject);
            }

            // Tiêu hủy chính viên đạn
            Destroy(gameObject);
        }
    }
}
