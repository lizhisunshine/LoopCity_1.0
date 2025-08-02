using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("子弹设置")]
    public Vector2 direction;
    public float speed = 10f;
    public float damage = 20f;
    public bool isPlayerBullet = false;
    public float lifetime = 5f; // 改为5秒

    private Rigidbody2D rb;

    void Start()
    {

        // 设置子弹层级
        gameObject.layer = LayerMask.NameToLayer("Bullet");
        // 获取或添加刚体组件
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0; // 禁用重力
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // 连续碰撞检测
        }

        // 设置子弹方向
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // 应用初始速度
            //rb.velocity = direction * speed;
        }

        // 自动销毁
        Destroy(gameObject, lifetime);
    }
    void Update()
    {
        // 使用Transform直接移动，避免物理引擎问题
        transform.position += (Vector3)direction * speed * Time.deltaTime;
    }

    // 使用物理更新确保碰撞检测
    void FixedUpdate()
    {
        // 保持恒定速度
        rb.velocity = direction * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 玩家子弹碰到敌人
        if (isPlayerBullet && other.CompareTag("Enemy"))
        {
            // 对敌人造成伤害
            Enemy_FSM enemy = other.GetComponent<Enemy_FSM>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            // 销毁子弹
            Destroy(gameObject);
        }
        // 碰到墙壁或其他障碍物
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
