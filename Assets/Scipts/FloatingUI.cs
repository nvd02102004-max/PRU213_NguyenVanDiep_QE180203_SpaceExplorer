using UnityEngine;

/// <summary>
/// Hiệu ứng nhấp nhô lơ lửng cho phi thuyền hoặc UI trong Menu
/// </summary>
public class FloatingUI : MonoBehaviour
{
    [Header("Cài đặt Lơ Lửng (Floating)")]
    [Tooltip("Độ cao nhấp nhô lên xuống")]
    public float amplitude = 12f;

    [Tooltip("Tốc độ nhấp nhô")]
    public float frequency = 2.2f;

    private RectTransform rectTransform;
    private Vector2 startAnchoredPos;
    private Vector3 startLocalPos;
    private bool isUI = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            isUI = true;
            startAnchoredPos = rectTransform.anchoredPosition;
        }
        else
        {
            isUI = false;
            startLocalPos = transform.localPosition;
        }
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.unscaledTime * frequency) * amplitude;

        if (isUI && rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(startAnchoredPos.x, startAnchoredPos.y + offset);
        }
        else
        {
            transform.localPosition = new Vector3(startLocalPos.x, startLocalPos.y + offset * 0.05f, startLocalPos.z);
        }
    }
}
