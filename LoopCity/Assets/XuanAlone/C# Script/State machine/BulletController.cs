using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("�ӵ�����")]
    public Vector2 direction;
    public float speed = 10f;
    public float damage = 20f;
    public bool isPlayerBullet = false;
    public float lifetime = 5f; // ��Ϊ5��

    private Rigidbody2D rb;

    void Start()
    {

        // �����ӵ��㼶
        gameObject.layer = LayerMask.NameToLayer("Bullet");
        // ��ȡ����Ӹ������
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0; // ��������
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // ������ײ���
        }

        // �����ӵ�����
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // Ӧ�ó�ʼ�ٶ�
            //rb.velocity = direction * speed;
        }

        // �Զ�����
        Destroy(gameObject, lifetime);
    }
    void Update()
    {
        // ʹ��Transformֱ���ƶ�������������������
        transform.position += (Vector3)direction * speed * Time.deltaTime;
    }

    // ʹ���������ȷ����ײ���
    void FixedUpdate()
    {
        // ���ֺ㶨�ٶ�
        rb.velocity = direction * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // ����ӵ���������
        if (isPlayerBullet && other.CompareTag("Enemy"))
        {
            // �Ե�������˺�
            Enemy_FSM enemy = other.GetComponent<Enemy_FSM>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            // �����ӵ�
            Destroy(gameObject);
        }
        // ����ǽ�ڻ������ϰ���
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
