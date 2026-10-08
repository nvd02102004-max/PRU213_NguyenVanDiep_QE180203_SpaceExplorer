using UnityEngine;

/// <summary>
/// Script điều khiển Ngôi sao (Star):
/// - Trôi dần từ trên xuống trong không gian.
/// - Khi Tàu (Player) chạm vào sẽ cộng điểm và tự biến mất.
/// </summary>
public class Star : MonoBehaviour
{
    [Header("Cài đặt di chuyển")]
    public float fallSpeed = 3f;

    [Header("Điểm cộng")]
    public int scorePoints = 100;

    void Update()
    {
        // Sao trôi từ trên xuống
        transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);

        // Tự hủy nếu rơi quá mép dưới màn hình
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Khi tàu người chơi thu thập sao
        if (collision.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scorePoints);
            }

            // Tích lũy sao/tiền tệ vào ví người chơi để mua tàu trong Shop
            int totalStars = PlayerPrefs.GetInt("TotalStars", 0);
            PlayerPrefs.SetInt("TotalStars", totalStars + 1);
            PlayerPrefs.Save();

            // Hủy ngôi sao sau khi ăn
            Destroy(gameObject);
        }
    }
}
