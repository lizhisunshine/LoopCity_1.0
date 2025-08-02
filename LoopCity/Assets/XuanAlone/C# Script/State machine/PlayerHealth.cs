using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    public UnityEvent onTakeDamage;
    public UnityEvent onDeath;

    void Start()
    {
        // 确保当前血量不会低于0
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        onTakeDamage.Invoke();

        if (currentHealth <= 0)
        {
            onDeath.Invoke();
        }
    }
}