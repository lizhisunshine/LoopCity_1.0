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

    [Header("��Ч")]
    public ParticleSystem bookGlowEffect;
    public AudioClip castSound;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

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

        // �����Ե�������˺�
        ApplyDamage(target.gameObject);

        // �ȴ�����ʱ��
        yield return new WaitForSeconds(circleDuration);

        // ����ħ����
        Destroy(circle);
    }

    private void ApplyDamage(GameObject enemy)
    {
        // �޸�Ϊʹ�õ���״̬�������˺�
        Enemy_FSM enemyFSM = enemy.GetComponent<Enemy_FSM>();
        if (enemyFSM != null)
        {
            enemyFSM.TakeDamage(damagePerCircle);
        }
        else
        {
            Debug.LogWarning("Enemy does not have Enemy_FSM component");
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

    // �ڱ༭���п��ӻ�������Χ
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.8f, 0.2f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}