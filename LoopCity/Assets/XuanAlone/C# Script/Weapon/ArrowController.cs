using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �򻯵ļ�ʸ������
// �޸��ļ�ʸ������
[RequireComponent(typeof(Rigidbody2D))]
public class ArrowController : MonoBehaviour
{
    [Header("��ʸ����")]
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

        // ������������
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Start()
    {
        // ���ü�ʸ�Զ�����
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 dir, float spd, float dmg)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;

        // Ӧ�ó�ʼ�ٶ�
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
    }

    void FixedUpdate()
    {
        // ȷ����ʸ�����ƶ�
        if (rb.velocity.magnitude < speed * 0.9f)
        {
            rb.velocity = direction * speed;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // ��Ҽ�ʸ���е���
        if (isPlayerArrow && collision.CompareTag("Enemy"))
        {
            Enemy_FSM enemy = collision.GetComponent<Enemy_FSM>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        // ���˼�ʸ�������
        else if (!isPlayerArrow && collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        // ����ǽ�ڻ������ϰ���
        else if (collision.CompareTag("Wall") || collision.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // ȷ������ײʱҲ����
        if (collision.gameObject.CompareTag("Wall") ||
            collision.gameObject.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}