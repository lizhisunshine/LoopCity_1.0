using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Settings")]
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f); // ��ͣʱ�Ŵ����
    public float scaleSpeed = 10f; // �����ٶ�

    private Vector3 originalScale; // ԭʼ��С

    void Start()
    {
        // ��¼ԭʼ��С
        originalScale = transform.localScale;
    }

    void Update()
    {
        // ƽ������Ч��
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            originalScale,
            scaleSpeed * Time.unscaledDeltaTime
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // �����ͣʱ�Ŵ�ť
        transform.localScale = hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // ����뿪ʱ�ָ�ԭʼ��С
        transform.localScale = originalScale;
    }
}