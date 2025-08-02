using System.Collections;
using UnityEngine;

public class EnemyDamageHandler : MonoBehaviour
{
    public float maxHealth = 100f;
    public GameObject deathEffect;

    private float currentHealth;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // 当受到魔法阵攻击时调用
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 受伤效果
            StartCoroutine(HitEffect());
        }
    }

    private IEnumerator HitEffect()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color original = sr.color;
        sr.color = new Color(1f, 0.5f, 0.5f);

        yield return new WaitForSeconds(0.1f);

        sr.color = original;
    }

    private void Die()
    {
        isDead = true;

        // 播放死亡效果
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // 禁用敌人
        gameObject.SetActive(false);

        // 可选：掉落物品、增加分数等
    }
}