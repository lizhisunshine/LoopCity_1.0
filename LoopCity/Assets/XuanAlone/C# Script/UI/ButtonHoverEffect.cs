using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Settings")]
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f); // 悬停时放大比例
    public float scaleSpeed = 10f; // 缩放速度

    private Vector3 originalScale; // 原始大小

    void Start()
    {
        // 记录原始大小
        originalScale = transform.localScale;
    }

    void Update()
    {
        // 平滑缩放效果
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            originalScale,
            scaleSpeed * Time.unscaledDeltaTime
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 鼠标悬停时放大按钮
        transform.localScale = hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 鼠标离开时恢复原始大小
        transform.localScale = originalScale;
    }
}