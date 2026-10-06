using UnityEngine;

/// <summary>
/// Vẽ khung viền trực quan của Camera trong cửa sổ Scene
/// Giúp dễ dàng quan sát ranh giới hiển thị của màn hình game.
/// </summary>
[ExecuteInEditMode]
public class CameraFrame : MonoBehaviour
{
    [Header("Cài đặt khung viền")]
    public Color frameColor = Color.green;

    private Camera cam;

    void OnDrawGizmos()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        if (cam == null) return;

        // Chỉ áp dụng cho Camera Orthographic (2D)
        if (cam.orthographic)
        {
            float height = 2f * cam.orthographicSize;
            float width = height * cam.aspect;

            Gizmos.color = frameColor;
            Gizmos.DrawWireCube(cam.transform.position, new Vector3(width, height, 0));
        }
    }
}
