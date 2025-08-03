using UnityEngine;
using System.Collections;

public class MagicCircleController : MonoBehaviour
{
    public float rotationSpeed = 45f; // 旋转速度
    public float scaleSpeed = 0.5f; // 缩放速度
    public float maxScale = 1.2f; // 最大缩放比例

    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;
    private bool isFading = false;
    private Transform target; // 要跟随的目标

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        StartCoroutine(GrowEffect());
    }

    void Update()
    {
        // 旋转效果（可选）
        // transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // 如果设置了跟随目标，则更新位置
        if (target != null)
        {
            // 如果目标已被销毁，则开始淡出效果
            if (target.gameObject == null || !target.gameObject.activeInHierarchy)
            {
                StartCoroutine(FadeOut());
                return;
            }

            transform.position = target.position;
        }
    }

    // 设置要跟随的目标
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    private IEnumerator GrowEffect()
    {
        // 快速放大效果
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(0f, maxScale, elapsed / duration);
            transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        // 设置到原始大小
        transform.localScale = originalScale;
    }

    // 添加淡出效果
    public IEnumerator FadeOut()
    {
        if (isFading) yield break;

        isFading = true;
        float fadeDuration = 0.3f;
        float elapsed = 0f;
        Color originalColor = spriteRenderer.color;

        while (elapsed < fadeDuration && spriteRenderer != null)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}