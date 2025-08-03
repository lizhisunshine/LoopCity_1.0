using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public float hurtFlashDuration = 0.3f; // 受击变红持续时间 

    public UnityEvent onTakeDamage;
    public UnityEvent onDeath;

    // 受击反馈相关
    private SpriteRenderer[] spriteRenderers; // 支持多个渲染器
    private Color[] originalColors; // 存储原始颜色
    private Coroutine hurtRoutine;

    // 是否正在受击状态
    public bool IsHurt { get; private set; }

    void Start()
    {
        // 确保当前血量不会低于0
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // 获取所有SpriteRenderer组件（包括子对象）
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[spriteRenderers.Length];

        // 存储原始颜色
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
        }

        // 如果没有渲染器，发出警告
        if (spriteRenderers.Length == 0)
        {
            Debug.LogWarning("PlayerHealth: No SpriteRenderer found on player or its children!");
        }
    }

    public void TakeDamage(float damage)
    {
        // 如果已经死亡，不再处理伤害
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        // 触发受击事件
        onTakeDamage.Invoke();

        // 触发受击反馈
        TriggerHurtEffect();

        // 检查是否死亡
        if (currentHealth <= 0)
        {
            onDeath.Invoke();
        }
    }

    private void TriggerHurtEffect()
    {
        // 如果已经有受击效果在运行，先停止它
        if (hurtRoutine != null)
        {
            StopCoroutine(hurtRoutine);
        }

        // 开始新的受击效果
        hurtRoutine = StartCoroutine(HurtFlash());
    }

    private IEnumerator HurtFlash()
    {
        IsHurt = true;

        // 将所有渲染器设置为红色
        foreach (var renderer in spriteRenderers)
        {
            renderer.color = Color.red;
        }

        // 等待指定时间
        yield return new WaitForSeconds(hurtFlashDuration);

        // 恢复所有渲染器的原始颜色
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

    // 可选：添加生命值恢复方法
    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
    }

    // 可选：设置最大生命值
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }
}