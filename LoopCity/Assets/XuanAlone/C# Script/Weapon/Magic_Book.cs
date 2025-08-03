using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MagicBook : MonoBehaviour
{
    [Header("��������")]
    public float attackRadius = 5f; // �����뾶
    public float attackCooldown = 1.2f; // ����Ƶ��
    private float lastAttackTime;

    [Header("ħ��������")]
    public GameObject magicCirclePrefab; // ħ����Ԥ����
    public float circleDuration = 1f; // ħ�������ʱ��
    public float damagePerCircle = 15f; // ÿ���˺�ֵ
    public int sortingOrder = -1; // ��Ⱦ�㼶����ֵ��ʾ�ڵ����·���

    [Header("��Ч")]
    public ParticleSystem bookGlowEffect;
    public AudioClip castSound;

    private AudioSource audioSource;
    private List<GameObject> activeCircles = new List<GameObject>(); // �������м����ħ����

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0.7f; // ��ӿռ�Ч��

        lastAttackTime = -attackCooldown;
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time > lastAttackTime + attackCooldown)
        {
            AttackEnemies();
            lastAttackTime = Time.time;
        }
    }

    private void AttackEnemies()
    {
        // ����ʩ��Ч��
        PlayCastEffects();

        // ���ҷ�Χ�����е���
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRadius);
        List<GameObject> enemies = new List<GameObject>();

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                enemies.Add(hit.gameObject);
            }
        }

        // ��ÿ������ʩ��ħ����
        foreach (GameObject enemy in enemies)
        {
            if (enemy.activeInHierarchy)
            {
                StartCoroutine(CastMagicCircle(enemy.transform));
            }
        }
    }

    private IEnumerator CastMagicCircle(Transform target)
    {
        // ʵ����ħ����
        GameObject circle = Instantiate(magicCirclePrefab, target.position, Quaternion.identity);

        // ����ħ������Ⱦ�㼶�ڵ����·�
        SpriteRenderer circleRenderer = circle.GetComponent<SpriteRenderer>();
        if (circleRenderer != null)
        {
            circleRenderer.sortingOrder = sortingOrder;
        }

        // ��ȡħ���������
        MagicCircleController circleController = circle.GetComponent<MagicCircleController>();
        if (circleController != null)
        {
            // ���ø���Ŀ��
            circleController.SetTarget(target);
        }

        // ��ӵ��ħ�����б�
        activeCircles.Add(circle);

        // �����Ե�������˺�
        ApplyDamage(target.gameObject);

        // �ȴ�����ʱ��
        float elapsed = 0f;
        while (elapsed < circleDuration && circle != null && target != null)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // ��ȫ����ħ����
        if (circle != null)
        {
            activeCircles.Remove(circle);
            Destroy(circle);
        }
    }

    private void ApplyDamage(GameObject enemy)
    {
        Enemy_FSM enemyFSM = enemy.GetComponent<Enemy_FSM>();
        if (enemyFSM != null)
        {
            enemyFSM.TakeDamage(damagePerCircle);
        }
    }

    private void PlayCastEffects()
    {
        // ��������Ч��
        if (bookGlowEffect != null)
        {
            bookGlowEffect.Play();
        }

        // ��������
        if (castSound != null)
        {
            audioSource.PlayOneShot(castSound);
        }
    }

    // �޸�����2��������������ʱ��������ħ����
    void OnDisable()
    {
        foreach (GameObject circle in activeCircles.ToArray()) // ʹ��ToArray�����޸ļ���ʱ����
        {
            if (circle != null)
            {
                Destroy(circle);
            }
        }
        activeCircles.Clear();
    }

    // �ڱ༭���п��ӻ�������Χ
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.8f, 0.2f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}