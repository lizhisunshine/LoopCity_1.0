using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 简化的箭矢控制器
// 修复的箭矢控制器
[RequireComponent(typeof(Rigidbody2D))]
public class ArrowController : MonoBehaviour
{
    [Header("箭矢设置")]
    public float damage = 15f;
    public float speed = 15f;
    public bool isPlayerArrow = true;
    public float lifetime = 3f;

    private Rigidbody2D rb;
    private Vector2 direction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        // 设置物理属性
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Start()
    {
        // 设置箭矢自动销毁
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 dir, float spd, float dmg)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;

        // 应用初始速度
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
    }

    void FixedUpdate()
    {
        // 确保箭矢持续移动
        if (rb.velocity.magnitude < speed * 0.9f)
        {
            rb.velocity = direction * speed;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 玩家箭矢击中敌人
        if (isPlayerArrow && collision.CompareTag("Enemy"))
        {
            Enemy_FSM enemy = collision.GetComponent<Enemy_FSM>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        // 敌人箭矢击中玩家
        else if (!isPlayerArrow && collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        // 击中墙壁或其他障碍物
        else if (collision.CompareTag("Wall") || collision.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 确保在碰撞时也销毁
        if (collision.gameObject.CompareTag("Wall") ||
            collision.gameObject.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}