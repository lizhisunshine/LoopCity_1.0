using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public float hurtFlashDuration = 0.3f; // �ܻ�������ʱ�� 

    public UnityEvent onTakeDamage;
    public UnityEvent onDeath;

    // �ܻ��������
    private SpriteRenderer[] spriteRenderers; // ֧�ֶ����Ⱦ��
    private Color[] originalColors; // �洢ԭʼ��ɫ
    private Coroutine hurtRoutine;

    // �Ƿ������ܻ�״̬
    public bool IsHurt { get; private set; }

    void Start()
    {
        // ȷ����ǰѪ���������0
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // ��ȡ����SpriteRenderer����������Ӷ���
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[spriteRenderers.Length];

        // �洢ԭʼ��ɫ
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
        }

        // ���û����Ⱦ������������
        if (spriteRenderers.Length == 0)
        {
            Debug.LogWarning("PlayerHealth: No SpriteRenderer found on player or its children!");
        }
    }

    public void TakeDamage(float damage)
    {
        // ����Ѿ����������ٴ����˺�
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        // �����ܻ��¼�
        onTakeDamage.Invoke();

        // �����ܻ�����
        TriggerHurtEffect();

        // ����Ƿ�����
        if (currentHealth <= 0)
        {
            onDeath.Invoke();
        }
    }

    private void TriggerHurtEffect()
    {
        // ����Ѿ����ܻ�Ч�������У���ֹͣ��
        if (hurtRoutine != null)
        {
            StopCoroutine(hurtRoutine);
        }

        // ��ʼ�µ��ܻ�Ч��
        hurtRoutine = StartCoroutine(HurtFlash());
    }

    private IEnumerator HurtFlash()
    {
        IsHurt = true;

        // ��������Ⱦ������Ϊ��ɫ
        foreach (var renderer in spriteRenderers)
        {
            renderer.color = Color.red;
        }

        // �ȴ�ָ��ʱ��
        yield return new WaitForSeconds(hurtFlashDuration);

        // �ָ�������Ⱦ����ԭʼ��ɫ
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].color = originalColors[i];
            }
        }

        IsHurt = false;
        hurtRoutine = null;
    }

    // ��ѡ���������ֵ�ָ�����
    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
    }

    // ��ѡ�������������ֵ
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }
}