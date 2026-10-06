using UnityEngine;

/// <summary>
/// Script tự động sinh Thiên thạch (Asteroid) và Ngôi sao (Star):
/// - Sinh ngẫu nhiên theo tọa độ ngang X trên đỉnh màn hình.
/// - Định kỳ lặp lại theo khoảng thời gian tùy chỉnh.
/// </summary>
public class Spawner : MonoBehaviour
{
    [Header("Prefabs cần sinh ra")]
    public GameObject asteroidPrefab;
    public GameObject starPrefab;

    [Header("Thời gian sinh (giây)")]
    public float asteroidInterval = 1.5f;
    public float starInterval = 3f;

    private Camera mainCamera;
    private float screenWidth;
    private float spawnY;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null && mainCamera.orthographic)
        {
            spawnY = mainCamera.orthographicSize + 1.5f;
            screenWidth = mainCamera.orthographicSize * mainCamera.aspect - 0.5f;
        }
        else
        {
            spawnY = 7f;
            screenWidth = 8f;
        }

        // Gọi lặp lại hàm sinh
        InvokeRepeating(nameof(SpawnAsteroid), 1f, asteroidInterval);
        InvokeRepeating(nameof(SpawnStar), 2f, starInterval);
    }

    void SpawnAsteroid()
    {
        if (asteroidPrefab == null) return;

        float randomX = Random.Range(-screenWidth, screenWidth);
        Vector3 spawnPos = new Vector3(randomX, spawnY, 0);

        Instantiate(asteroidPrefab, spawnPos, Quaternion.identity);
    }

    void SpawnStar()
    {
        if (starPrefab == null) return;

        float randomX = Random.Range(-screenWidth, screenWidth);
        Vector3 spawnPos = new Vector3(randomX, spawnY, 0);

        Instantiate(starPrefab, spawnPos, Quaternion.identity);
    }
}
