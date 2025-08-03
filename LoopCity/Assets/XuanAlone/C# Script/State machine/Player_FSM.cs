using UnityEngine;
using MY_FSM;
using System;
using Random = UnityEngine.Random;
using static UnityEditor.Rendering.CoreEditorDrawer<TData>;

// 主角黑板数据
[Serializable]

#region  玩家黑板
public class PlayerBlackboard : Blackboard
{

    // 添加以下字段
    [Header("当前武器")]
    public WeaponType currentWeapon;

    [Header("组件引用")]
    public PlayerHealth playerHealth;

    [Header("角色属性")]
    public float maxHealth = 100f;
    public float moveSpeed = 100f;
    public float attackDamage = 20f;
    public float attackCooldown = 0.5f;

    [Header("引用")]
    public Transform transform;
    public Rigidbody2D rb;
    public Transform firePoint;
    public GameObject bulletPrefab;

    [Header("运行时数据")]
    public float currentHealth;
    public Vector2 moveDirection;
    public Vector2 aimDirection;
    public float lastAttackTime;

    [Header("动画控制")]
    public Animator animator;
    public float lastHorizontal = 1f; // 默认朝右
    [Header("方向控制")]
    public float lastValidHorizontal = 1f; // 默认朝右
    public bool facingRight = true; // 当前朝向

    // 新增方法：更新瞄准方向
    public void UpdateAimDirection()
    {
        // 获取鼠标位置并转换为世界坐标
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        // 计算朝向鼠标的方向
        aimDirection = (mousePosition - transform.position).normalized;
    }
}
#endregion

#region  待机状态
// 待机状态
public class Player_IdleState : IState
{
    private FSM fsm;
    public PlayerBlackboard blackboard;

    public Player_IdleState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as PlayerBlackboard;
    }

    public void OnEnter()
    {
        // 使用最后有效方向
        if (blackboard.animator)
        {
            blackboard.animator.SetFloat("Horizontal", blackboard.lastValidHorizontal);
            blackboard.animator.SetFloat("Vertical", 0f);
            blackboard.animator.SetFloat("Speed", 0f);
        }
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        if (blackboard.playerHealth.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
            return;
        }
        // 获取输入
        blackboard.moveDirection = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        // 更新瞄准方向（关键修复：在Idle状态也要更新）
        blackboard.UpdateAimDirection();

        // 如果有移动输入，切换到移动状态
        if (blackboard.moveDirection.magnitude > 0.1f)
        {
            fsm.SwitchState(MY_FSM.StateType.Move);
        }

        // 处理攻击输入
        HandleAttackInput();
    }

    private void HandleAttackInput()
    {
        // 鼠标左键攻击
        if (Input.GetMouseButton(0))
        {
            // 检查攻击冷却
            if (Time.time - blackboard.lastAttackTime > blackboard.attackCooldown)
            {
                fsm.SwitchState(MY_FSM.StateType.Attack);
            }
        }
    }
}
#endregion

#region  移动状态
// 移动状态
public class Player_MoveState : IState
{
    private FSM fsm;
    private PlayerBlackboard blackboard;

    public Player_MoveState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as PlayerBlackboard;
    }

    public void OnEnter() { }

    public void OnExit() { }

    public void OnUpdate()
    {
        if (blackboard.playerHealth.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
            return;
        }

        // 获取输入
        blackboard.moveDirection = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        // 方向控制 ================================
        if (blackboard.moveDirection.x != 0)
        {
            // 更新水平方向
            blackboard.facingRight = blackboard.moveDirection.x > 0;
            blackboard.lastValidHorizontal = blackboard.facingRight ? 1f : -1f;
        }

        // 设置动画参数
        if (blackboard.animator)
        {
            // 上下移动时使用最后有效水平方向
            float animHorizontal = blackboard.moveDirection.x != 0 ?
                blackboard.moveDirection.x :
                blackboard.lastValidHorizontal;

            blackboard.animator.SetFloat("Horizontal", animHorizontal);
            blackboard.animator.SetFloat("Vertical", blackboard.moveDirection.y);
            blackboard.animator.SetFloat("Speed", blackboard.moveDirection.magnitude);
        }

        // 更新瞄准方向
        blackboard.UpdateAimDirection();

        // 应用移动
        blackboard.rb.velocity = blackboard.moveDirection * blackboard.moveSpeed;

        // 如果没有移动输入，切换到待机状态
        if (blackboard.moveDirection.magnitude < 0.1f)
        {
            fsm.SwitchState(MY_FSM.StateType.Idle);
        }

        // 处理攻击输入
        HandleAttackInput();
    }

    private void HandleAttackInput()
    {
        if (Input.GetMouseButton(0))
        {
            if (Time.time - blackboard.lastAttackTime > blackboard.attackCooldown)
            {
                fsm.SwitchState(MY_FSM.StateType.Attack);
            }
        }
    }
}

#endregion

#region 攻击状态
// 攻击状态
public class Player_AttackState : IState
{
    private FSM fsm;
    private PlayerBlackboard blackboard;

    public Player_AttackState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as PlayerBlackboard;
    }

    public void OnEnter()
    {
        // 使用实时方向（鼠标瞄准方向）
        if (blackboard.animator)
        {
            // 计算水平方向：如果鼠标在右侧则为1，左侧则为-1
            float horizontal = blackboard.aimDirection.x > 0 ? 1f : -1f;
            blackboard.animator.SetFloat("Horizontal", horizontal);
            blackboard.animator.SetTrigger("Attack");
        }

        // 发射子弹
        FireBullet();

        // 记录攻击时间
        blackboard.lastAttackTime = Time.time;

        // 立即返回到上一个状态
        if (fsm.prevState != null)
        {
            fsm.SwitchState(fsm.prevStateType);
        }
        else
        {
            // 默认返回待机状态
            fsm.SwitchState(MY_FSM.StateType.Idle);
        }
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        if (blackboard.playerHealth.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
            return;
        }
    }

    private void FireBullet()
    {
        // 只有魔法棒武器才发射子弹
        if (blackboard.currentWeapon != WeaponType.MagicWand)
        {
            // 记录攻击时间但不发射子弹
            blackboard.lastAttackTime = Time.time;
            return;
        }

        // 原始发射子弹的代码保持不变...
        if (blackboard.bulletPrefab && blackboard.firePoint)
        {
            GameObject bullet = GameObject.Instantiate(
                blackboard.bulletPrefab,
                blackboard.firePoint.position,
                Quaternion.identity
            );

            BulletController bulletController = bullet.GetComponent<BulletController>();
            if (bulletController)
            {
                bulletController.direction = blackboard.aimDirection;
                bulletController.damage = blackboard.attackDamage;
                bulletController.speed = 10f;
                bulletController.isPlayerBullet = true;

                if (bulletController.direction != Vector2.zero)
                {
                    float angle = Mathf.Atan2(
                        bulletController.direction.y,
                        bulletController.direction.x
                    ) * Mathf.Rad2Deg;

                    bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                }
            }
        }
    }
}
#endregion

#region  死亡状态
// 死亡状态
public class Player_DieState : IState
{
    private FSM fsm;
    private PlayerBlackboard blackboard;
    private float deathAnimationLength = 1.5f; // 死亡动画长度（秒）
    private float deathTimer;
    private bool animationStarted;

    public Player_DieState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as PlayerBlackboard;
    }

    public void OnEnter()
    {
        // 停止所有移动
        blackboard.rb.velocity = Vector2.zero;

        // 禁用物理碰撞
        if (blackboard.rb != null)
        {
            blackboard.rb.simulated = false;
        }

        // 播放死亡动画
        if (blackboard.animator != null)
        {
            // 确保动画控制器有"Die"触发器
            if (HasAnimationTrigger("Die"))
            {
                blackboard.animator.SetTrigger("Die");
                animationStarted = true;

                // 获取动画长度（如果可能）
                deathAnimationLength = GetAnimationLength("Die");
            }
            else
            {
                Debug.LogWarning("Animator does not have 'Die' trigger");
                animationStarted = false;
            }
        }
        else
        {
            animationStarted = false;
        }

        // 禁用玩家控制组件
        if (blackboard.transform.GetComponent<Player_FSM>() != null)
        {
            blackboard.transform.GetComponent<Player_FSM>().enabled = false;
        }

        // 禁用所有碰撞体
        Collider2D[] colliders = blackboard.transform.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        // 重置计时器
        deathTimer = 0f;
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        // 更新计时器
        deathTimer += Time.deltaTime;

        // 如果动画已播放完毕，销毁玩家对象
        if (deathTimer >= deathAnimationLength)
        {
            // 可以在这里添加游戏结束逻辑
            GameObject.Destroy(blackboard.transform.gameObject);
        }
    }

    // 检查动画控制器是否有指定的触发器
    private bool HasAnimationTrigger(string triggerName)
    {
        if (blackboard.animator == null) return false;

        foreach (AnimatorControllerParameter param in blackboard.animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger &&
                param.name == triggerName)
            {
                return true;
            }
        }
        return false;
    }

    // 获取动画长度
    private float GetAnimationLength(string animationName)
    {
        if (blackboard.animator == null) return deathAnimationLength;

        RuntimeAnimatorController ac = blackboard.animator.runtimeAnimatorController;
        foreach (AnimationClip clip in ac.animationClips)
        {
            if (clip.name == animationName)
            {
                return clip.length;
            }
        }
        return deathAnimationLength; // 默认值
    }
}
#endregion

#region  主角FSM行为类
// 主角状态机控制器
public class Player_FSM : MonoBehaviour
{
    [Header("玩家状态机设置")]
    public PlayerBlackboard blackboard;
    private FSM fsm;

    // 添加缺失的子弹设置字段
    [Header("子弹设置")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    void Start()
    {
        Debug.Log("Player_FSM Start begin");

        // 1. 确保 PlayerHealth 组件存在
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.Log("Adding PlayerHealth component");
            playerHealth = gameObject.AddComponent<PlayerHealth>();
        }

        // 2. 确保黑板对象已创建
        if (blackboard == null)
        {
            Debug.Log("Creating new PlayerBlackboard");
            blackboard = new PlayerBlackboard();
        }

        // 3. 设置 transform 引用
        blackboard.transform = transform;
        Debug.Log($"Transform set: {blackboard.transform != null}");

        // 4. 获取 Rigidbody2D 组件
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogWarning("Adding Rigidbody2D component");
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
        }
        blackboard.rb = rb;
        Debug.Log($"Rigidbody2D set: {blackboard.rb != null}");

        // 5. 确保 firePoint 有效
        if (firePoint == null)
        {
            Debug.LogWarning("FirePoint not set, trying to find or create one");
            firePoint = transform.Find("FirePoint");
            if (firePoint == null)
            {
                Debug.Log("Creating FirePoint child object");
                GameObject fpObj = new GameObject("FirePoint");
                fpObj.transform.SetParent(transform);
                fpObj.transform.localPosition = new Vector3(0.5f, 0, 0); // 默认位置
                firePoint = fpObj.transform;
            }
        }
        blackboard.firePoint = firePoint;
        Debug.Log($"FirePoint set: {blackboard.firePoint != null}");

        // 6. 确保子弹预制体有效
        if (bulletPrefab == null)
        {
            Debug.LogWarning("BulletPrefab not set, trying to load default");
            bulletPrefab = Resources.Load<GameObject>("DefaultBullet");
            if (bulletPrefab == null)
            {
                Debug.LogError("Default bullet prefab not found in Resources");
                // 创建临时子弹作为后备
                bulletPrefab = CreateFallbackBullet();
            }
        }
        blackboard.bulletPrefab = bulletPrefab;
        Debug.Log($"BulletPrefab set: {blackboard.bulletPrefab != null}");

        // 7. 获取动画控制器
        Animator animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("Animator not found on player");
        }
        blackboard.animator = animator;
        Debug.Log($"Animator set: {blackboard.animator != null}");

        // 8. 设置玩家健康组件
        blackboard.playerHealth = playerHealth;
        Debug.Log($"PlayerHealth set: {blackboard.playerHealth != null}");

        // 9. 初始化当前生命值
        blackboard.currentHealth = blackboard.maxHealth;
        playerHealth.currentHealth = blackboard.maxHealth; // 确保PlayerHealth组件同步
        playerHealth.maxHealth = blackboard.maxHealth;
        Debug.Log($"Current health set: {blackboard.currentHealth}");

        // 10. 初始化状态机
        fsm = new FSM(blackboard);
        Debug.Log("FSM created");

        // 设置游戏对象层
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer != -1)
        {
            gameObject.layer = playerLayer;
            Debug.Log($"Layer set to Player: {playerLayer}");
        }
        else
        {
            Debug.LogWarning("Player layer not defined");
        }

        // 11. 添加状态
        try
        {
            fsm.AddState(MY_FSM.StateType.Idle, new Player_IdleState(fsm));
            fsm.AddState(MY_FSM.StateType.Move, new Player_MoveState(fsm));
            fsm.AddState(MY_FSM.StateType.Attack, new Player_AttackState(fsm));
            fsm.AddState(MY_FSM.StateType.Die, new Player_DieState(fsm));
            Debug.Log("States added to FSM");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error adding states: {ex.Message}");
        }

        // 12. 设置初始状态
        fsm.SwitchState(MY_FSM.StateType.Idle);
        Debug.Log("Initial state set to Idle");

        // 13. 注册死亡事件
        playerHealth.onDeath.AddListener(OnPlayerDeath);
        Debug.Log("Death event listener added");

        Debug.Log("Player_FSM Start completed");
    }

    // 创建临时子弹作为后备方案
    private GameObject CreateFallbackBullet()
    {
        GameObject bullet = new GameObject("FallbackBullet");
        bullet.AddComponent<SpriteRenderer>();
        bullet.AddComponent<CircleCollider2D>().isTrigger = true;
        BulletController bc = bullet.AddComponent<BulletController>();
        bc.damage = 10f;
        bc.speed = 10f;

        return bullet;
    }

    void OnPlayerDeath()
    {
        if (fsm.curState != fsm.states[MY_FSM.StateType.Die])
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
        }
    }

    void Update()
    {
        fsm.OnUpdate();
    }

    void FixedUpdate()
    {
        // 物理更新
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // 传递伤害值，而不是固定10点
            EnemyBlackboard enemyBlackboard = collision.gameObject.GetComponent<Enemy_FSM>()?.blackboard;
            if (enemyBlackboard != null)
            {
                // 根据敌人类型决定伤害
                float damage = 10f; // 默认伤害
                switch (enemyBlackboard.enemyType)
                {
                    case EnemyBlackboard.EnemyType.Exploder:
                        damage = enemyBlackboard.explosionDamage;
                        break;
                    case EnemyBlackboard.EnemyType.Dasher:
                        damage = enemyBlackboard.dashDamage;
                        break;
                    case EnemyBlackboard.EnemyType.Shooter:
                        damage = enemyBlackboard.projectileDamage;
                        break;
                }

                blackboard.playerHealth.TakeDamage(damage);
            }
            else
            {
                // 默认伤害
                blackboard.playerHealth.TakeDamage(10f);
            }
        }
    }
}
#endregion