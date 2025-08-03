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
    public int sortingOrder = -1; // 渲染层级（负值表示在敌人下方）

    [Header("特效")]
    public ParticleSystem bookGlowEffect;
    public AudioClip castSound;

    private AudioSource audioSource;
    private List<GameObject> activeCircles = new List<GameObject>(); // 跟踪所有激活的魔法阵

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0.7f; // 添加空间效果

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

        // 设置魔法阵渲染层级在敌人下方
        SpriteRenderer circleRenderer = circle.GetComponent<SpriteRenderer>();
        if (circleRenderer != null)
        {
            circleRenderer.sortingOrder = sortingOrder;
        }

        // 获取魔法阵控制器
        MagicCircleController circleController = circle.GetComponent<MagicCircleController>();
        if (circleController != null)
        {
            // 设置跟随目标
            circleController.SetTarget(target);
        }

        // 添加到活动魔法阵列表
        activeCircles.Add(circle);

        // 立即对敌人造成伤害
        ApplyDamage(target.gameObject);

        // 等待持续时间
        float elapsed = 0f;
        while (elapsed < circleDuration && circle != null && target != null)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 安全销毁魔法阵
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

    // 修复问题2：当武器被禁用时销毁所有魔法阵
    void OnDisable()
    {
        foreach (GameObject circle in activeCircles.ToArray()) // 使用ToArray避免修改集合时迭代
        {
            if (circle != null)
            {
                Destroy(circle);
            }
        }
        activeCircles.Clear();
    }

    // 在编辑器中可视化攻击范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.8f, 0.2f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}