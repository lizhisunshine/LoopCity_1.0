using System.Collections;
using UnityEngine;

public class MagicCircleController : MonoBehaviour
{
    public float rotationSpeed = 45f; // 旋转速度
    public float scaleSpeed = 0.5f; // 缩放速度
    public float maxScale = 1.2f; // 最大缩放比例

    private Vector3 originalScale;
    private bool scalingUp = true;

    void Start()
    {
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        StartCoroutine(GrowEffect());
    }

    void Update()
    {
        // 旋转效果
        //transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
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
}