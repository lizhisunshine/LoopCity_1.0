using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("�ӵ�����")]
    public float speed = 10f;
    public float damage = 5f;
    public bool isPlayerBullet = true;
    public float lifetime = 3f; // �ӵ����ʱ��

    private Vector2 direction;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime); // �Զ�����
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        // �����ӵ�����
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void FixedUpdate()
    {
        // �ƶ��ӵ�
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
        else
        {
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // ����ӵ����е���
        if (isPlayerBullet && other.CompareTag("Enemy"))
        {
            Enemy_FSM enemy = other.GetComponent<Enemy_FSM>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        // �����ӵ��������
        else if (!isPlayerBullet && other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        // ����ǽ��
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}