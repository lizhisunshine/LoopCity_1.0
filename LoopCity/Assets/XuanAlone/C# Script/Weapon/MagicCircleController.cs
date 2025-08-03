using System.Collections;
using UnityEngine;

public class MagicCircleController : MonoBehaviour
{
    public float rotationSpeed = 45f; // ��ת�ٶ�
    public float scaleSpeed = 0.5f; // �����ٶ�
    public float maxScale = 1.2f; // ������ű���

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
        // ��תЧ��
        //transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
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
}