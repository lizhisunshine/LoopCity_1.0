using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Ѫ������")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("�¼�")]
    public UnityEvent onDamageTaken;
    public UnityEvent onDeath;
    public UnityEvent onHeal;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return; // ������

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        onDamageTaken.Invoke();
        Debug.Log($"Player took {damage} damage! Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        onHeal.Invoke();
    }

    private void Die()
    {
        onDeath.Invoke();
        Debug.Log("Player died!");
    }
}