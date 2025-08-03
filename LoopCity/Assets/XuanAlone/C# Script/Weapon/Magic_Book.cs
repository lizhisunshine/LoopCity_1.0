using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MagicBook : MonoBehaviour
{
    [Header("攻击设置")]
    public float attackRadius = 5f; // 攻击半径
    public float attackCooldown = 1.2f; // 攻击频率
    private float lastAttackTime;

    [Header("魔法阵设置")]
    public GameObject magicCirclePrefab; // 魔法阵预制体
    public float circleDuration = 1f; // 魔法阵持续时间
    public float damagePerCircle = 15f; // 每次伤害值

    [Header("特效")]
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
        // 播放施法效果
        PlayCastEffects();

        // 查找范围内所有敌人
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRadius);
        List<GameObject> enemies = new List<GameObject>();

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                enemies.Add(hit.gameObject);
            }
        }

        // 对每个敌人施放魔法阵
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
        // 实例化魔法阵
        GameObject circle = Instantiate(magicCirclePrefab, target.position, Quaternion.identity);

        // 立即对敌人造成伤害
        ApplyDamage(target.gameObject);

        // 等待持续时间
        yield return new WaitForSeconds(circleDuration);

        // 销毁魔法阵
        Destroy(circle);
    }

    private void ApplyDamage(GameObject enemy)
    {
        // 修改为使用敌人状态机处理伤害
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
        // 播放粒子效果
        if (bookGlowEffect != null)
        {
            bookGlowEffect.Play();
        }

        // 播放声音
        if (castSound != null)
        {
            audioSource.PlayOneShot(castSound);
        }
    }

    // 在编辑器中可视化攻击范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.8f, 0.2f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}