using UnityEngine;
using System.Collections;

public class MagicCircleController : MonoBehaviour
{
    public float rotationSpeed = 45f; // ��ת�ٶ�
    public float scaleSpeed = 0.5f; // �����ٶ�
    public float maxScale = 1.2f; // ������ű���

    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;
    private bool isFading = false;
    private Transform target; // Ҫ�����Ŀ��

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        StartCoroutine(GrowEffect());
    }

    void Update()
    {
        // ��תЧ������ѡ��
        // transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // ��������˸���Ŀ�꣬�����λ��
        if (target != null)
        {
            // ���Ŀ���ѱ����٣���ʼ����Ч��
            if (target.gameObject == null || !target.gameObject.activeInHierarchy)
            {
                StartCoroutine(FadeOut());
                return;
            }

            transform.position = target.position;
        }
    }

    // ����Ҫ�����Ŀ��
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    private IEnumerator GrowEffect()
    {
        // ���ٷŴ�Ч��
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(0f, maxScale, elapsed / duration);
            transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        // ���õ�ԭʼ��С
        transform.localScale = originalScale;
    }

    // ��ӵ���Ч��
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