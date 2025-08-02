using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public PlayerHealth playerHealth;

    void Start()
    {
        // 确保找到玩家健康组件
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
        }

        if (playerHealth != null && healthSlider != null)
        {
            // 设置血条最大值
            healthSlider.maxValue = playerHealth.maxHealth;
            
            // 关键修复：立即设置当前血量值
            healthSlider.value = playerHealth.maxHealth;
            
            // 注册事件
            playerHealth.onTakeDamage.AddListener(UpdateHealthBar);
            playerHealth.onDeath.AddListener(HandleDeath);
        }
        else
        {
            Debug.LogError("HealthBar: Missing playerHealth or healthSlider reference");
        }
    }

    void UpdateHealthBar()
    {
        if (healthSlider != null)
        {
            // 平滑过渡效果
            healthSlider.value = playerHealth.currentHealth;
        }
    }

    void HandleDeath()
    {
        healthSlider.value = 0;
    }

    void OnDestroy()
    {
        // 安全取消事件监听
        if (playerHealth != null)
        {
            playerHealth.onTakeDamage.RemoveListener(UpdateHealthBar);
            playerHealth.onDeath.RemoveListener(HandleDeath);
        }
    }
}