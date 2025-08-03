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

    // ���ܵ�ħ���󹥻�ʱ����
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
            // ����Ч��
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

        // ��������Ч��
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // ���õ���
        gameObject.SetActive(false);

        // ��ѡ��������Ʒ�����ӷ�����
    }
}