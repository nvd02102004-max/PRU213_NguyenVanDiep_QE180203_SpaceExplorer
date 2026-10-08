using UnityEngine;

/// <summary>
/// Script điều khiển Đạn Laser:
/// - Bay thẳng lên trên theo trục Y.
/// - Tự hủy sau thời gian tồn tại (lifetime).
/// - Xử lý va chạm với Thiên thạch (Asteroid) và Trùm cuối (Boss).
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
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Đạn bay theo hướng trục Y của chính nó
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Bắn trúng Thiên thạch / Quái thường
        if (collision.CompareTag("Asteroid"))
        {
            Asteroid asteroid = collision.GetComponent<Asteroid>();
            if (asteroid != null)
            {
                asteroid.TakeDamage(1);
            }
            else
            {
                Destroy(collision.gameObject);
            }

            Destroy(gameObject);
        }
        // 2. Bắn trúng Boss
        else if (collision.CompareTag("Boss") || collision.GetComponent<Boss>() != null)
        {
            Boss boss = collision.GetComponent<Boss>();
            if (boss != null)
            {
                boss.TakeDamage(1);
            }

            Destroy(gameObject);
        }
    }
}
