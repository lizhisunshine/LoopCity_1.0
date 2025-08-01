using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("子弹设置")]
    public float speed = 10f;
    public float damage = 5f;
    public bool isPlayerBullet = true;
    public float lifetime = 3f; // 子弹存活时间

    private Vector2 direction;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime); // 自动销毁
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        // 设置子弹朝向
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void FixedUpdate()
    {
        // 移动子弹
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
        // 玩家子弹击中敌人
        if (isPlayerBullet && other.CompareTag("Enemy"))
        {
            Enemy_FSM enemy = other.GetComponent<Enemy_FSM>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        // 敌人子弹击中玩家
        else if (!isPlayerBullet && other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        // 击中墙壁
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}