using UnityEngine;

/// <summary>
/// Các loại vật phẩm bổ trợ (Power-Up)
/// </summary>
public enum PowerUpType
{
    WeaponUpgrade, // Nâng cấp vũ khí (bắn nhiều đạn hơn)
    Shield         // Lá chắn năng lượng đỡ 1 đòn va chạm
}

/// <summary>
/// Quản lý vật phẩm nâng cấp rơi trong không gian
/// </summary>
public class PowerUp : MonoBehaviour
{
    [Header("Loại Vật Phẩm")]
    public PowerUpType powerUpType = PowerUpType.WeaponUpgrade;

    [Header("Cài đặt di chuyển")]
    public float fallSpeed = 2f;
    public float rotationSpeed = 30f;

    [Header("Điểm thưởng khi ăn")]
    public int bonusScore = 50;

    void Update()
    {
        // Rơi nhẹ nhàng từ trên xuống
        transform.Translate(Vector2.down * fallSpeed * Time.deltaTime, Space.World);

        // Tự xoay nhẹ để tạo sự bắt mắt
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);

        // Tự hủy nếu trôi quá mép dưới màn hình
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
                if (powerUpType == PowerUpType.WeaponUpgrade)
                {
                    player.UpgradeWeapon();
                }
                else if (powerUpType == PowerUpType.Shield)
                {
                    player.ActivateShield();
                }
            }

            // Cộng thêm điểm thưởng nếu có GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(bonusScore);
            }

            // Biến mất sau khi nhặt
            Destroy(gameObject);
        }
    }
}
