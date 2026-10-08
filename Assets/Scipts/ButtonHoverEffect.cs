using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Hiệu ứng rê chuột cao cấp cho các nút bấm UI (Button):
/// - Tự động phóng to mượt mà (1.08x) khi đưa chuột vào.
/// - Thu nhỏ nhẹ khi click chuột xuống.
/// - Trở lại kích thước ban đầu khi chuột rời đi.
/// Chỉ cần gắn script này vào bất kỳ nút nào là nút đó có hiệu ứng ngay!
/// </summary>
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Cài đặt Hiệu ứng")]
    [Tooltip("Tỷ lệ phóng to khi rê chuột vào")]
    public float hoverScale = 1.08f;

    [Tooltip("Tỷ lệ khi ấn chuột xuống")]
    public float clickScale = 0.95f;

    [Tooltip("Tốc độ chuyển đổi mượt")]
    public float transitionSpeed = 12f;

    private Vector3 originalScale;
    private Vector3 targetScale;

    void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void OnDisable()
    {
        transform.localScale = originalScale;
        targetScale = originalScale;
    }

    void Update()
    {
        // Chuyển động co giãn mượt mà theo thời gian thực (không bị ảnh hưởng bởi Time.timeScale)
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * transitionSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = originalScale * clickScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
    }
}
