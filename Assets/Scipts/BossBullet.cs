using UnityEngine;

/// <summary>
/// Đạn do Boss hoặc quái vật bắn ra hướng về phía người chơi
/// </summary>
public class BossBullet : MonoBehaviour
{
    [Tooltip("Tốc độ bay của đạn")]
    public float speed = 6f;

    [Tooltip("Thời gian tự hủy")]
    public float lifeTime = 5f;

    private Vector2 moveDirection = Vector2.down;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// Cho phép thiết lập hướng bắn linh hoạt (ví dụ: bắn chùm tỏa góc)
    /// </summary>
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }

    void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage();
            }

            Destroy(gameObject);
        }
    }
}
